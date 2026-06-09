using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    private Dictionary<string, ItemData> itemLookup;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        itemLookup = new Dictionary<string, ItemData>();

        ItemData[] allItems = Resources.LoadAll<ItemData>("Items");

        foreach (ItemData item in allItems)
        {
            if (item == null)
                continue;

            if (string.IsNullOrWhiteSpace(item.itemID))
            {
                Debug.LogWarning($"Item without ID: {item.name}");
                continue;
            }

            if (itemLookup.ContainsKey(item.itemID))
            {
                Debug.LogWarning($"Duplicate Item ID: {item.itemID}");
                continue;
            }

            itemLookup.Add(item.itemID, item);

            // Debug.Log($"Registered Item: {item.itemID}");
        }

        Debug.Log($"ItemDatabase Loaded: {itemLookup.Count} items");
    }

    public ItemData GetItem(string id)
    {
        if (itemLookup.TryGetValue(id, out ItemData item))
            return item;

        Debug.LogWarning($"Item ID not found: {id}");
        return null;
    }
}