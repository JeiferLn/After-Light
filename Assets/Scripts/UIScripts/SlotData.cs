[System.Serializable]
public class SlotData
{
    public ItemData item;
    public int amount;

    public bool HasItem => item != null;
}