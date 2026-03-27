using UnityEngine;

public sealed class PlayerRotationController
{
    private readonly Transform transform;
    private readonly CameraController cameraController;
    private readonly float minMoveSqrMagnitude;

    public PlayerRotationController(Transform transform, CameraController cameraController, float minMoveSqrMagnitude)
    {
        this.transform = transform;
        this.cameraController = cameraController;
        this.minMoveSqrMagnitude = minMoveSqrMagnitude;
    }

    public void HandleRotation(PlayerStatus playerStatus, Vector3 movement, ref PlayerStatus mutableStatus, float rotationSharpness)
    {
        if (PlayerStatusHelpers.IsAimingStatus(playerStatus))
        {
            HandleAimingRotation(rotationSharpness);
            return;
        }

        HandleDefaultRotation(movement, ref mutableStatus, rotationSharpness);
    }

    private void HandleAimingRotation(float rotationSharpness)
    {
        Vector3 aimDirection = PlayerCameraPlanar.GetPlanarForward(cameraController, transform, minMoveSqrMagnitude);
        aimDirection.y = 0f;

        if (aimDirection.sqrMagnitude < minMoveSqrMagnitude)
            return;

        RotateTowards(aimDirection, rotationSharpness);
    }

    private void HandleDefaultRotation(Vector3 movement, ref PlayerStatus mutableStatus, float rotationSharpness)
    {
        if (movement.sqrMagnitude > minMoveSqrMagnitude)
        {
            Vector3 faceDirection = PlayerCameraPlanar.GetPlanarForward(cameraController, transform, minMoveSqrMagnitude);
            faceDirection.y = 0f;

            if (faceDirection.sqrMagnitude < minMoveSqrMagnitude)
                return;

            RotateTowards(faceDirection, rotationSharpness);
        }
        else if (mutableStatus == PlayerStatus.Walking || mutableStatus == PlayerStatus.Running)
        {
            mutableStatus = PlayerStatus.Idle;
        }
    }

    private void RotateTowards(Vector3 direction, float rotationSharpness)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            1 - Mathf.Exp(-rotationSharpness * Time.deltaTime)
        );

        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
    }

}
