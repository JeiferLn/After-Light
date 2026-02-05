using UnityEngine;

public class VisionCone : MonoBehaviour
{
    // ------------- VISUALS -------------
    public GameObject cone;

    private void OnEnable()
    {
        PlayerMovementEvents.OnPlayerMoved += Hide;
    }

    private void OnDisable()
    {
        PlayerMovementEvents.OnPlayerMoved -= Hide;
    }

    private void Start()
    {
        ShowCone();
    }

    public bool IsVisible => cone != null && cone.activeSelf;

    public void ShowCone()
    {
        if (cone == null)
            return;
        cone.SetActive(true);
    }

    public void Hide()
    {
        if (cone != null)
            cone.SetActive(false);
    }
}
