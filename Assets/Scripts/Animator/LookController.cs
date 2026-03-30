using UnityEngine;

public class LookController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [Tooltip("Opcional: si no se asigna, se busca en padres. Si no existe, la mirada IK sigue activa.")]
    [SerializeField] private PlayerController playerController;

    [Header("IK Settings")]
    [SerializeField] private float lookWeight = 1f;
    [SerializeField] private float bodyWeight = 0.05f;
    [SerializeField] private float headWeight = 1f;
    [SerializeField] private float clampWeight = 0.7f;

    [Header("Settings")]
    [SerializeField] private float lookDistance = 10f;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float maxLookAngle = 60f;

    private Animator animator;
    private Vector3 currentLookPosition;

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (playerController == null)
            playerController = GetComponentInParent<PlayerController>();

        // Inicializar para evitar saltos raros
        currentLookPosition = transform.position + transform.forward * lookDistance;
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null || cameraTransform == null) return;

        if (playerController != null && playerController.PlayerStatus == PlayerStatus.Walking)
        {
            animator.SetLookAtWeight(0f);
            return;
        }

        Vector3 rawTarget = cameraTransform.position + cameraTransform.forward * lookDistance;
        rawTarget.y = Mathf.Max(rawTarget.y, transform.position.y + 1.2f);

        Vector3 target = GetClampedLookTargetPosition(rawTarget);

        currentLookPosition = Vector3.Lerp(currentLookPosition, target, Time.deltaTime * smoothSpeed);

        animator.SetLookAtWeight(lookWeight, bodyWeight, headWeight, 0.5f, clampWeight);
        animator.SetLookAtPosition(currentLookPosition);
    }

    private Vector3 GetClampedLookTargetPosition(Vector3 rawTarget)
    {
        Vector3 origin = transform.position;
        rawTarget.y = Mathf.Max(rawTarget.y, origin.y + 1.2f);

        Vector3 toTarget = rawTarget - origin;
        float dist = toTarget.magnitude;
        if (dist < 1e-4f)
            return origin + transform.forward * lookDistance;

        Vector3 desired = toTarget / dist;

        Vector3 fwdH = transform.forward;
        fwdH.y = 0f;
        if (fwdH.sqrMagnitude < 1e-6f)
            fwdH = Vector3.forward;
        fwdH.Normalize();

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
}
