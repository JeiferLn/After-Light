using UnityEngine;

public class NPCInteractable : Interactable
{
    [Header("NPC Dialogue")]
    [TextArea]
    [SerializeField] private string[] dialogueLines;

    private int currentLine;

    public override void Interact()
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
            return;
        
        Debug.Log(dialogueLines[currentLine]);

        currentLine++;

        if (currentLine >= dialogueLines.Length)
            currentLine = 0;
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();
        currentLine = 0;
    }
}