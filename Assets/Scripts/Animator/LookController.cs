using UnityEngine;

public class LookController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;

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

        // Inicializar para evitar saltos raros
        currentLookPosition = transform.position + transform.forward * lookDistance;
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null || cameraTransform == null) return;

        // 🎯 1. Calcular hacia dónde mira la cámara
        Vector3 target = cameraTransform.position + cameraTransform.forward * lookDistance;

        // 🧠 2. Evitar mirar demasiado abajo (importante)
        target.y = Mathf.Max(target.y, transform.position.y + 1.2f);

        // 🔒 3. Limitar ángulo (evita giros irreales)
        if (!IsWithinAngle(target))
        {
            target = transform.position + transform.forward * lookDistance;
        }

        // 🧊 4. Suavizado (elimina vibración)
        currentLookPosition = Vector3.Lerp(currentLookPosition, target, Time.deltaTime * smoothSpeed);

        // 🎮 5. Aplicar IK
        animator.SetLookAtWeight(lookWeight, bodyWeight, headWeight, 0.5f, clampWeight);
        animator.SetLookAtPosition(currentLookPosition);
    }

    // 🔍 Limitar ángulo de mirada
    private bool IsWithinAngle(Vector3 targetPosition)
    {
        Vector3 dir = (targetPosition - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dir);
        return angle < maxLookAngle;
    }
}