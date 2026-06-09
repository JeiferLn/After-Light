using UnityEngine;

public class FlashlightLight : MonoBehaviour
{
    public enum Mode
    {
        Off,
        Normal,
        Strong,
    }

    private struct LightPreset
    {
        public float intensity;
        public float innerSpotAngle;
        public float outerSpotAngle;
    }

    private class ManagedLight
    {
        public Light light;
        public LightPreset normal;
        public LightPreset strong;
        public LightPreset target;

        public void SetTarget(Mode mode)
        {
            switch (mode)
            {
                case Mode.Off:
                    target = new LightPreset
                    {
                        intensity = 0f,
                        innerSpotAngle = normal.innerSpotAngle,
                        outerSpotAngle = normal.outerSpotAngle,
                    };
                    break;
                case Mode.Normal:
                    target = normal;
                    break;
                case Mode.Strong:
                    target = strong;
                    break;
            }
        }

        public void LerpTowardsTarget(float t)
        {
            if (light == null)
                return;

            light.intensity = Mathf.Lerp(light.intensity, target.intensity, t);
            light.innerSpotAngle = Mathf.Lerp(light.innerSpotAngle, target.innerSpotAngle, t);
            light.spotAngle = Mathf.Lerp(light.spotAngle, target.outerSpotAngle, t);
        }

        public void SnapToTarget()
        {
            if (light == null)
                return;

            light.intensity = target.intensity;
            light.innerSpotAngle = target.innerSpotAngle;
            light.spotAngle = target.outerSpotAngle;
        }
    }

    [Header("References inner light")]
    [SerializeField] private Light flashlightLight;

    [Header("Intensities & Angles")]
    [SerializeField] private float normalIntensity = 15f;
    [SerializeField] private float strongIntensity = 25f;
    [SerializeField] private float normalInnerSpotAngle = 15f;
    [SerializeField] private float strongInnerSpotAngle = 20f;
    [SerializeField] private float normalOuterSpotAngle = 35f;
    [SerializeField] private float strongOuterSpotAngle = 45f;

    [Header("References outer light")]
    [SerializeField] private Light outerLight;

    [Header("Intensities & Angles")]
    [SerializeField] private float normalOuterLightIntensity = 40f;
    [SerializeField] private float strongOuterLightIntensity = 500f;
    [SerializeField] private float normalOuterInnerSpotAngle = 70f;
    [SerializeField] private float strongOuterInnerSpotAngle = 20f;
    [SerializeField] private float normalOuterOuterSpotAngle = 135f;
    [SerializeField] private float strongOuterOuterSpotAngle = 40f;

    [Header("Smoothing")]
    [SerializeField, Range(1f, 25f)]
    private float lightLerpSpeed = 10f;

    private Mode currentMode = Mode.Off;
    private ManagedLight[] lights;

    void Awake()
    {
        lights = new[]
        {
            CreateManagedLight(
                flashlightLight,
                normalIntensity, strongIntensity,
                normalInnerSpotAngle, strongInnerSpotAngle,
                normalOuterSpotAngle, strongOuterSpotAngle),
            CreateManagedLight(
                outerLight,
                normalOuterLightIntensity, strongOuterLightIntensity,
                normalOuterInnerSpotAngle, strongOuterInnerSpotAngle,
                normalOuterOuterSpotAngle, strongOuterOuterSpotAngle),
        };

        ApplyMode(currentMode, true);
    }

    void LateUpdate()
    {
        float t = Mathf.Clamp01(lightLerpSpeed * Time.deltaTime);

        foreach (var managedLight in lights)
            managedLight.LerpTowardsTarget(t);
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

        foreach (var managedLight in lights)
            managedLight.SetTarget(mode);

        if (snap)
        {
            foreach (var managedLight in lights)
                managedLight.SnapToTarget();
        }
    }

    private static ManagedLight CreateManagedLight(
        Light light,
        float normalIntensity, float strongIntensity,
        float normalInnerSpotAngle, float strongInnerSpotAngle,
        float normalOuterSpotAngle, float strongOuterSpotAngle)
    {
        return new ManagedLight
        {
            light = light,
            normal = new LightPreset
            {
                intensity = normalIntensity,
                innerSpotAngle = normalInnerSpotAngle,
                outerSpotAngle = normalOuterSpotAngle,
            },
            strong = new LightPreset
            {
                intensity = strongIntensity,
                innerSpotAngle = strongInnerSpotAngle,
                outerSpotAngle = strongOuterSpotAngle,
            },
        };
    }
}
