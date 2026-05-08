using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FlashlightHandRig handRig;
    [SerializeField] private FlashlightLight flashlightLight;
    [SerializeField] private PlayerController playerController;

    [Header("Initial State")]
    [SerializeField] private bool hasFlashlight = true;
    [SerializeField] private bool hasStrongHoldFlashlight = false;

    public bool HasFlashlight => hasFlashlight;
    public bool HasStrongHoldFlashlight => hasStrongHoldFlashlight;

    void Awake()
    {
        if (playerController == null)
            playerController = GetComponentInParent<PlayerController>();
        if (handRig == null)
            handRig = GetComponent<FlashlightHandRig>();
        if (flashlightLight == null)
            flashlightLight = GetComponent<FlashlightLight>();
    }

    // Start (no Awake) para que FlashlightHandRig haya inicializado su delegate del rig weight.
    void Start()
    {
        ApplyState();
    }

    void LateUpdate()
    {
        if (hasStrongHoldFlashlight &&
            (playerController == null || playerController.PlayerStatus != PlayerStatus.Aiming))
        {
            SetStrongHoldFlashlightActive(false);
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

    public void SetFlashlightActive(bool isActive)
    {
        hasFlashlight = isActive;
        if (!isActive)
            hasStrongHoldFlashlight = false;
        ApplyState();
    }

    public void SetStrongHoldFlashlightActive(bool isActive)
    {
        hasStrongHoldFlashlight = isActive;
        ApplyState();
    }

    private void ApplyState()
    {
        if (handRig != null)
            handRig.SetActive(hasFlashlight);

        if (flashlightLight != null)
        {
            FlashlightLight.Mode mode;
            if (!hasFlashlight)
                mode = FlashlightLight.Mode.Off;
            else if (hasStrongHoldFlashlight)
                mode = FlashlightLight.Mode.Strong;
            else
                mode = FlashlightLight.Mode.Normal;

            flashlightLight.SetMode(mode);
        }
    }
}
