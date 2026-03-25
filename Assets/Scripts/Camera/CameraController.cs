using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera normalCamera;
    [SerializeField] private CinemachineCamera aimCamera;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 0.2f;
    [SerializeField] private float minPitch = -40f;
    [SerializeField] private float maxPitch = 40f;

    private PlayerController playerController;
    private Vector2 currentLook;
    private float yaw;
    private float pitch;

    public float HorizontalYaw => yaw;

    public void SetLook(Vector2 input)
    {
        currentLook = input;
    }

    void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();

        Vector3 e = transform.eulerAngles;
        yaw = e.y;
        pitch = e.x;

        if (pitch > 180f) pitch -= 360f;
    }

    void Update()
    {
        // 🎮 ROTACIÓN (igual que antes)
        yaw += currentLook.x * rotationSpeed;
        pitch = Mathf.Clamp(pitch - currentLook.y * rotationSpeed, minPitch, maxPitch);

        Quaternion yawRotation = Quaternion.AngleAxis(yaw, Vector3.up);
        Quaternion pitchRotation = Quaternion.AngleAxis(pitch, Vector3.right);
        transform.rotation = yawRotation * pitchRotation;

        bool aiming = playerController != null && playerController.PlayerStatus == PlayerStatus.Aiming || playerController.PlayerStatus == PlayerStatus.CrounchAiming;

        if (aiming)
        {
            aimCamera.Priority = 20;
            normalCamera.Priority = 10;
        }
        else
        {
            aimCamera.Priority = 10;
            normalCamera.Priority = 20;
        }
    }
}