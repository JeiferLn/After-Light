using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InventoryItemDescriptionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemAmountText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private RectTransform item3dViewRoot;
    [SerializeField] private Transform modelPivot;

    [Header("Model")]
    [SerializeField] private Vector3 modelLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 modelLocalEuler = Vector3.zero;
    [SerializeField] private Vector3 modelLocalScale = Vector3.one;

    [Header("Rotation")]
    [SerializeField] private float yawSpeed = 120f;
    [SerializeField] private float pitchSpeed = 85f;
    [SerializeField] private float stickDeadzone = 0.2f;

    private GameObject currentModel;
    private Transform runtimeModelAnchor;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInstallOnInventoryScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid())
            return;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform target = root.transform.Find("Canvas/Panel/BGPaper/Layout/InventoryZone/InventoryItemDescription");
            if (target == null)
                continue;

            if (target.GetComponent<InventoryItemDescriptionUI>() == null)
                target.gameObject.AddComponent<InventoryItemDescriptionUI>();
        }
    }

    private void Awake()
    {
        AutoAssignReferences();
        EnsureRuntimeAnchor();
        SetContentVisible(false);
    }

    private void OnEnable()
    {
        if (inventoryUI == null)
            inventoryUI = FindFirstObjectByType<InventoryUI>();

        if (inventoryUI != null)
        {
            inventoryUI.OnSelectionChanged += HandleSelectionChanged;
            HandleSelectionChanged(inventoryUI.SelectedIndex, inventoryUI.GetSelectedSlot());
        }
        else
        {
            SetContentVisible(false);
        }
    }

    private void OnDisable()
    {
        if (inventoryUI != null)
            inventoryUI.OnSelectionChanged -= HandleSelectionChanged;
    }

    private void Update()
    {
        if (currentModel == null || !gameObject.activeInHierarchy)
            return;

        Vector2 look = ReadLookInput();
        if (look == Vector2.zero)
            return;

        float dt = Time.unscaledDeltaTime;
        runtimeModelAnchor.Rotate(Vector3.up, look.x * yawSpeed * dt, Space.World);
        runtimeModelAnchor.Rotate(Vector3.right, -look.y * pitchSpeed * dt, Space.Self);
    }

    private void HandleSelectionChanged(int index, SlotData slot)
    {
        if (slot == null || !slot.HasItem)
        {
            ClearAndHide();
            return;
        }

        ItemData item = slot.item;
        itemNameText.text = item.itemName;
        itemAmountText.text = slot.amount.ToString();
        itemDescriptionText.text = item.itemDescription;
        SetContentVisible(true);
        SpawnModel(item.itemModel3d);
    }

    private void SpawnModel(GameObject modelPrefab)
    {
        if (currentModel != null)
            Destroy(currentModel);

        if (modelPrefab == null)
            return;

        currentModel = Instantiate(modelPrefab, runtimeModelAnchor);
        currentModel.transform.localPosition = modelLocalPosition;
        currentModel.transform.localRotation = Quaternion.Euler(modelLocalEuler);
        currentModel.transform.localScale = modelLocalScale;
        runtimeModelAnchor.localRotation = Quaternion.identity;
    }

    private void ClearAndHide()
    {
        itemNameText.text = string.Empty;
        itemAmountText.text = string.Empty;
        itemDescriptionText.text = string.Empty;

        if (currentModel != null)
            Destroy(currentModel);

        SetContentVisible(false);
    }

    private void SetContentVisible(bool visible)
    {
        if (itemNameText != null && itemNameText.transform.parent != null)
            itemNameText.transform.parent.gameObject.SetActive(visible);
        if (itemAmountText != null && itemAmountText.transform.parent != null)
            itemAmountText.transform.parent.gameObject.SetActive(visible);
        if (itemDescriptionText != null && itemDescriptionText.transform.parent != null)
            itemDescriptionText.transform.parent.gameObject.SetActive(visible);
        if (item3dViewRoot != null)
            item3dViewRoot.gameObject.SetActive(visible);
    }

    private Vector2 ReadLookInput()
    {
        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.rightStick.ReadValue();
            if (stick.sqrMagnitude >= stickDeadzone * stickDeadzone)
                return stick;
        }

        if (Keyboard.current == null)
            return Vector2.zero;

        float x = (Keyboard.current.rightArrowKey.isPressed ? 1f : 0f) - (Keyboard.current.leftArrowKey.isPressed ? 1f : 0f);
        float y = (Keyboard.current.upArrowKey.isPressed ? 1f : 0f) - (Keyboard.current.downArrowKey.isPressed ? 1f : 0f);
        return new Vector2(x, y);
    }

    private void AutoAssignReferences()
    {
        if (inventoryUI == null)
            inventoryUI = FindFirstObjectByType<InventoryUI>();

        if (itemNameText == null)
            itemNameText = FindTextInside("ItemName");
        if (itemAmountText == null)
            itemAmountText = FindTextInside("ItemAmmount");
        if (itemDescriptionText == null)
            itemDescriptionText = FindTextInside("ItemDescription");
        if (item3dViewRoot == null)
        {
            Transform view = transform.Find("Item3DView");
            if (view != null)
                item3dViewRoot = view as RectTransform;
        }
        if (modelPivot == null && item3dViewRoot != null)
            modelPivot = item3dViewRoot;
    }

    private TextMeshProUGUI FindTextInside(string parentName)
    {
        Transform parent = transform.Find(parentName);
        if (parent == null)
            return null;

        return parent.GetComponentInChildren<TextMeshProUGUI>(true);
    }

    private void EnsureRuntimeAnchor()
    {
        if (modelPivot == null)
            return;

        Transform existing = modelPivot.Find("RuntimeModelAnchor");
        if (existing != null)
        {
            runtimeModelAnchor = existing;
            return;
        }

        GameObject anchor = new("RuntimeModelAnchor");
        runtimeModelAnchor = anchor.transform;
        runtimeModelAnchor.SetParent(modelPivot, false);
        runtimeModelAnchor.localPosition = Vector3.zero;
        runtimeModelAnchor.localRotation = Quaternion.identity;
        runtimeModelAnchor.localScale = Vector3.one;
    }
}
