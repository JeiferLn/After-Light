using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] protected string interactionText = "Interactuar";

    public virtual void OnFocus()
    {
        // Aquí luego puedes poner outline, sprite highlight, etc.
        Debug.Log($"Focus: {name}");
    }

    public virtual void OnLoseFocus()
    {
        Debug.Log($"Lose Focus: {name}");
    }

    public abstract void Interact();
}