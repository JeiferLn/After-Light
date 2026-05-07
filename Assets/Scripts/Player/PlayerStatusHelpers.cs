public static class PlayerStatusHelpers
{
    public static bool IsAimingStatus(PlayerStatus s) =>
        s == PlayerStatus.Aiming || s == PlayerStatus.CrounchAiming;

    public static bool IsCrouchedPose(PlayerStatus s) =>
        s == PlayerStatus.Crounched || s == PlayerStatus.CrounchAiming;

    public static bool IsIdle(PlayerStatus s) =>
        s == PlayerStatus.Idle;

    public static bool IsActiveLocomotion(PlayerStatus s) =>
        s == PlayerStatus.Walking || s == PlayerStatus.Running;
}
