using UnityEngine;

public sealed class PlayerAnimatorLocomotion
{
    private readonly Animator animator;
    private readonly float minMoveSqrMagnitude;
    private float locomotionSpeedTarget;
    private float smoothedAnimatorSpeed;
    private float animatorSpeedSmoothVelocity;
    private Vector2 smoothedAnimatorInput;
    private Vector2 animatorSmoothVelocity;

    private float animatorLocomotionSmoothTime;
    private float animatorSpeedSmoothTime;
    private float animatorSpeedWalk;
    private float animatorSpeedRunExploration;
    private float animatorSpeedRunCombat;

    public PlayerAnimatorLocomotion(Animator animator, float minMoveSqrMagnitude, float animatorLocomotionSmoothTime, float animatorSpeedSmoothTime, float animatorSpeedWalk, float animatorSpeedRunExploration, float animatorSpeedRunCombat)
    {
        this.animator = animator;
        this.minMoveSqrMagnitude = minMoveSqrMagnitude;
        this.animatorLocomotionSmoothTime = animatorLocomotionSmoothTime;
        this.animatorSpeedSmoothTime = animatorSpeedSmoothTime;
        this.animatorSpeedWalk = animatorSpeedWalk;
        this.animatorSpeedRunExploration = animatorSpeedRunExploration;
        this.animatorSpeedRunCombat = animatorSpeedRunCombat;
    }

    public void CopySettingsFrom(float animatorLocomotionSmoothTime, float animatorSpeedSmoothTime, float animatorSpeedWalk, float animatorSpeedRunExploration, float animatorSpeedRunCombat)
    {
        this.animatorLocomotionSmoothTime = animatorLocomotionSmoothTime;
        this.animatorSpeedSmoothTime = animatorSpeedSmoothTime;
        this.animatorSpeedWalk = animatorSpeedWalk;
        this.animatorSpeedRunExploration = animatorSpeedRunExploration;
        this.animatorSpeedRunCombat = animatorSpeedRunCombat;
    }

    public void SyncLocomotionFromInput(ref PlayerStatus playerStatus, GameStatus gameStatus, Vector2 currentInput, bool sprintHeld)
    {
        if (PlayerStatusHelpers.IsAimingStatus(playerStatus) || playerStatus == PlayerStatus.Inventory)
        {
            locomotionSpeedTarget = smoothedAnimatorSpeed;
            return;
        }

        if (playerStatus == PlayerStatus.Crounched)
        {
            if (!sprintHeld)
            {
                locomotionSpeedTarget = smoothedAnimatorSpeed;
                return;
            }
            // Sprint: levantarse y usar la misma locomoción que de pie (Idle / Walk / Run).
        }

        bool hasMovement = currentInput.sqrMagnitude > minMoveSqrMagnitude;

        if (!hasMovement)
        {
            playerStatus = PlayerStatus.Idle;
            locomotionSpeedTarget = 0f;
            return;
        }

        if (sprintHeld)
        {
            playerStatus = PlayerStatus.Running;
            locomotionSpeedTarget = gameStatus == GameStatus.Combat ? animatorSpeedRunCombat : animatorSpeedRunExploration;
        }
        else
        {
            playerStatus = PlayerStatus.Walking;
            locomotionSpeedTarget = animatorSpeedWalk;
        }
    }

    public void UpdateSmoothedAnimatorParameters(PlayerStatus playerStatus, Vector2 currentInput)
    {
        if (animator == null)
            return;

        Vector2 target = PlayerStatusHelpers.IsAimingStatus(playerStatus)
            ? Vector2.zero
            : (currentInput.sqrMagnitude > minMoveSqrMagnitude ? currentInput : Vector2.zero);

        if (playerStatus == PlayerStatus.CrounchAiming)
        {
            smoothedAnimatorInput = Vector2.zero;
            animatorSmoothVelocity = Vector2.zero;
        }
        else
        {
            float smooth = Mathf.Max(0.0001f, animatorLocomotionSmoothTime);
            smoothedAnimatorInput = Vector2.SmoothDamp(
                smoothedAnimatorInput,
                target,
                ref animatorSmoothVelocity,
                smooth);
        }

        animator.SetBool("IsCrouching", PlayerStatusHelpers.IsCrouchedPose(playerStatus));
        animator.SetFloat("MoveX", smoothedAnimatorInput.x);
        animator.SetFloat("MoveY", smoothedAnimatorInput.y);

        float speedSmooth = Mathf.Max(0.0001f, animatorSpeedSmoothTime);
        smoothedAnimatorSpeed = Mathf.SmoothDamp(
            smoothedAnimatorSpeed,
            locomotionSpeedTarget,
            ref animatorSpeedSmoothVelocity,
            speedSmooth);
        animator.SetFloat("Speed", smoothedAnimatorSpeed);
    }

}
