using UnityEngine;

public enum ConeOrientation
{
    HorizontalRight,
    HorizontalLeft,
    VerticalUp,
    VerticalDown,
}

public class VisionCone : MonoBehaviour
{
    private const float RotationStep = 1f;
    public static Vector2 PeekLookInput { get; set; }

    [Header("Orientación")]
    [SerializeField]
    [Tooltip("Orientación inicial del cono para determinar los límites de rotación")]
    ConeOrientation coneOrientation = ConeOrientation.HorizontalRight;

    [Header("Smoothing")]
    [SerializeField]
    [Tooltip("Velocidad de rotación en grados por segundo")]
    float rotationSpeed = 90f;

    [SerializeField]
    [Range(0.01f, 1f)]
    [Tooltip("Suavizado: 1 = sin suavizado, valores bajos = más suave")]
    float rotationSmoothing = 0.15f;

    float currentRotationSpeed;
    float rotationSpeedVelocity;

    // ------------- VISUALS -------------
    public GameObject cone;

    // ------------- EVENTS -------------
    private void OnEnable()
    {
        PlayerMovementEvents.OnPlayerMoved += Hide;
    }

    private void OnDisable()
    {
        PlayerMovementEvents.OnPlayerMoved -= Hide;
    }

    // ------------- START -------------
    private void Start()
    {
        ShowCone();
    }

    // ------------- UPDATE -------------
    private void Update()
    {
        if (IsVisible)
            SetLookDirection(PeekLookInput);
    }

    public bool IsVisible => cone != null && cone.activeSelf;

    // ------------- SHOW CONE -------------
    public void ShowCone()
    {
        if (cone == null)
            return;
        cone.SetActive(true);
    }

    // ------------- HIDE CONE -------------
    public void Hide()
    {
        if (cone != null)
            cone.SetActive(false);
    }

    // ------------- SET LOOK DIRECTION -------------
    public void SetLookDirection(Vector2 lookDirection)
    {
        if (cone == null)
            return;

        if (lookDirection.sqrMagnitude < 0.01f)
        {
            currentRotationSpeed = 0f;
            return;
        }

        float currentAngle = NormalizeAngle(cone.transform.eulerAngles.z);
        float targetDelta = GetRotationDelta(lookDirection, currentAngle, coneOrientation);

        if (Mathf.Abs(targetDelta) < 0.001f)
        {
            currentRotationSpeed = 0f;
            rotationSpeedVelocity = 0f;
            return;
        }

        float targetSpeed = Mathf.Sign(targetDelta) * rotationSpeed;
        currentRotationSpeed = Mathf.SmoothDamp(
            currentRotationSpeed,
            targetSpeed,
            ref rotationSpeedVelocity,
            rotationSmoothing
        );

        float delta = currentRotationSpeed * Time.deltaTime;
        cone.transform.rotation *= Quaternion.Euler(0, 0, delta);

        float newAngle = NormalizeAngle(cone.transform.eulerAngles.z);
        float clampedAngle = ClampAngleToLimits(newAngle, coneOrientation);
        if (Mathf.Abs(Mathf.DeltaAngle(newAngle, clampedAngle)) > 2f)
        {
            cone.transform.rotation = Quaternion.Euler(0, 0, clampedAngle);
            currentRotationSpeed = 0f;
            rotationSpeedVelocity = 0f;
        }
    }

    private static float NormalizeAngle(float angle)
    {
        while (angle > 180f)
            angle -= 360f;
        while (angle < -180f)
            angle += 360f;
        return angle;
    }

