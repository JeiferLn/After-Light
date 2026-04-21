using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    private CameraController cameraController;
    private Animator animator;

    private PlayerAnimatorLocomotion animatorLocomotion;
    private PlayerCharacterMotor motor;
    private PlayerRotationController rotation;

    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeedOutsideCombat = 5f;
    [SerializeField] private float runSpeedInCombat = 8f;
    [SerializeField] private float rotationSharpness = 10f;
    [SerializeField] private float animatorLocomotionSmoothTime = 0.12f;
    [SerializeField][Range(0.01f, 1f)] private float animatorSpeedSmoothTime = 0.18f;

    [Header("Move speed multipliers")]
    [SerializeField][Range(0.05f, 1f)] private float aimMoveSpeedMultiplier = 0.3f;
    [SerializeField][Range(0.05f, 1f)] private float crouchMoveSpeedMultiplier = 0.5f;
    [SerializeField][Range(0.05f, 1f)] private float crouchAimMoveSpeedMultiplier = 0.2f;

    [Header("Animator locomotion Speed")]
    [SerializeField][Range(0f, 1f)] private float animatorSpeedWalk = 0.3f;
    [SerializeField][Range(0f, 1f)] private float animatorSpeedRunExploration = 0.6f;
    [SerializeField][Range(0f, 1f)] private float animatorSpeedRunCombat = 1f;

    [SerializeField] private GameStatus gameStatus;

    private Vector2 currentInput;
    private bool sprintHeld;
    private Vector3 movement;
    [SerializeField] private float gravity = -9.81f;

    private PlayerStatus playerStatus;

    public PlayerStatus PlayerStatus { get { return playerStatus; } set { playerStatus = value; } }

    public GameStatus GameStatus { get { return gameStatus; } set { gameStatus = value; } }

    public void SetMovementInput(Vector2 input)
    {
        currentInput = input;
    }

    public void SetSprint(bool sprintHeld)
    {
        this.sprintHeld = sprintHeld;
    }

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        cameraController = GetComponentInChildren<CameraController>();
        animator = GetComponentInChildren<Animator>();

        animatorLocomotion = new PlayerAnimatorLocomotion(
            animator,
            PlayerCameraPlanar.DefaultMinSqrMagnitude,
            animatorLocomotionSmoothTime,
            animatorSpeedSmoothTime,
            animatorSpeedWalk,
            animatorSpeedRunExploration,
            animatorSpeedRunCombat);

        motor = new PlayerCharacterMotor(characterController);
        rotation = new PlayerRotationController(transform, cameraController, PlayerCameraPlanar.DefaultMinSqrMagnitude);
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        animatorLocomotion.CopySettingsFrom(
            animatorLocomotionSmoothTime,
            animatorSpeedSmoothTime,
            animatorSpeedWalk,
            animatorSpeedRunExploration,
            animatorSpeedRunCombat);

        PromoteCrouchAimToStandingAimIfMoving();
        animatorLocomotion.SyncLocomotionFromInput(ref playerStatus, gameStatus, currentInput, sprintHeld);
        movement = PlayerCameraPlanar.ComputePlanarMovement(currentInput, cameraController, transform, PlayerCameraPlanar.DefaultMinSqrMagnitude);
        animatorLocomotion.UpdateSmoothedAnimatorParameters(playerStatus, currentInput);

        motor.ApplyGravity(gravity, Time.deltaTime);
        rotation.HandleRotation(playerStatus, movement, ref playerStatus, rotationSharpness);

        float speedMul = playerStatus switch
        {
            PlayerStatus.Aiming => aimMoveSpeedMultiplier,
            PlayerStatus.Crounched => crouchMoveSpeedMultiplier,
            PlayerStatus.CrounchAiming => crouchAimMoveSpeedMultiplier,
            _ => 1f,
        };

        float runSpeed = gameStatus == GameStatus.Combat ? runSpeedInCombat : runSpeedOutsideCombat;
        float horizontalSpeed = playerStatus == PlayerStatus.Running ? runSpeed : walkSpeed;
        Vector3 planarVelocity = horizontalSpeed * speedMul * movement;
        motor.Move(planarVelocity, Time.deltaTime);
    }

    void PromoteCrouchAimToStandingAimIfMoving()
    {
        if (playerStatus != PlayerStatus.CrounchAiming)
            return;
        if (currentInput.sqrMagnitude <= PlayerCameraPlanar.DefaultMinSqrMagnitude)
            return;

        playerStatus = PlayerStatus.Aiming;
    }
}
