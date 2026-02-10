using System.Collections;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    // ---------- COMPONENTS ----------
    private PlayerMovementController playerMovementController;

    [Header("Interaction")]
    [SerializeField]
    private float interactionRadius = 1.2f;

    // ---------- INTERACTION LAYER ----------
    [Header("Interaction Layer")]
    [SerializeField]
    private LayerMask interactionLayer;

    // ---------- STATE ----------
    private Interactable currentInteractable;
    private InteractableEntrance currentEntrance;

    // ---------- ENTRANCE INTERACTION STATE ----------
    private float lastTapTime;
    private bool peekTriggeredThisHold;
    private Coroutine peekHoldRoutine;
    private Coroutine slowTapRoutine;
    private const float doubleTapWindow = 0.35f;
    private const float peekHoldDuration = 1f;

    // ---------- UNITY ----------
    private void Awake()
    {
        playerMovementController = GetComponent<PlayerMovementController>();
    }

    private void Update()
    {
        DetectInteractable();
    }

    // ---------- PUBLIC METHODS (CALLED BY INPUT SYSTEM) ----------
    public void OnInteractionStarted()
    {
        DetectInteractable();

        // Si hay una puerta
        if (currentEntrance != null)
        {
            // Si el jugador ya está en estado Peeking, cerrar el cono
            if (
                playerMovementController != null
                && playerMovementController.CurrentState == PlayerState.Peeking
            )
            {
                if (peekHoldRoutine != null)
                {
                    StopCoroutine(peekHoldRoutine);
                    peekHoldRoutine = null;
                }
                currentEntrance.ClosePeek(playerMovementController);
                return;
            }

            // Si no está en peek, iniciar lógica de hold para peek
            peekTriggeredThisHold = false;
            if (peekHoldRoutine != null)
                StopCoroutine(peekHoldRoutine);
            peekHoldRoutine = StartCoroutine(PeekAfterHold());
            return;
        }

        // Si hay un interactable genérico, interactuar inmediatamente
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    public void OnInteractionCanceled()
    {
        DetectInteractable();

        if (currentEntrance != null)
        {
            if (
                playerMovementController != null
                && playerMovementController.CurrentState == PlayerState.Peeking
            )
            {
                if (peekHoldRoutine != null)
                {
                    StopCoroutine(peekHoldRoutine);
                    peekHoldRoutine = null;
                }
                return;
            }

            if (peekHoldRoutine != null)
            {
                StopCoroutine(peekHoldRoutine);
                peekHoldRoutine = null;
            }

            if (peekTriggeredThisHold)
                return;

            float timeSinceLastTap = Time.time - lastTapTime;

            if (timeSinceLastTap < doubleTapWindow)
            {
                // Double tap - abrir/cerrar rápido
                if (slowTapRoutine != null)
                {
                    StopCoroutine(slowTapRoutine);
                    slowTapRoutine = null;
                }

                currentEntrance.OpenOrCloseFast(playerMovementController);
            }
            else
            {
                // Single tap - esperar para ver si es double tap
                slowTapRoutine = StartCoroutine(SlowTapDelayed());
            }

            lastTapTime = Time.time;
        }
    }

    // ---------- DETECTION ----------
    private void DetectInteractable()
    {
        Collider2D hit = Physics2D.OverlapCircle(
            transform.position,
            interactionRadius,
            interactionLayer
        );

        if (hit == null)
        {
            ClearCurrentInteractable();
            return;
        }

        if (hit.TryGetComponent(out Interactable interactable))
        {
            if (currentInteractable != interactable)
            {
                ClearCurrentInteractable();
                currentInteractable = interactable;
                currentInteractable.OnFocus();
                return;
            }
        }

        if (hit.TryGetComponent(out InteractableEntrance entrance))
        {
            if (currentEntrance != entrance)
            {
                currentEntrance = null;
                currentEntrance = entrance;
                return;
            }
        }
    }

    // ---------- CLEAR METHODS ----------
    private void ClearCurrentInteractable()
    {
        if (currentInteractable != null)
        {
            currentInteractable.OnLoseFocus();
            currentInteractable = null;
        }
        if (currentEntrance != null)
        {
            currentEntrance = null;
        }
    }

    // ---------- COROUTINES ----------
    private IEnumerator PeekAfterHold()
    {
        yield return new WaitForSeconds(peekHoldDuration);
        peekHoldRoutine = null;
        DetectInteractable();
        if (currentEntrance != null && playerMovementController != null)
        {
            currentEntrance.Peek(playerMovementController);
            peekTriggeredThisHold = true;
        }
    }

    private IEnumerator SlowTapDelayed()
    {
        yield return new WaitForSeconds(doubleTapWindow);

        if (currentEntrance != null)
        {
            currentEntrance.OpenOrCloseSlow(playerMovementController);
        }

        slowTapRoutine = null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Generic interaction radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);

        // Entrance interaction range
        Vector2 direction = Vector2.right * Mathf.Sign(transform.localScale.x);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, direction * interactionRadius);
    }
#endif
}