    private static float GetRotationDelta(
        Vector2 lookDirection,
        float currentAngle,
        ConeOrientation orientation
    )
    {
        float absAngle = Mathf.Abs(currentAngle);

        if (orientation == ConeOrientation.VerticalUp)
        {
            if (
                IsInRange(currentAngle, -25f, 45f)
                || IsInRange(currentAngle, 85f, 95f)
                || IsInRange(currentAngle, -95f, -85f)
            )
                return GetRotationDeltaVerticalUp(lookDirection, currentAngle, absAngle);
            if (IsInRange(currentAngle, 135f, 180f) || IsInRange(currentAngle, -180f, -135f))
                return GetRotationDeltaVerticalDown(lookDirection, currentAngle, absAngle);
        }
        if (orientation == ConeOrientation.VerticalDown)
        {
            if (
                IsInRange(currentAngle, -25f, 25f)
                || IsInRange(currentAngle, 135f, 180f)
                || IsInRange(currentAngle, -180f, -135f)
            )
                return GetRotationDeltaVerticalDown(lookDirection, currentAngle, absAngle);
        }

        if (IsInRange(currentAngle, -112f, -70f))
            return GetRotationDeltaHorizontalLeft(lookDirection, currentAngle, absAngle);
        if (IsInRange(currentAngle, 70f, 112f))
            return GetRotationDeltaHorizontalRight(lookDirection, currentAngle, absAngle);

        switch (orientation)
        {
            case ConeOrientation.HorizontalRight:
                return GetRotationDeltaHorizontalRight(lookDirection, currentAngle, absAngle);
            case ConeOrientation.HorizontalLeft:
                return GetRotationDeltaHorizontalLeft(lookDirection, currentAngle, absAngle);
            case ConeOrientation.VerticalUp:
                return GetRotationDeltaVerticalUp(lookDirection, currentAngle, absAngle);
            case ConeOrientation.VerticalDown:
                return GetRotationDeltaVerticalDown(lookDirection, currentAngle, absAngle);
        }

        return 0f;
    }

    private static float GetRotationDeltaHorizontalRight(
        Vector2 lookDirection,
        float currentAngle,
        float absAngle
    )
    {
        bool inRange = IsInRange(currentAngle, 88f, 95f);
        bool pastUpper = IsInRange(absAngle, 95.1f, 112f);
        bool pastLower = IsInRange(absAngle, 70f, 87.9f);

        if (currentAngle >= 94f && currentAngle <= 95f && lookDirection.y > 0)
            return 0f;
        if (currentAngle >= 88f && currentAngle <= 89f && lookDirection.y < 0)
            return 0f;

        if (lookDirection.y > 0 && inRange)
            return RotationStep;
        if (lookDirection.y < 0 && inRange)
            return -RotationStep;
        if (lookDirection.x > 0 && inRange)
            return RotationStep;
        if (lookDirection.x < 0 && inRange)
            return -RotationStep;

        if (lookDirection.y < 0 && pastUpper)
            return -RotationStep;
        if (lookDirection.y > 0 && pastLower)
            return RotationStep;
        if (lookDirection.x > 0 && pastUpper)
            return RotationStep;
        if (lookDirection.x < 0 && pastLower)
            return -RotationStep;

        return 0f;
    }

    private static float GetRotationDeltaHorizontalLeft(
        Vector2 lookDirection,
        float currentAngle,
        float absAngle
    )
    {
        bool inRange = IsInRange(currentAngle, -95f, -88f);
        bool pastUpper = IsInRange(currentAngle, -112f, -95.1f);
        bool pastLower = IsInRange(currentAngle, -87.9f, -70f);

        if (currentAngle >= -95f && currentAngle <= -94f && lookDirection.y > 0)
            return 0f;
        if (currentAngle >= -88.5f && currentAngle <= -87f && lookDirection.y < 0)
            return 0f;

        if (lookDirection.y > 0 && inRange)
            return -RotationStep;
        if (lookDirection.y < 0 && inRange)
            return RotationStep;
        if (lookDirection.x > 0 && inRange)
            return RotationStep;
        if (lookDirection.x < 0 && inRange)
            return -RotationStep;

        if (lookDirection.y > 0 && pastUpper)
            return -RotationStep;
        if (lookDirection.y < 0 && pastLower)
            return RotationStep;
        if (lookDirection.x > 0 && pastUpper)
            return RotationStep;
        if (lookDirection.x < 0 && pastLower)
            return -RotationStep;

        return 0f;
    }

