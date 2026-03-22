using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Slot UI Prefab")]
    [SerializeField] private GameObject slotPrefab;

    [SerializeField] private int initialSlots = 8;

    private int currentSlots;
    private SlotData[] slots;

    private void Awake()
    {
        currentSlots = initialSlots;
        slots = new SlotData[initialSlots];

        for (int i = 0; i < initialSlots; i++)
        {
            slots[i] = new SlotData();

            GameObject go = Instantiate(slotPrefab, transform);

            SlotUI slotUI = go.GetComponent<SlotUI>();
            slotUI.Init(this, i);
        }
    }

    public bool AddItem(ItemData item, int amount)
    {
        // 1️⃣ Intentar stackear primero
        if (item.stackable)
        {
            for (int i = 0; i < currentSlots; i++)
            {
                if (slots[i].HasItem && slots[i].item == item)
                {
                    int espacio = item.maxStack - slots[i].amount;

                    if (espacio > 0)
                    {
                        int agregar = Mathf.Min(espacio, amount);

                        slots[i].amount += agregar;
                        amount -= agregar;

                        UpdateSlotUI(i);

                        if (amount <= 0)
                            return true;
                    }
                }
            }
        }

        for (int i = 0; i < currentSlots; i++)
        {
            if (!slots[i].HasItem)
            {
                int agregar = item.stackable
                    ? Mathf.Min(item.maxStack, amount)
                    : 1;

                slots[i].item = item;
                slots[i].amount= agregar;

                amount-= agregar;

                UpdateSlotUI(i);

                if (amount<= 0)
                    return true;
            }
        }

        Debug.Log("Inventario lleno o sin espacio suficiente");
        return false;
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

            GameObject go = Instantiate(slotPrefab, transform);
            SlotUI slotUI = go.GetComponent<SlotUI>();
            slotUI.Init(this, i);
        }

        slots = newSlots;
        currentSlots = newTotal;
    }

  
    public void UpdateSlotUI(int index)
    {
        if (!IsValidIndex(index)) return;

        Transform slotTransform = transform.GetChild(index);
        SlotUI slotUI = slotTransform.GetComponent<SlotUI>();
        slotUI.UpdateUI();
    }

   
    // TESTING

    public SlotData GetSlot(int index)
    {
        if (!IsValidIndex(index)) return null;
        return slots[index];
    }

   
    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < currentSlots;
    }

    [SerializeField] private ItemData testItem;
    [ContextMenu("TESTING")]
    public void Test()
    {
        AddItem(testItem, 10);
    }
}