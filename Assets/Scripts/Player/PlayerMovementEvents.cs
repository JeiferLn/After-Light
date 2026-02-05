using System;

public static class PlayerMovementEvents
{
    public static event Action OnPlayerMoved;

    public static void NotifyPlayerMoved()
    {
        OnPlayerMoved?.Invoke();
    }
}
