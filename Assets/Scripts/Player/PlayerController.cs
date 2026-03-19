using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerController : MonoBehaviour
{
    // ------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------
    private PlayerMovement playerMovement;

    // ------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------
    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        playerMovement.Move();
    }
}