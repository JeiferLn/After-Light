using UnityEngine;

[CreateAssetMenu(menuName = "Crafting/Recipe Data")]
public class RecipeData : ScriptableObject
{
    [System.Serializable]
    public class Ingredient
    {
        public ItemData item;
        public int amount;
    }

    public Ingredient[] ingredients;

    public ItemData result;
    public int resultAmount = 1;
}
