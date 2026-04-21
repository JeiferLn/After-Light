using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Serializable]
    public struct ThirdPersonRigSettings
    {
        public Vector3 damping;
        public Vector3 shoulderOffset;
        public float verticalArmLength;
        [Range(0f, 1f)] public float cameraSide;
        public float cameraDistance;
        [Tooltip("Vertical field of view")]
        public float fieldOfView;
    }

    [Header("References")]
    [Tooltip("GameObject con CinemachineCamera (p. ej. PlayerCamera_Normal).")]
    [SerializeField] private GameObject cinemachineCameraObject;

    private CinemachineCamera cinemachineCamera;
    private CinemachineThirdPersonFollow thirdPersonFollow;
    private CinemachineThirdPersonAim thirdPersonAim;

    [Header("Rig profiles")]
    [SerializeField]
    private ThirdPersonRigSettings explorationRig = new()
    {
        damping = new Vector3(0.1f, 1f, 0.3f),
        shoulderOffset = new Vector3(0.7f, 0f, 0f),
        verticalArmLength = 0.2f,
        cameraSide = 1f,
        cameraDistance = 2f,
        fieldOfView = 65f,
    };

    [SerializeField]
    private ThirdPersonRigSettings aimRig = new()
    {
        damping = Vector3.zero,
        shoulderOffset = new Vector3(0.7f, 0f, 0f),
        verticalArmLength = 0.2f,
        cameraSide = 1f,
        cameraDistance = 0.9f,
        fieldOfView = 65f,
    };

    [Header("Blend")]
    [SerializeField] private float rigBlendSmoothTime = 0.22f;
    [Tooltip("Third Person Aim activates above this blend (reduces pops mid-transition).")]
    [SerializeField][Range(0f, 1f)] private float aimModuleEnableThreshold = 0.65f;

    [Header("Rotation (CameraTarget)")]
    [SerializeField] private float rotationSpeed = 0.2f;
    [SerializeField] private float minPitch = -40f;
    [SerializeField] private float maxPitch = 40f;

    private PlayerController playerController;
    private Vector2 currentLook;
    private float yaw;
    private float pitch;
    private float rigBlend;
    private float rigBlendVelocity;

    public float HorizontalYaw => yaw;

    public void SetLook(Vector2 input)
    {
        currentLook = input;
    }

    void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();

        if (cinemachineCameraObject != null)
        {
            cinemachineCamera = cinemachineCameraObject.GetComponent<CinemachineCamera>();
            thirdPersonFollow = cinemachineCameraObject.GetComponent<CinemachineThirdPersonFollow>();
            thirdPersonAim = cinemachineCameraObject.GetComponent<CinemachineThirdPersonAim>();
        }

        Vector3 e = transform.eulerAngles;
        yaw = e.y;
        pitch = e.x;

        if (pitch > 180f) pitch -= 360f;

        rigBlend = ComputeWantingAimBlendTarget();
        ApplyRigSettings(Mathf.Clamp01(rigBlend));
        if (thirdPersonAim != null)
            thirdPersonAim.enabled = rigBlend >= aimModuleEnableThreshold;
    }

    void LateUpdate()
    {
        yaw += currentLook.x * rotationSpeed;
        pitch = Mathf.Clamp(pitch - currentLook.y * rotationSpeed, minPitch, maxPitch);

        Quaternion yawRotation = Quaternion.AngleAxis(yaw, Vector3.up);
        Quaternion pitchRotation = Quaternion.AngleAxis(pitch, Vector3.right);
        transform.rotation = yawRotation * pitchRotation;

        float targetBlend = ComputeWantingAimBlendTarget();
        float smooth = Mathf.Max(0.0001f, rigBlendSmoothTime);
        rigBlend = Mathf.SmoothDamp(rigBlend, targetBlend, ref rigBlendVelocity, smooth);
        rigBlend = Mathf.Clamp01(rigBlend);

        ApplyRigSettings(rigBlend);

        if (thirdPersonAim != null)
            thirdPersonAim.enabled = rigBlend >= aimModuleEnableThreshold;
    }

    float ComputeWantingAimBlendTarget()
    {
        if (playerController == null)
            return 0f;

        PlayerStatus s = playerController.PlayerStatus;
        return (s == PlayerStatus.Aiming || s == PlayerStatus.CrounchAiming) ? 1f : 0f;
    }

    void ApplyRigSettings(float t)
    {
        if (thirdPersonFollow != null)
        {
            thirdPersonFollow.Damping = Vector3.Lerp(explorationRig.damping, aimRig.damping, t);
            thirdPersonFollow.ShoulderOffset = Vector3.Lerp(explorationRig.shoulderOffset, aimRig.shoulderOffset, t);
            thirdPersonFollow.VerticalArmLength = Mathf.Lerp(explorationRig.verticalArmLength, aimRig.verticalArmLength, t);
            thirdPersonFollow.CameraSide = Mathf.Lerp(explorationRig.cameraSide, aimRig.cameraSide, t);
            thirdPersonFollow.CameraDistance = Mathf.Lerp(explorationRig.cameraDistance, aimRig.cameraDistance, t);
        }

        if (cinemachineCamera != null)
        {
            LensSettings lens = cinemachineCamera.Lens;
            lens.FieldOfView = Mathf.Lerp(explorationRig.fieldOfView, aimRig.fieldOfView, t);
            cinemachineCamera.Lens = lens;
        }
    }
}
