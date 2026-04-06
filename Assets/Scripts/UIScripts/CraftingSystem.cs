using System;
using System.Collections.Generic;
using UnityEngine;

public class CraftingSystem : MonoBehaviour
{
    [SerializeField] private InventoryUI inventory;

    public bool CanCraft(RecipeData recipe)
    {
        if (recipe == null || recipe.ingredients == null) return false;

        foreach (var ing in recipe.ingredients)
        {
            int total = 0;

            for (int i = 0; i < inventory.SlotCount; i++)
            {
                var slot = inventory.GetSlot(i);

                if (slot != null && slot.HasItem && slot.item == ing.item)
                {
                    total += slot.amount;
                }
            }

            if (total < ing.amount)
            {
                return false;
            }
        }

        return true;
    }

    public bool Craft(RecipeData recipe)
    {
        if (!CanCraft(recipe))
        {
            return false;
        }

        Dictionary<ItemData, int> consumedItems = new Dictionary<ItemData, int>();

        foreach (var ing in recipe.ingredients)
        {
            int remaining = ing.amount;

            for (int i = 0; i < inventory.SlotCount; i++)
            {
                var slot = inventory.GetSlot(i);

                if (slot != null && slot.HasItem && slot.item == ing.item)
                {
                    int take = Math.Min(slot.amount, remaining);

                    bool consumed = inventory.ConsumeItem(ing.item, take);
                    if (!consumed)
                    {
                        RollbackConsumed(consumedItems);
                        return false;
                    }

                    if (!consumedItems.ContainsKey(ing.item))
                    {
                        consumedItems[ing.item] = 0;
                    }

                    consumedItems[ing.item] += take;
                    remaining -= take;

                    if (remaining <= 0) break;
                }
            }
        }

        bool addedResult = inventory.AddItem(recipe.result, recipe.resultAmount);
        if (!addedResult)
        {
            RollbackConsumed(consumedItems);
            return false;
        }

        return true;
    }

    private void RollbackConsumed(Dictionary<ItemData, int> consumedItems)
    {
        foreach (var entry in consumedItems)
        {
            inventory.AddItem(entry.Key, entry.Value);
        }
    }
}
