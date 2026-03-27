using UnityEngine;

public sealed class PlayerCharacterMotor
{
    private readonly CharacterController characterController;
    private const float GroundedStickForce = -2f;

    private float yVelocity;

    public PlayerCharacterMotor(CharacterController characterController)
    {
        this.characterController = characterController;
    }

    public void ApplyGravity(float gravity, float deltaTime)
    {
        if (characterController.isGrounded && yVelocity < 0)
            yVelocity = GroundedStickForce;

        yVelocity += gravity * deltaTime;
    }

    public void Move(Vector3 planarHorizontal, float deltaTime)
    {
        Vector3 finalMove = planarHorizontal;
        finalMove.y = yVelocity;
        characterController.Move(finalMove * deltaTime);
    }
}
