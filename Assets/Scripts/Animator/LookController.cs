using UnityEngine;

public class LookController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    private PlayerController playerController;

    [Header("Detection")]
    [SerializeField] private float interactRadius = 5f;
    [Tooltip("Si está vacío (Nothing), se aceptan colliders de cualquier capa.")]
    [SerializeField] private LayerMask interactableLayer;

    [Header("IK Settings")]
    [SerializeField] private float lookWeight = 1f;
    [SerializeField] private float bodyWeight = 0.05f;
    [SerializeField] private float headWeight = 1f;
    [SerializeField] private float clampWeight = 0.7f;

    [Header("Settings")]
    [SerializeField] private float lookDistance = 10f;
    [SerializeField] private float maxLookAngle = 60f;
    [Tooltip("Suavizado del punto de mira (objeto vs cámara) antes de aplicar límites de cabeza.")]
    [SerializeField][Range(0.02f, 0.8f)] private float goalSmoothTime = 0.18f;
    [Tooltip("Suavizado final del punto que recibe el IK (cabeza).")]
    [SerializeField][Range(0.02f, 0.8f)] private float lookIkSmoothTime = 0.14f;
    [Tooltip("Tiempo para bajar el peso del IK al pasar a caminar (evita salto brusco a mirar al frente).")]
    [SerializeField][Range(0.05f, 1.5f)] private float walkIkFadeTime = 0.28f;

    private Animator animator;
    private Vector3 currentLookPosition;
    private Vector3 smoothedGoal;
    private Vector3 goalSmoothVelocity;
    private Vector3 lookIkSmoothVelocity;
    private float ikBlend = 1f;
    private float ikBlendVelocity;

    private Transform BodyRoot => playerController != null ? playerController.transform : transform;

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (playerController == null)
            playerController = GetComponentInParent<PlayerController>();

        Vector3 initial = cameraTransform != null
            ? cameraTransform.position + cameraTransform.forward * lookDistance
            : BodyRoot.position + BodyRoot.forward * lookDistance;
        initial.y = Mathf.Max(initial.y, BodyRoot.position.y + 1.2f);
        smoothedGoal = initial;
        currentLookPosition = initial;
        goalSmoothVelocity = Vector3.zero;
        lookIkSmoothVelocity = Vector3.zero;
        ikBlend = 1f;
        ikBlendVelocity = 0f;
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null || cameraTransform == null) return;

        bool walking = playerController != null && playerController.PlayerStatus == PlayerStatus.Walking;
        bool hasInteractable = TryGetInteractableLookPoint(out Vector3 interactPoint);

        bool suppressLookIkWhileWalking = walking && !hasInteractable;
        float targetBlend = suppressLookIkWhileWalking ? 0f : 1f;
        float fadeT = Mathf.Max(0.0001f, walkIkFadeTime);
        ikBlend = Mathf.SmoothDamp(ikBlend, targetBlend, ref ikBlendVelocity, fadeT);

        Vector3 instantGoal;
        if (suppressLookIkWhileWalking)
            instantGoal = GetNeutralLookPoint();
        else if (hasInteractable)
            instantGoal = interactPoint;
        else
            instantGoal = cameraTransform.position + cameraTransform.forward * lookDistance;

        instantGoal.y = Mathf.Max(instantGoal.y, BodyRoot.position.y + 1.2f);

        float gT = Mathf.Max(0.0001f, goalSmoothTime);
        smoothedGoal = Vector3.SmoothDamp(smoothedGoal, instantGoal, ref goalSmoothVelocity, gT);

        Vector3 target = GetClampedLookTargetPosition(smoothedGoal);

        float ikT = Mathf.Max(0.0001f, lookIkSmoothTime);
        currentLookPosition = Vector3.SmoothDamp(currentLookPosition, target, ref lookIkSmoothVelocity, ikT);

        float bw = ikBlend * lookWeight;
        if (bw < 0.0001f)
        {
            animator.SetLookAtWeight(0f);
            return;
        }

        animator.SetLookAtWeight(ikBlend * lookWeight, ikBlend * bodyWeight, ikBlend * headWeight, ikBlend * 0.5f, clampWeight);
        animator.SetLookAtPosition(currentLookPosition);
    }

    private Vector3 GetNeutralLookPoint()
    {
        Vector3 p = BodyRoot.position + BodyRoot.forward * lookDistance;
        p.y = Mathf.Max(p.y, BodyRoot.position.y + 1.2f);
        return p;
    }

    private bool TryGetInteractableLookPoint(out Vector3 worldPoint)
    {
        worldPoint = default;
        Vector3 origin = BodyRoot.position;
        GetHorizontalForward(out Vector3 fwdH);

        Collider[] hits = Physics.OverlapSphere(
            origin,
            interactRadius,
            ~0,
            QueryTriggerInteraction.Collide);

        Collider best = null;
        float bestScore = float.MaxValue;

        foreach (Collider col in hits)
        {
            if (col == null) continue;
            if (!PassesLayerFilter(col.gameObject.layer)) continue;

            Vector3 targetPoint = col.bounds.center;
            Vector3 toTarget = targetPoint - origin;
            float dist = toTarget.magnitude;
            if (dist < 1e-4f || dist > interactRadius) continue;

            Vector3 toH = toTarget;
            toH.y = 0f;
            float hLen = toH.magnitude;
            if (hLen < 1e-4f) continue;

            toH /= hLen;
            float yaw = Vector3.Angle(fwdH, toH);
            if (yaw > maxLookAngle) continue;

            float score = yaw + dist * 0.01f;
            if (score < bestScore)
            {
                bestScore = score;
                best = col;
            }
        }

        if (best == null) return false;

        worldPoint = best.bounds.center;
        return true;
    }

    private bool PassesLayerFilter(int layer)
    {
        if (interactableLayer.value == 0) return true;
        return (interactableLayer.value & (1 << layer)) != 0;
    }

    private void GetHorizontalForward(out Vector3 fwdH)
    {
        fwdH = BodyRoot.forward;
        fwdH.y = 0f;
        if (fwdH.sqrMagnitude < 1e-6f) fwdH = Vector3.forward;
        fwdH.Normalize();
    }

    private Vector3 GetClampedLookTargetPosition(Vector3 rawTarget)
    {
        Vector3 origin = BodyRoot.position;
        rawTarget.y = Mathf.Max(rawTarget.y, origin.y + 1.2f);

        Vector3 toTarget = rawTarget - origin;
        float dist = toTarget.magnitude;
        if (dist < 1e-4f)
            return origin + BodyRoot.forward * lookDistance;

        Vector3 desired = toTarget / dist;

        GetHorizontalForward(out Vector3 fwdH);

        Vector3 desiredH = desired;
        desiredH.y = 0f;
        float hLenSq = desiredH.sqrMagnitude;
        if (hLenSq < 1e-8f)
            return origin + desired * lookDistance;

        desiredH.Normalize();

        float yawAngle = Vector3.Angle(fwdH, desiredH);
        Vector3 yawDir = desiredH;
        if (yawAngle > maxLookAngle)
            yawDir = Vector3.Slerp(fwdH, desiredH, maxLookAngle / yawAngle).normalized;

        float hMag = Mathf.Sqrt(hLenSq);
        Vector3 newDir = new Vector3(yawDir.x * hMag, desired.y, yawDir.z * hMag);
        if (newDir.sqrMagnitude < 1e-6f)
            newDir = yawDir;
        else
            newDir.Normalize();

        Vector3 result = origin + newDir * lookDistance;
        result.y = Mathf.Max(result.y, origin.y + 1.2f);
        return result;
    }

    private void OnDrawGizmos()
    {
        var pc = GetComponentInParent<PlayerController>();
        Vector3 p = pc != null ? pc.transform.position : transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(p, interactRadius);
    }
}

