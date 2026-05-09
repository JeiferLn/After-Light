using UnityEngine;

[RequireComponent(typeof(Animator))]
[DefaultExecutionOrder(-100)]
public class PlayerLookTargetProvider : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private PlayerController playerController;

    [Header("Detection")]
    [SerializeField] private float interactRadius = 5f;
    [Tooltip("Si está vacío (Nothing), se aceptan colliders de cualquier capa.")]
    [SerializeField] private LayerMask interactableLayer;

    [Header("Settings")]
    [SerializeField] private float minHeightOffset = 0.8f;
    [SerializeField] private float lookDistance = 10f;
    [SerializeField] private float maxLookAngle = 60f;

    [Header("Suavizado — Sin interactuable")]
    [Tooltip("Suavizado del goal cuando no hay interactuable (sigue la cámara).")]
    [SerializeField][Range(0.02f, 0.8f)] private float goalSmoothTime = 0.18f;
    [Tooltip("Suavizado final del IK cuando no hay interactuable.")]
    [SerializeField][Range(0.02f, 0.8f)] private float lookIkSmoothTime = 0.14f;

    [Header("Suavizado — Con interactuable")]
    [Tooltip("Suavizado del goal al mirar un interactuable. Más bajo = más responsivo.")]
    [SerializeField][Range(0.02f, 0.4f)] private float interactableGoalSmoothTime = 0.1f;
    [Tooltip("Suavizado final del IK al mirar un interactuable.")]
    [SerializeField][Range(0.02f, 0.4f)] private float interactableLookIkSmoothTime = 0.08f;

    private InteractableScanner interactableScanner;
    private Vector3 currentLookPosition;
    private Vector3 smoothedGoal;
    private Vector3 goalSmoothVelocity;
    private Vector3 lookIkSmoothVelocity;
    private bool lookingAtInteractable;
    private int lastUpdatedFrame = -1;

    private Transform BodyRoot => playerController != null ? playerController.transform : transform;

    public Vector3 CurrentLookPosition => currentLookPosition;
    public float CurrentBlend => 1f;
    public Transform CameraTransform => cameraTransform;
    public bool IsInteractableSmoothingActive => lookingAtInteractable;

    void Awake()
    {
        if (playerController == null)
            playerController = GetComponentInParent<PlayerController>();

        interactableScanner = new InteractableScanner();

        Vector3 initial = cameraTransform != null
            ? cameraTransform.position + cameraTransform.forward * lookDistance
            : BodyRoot.position + BodyRoot.forward * lookDistance;
        initial.y = Mathf.Max(initial.y, BodyRoot.position.y + minHeightOffset);
        smoothedGoal = initial;
        currentLookPosition = initial;
    }

    void OnAnimatorIK(int layerIndex)
    {
        UpdateFrame();
    }

    public void UpdateFrame()
    {
        int frame = Time.frameCount;
        if (lastUpdatedFrame == frame) return;
        lastUpdatedFrame = frame;
        ComputeLookTarget();
    }

    private void ComputeLookTarget()
    {
        if (cameraTransform == null) return;

        bool aiming = playerController != null && PlayerStatusHelpers.IsAimingStatus(playerController.PlayerStatus);
        bool hasInteractable = TryGetInteractableLookPoint(out Vector3 interactPoint);

        lookingAtInteractable = hasInteractable && !aiming;

        Vector3 instantGoal;
        if (aiming)
            instantGoal = GetAimingPitchOnlyLookPoint();
        else if (hasInteractable)
            instantGoal = interactPoint;
        else
            instantGoal = cameraTransform.position + cameraTransform.forward * lookDistance;

        instantGoal.y = Mathf.Max(instantGoal.y, BodyRoot.position.y + minHeightOffset);

        float gT = Mathf.Max(0.0001f, lookingAtInteractable ? interactableGoalSmoothTime : goalSmoothTime);
        smoothedGoal = Vector3.SmoothDamp(smoothedGoal, instantGoal, ref goalSmoothVelocity, gT);

        Vector3 target = aiming
            ? GetClampedLookTargetPositionPitchOnly(smoothedGoal)
            : GetClampedLookTargetPosition(smoothedGoal);

        float ikT = Mathf.Max(0.0001f, lookingAtInteractable ? interactableLookIkSmoothTime : lookIkSmoothTime);
        currentLookPosition = Vector3.SmoothDamp(currentLookPosition, target, ref lookIkSmoothVelocity, ikT);
    }

    private Vector3 GetAimingPitchOnlyLookPoint()
    {
        GetHorizontalForward(out Vector3 fwdH);
        Vector3 right = Vector3.Cross(Vector3.up, fwdH);
        if (right.sqrMagnitude < 1e-8f) right = Vector3.right;
        right.Normalize();

        Vector3 camF = cameraTransform.forward;
        float horizontalMag = new Vector3(camF.x, 0f, camF.z).magnitude;
        float pitch = Mathf.Atan2(camF.y, Mathf.Max(horizontalMag, 1e-4f));
        Vector3 lookDir = Quaternion.AngleAxis(-pitch * Mathf.Rad2Deg, right) * fwdH;
        lookDir.Normalize();

        return BodyRoot.position + lookDir * lookDistance;
    }

    private bool TryGetInteractableLookPoint(out Vector3 worldPoint)
    {
        GetHorizontalForward(out Vector3 fwdH);
        return interactableScanner.TryGetBest(
            BodyRoot.position,
            fwdH,
            interactRadius,
            maxLookAngle,
            interactableLayer,
            out worldPoint);
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

        Vector3 newDir = yawDir;
        newDir.y = desired.y;

        if (newDir.sqrMagnitude < 1e-6f)
            newDir = yawDir;
        else
            newDir.Normalize();

        return origin + newDir * lookDistance;
    }

    private Vector3 GetClampedLookTargetPositionPitchOnly(Vector3 rawTarget)
    {
        Vector3 origin = BodyRoot.position;
        Vector3 toTarget = rawTarget - origin;
        if (toTarget.sqrMagnitude < 1e-6f)
            return GetAimingPitchOnlyLookPoint();

        Vector3 desired = toTarget.normalized;
        GetHorizontalForward(out Vector3 fwdH);
        Vector3 right = Vector3.Cross(Vector3.up, fwdH);
        if (right.sqrMagnitude < 1e-8f) right = Vector3.right;
        right.Normalize();

        Vector3 onPlane = desired - right * Vector3.Dot(desired, right);
        if (onPlane.sqrMagnitude < 1e-8f)
            onPlane = fwdH;
        onPlane.Normalize();

        float angle = Vector3.Angle(fwdH, onPlane);
        if (angle > maxLookAngle)
            onPlane = Vector3.Slerp(fwdH, onPlane, maxLookAngle / Mathf.Max(angle, 1e-4f)).normalized;

        return origin + onPlane * lookDistance;
    }

    private void OnDrawGizmos()
    {
        var pc = playerController != null ? playerController : GetComponentInParent<PlayerController>();
        Vector3 p = pc != null ? pc.transform.position : transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(p, interactRadius);
    }
}
