using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField]
    protected string interactionText = "Interactuar";

    [Header("Outline")]
    [SerializeField]
    private Renderer objectRenderer;

    [SerializeField]
    private string outlineProperty = "_OutlineEnabled";

    protected Material materialInstance;

    protected virtual void Awake()
    {
        if (objectRenderer == null)
            objectRenderer = GetComponent<Renderer>();

        materialInstance = objectRenderer.material;
    }

    public virtual void OnFocus()
    {
        if (materialInstance == null)
            return;

        Debug.Log($"Focus: {name}");
        materialInstance.SetFloat(outlineProperty, 1f);
    }

    public virtual void OnLoseFocus()
    {
        if (materialInstance == null)
            return;

        Debug.Log($"LoseFocus: {name}");
        materialInstance.SetFloat(outlineProperty, 0f);
    }

    public abstract void Interact();
}
