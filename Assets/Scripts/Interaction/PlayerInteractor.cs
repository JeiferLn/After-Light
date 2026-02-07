using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactionRadius = 1.2f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private Interactable currentInteractable;

    void Update()
    {
        DetectInteractable();

        if (currentInteractable != null && Input.GetKeyDown(interactKey))
        {
            currentInteractable.Interact();
        }
    }

    void DetectInteractable()
    {
        Collider2D hit = Physics2D.OverlapCircle(
            transform.position,
            interactionRadius,
            interactableLayer
        );

        if (hit == null)
        {
            ClearCurrent();
            return;
        }

        if (hit.TryGetComponent(out Interactable interactable))
        {
            if (currentInteractable != interactable)
            {
                ClearCurrent();
                currentInteractable = interactable;
                currentInteractable.OnFocus();
            }
        }
    }

    void ClearCurrent()
    {
        if (currentInteractable != null)
        {
            currentInteractable.OnLoseFocus();
            currentInteractable = null;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
#endif
}