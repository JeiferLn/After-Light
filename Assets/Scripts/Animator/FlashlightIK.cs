using System;
using System.Reflection;
using UnityEngine;

public class FlashlightIK : MonoBehaviour
{
    public Transform gripPoint;        // de la linterna
    public Transform leftHandTarget;   // del rig
    public MonoBehaviour rig;

    public bool hasFlashlight = true;

    private Action<float> setRigWeight;
    private float currentRigWeight = -1f;

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
    }

    void LateUpdate()
    {
        if (hasFlashlight && gripPoint != null)
        {
            leftHandTarget.SetPositionAndRotation(gripPoint.position, gripPoint.rotation);
        }

        float targetRigWeight = hasFlashlight ? 1f : 0f;

        if (setRigWeight != null && !Mathf.Approximately(currentRigWeight, targetRigWeight))
        {
            setRigWeight(targetRigWeight);
            currentRigWeight = targetRigWeight;
        }
    }
}