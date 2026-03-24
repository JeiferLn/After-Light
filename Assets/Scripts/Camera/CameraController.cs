using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 0.2f;
    [SerializeField] private float minPitch = -40f;
    [SerializeField] private float maxPitch = 40f;

    [Header("Offsets")]
    [SerializeField] private Vector3 normalCameraOffset = new Vector3(0.3f, 2.5f, 0f);
    [SerializeField] private Vector3 aimingCameraOffset = new Vector3(0.3f, 2.5f, 1f);

    [Header("Aiming (Cinemachine)")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private Transform aimFollowAnchor;
    [SerializeField] private float aimLookFocusDistance = 12f;

    private PlayerController playerController;
    private Vector2 currentLook;
    private float yaw;
    private float pitch;
    private bool lastAiming;

    public float HorizontalYaw => yaw;

    public void SetLook(Vector2 input)
    {
        currentLook = input;
    }

    void Awake()
    {
        Vector3 e = transform.eulerAngles;
        yaw = e.y;
        pitch = e.x;

        if (pitch > 180f) pitch -= 360f;

        playerController = GetComponentInParent<PlayerController>();
        EnsureAimFollowAnchor();
    }

    void EnsureAimFollowAnchor()
    {
        if (aimFollowAnchor != null || playerController == null)
            return;

        Transform playerRoot = playerController.transform;
        Transform existing = playerRoot.Find("AimFollowAnchor");
        if (existing != null)
        {
            aimFollowAnchor = existing;
            return;
        }

        var go = new GameObject("AimFollowAnchor");
        go.transform.SetParent(playerRoot, false);
        go.transform.localPosition = aimingCameraOffset;
        go.transform.localRotation = Quaternion.identity;
        aimFollowAnchor = go.transform;
    }

    void Start()
    {
        if (playerController == null)
            return;

        bool aiming = playerController.PlayerStatus == PlayerStatus.Aiming;
        lastAiming = aiming;
        ApplyCinemachineAimSplit(aiming);
    }

    void Update()
    {
        yaw += currentLook.x * rotationSpeed;
        pitch = Mathf.Clamp(pitch - currentLook.y * rotationSpeed, minPitch, maxPitch);

        bool aiming = playerController.PlayerStatus == PlayerStatus.Aiming;

        if (aiming)
        {
            EnsureAimFollowAnchor();
            if (aimFollowAnchor != null)
                aimFollowAnchor.localPosition = aimingCameraOffset;

            Vector3 lookDir = Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.AngleAxis(pitch, Vector3.right) * Vector3.forward;
            if (aimFollowAnchor != null)
                transform.position = aimFollowAnchor.position + lookDir * aimLookFocusDistance;
            transform.rotation = Quaternion.LookRotation(lookDir);
        }
        else
        {
            transform.localPosition = normalCameraOffset;
            transform.rotation = Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.AngleAxis(pitch, Vector3.right);
        }

        if (lastAiming != aiming)
        {
            lastAiming = aiming;
            ApplyCinemachineAimSplit(aiming);
        }
    }

    void ApplyCinemachineAimSplit(bool aiming)
    {
        if (cinemachineCamera == null)
            return;

        if (aiming && aimFollowAnchor != null)
        {
            cinemachineCamera.Target.TrackingTarget = aimFollowAnchor;
            cinemachineCamera.Target.CustomLookAtTarget = true;
            cinemachineCamera.Target.LookAtTarget = transform;
        }
        else
        {
            cinemachineCamera.Target.TrackingTarget = transform;
            cinemachineCamera.Target.CustomLookAtTarget = false;
            cinemachineCamera.Target.LookAtTarget = null;
        }
    }
}
