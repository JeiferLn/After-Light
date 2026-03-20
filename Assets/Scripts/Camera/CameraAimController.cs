using UnityEngine;

public class CameraAimController : MonoBehaviour
{
    [SerializeField] private float sensitivity = 100f;
    [SerializeField] private float minVerticalAngle = -40f;
    [SerializeField] private float maxVerticalAngle = 30f;

    private Vector3 normalPos;

    private Vector2 lookInput;

    private float verticalRotation = 0f;
    private float horizontalRotation = 0f;

    void Awake()
    {
        normalPos = transform.localPosition;

        horizontalRotation = transform.eulerAngles.y;
        verticalRotation = transform.eulerAngles.x;
    }

    public void SetLookInput(Vector2 look)
    {
        lookInput = look;
    }

    void Update()
    {
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            normalPos,
            Time.deltaTime * 15f
        );

        if (lookInput.sqrMagnitude < 0.01f) return;

        float mouseX = lookInput.x * sensitivity * Time.deltaTime;
        float mouseY = lookInput.y * sensitivity * Time.deltaTime;

        horizontalRotation += mouseX;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);

        transform.rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
    }
}