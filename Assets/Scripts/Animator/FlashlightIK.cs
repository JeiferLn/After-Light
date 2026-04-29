using System;
using System.Reflection;
using UnityEngine;

public class FlashlightIK : MonoBehaviour
{
    private PlayerController playerController;
    [SerializeField] private Light flashlightLight;
    
    [SerializeField] private Transform gripPoint;
    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private MonoBehaviour rig;
    [SerializeField] private GameObject flashlightModel;
    [Header("Flashlight Light")]
    [SerializeField] private float normalIntensity = 1f;
    [SerializeField] private float strongIntensity = 2f;
    [SerializeField] private float normalInnerSpotAngle = 15f;
    [SerializeField] private float strongInnerSpotAngle = 20f;
    [SerializeField] private float normalOuterSpotAngle = 60f;
    [SerializeField] private float strongOuterSpotAngle = 70f;
    [SerializeField][Range(1f, 25f)] private float lightLerpSpeed = 10f;

    [Header("Elbow Hint")]
    [SerializeField] private Transform elbowHint;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Vector3 elbowHintDefaultPos;
    [SerializeField] private Vector3 elbowHintRightPos;
    [Tooltip("Ángulo (derecha) a partir del cual empieza la mezcla hacia la pose 'right'.")]
    [SerializeField][Range(0f, 90f)] private float rightBlendStartAngle = 5f;
    [Tooltip("Ángulo (derecha) en el que la pose 'right' ya está al 100%.")]
    [SerializeField][Range(0f, 180f)] private float rightBlendMaxAngle = 45f;
    [SerializeField][Range(0.01f, 0.3f)] private float elbowSmoothTime = 0.1f;

    public bool hasFlashlight = true;
    public bool hasStrongHoldFlashlight = false;

    private Action<float> setRigWeight;
    private float currentRigWeight = -1f;
    private Vector3 elbowSmoothVelocity;
    private float targetIntensity;
    private float targetInnerSpotAngle;
    private float targetOuterSpotAngle;

    void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();

        if (rig != null)
        {
            var rigWeightProperty = rig.GetType().GetProperty("weight", BindingFlags.Public | BindingFlags.Instance);
            var setMethod = rigWeightProperty?.SetMethod;

            if (setMethod != null)
            {
                setRigWeight = (Action<float>)Delegate.CreateDelegate(typeof(Action<float>), rig, setMethod);
            }
        }

        ApplyFlashlightState(hasFlashlight);
    }

    void LateUpdate()
    {
        if (hasStrongHoldFlashlight &&
            (playerController == null || playerController.PlayerStatus != PlayerStatus.Aiming))
        {
            SetStrongHoldFlashlightActive(false);
        }

        if (hasFlashlight && gripPoint != null)
        {
            leftHandTarget.SetPositionAndRotation(gripPoint.position, gripPoint.rotation);
        }

        if (elbowHint != null && cameraTransform != null)
        {
            Vector3 bodyForward = transform.forward;
            bodyForward.y = 0f;
            if (bodyForward.sqrMagnitude < 1e-6f)
                bodyForward = Vector3.forward;
            else
                bodyForward.Normalize();

            Vector3 camForward = cameraTransform.forward;
            camForward.y = 0f;
            if (camForward.sqrMagnitude < 1e-6f)
                camForward = bodyForward;
            else
                camForward.Normalize();

            float angle = Vector3.SignedAngle(bodyForward, camForward, Vector3.up);
            float rightAngle = Mathf.Max(0f, angle);
            float maxAngle = Mathf.Max(rightBlendStartAngle + 0.01f, rightBlendMaxAngle);
            float blend = Mathf.InverseLerp(rightBlendStartAngle, maxAngle, rightAngle);

            Vector3 targetLocal = Vector3.Lerp(elbowHintDefaultPos, elbowHintRightPos, blend);

            elbowHint.localPosition = Vector3.SmoothDamp(
                elbowHint.localPosition,
                targetLocal,
                ref elbowSmoothVelocity,
                Mathf.Max(0.0001f, elbowSmoothTime));
        }

        float targetRigWeight = hasFlashlight ? 1f : 0f;

        if (setRigWeight != null && !Mathf.Approximately(currentRigWeight, targetRigWeight))
        {
            setRigWeight(targetRigWeight);
            currentRigWeight = targetRigWeight;
        }

        if (flashlightLight != null)
        {
            float t = Mathf.Clamp01(lightLerpSpeed * Time.deltaTime);
            flashlightLight.intensity = Mathf.Lerp(flashlightLight.intensity, targetIntensity, t);
            flashlightLight.innerSpotAngle = Mathf.Lerp(flashlightLight.innerSpotAngle, targetInnerSpotAngle, t);
            flashlightLight.spotAngle = Mathf.Lerp(flashlightLight.spotAngle, targetOuterSpotAngle, t);
        }
    }

    public void ToggleFlashlight()
    {
        if (playerController != null && playerController.PlayerStatus == PlayerStatus.Aiming && hasFlashlight)
        {
            SetStrongHoldFlashlightActive(!hasStrongHoldFlashlight);
            return;
        }

        SetFlashlightActive(!hasFlashlight);
    }

    public void SetStrongHoldFlashlightActive(bool isActive)
    {
        hasStrongHoldFlashlight = isActive;
        ApplyStrongHoldFlashlightState();
    }

    public void SetFlashlightActive(bool isActive)
    {
        hasFlashlight = isActive;
        ApplyFlashlightState(isActive);
    }

    private void ApplyFlashlightState(bool isActive)
    {
        if (flashlightModel != null && flashlightModel.activeSelf != isActive)
            flashlightModel.SetActive(isActive);

        float targetRigWeight = isActive ? 1f : 0f;
        if (setRigWeight != null)
        {
            setRigWeight(targetRigWeight);
            currentRigWeight = targetRigWeight;
        }
        else
        {
            currentRigWeight = targetRigWeight;
        }

        if (!isActive)
            hasStrongHoldFlashlight = false;

        ApplyStrongHoldFlashlightState();
    }

    private void ApplyStrongHoldFlashlightState(){
        if (flashlightLight == null) return;

        bool strongModeActive = hasFlashlight && hasStrongHoldFlashlight;
        targetIntensity = strongModeActive ? strongIntensity : normalIntensity;
        targetInnerSpotAngle = strongModeActive ? strongInnerSpotAngle : normalInnerSpotAngle;
        targetOuterSpotAngle = strongModeActive ? strongOuterSpotAngle : normalOuterSpotAngle;
    }
}