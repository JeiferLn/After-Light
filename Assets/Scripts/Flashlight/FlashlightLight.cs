using UnityEngine;

public class FlashlightLight : MonoBehaviour
{
    public enum Mode
    {
        Off,
        Normal,
        Strong,
    }

    [Header("References")]
    [SerializeField] private Light flashlightLight;

    [Header("Intensities & Angles")]
    [SerializeField] private float normalIntensity = 15f;
    [SerializeField] private float strongIntensity = 25f;

    [SerializeField] private float normalInnerSpotAngle = 15f;
    [SerializeField] private float strongInnerSpotAngle = 20f;

    [SerializeField] private float normalOuterSpotAngle = 35f;
    [SerializeField] private float strongOuterSpotAngle = 45f;

    [Header("Smoothing")]
    [SerializeField, Range(1f, 25f)]
    private float lightLerpSpeed = 10f;

    private Mode currentMode = Mode.Off;

    private float targetIntensity;
    private float targetInnerSpotAngle;
    private float targetOuterSpotAngle;

    void Awake()
    {
        ApplyMode(currentMode, true);
    }

    void LateUpdate()
    {
        if (flashlightLight == null)
            return;

        float t = Mathf.Clamp01(lightLerpSpeed * Time.deltaTime);

        flashlightLight.intensity =
            Mathf.Lerp(flashlightLight.intensity, targetIntensity, t);

        flashlightLight.innerSpotAngle =
            Mathf.Lerp(flashlightLight.innerSpotAngle, targetInnerSpotAngle, t);

        flashlightLight.spotAngle =
            Mathf.Lerp(flashlightLight.spotAngle, targetOuterSpotAngle, t);
    }

    public void SetMode(Mode mode)
    {
        if (currentMode == mode)
            return;

        ApplyMode(mode, false);
    }

    private void ApplyMode(Mode mode, bool snap)
    {
        currentMode = mode;

        switch (mode)
        {
            case Mode.Off:
                targetIntensity = 0f;
                targetInnerSpotAngle = normalInnerSpotAngle;
                targetOuterSpotAngle = normalOuterSpotAngle;
                break;

            case Mode.Normal:
                targetIntensity = normalIntensity;
                targetInnerSpotAngle = normalInnerSpotAngle;
                targetOuterSpotAngle = normalOuterSpotAngle;
                break;

            case Mode.Strong:
                targetIntensity = strongIntensity;
                targetInnerSpotAngle = strongInnerSpotAngle;
                targetOuterSpotAngle = strongOuterSpotAngle;
                break;
        }

        if (snap && flashlightLight != null)
        {
            flashlightLight.intensity = targetIntensity;
            flashlightLight.innerSpotAngle = targetInnerSpotAngle;
            flashlightLight.spotAngle = targetOuterSpotAngle;
        }
    }
}