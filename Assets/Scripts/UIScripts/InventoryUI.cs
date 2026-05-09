using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("UI")]

    [Tooltip("Slot UI Cell")]
    [SerializeField] private GameObject slotPrefab;

    [Tooltip("visual UI Item")]
    [SerializeField] private GameObject itemPrefab;

    [SerializeField] private int initialSlots = 8;
    [SerializeField] private Color normalSlotColor = new(0.6150943f, 0.6150943f, 0.6150943f, 1f);
    [SerializeField] private Color selectedSlotColor = new(0.95f, 0.85f, 0.25f, 1f);
    [SerializeField] private float moveRepeatDelay = 0.35f;
    [SerializeField] private float moveRepeatRate = 0.12f;
    [SerializeField] private float moveDeadzone = 0.45f;

    private int currentSlots;
    private SlotData[] slots;
    private int selectedIndex = -1;
    private float nextMoveTime;
    private bool moveHeld;
    private int gridColumns = 4;

    private string SavePath => Application.persistentDataPath + "/inventory.json";

    private void Awake()
    {
        GridLayoutGroup grid = GetComponent<GridLayoutGroup>();
        if (grid != null && grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
        {
            gridColumns = Mathf.Max(1, grid.constraintCount);
        }

        currentSlots = initialSlots;
        slots = new SlotData[initialSlots];

        for (int i = 0; i < initialSlots; i++)
        {
            slots[i] = new SlotData();

            CreateSlotGO(i);
        }

        SetSelectedIndex(0);
    }
    private void Update()
    {
        if (!isActiveAndEnabled || !gameObject.activeInHierarchy || currentSlots == 0)
            return;

        Vector2 moveVector = ReadMoveInput();
        int horizontal = Mathf.Abs(moveVector.x) >= moveDeadzone ? (moveVector.x > 0f ? 1 : -1) : 0;
        int vertical = Mathf.Abs(moveVector.y) >= moveDeadzone ? (moveVector.y > 0f ? 1 : -1) : 0;

        if (horizontal == 0 && vertical == 0)
        {
            moveHeld = false;
            return;
        }

        float now = Time.unscaledTime;
        if (!moveHeld)
        {
            moveHeld = true;
            nextMoveTime = now + moveRepeatDelay;
            MoveSelection(horizontal, vertical);
            return;
        }

        if (now >= nextMoveTime)
        {
            nextMoveTime = now + moveRepeatRate;
            MoveSelection(horizontal, vertical);
        }
    }

    public bool AddItem(ItemData item, int amount)
    {
        if (item.stackable)
        {
            for (int i = 0; i < currentSlots; i++)
            {
                if (slots[i].HasItem && slots[i].item == item)
                {
                    int space = item.maxStack - slots[i].amount;

                    if (space > 0)
                    {
                        int add = Mathf.Min(space, amount);

                        slots[i].amount += add;
                        amount -= add;

                        UpdateSlotUI(i);

                        if (amount <= 0)
                            return true;
                    }
                }
            }
        }

        while (amount > 0)
        {
            int index = FindFreeSlot();

            if (index == -1)
            {
                Debug.Log("Inventario lleno o sin espacio suficiente");
                return false;
            }

            int add = item.stackable
                ? Mathf.Min(item.maxStack, amount)
                : 1;

            slots[index].item = item;
            slots[index].amount = add;

            amount -= add;

            UpdateSlotUI(index);
        }

        return true;
    }

    public void RemoveItem(int index)
    {
        if (!IsValidIndex(index)) return;

        slots[index] = new SlotData();
        UpdateSlotUI(index);
    }

    private int FindFreeSlot()
    {
        for (int i = 0; i < currentSlots; i++)
        {
            if (!slots[i].HasItem)
                return i;
        }
        return -1;
    }

    public void AddSlots(int amount)
    {
        if (amount <= 0) return;

        int newTotal = currentSlots + amount;
        SlotData[] newSlots = new SlotData[newTotal];

        // copiar
        for (int i = 0; i < currentSlots; i++)
            newSlots[i] = slots[i];

        // nuevos slots
        for (int i = currentSlots; i < newTotal; i++)
        {
            newSlots[i] = new SlotData();
            CreateSlotGO(i);
        }

        slots = newSlots;
        currentSlots = newTotal;
        SetSelectedIndex(selectedIndex < 0 ? 0 : selectedIndex);
    }


    public void UpdateSlotUI(int index)
    {
        if (!IsValidIndex(index)) return;

        Transform slot = transform.GetChild(index);

        if (slot.childCount > 0)
        {
            Destroy(slot.GetChild(0).gameObject);
        }

        if (!slots[index].HasItem) return;

        GameObject itemGO = Instantiate(itemPrefab, slot);
        CanvasGroup canvasGroup = itemGO.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = itemGO.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;

        ItemUI itemUI = itemGO.GetComponent<ItemUI>();
        itemUI.Setup(slots[index].item, slots[index].amount);
    }

    public bool ConsumeItem(ItemData item, int amount)
    {
        for (int i = 0; i < currentSlots; i++)
        {
            if (slots[i].HasItem && slots[i].item == item)
            {
                if (slots[i].amount >= amount)
                {
                    slots[i].amount -= amount;

                    if (slots[i].amount <= 0)
                    {
                        slots[i] = new SlotData();
                    }

                    UpdateSlotUI(i);
                    return true;
                }
            }
        }

        Debug.Log("Its Empty or not enought amount");
        return false;
    }

    public int SlotCount => currentSlots;

    public SlotData GetSlot(int index)
    {
        if (!IsValidIndex(index)) return null;
        return slots[index];
    }

    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < currentSlots;
    }

    public void SaveInventory()
    {
        InventorySaveData saveData = new();

        saveData.slotCount = currentSlots;

        for (int i = 0; i < currentSlots; i++)
        {
            InventorySlotSave slotSave = new();

            if (slots[i].HasItem)
            {
                slotSave.itemID = slots[i].item.itemID;
                slotSave.amount = slots[i].amount;
            }
            else
            {
                slotSave.itemID = "";
                slotSave.amount = 0;
            }

            saveData.slots.Add(slotSave);
        }

        string json = JsonUtility.ToJson(saveData, true);

        System.IO.File.WriteAllText(SavePath, json);

        Debug.Log("Inventory Saved: " + SavePath);
    }

    public void LoadInventory()
    {
        if (!System.IO.File.Exists(SavePath))
        {
            Debug.Log("No inventory save found");
            return;
        }

        string json = System.IO.File.ReadAllText(SavePath);

        InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);


        for (int i = 0; i < currentSlots; i++)
        {
            slots[i] = new SlotData();
            UpdateSlotUI(i);
        }


        if (saveData.slotCount > currentSlots)
        {
            AddSlots(saveData.slotCount - currentSlots);
        }


        for (int i = 0; i < saveData.slots.Count; i++)
        {
            InventorySlotSave slotSave = saveData.slots[i];

            if (string.IsNullOrEmpty(slotSave.itemID))
                continue;

            ItemData item = ItemDatabase.Instance.GetItem(slotSave.itemID);

            if (item == null)
                continue;

            slots[i].item = item;
            slots[i].amount = slotSave.amount;

            UpdateSlotUI(i);
        }

        SetSelectedIndex(Mathf.Clamp(selectedIndex < 0 ? 0 : selectedIndex, 0, currentSlots - 1));

        Debug.Log("Inventory Loaded");
    }

    public void ResetInventorySave()
    {
        if (System.IO.File.Exists(SavePath))
        {
            System.IO.File.Delete(SavePath);
            Debug.Log("Inventory Save Deleted");
        }

        // limpiar inventario actual
        for (int i = 0; i < currentSlots; i++)
        {
            slots[i] = new SlotData();
            UpdateSlotUI(i);
        }

        SetSelectedIndex(0);
    }

    public void NotifySlotHover(int index)
    {
        if (!gameObject.activeInHierarchy) return;
        SetSelectedIndex(index);
    }

    public void NotifySlotClick(int index)
    {
        if (!gameObject.activeInHierarchy) return;
        SetSelectedIndex(index);
    }

    private GameObject CreateSlotGO(int index)
    {
        GameObject slot = Instantiate(slotPrefab, transform);
        Image image = slot.GetComponent<Image>();
        if (image != null)
            image.raycastTarget = true;

        InventorySlotPointer pointer = slot.GetComponent<InventorySlotPointer>();
        if (pointer == null)
            pointer = slot.AddComponent<InventorySlotPointer>();
        pointer.Initialize(this, index);

        ApplySlotVisual(index, index == selectedIndex);
        return slot;
    }

    private Vector2 ReadMoveInput()
    {
        if (Gamepad.current == null)
            return Vector2.zero;

        Vector2 dpad = Gamepad.current.dpad.ReadValue();
        if (dpad.sqrMagnitude > 0.01f)
            return dpad;

        return Gamepad.current.leftStick.ReadValue();
    }

    private void MoveSelection(int horizontal, int vertical)
    {
        if (selectedIndex < 0)
        {
            SetSelectedIndex(0);
            return;
        }

        int next = selectedIndex;
        if (horizontal != 0)
            next = GetWrappedHorizontalIndex(next, horizontal);
        else if (vertical != 0)
            next = GetWrappedVerticalIndex(next, vertical);

        SetSelectedIndex(next);
    }

    private int GetWrappedHorizontalIndex(int startIndex, int direction)
    {
        int rowStart = (startIndex / gridColumns) * gridColumns;
        int rowEnd = Mathf.Min(rowStart + gridColumns - 1, currentSlots - 1);
        int candidate = startIndex + direction;

        if (candidate < rowStart)
            return rowEnd;
        if (candidate > rowEnd)
            return rowStart;
        return candidate;
    }

    private int GetWrappedVerticalIndex(int startIndex, int direction)
    {
        int column = startIndex % gridColumns;
        int candidate = startIndex + (direction > 0 ? -gridColumns : gridColumns);

        if (candidate >= 0 && candidate < currentSlots)
            return candidate;

        if (direction > 0)
        {
            int lastFullRow = ((currentSlots - 1) / gridColumns) * gridColumns;
            int wrapped = lastFullRow + column;
            if (wrapped >= currentSlots)
                wrapped -= gridColumns;
            return Mathf.Clamp(wrapped, 0, currentSlots - 1);
        }

        int wrappedDown = column;
        while (wrappedDown + gridColumns < currentSlots)
            wrappedDown += gridColumns;
        return wrappedDown;
    }

    private void SetSelectedIndex(int index)
    {
        if (!IsValidIndex(index))
            return;

        int previous = selectedIndex;
        selectedIndex = index;

        if (IsValidIndex(previous))
            ApplySlotVisual(previous, false);
        ApplySlotVisual(selectedIndex, true);
    }

    private void ApplySlotVisual(int index, bool isSelected)
    {
        if (!IsValidIndex(index) || index >= transform.childCount)
            return;

        Image slotImage = transform.GetChild(index).GetComponent<Image>();
        if (slotImage != null)
            slotImage.color = isSelected ? selectedSlotColor : normalSlotColor;
    }

    //---------------------funciones para testear------------------------//

    // Agregar item
    [Header("Testing")]
    [SerializeField] private ItemData testItem;
    [SerializeField] private int testAmount = 1;

    [ContextMenu("Test Add Item")]
    private void TestAddItem()
    {
        if (testItem == null)
        {
            Debug.LogWarning("No test item assigned");
            return;
        }

        testAmount = testItem.amount;
        AddItem(testItem, testAmount);
    }

    // Consumir Item
    [Header("Testing Consume")]
    [SerializeField] private ItemData consumeTestItem;
    [SerializeField] private int consumeAmount = 1;

    [ContextMenu("Test Consume Item")]
    private void TestConsumeItem()
    {
        if (consumeTestItem == null)
        {
            Debug.LogWarning("No consume test item assigned");
            return;
        }

        bool success = ConsumeItem(consumeTestItem, consumeAmount);

        Debug.Log(success
            ? $"Consumed {consumeAmount} of {consumeTestItem.itemName}"
            : "Could not consume item");
    }

    //-------------- SAVE-LOAD ---------------------//
    [ContextMenu("SAVE INVENTORY")]
    private void TestSave()
    {
        SaveInventory();
    }

    [ContextMenu("LOAD INVENTORY")]
    private void TestLoad()
    {
        LoadInventory();
    }

    [ContextMenu("RESET INVENTORY SAVE")]
    private void TestResetSave()
    {
        ResetInventorySave();
    }
}