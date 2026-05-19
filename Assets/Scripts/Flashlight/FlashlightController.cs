using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FlashlightHandRig handRig;
    [SerializeField] private FlashlightLight flashlightLight;
    [SerializeField] private PlayerController playerController;

    [Header("Crystal")]
    [SerializeField] private InventoryUI inventoryUI;
    [Tooltip("Unidades de carga drenadas por segundo mientras el modo Strong está activo.")]
    [SerializeField] private float drainRatePerSecond = 10f;

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
        if (inventoryUI == null)
            inventoryUI = FindFirstObjectByType<InventoryUI>();
    }

    // Start (no Awake) para que FlashlightHandRig haya inicializado su delegate del rig weight.
    void Start()
    {
        ApplyState();
    }

    void LateUpdate()
    {
        if (hasStrongHoldFlashlight)
        {
            if (playerController == null || playerController.PlayerStatus != PlayerStatus.Aiming)
            {
                SetStrongHoldFlashlightActive(false);
                return;
            }

            if (PlayerDatabase.Instance != null)
            {
                PlayerDatabase.Instance.DrainCrystalCharge(drainRatePerSecond * Time.deltaTime);

                if (PlayerDatabase.Instance.CurrentCrystalCharge <= 0f)
                {
                    Debug.Log("[FlashlightController] Cristal agotado — modo Strong desactivado.");
                    SetStrongHoldFlashlightActive(false);
                }
            }
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
        if (isActive)
        {
            if (PlayerDatabase.Instance == null || PlayerDatabase.Instance.CurrentCrystalCharge <= 0f)
            {
                Debug.Log("[FlashlightController] Sin carga de cristal — modo Strong bloqueado.");
                return;
            }
        }

        hasStrongHoldFlashlight = isActive;
        ApplyState();
    }

    public void TryRecharge()
    {
        if (PlayerDatabase.Instance == null) return;

        ItemData equipped = PlayerDatabase.Instance.EquippedConsumable;
        if (equipped == null)
        {
            Debug.Log("[FlashlightController] No hay cristal equipado.");
            return;
        }

        if (inventoryUI == null)
        {
            Debug.LogWarning("[FlashlightController] inventoryUI no encontrado.");
            return;
        }

        if (PlayerDatabase.Instance.CurrentCrystalCharge >= PlayerDatabase.Instance.MaxCrystalCharge)
        {
            Debug.Log("[FlashlightController] El cristal ya está al máximo, no se consume ninguno.");
            return;
        }

        bool consumed = inventoryUI.ConsumeItem(equipped, 1);
        if (consumed)
        {
            PlayerDatabase.Instance.AddCrystalCharge(equipped.recoveryVal);
            Debug.Log($"[FlashlightController] Recargando con {equipped.itemName} (+{equipped.recoveryVal}) | Carga: {PlayerDatabase.Instance.CurrentCrystalCharge}/{PlayerDatabase.Instance.MaxCrystalCharge}");
        }
        else
        {
            Debug.Log($"[FlashlightController] No hay {equipped.itemName} en el inventario.");
        }
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
