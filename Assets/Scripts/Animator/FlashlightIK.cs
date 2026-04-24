using System;
using System.Reflection;
using UnityEngine;

public class FlashlightIK : MonoBehaviour
{
    [SerializeField] private Transform gripPoint;
    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private MonoBehaviour rig;
    [SerializeField] private GameObject flashlightModel;

    [Header("Elbow Hint")]
    [SerializeField] private Transform elbowHint;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Vector3 elbowHintDefaultPos;
    [SerializeField] private Vector3 elbowHintRightPos;
    [SerializeField][Range(0f, 90f)] private float rightAngleThreshold = 10f;
    [SerializeField][Range(0.01f, 0.3f)] private float elbowSmoothTime = 0.1f;

    public bool hasFlashlight = true;

    private Action<float> setRigWeight;
    private float currentRigWeight = -1f;
    private Vector3 elbowSmoothVelocity;

    void Awake()
    {
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
        if (hasFlashlight && gripPoint != null)
        {
            leftHandTarget.SetPositionAndRotation(gripPoint.position, gripPoint.rotation);
        }

        if (elbowHint != null && cameraTransform != null)
        {
            Vector3 bodyForward = transform.forward;
            bodyForward.y = 0f;
            bodyForward.Normalize();

            Vector3 camForward = cameraTransform.forward;
            camForward.y = 0f;
            camForward.Normalize();

            float angle = Vector3.SignedAngle(bodyForward, camForward, Vector3.up);

            Vector3 targetLocal = angle > rightAngleThreshold ? elbowHintRightPos : elbowHintDefaultPos;

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
    }

    public void ToggleFlashlight()
    {
        SetFlashlightActive(!hasFlashlight);
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
    }
}