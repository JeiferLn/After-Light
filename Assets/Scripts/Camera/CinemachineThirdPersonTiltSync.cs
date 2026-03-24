using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// While aiming: follow target stays yaw-only and vertical look is applied via Cinemachine Pan Tilt.
/// While not aiming, Pan Tilt is disabled so the camera uses the target's full yaw+pitch (previous behavior).
/// </summary>
[DefaultExecutionOrder(-100)]
[DisallowMultipleComponent]
[RequireComponent(typeof(CinemachineCamera))]
public class CinemachineThirdPersonTiltSync : MonoBehaviour
{
    [SerializeField] private CameraController cameraController;
    private CinemachinePanTilt panTilt;

    void Awake()
    {
        if (cameraController == null)
            cameraController = FindFirstObjectByType<CameraController>();

        if (panTilt == null)
            panTilt = transform.GetComponent<CinemachinePanTilt>();
        if (panTilt == null)
            panTilt = gameObject.AddComponent<CinemachinePanTilt>();

        panTilt.ReferenceFrame = CinemachinePanTilt.ReferenceFrames.TrackingTarget;
        panTilt.PanAxis.Value = 0f;
        panTilt.TiltAxis.Range = new Vector2(
            cameraController != null ? cameraController.MinPitchDegrees : -70f,
            cameraController != null ? cameraController.MaxPitchDegrees : 70f);
    }

    void LateUpdate()
    {
        // Runs before default CinemachineBrain so Pan Tilt sees this frame's pitch when aiming.
        if (panTilt == null || cameraController == null)
            return;

        bool aimRig = cameraController.UsesAimPitchViaPanTilt;
        panTilt.enabled = aimRig;
        if (!aimRig)
            return;

        panTilt.PanAxis.Value = 0f;
        panTilt.TiltAxis.Value = cameraController.PitchDegrees;
    }
}
