using UnityEngine;

public static class PlayerCameraPlanar
{
    public const float DefaultMinSqrMagnitude = 0.01f;

    public static Vector3 GetPlanarForward(CameraController cameraController, Transform transform, float minSqrMagnitude = DefaultMinSqrMagnitude)
    {
        if (cameraController != null)
        {
            Quaternion yawOnly = Quaternion.AngleAxis(cameraController.HorizontalYaw, Vector3.up);
            Vector3 f = yawOnly * Vector3.forward;
            f.y = 0f;
            return f.sqrMagnitude > minSqrMagnitude ? f.normalized : Vector3.forward;
        }

        Transform rig = transform.childCount > 0 ? transform.GetChild(0) : transform;
        Vector3 forward = rig.forward;
        forward.y = 0f;
        return forward.sqrMagnitude > minSqrMagnitude ? forward.normalized : Vector3.forward;
    }

    public static void GetAxes(CameraController cameraController, Transform transform, float minSqrMagnitude, out Vector3 forward, out Vector3 right)
    {
        forward = GetPlanarForward(cameraController, transform, minSqrMagnitude);
        right = Vector3.Cross(Vector3.up, forward).normalized;
    }

    public static Vector3 ComputePlanarMovement(Vector2 input, CameraController cameraController, Transform transform, float minSqrMagnitude)
    {
        GetAxes(cameraController, transform, minSqrMagnitude, out Vector3 forward, out Vector3 right);
        Vector3 movement = forward * input.y + right * input.x;
        return Vector3.ClampMagnitude(movement, 1f);
    }
}
