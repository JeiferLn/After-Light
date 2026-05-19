using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemID = System.Guid.NewGuid().ToString();
    public string itemName;

    [Multiline]
    public string itemDescription;

    public Sprite icon;
    public GameObject itemModel3d;
    public ItemType itemType;

    [ShowIf("@itemType == ItemType.Consumable || itemType == ItemType.Crystal")]
    [Header("Consumible / Crystal Values")]
    public int recoveryVal = 10;

    [Header("Stack")]
    public bool stackable = true;
    public int amount = 1;
    public int maxStack = 99;
}