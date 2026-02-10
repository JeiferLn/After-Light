using UnityEngine;

public class TextInteractable : Interactable
{
    [TextArea]
    [SerializeField]
    private string message;

    public override void Interact()
    {
        Debug.Log(message);
        // Más adelante: UIManager.Show(message)
    }
}
