using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Vector3 movement;
    private CharacterController characterController;

    private float gravity = -9.81f;
    private float yVelocity;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SetMovement(Vector2 input)
    {
        Vector3 move = new Vector3(input.x, 0f, input.y);
        movement = transform.TransformDirection(move);
    }

    void Update()
    {
        Move();
    }

    void Move()
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