    private static float GetRotationDeltaVerticalUp(
        Vector2 lookDirection,
        float currentAngle,
        float absAngle
    )
    {
        bool inRange0 = IsInRange(currentAngle, 0f, 20f);
        bool pastRight0 = IsInRange(currentAngle, 20f, 45f);
        bool pastLeft0 = IsInRange(currentAngle, -20f, 0f);

        bool inRange90 = IsInRange(currentAngle, 85f, 95f);
        bool pastRight90 = IsInRange(currentAngle, 95f, 115f);
        bool pastLeft90 = IsInRange(currentAngle, 65f, 85f);

        bool inRange = inRange0 || inRange90;
        bool pastRight = pastRight0 || pastRight90;
        bool pastLeft = pastLeft0 || pastLeft90;

        if (lookDirection.x > 0 && inRange)
            return RotationStep;
        if (lookDirection.x < 0 && inRange)
            return -RotationStep;

        if (lookDirection.x < 0 && pastRight)
            return -RotationStep;
        if (lookDirection.x > 0 && pastLeft)
            return RotationStep;

        return 0f;
    }

    private static float GetRotationDeltaVerticalDown(
        Vector2 lookDirection,
        float currentAngle,
        float absAngle
    )
    {
        const float minAngle0 = -20f;
        const float maxAngle0 = 20f;
        const float tolerance = 0.5f;

        if (IsInRange(currentAngle, minAngle0 - 25f, maxAngle0 + 25f))
        {
            bool inRange0 = IsInRange(currentAngle, minAngle0, maxAngle0);
            bool pastRight0 = currentAngle > maxAngle0;
            bool pastLeft0 = currentAngle < minAngle0;

            if (currentAngle >= maxAngle0 - tolerance && lookDirection.x > 0)
                return 0f;
            if (currentAngle <= minAngle0 + tolerance && lookDirection.x < 0)
                return 0f;

            if (lookDirection.x > 0 && inRange0)
                return RotationStep;
            if (lookDirection.x < 0 && inRange0)
                return -RotationStep;

            if (lookDirection.x < 0 && pastRight0)
                return -RotationStep;
            if (lookDirection.x > 0 && pastLeft0)
                return RotationStep;

            return 0f;
        }

        const float minAngle180 = 158f;

        bool inRange =
            IsInRange(currentAngle, minAngle180, 180f)
            || IsInRange(currentAngle, -180f, -minAngle180);
        bool pastRight = IsInRange(currentAngle, 140f, minAngle180);
        bool pastLeft = IsInRange(currentAngle, -minAngle180, -140f);

        if (lookDirection.x > 0 && inRange)
            return -RotationStep;
        if (lookDirection.x < 0 && inRange)
            return RotationStep;

        if (lookDirection.x < 0 && pastRight)
            return RotationStep;
        if (lookDirection.x > 0 && pastLeft)
            return -RotationStep;

        return 0f;
    }

    private static bool IsInRange(float value, float min, float max) =>
        value >= min && value <= max;

    private static float ClampAngleToLimits(float angle, ConeOrientation orientation)
    {
        if (IsInRange(angle, -112f, -70f))
            return Mathf.Clamp(angle, -95f, -88f);
        if (IsInRange(angle, 70f, 112f))
            return Mathf.Clamp(angle, 88f, 95f);
        if (IsInRange(angle, 0f, 45f) || IsInRange(angle, -45f, 0f))
        {
            if (orientation == ConeOrientation.VerticalUp)
                return Mathf.Clamp(angle, -10f, 20f);
            if (orientation == ConeOrientation.VerticalDown)
                return Mathf.Clamp(angle, -20f, 20f);
            return angle;
        }

        const float verticalMin = 158f;

        if (
            (
                orientation == ConeOrientation.VerticalUp
                || orientation == ConeOrientation.VerticalDown
            ) && (IsInRange(angle, 90f, 180f) || IsInRange(angle, -180f, -90f))
        )
        {
            if (angle >= verticalMin && angle <= 180f)
                return Mathf.Clamp(angle, verticalMin, 180f);
            if (angle >= -180f && angle <= -verticalMin)
                return Mathf.Clamp(angle, -180f, -verticalMin);
            if (angle > 0f)
                return verticalMin;
            return -verticalMin;
        }

        return angle;
    }
}
