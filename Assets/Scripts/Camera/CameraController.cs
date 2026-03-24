using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 0.2f;
    [SerializeField] private float minPitch = -40f;
    [SerializeField] private float maxPitch = 40f;
    [SerializeField] private Vector3 normalCameraOffsetZ = new Vector3(0.3f, 2.5f, 0f);
    [SerializeField] private Vector3 aimingCameraOffsetZ = new Vector3(0.3f, 2.5f, 1f);

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
        Vector3 e = transform.eulerAngles;
        yaw = e.y;
        pitch = e.x;
        if (pitch > 180f) pitch -= 360f;

        playerController = GetComponentInParent<PlayerController>();
    }

    void Update()
    {
        transform.localPosition = playerController.PlayerStatus == PlayerStatus.Aiming ? aimingCameraOffsetZ : normalCameraOffsetZ;

        yaw += currentLook.x * rotationSpeed;
        pitch = Mathf.Clamp(pitch - currentLook.y * rotationSpeed, minPitch, maxPitch);

        transform.rotation = Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.AngleAxis(pitch, Vector3.right);
    }
}
