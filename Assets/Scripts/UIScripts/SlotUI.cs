using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI textAmount;

    private int index;
    private InventoryUI inventory;

    // =========================
    // 🔗 INIT
    // =========================
    public void Init(InventoryUI inventory, int index)
    {
        this.inventory = inventory;
        this.index = index;

        UpdateUI();
    }

    // =========================
    // 🔄 UPDATE UI
    // =========================
    public void UpdateUI()
    {
        SlotData slot = inventory.GetSlot(index);

        if (slot == null || !slot.HasItem)
        {
            icon.enabled = false;
            textAmount.text = "";
            return;
        }

        icon.enabled = true;
        icon.sprite = slot.item.icon;

        textAmount.text = slot.amount > 1
            ? slot.amount.ToString()
            : "";
    }

    // =========================
    // 🖱️ CLICK (DEBUG / TEST)
    // =========================
    public void OnPointerClick(PointerEventData eventData)
    {
        SlotData slot = inventory.GetSlot(index);

        if (slot == null || !slot.HasItem)
            return;

        // Click izquierdo → eliminar item (debug)
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log("Removiendo item de slot: " + index);
            inventory.RemoveItem(index);
        }

        // Click derecho → reducir stack
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            slot.amount--;

            if (slot.amount <= 0)
            {
                inventory.RemoveItem(index);
            }
            else
            {
                inventory.UpdateSlotUI(index);
            }
        }
    }

    // =========================
    // 📌 GETTERS (útiles después)
    // =========================
    public int GetIndex() => index;
    public InventoryUI GetInventory() => inventory;
}