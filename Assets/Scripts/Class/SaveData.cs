using System;
using System.Collections.Generic;

[Serializable]
public class InventorySaveData
{
    public int slotCount;
    public List<InventorySlotSave> slots = new();
}

[Serializable]
public class InventorySlotSave
{
    public string itemID;
    public int amount;
}