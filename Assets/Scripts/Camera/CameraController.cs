using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 0.2f;
    [SerializeField] private float minPitch = -40f;
    [SerializeField] private float maxPitch = 40f;

    [Header("Offset")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0.3f, 2.5f, 0f);

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
    }

    void Update()
    {
        yaw += currentLook.x * rotationSpeed;
        pitch = Mathf.Clamp(pitch - currentLook.y * rotationSpeed, minPitch, maxPitch);

        transform.localPosition = cameraOffset;

        Quaternion yawRotation = Quaternion.AngleAxis(yaw, Vector3.up);
        Quaternion pitchRotation = Quaternion.AngleAxis(pitch, Vector3.right);
        transform.rotation = yawRotation * pitchRotation;
    }
}
