using UnityEngine;

[RequireComponent(typeof(Animator))]
// Corre antes que HeadLookIK para que su OnAnimatorIK actualice CurrentLookPosition primero.
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

    [Header("Suavizado — Idle")]
    [Tooltip("Suavizado del punto de mira en idle.")]
    [SerializeField][Range(0.02f, 0.8f)] private float goalSmoothTime = 0.18f;
    [Tooltip("Suavizado final del punto de mira en idle.")]
    [SerializeField][Range(0.02f, 0.8f)] private float lookIkSmoothTime = 0.14f;
    [Tooltip("Tiempo para bajar el blend al pasar a caminar.")]
    [SerializeField][Range(0.05f, 1.5f)] private float walkIkFadeTime = 0.28f;

    [Header("Suavizado — Activo (caminar / correr / agachado / apuntar)")]
    [Tooltip("Suavizado del punto de mira cuando el jugador no está en idle.")]
    [SerializeField][Range(0.01f, 0.4f)] private float goalSmoothTimeActive = 0.04f;
    [Tooltip("Suavizado final del punto de mira cuando el jugador no está en idle.")]
    [SerializeField][Range(0.01f, 0.4f)] private float lookIkSmoothTimeActive = 0.03f;

    [Header("Suavizado — Transición de interactuable")]
    [Tooltip("Suavizado del goal mientras se transita entre 'mira al objeto' y 'deja de mirarlo'.")]
    [SerializeField][Range(0.05f, 1.5f)] private float interactableTransitionGoalSmoothTime = 0.5f;
    [Tooltip("Suavizado final mientras dura la transición de interactuable.")]
    [SerializeField][Range(0.05f, 1.5f)] private float interactableTransitionLookSmoothTime = 0.4f;
    [Tooltip("Duración (segundos) en que se aplica el suavizado más alto tras cambiar de estado de interactuable.")]
    [SerializeField][Range(0.05f, 2f)] private float interactableTransitionDuration = 0.7f;

    private InteractableScanner interactableScanner;
    private Vector3 currentLookPosition;
    private Vector3 smoothedGoal;
    private Vector3 goalSmoothVelocity;
    private Vector3 lookIkSmoothVelocity;
    private float currentBlend = 1f;
    private float blendVelocity;
    private int lastUpdatedFrame = -1;
    private bool lastLookingAtInteractable;
    private bool lookingAtInteractable;
    private float interactableTransitionTimer;

    private Transform BodyRoot => playerController != null ? playerController.transform : transform;

    public Vector3 CurrentLookPosition => currentLookPosition;

    public float CurrentBlend => currentBlend;

    public Transform CameraTransform => cameraTransform;

    // True mientras la cabeza está enfocada en un interactuable, o durante la ventana de
    // transición tras dejar de enfocarlo. Otros sistemas (linterna, etc.) pueden usar este
    // flag para aplicar un suavizado más alto y mantener un look feel consistente.
    public bool IsInteractableSmoothingActive =>
        lookingAtInteractable || interactableTransitionTimer > 0f;

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
        goalSmoothVelocity = Vector3.zero;
        lookIkSmoothVelocity = Vector3.zero;
        currentBlend = 1f;
        blendVelocity = 0f;
    }

    // Cálculo dentro de OnAnimatorIK (igual que el LookController original) para que coincida
    // con el momento en que el Animator está aplicando IK. Idempotente por frame.
    void OnAnimatorIK(int layerIndex)
    {
        UpdateFrame();
    }

    // Permite a cualquier consumer (HeadLookIK, FlashlightFollow, etc.) forzar la actualización
    // del look target dentro del mismo frame antes de leer. No vuelve a recomputar si ya se hizo.
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

        bool walking = playerController != null && playerController.PlayerStatus == PlayerStatus.Walking;
        bool aiming = playerController != null && PlayerStatusHelpers.IsAimingStatus(playerController.PlayerStatus);
        bool hasInteractable = TryGetInteractableLookPoint(out Vector3 interactPoint);

        bool suppressLook = walking && !hasInteractable;
        // El IK realmente está enfocado al interactuable cuando lo detecta y no está
        // ni apuntando ni en la pose neutral por caminar.
        lookingAtInteractable = hasInteractable && !aiming && !suppressLook;

        // Detecta cambio de "miro/no miro al interactuable" y arma el timer de transición.
        // Reseteamos las velocidades del SmoothDamp para que la transición arranque desde
        // velocidad cero (sin "kick" heredado del seguimiento previo de la cámara).
        if (lookingAtInteractable != lastLookingAtInteractable)
        {
            lastLookingAtInteractable = lookingAtInteractable;
            interactableTransitionTimer = interactableTransitionDuration;
            goalSmoothVelocity = Vector3.zero;
            lookIkSmoothVelocity = Vector3.zero;
        }
        if (interactableTransitionTimer > 0f)
            interactableTransitionTimer = Mathf.Max(0f, interactableTransitionTimer - Time.deltaTime);

        // Suavizado alto mientras se enfoca al interactuable o durante la ventana de transición.
        bool useInteractableSmoothing = lookingAtInteractable || interactableTransitionTimer > 0f;
        float targetBlend = suppressLook ? 0f : 1f;
        float fadeT = Mathf.Max(0.0001f, walkIkFadeTime);
        currentBlend = Mathf.SmoothDamp(currentBlend, targetBlend, ref blendVelocity, fadeT);

        Vector3 instantGoal;
        if (suppressLook)
            instantGoal = GetNeutralLookPoint();
        else if (aiming)
            instantGoal = GetAimingPitchOnlyLookPoint();
        else if (hasInteractable)
            instantGoal = interactPoint;
        else
            instantGoal = cameraTransform.position + cameraTransform.forward * lookDistance;

        instantGoal.y = Mathf.Max(instantGoal.y, BodyRoot.position.y + minHeightOffset);

        bool isIdle = playerController == null || playerController.PlayerStatus == PlayerStatus.Idle;

        float gT = useInteractableSmoothing
            ? interactableTransitionGoalSmoothTime
            : (isIdle ? goalSmoothTime : goalSmoothTimeActive);
        gT = Mathf.Max(0.0001f, gT);
        smoothedGoal = Vector3.SmoothDamp(smoothedGoal, instantGoal, ref goalSmoothVelocity, gT);

        Vector3 target = aiming
            ? GetClampedLookTargetPositionPitchOnly(smoothedGoal)
            : GetClampedLookTargetPosition(smoothedGoal);

        float ikT = useInteractableSmoothing
            ? interactableTransitionLookSmoothTime
            : (isIdle ? lookIkSmoothTime : lookIkSmoothTimeActive);
        ikT = Mathf.Max(0.0001f, ikT);
        currentLookPosition = Vector3.SmoothDamp(currentLookPosition, target, ref lookIkSmoothVelocity, ikT);
    }

    private Vector3 GetNeutralLookPoint()
    {
        Vector3 p = BodyRoot.position + BodyRoot.forward * lookDistance;
        p.y = Mathf.Max(p.y, BodyRoot.position.y + 1.2f);
        return p;
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

        // Si estás mirando casi completamente arriba/abajo, respeta dirección original
        if (hLenSq < 1e-8f)
            return origin + desired * lookDistance;

        desiredH.Normalize();

        float yawAngle = Vector3.Angle(fwdH, desiredH);

        Vector3 yawDir = desiredH;
        if (yawAngle > maxLookAngle)
            yawDir = Vector3.Slerp(fwdH, desiredH, maxLookAngle / yawAngle).normalized;

        // Mantener el pitch original
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
