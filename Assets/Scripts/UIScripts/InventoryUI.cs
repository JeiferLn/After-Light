using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("UI")]

    [Tooltip("Slot UI Cell")]
    [SerializeField] private GameObject slotPrefab;

    [Tooltip("visual UI Item")]
    [SerializeField] private GameObject itemPrefab;

    [SerializeField] private int initialSlots = 8;

    private int currentSlots;
    private SlotData[] slots;

    private string SavePath => Application.persistentDataPath + "/inventory.json";

    private void Awake()
    {
        currentSlots = initialSlots;
        slots = new SlotData[initialSlots];

        for (int i = 0; i < initialSlots; i++)
        {
            slots[i] = new SlotData();

            Instantiate(slotPrefab, transform);
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
            Instantiate(slotPrefab, transform);
        }

        slots = newSlots;
        currentSlots = newTotal;
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