using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotPointer : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    private InventoryUI owner;
    private int slotIndex;

    public void Initialize(InventoryUI inventoryUI, int index)
    {
        owner = inventoryUI;
        slotIndex = index;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        owner?.NotifySlotHover(slotIndex);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        owner?.NotifySlotClick(slotIndex);
    }
}
