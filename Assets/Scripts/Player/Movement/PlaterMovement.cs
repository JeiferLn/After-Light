using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    // ------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------
    [SerializeField] private float moveSpeed = 5f;
    private Vector3 movement;
    private CharacterController characterController;
    private float gravity = -9.81f;
    private float yVelocity;

    // ------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    public void SetMovement(Vector2 movement)
    {
        this.movement = new Vector3(movement.x, 0f, movement.y);
    }

    public void Move()
    {
        if (characterController.isGrounded && yVelocity < 0)
        {
            yVelocity = -2f;
        }

        yVelocity += gravity * Time.deltaTime;

        Vector3 finalMove = movement * moveSpeed;
        finalMove.y = yVelocity;

        characterController.Move(finalMove * Time.deltaTime);
    }
}