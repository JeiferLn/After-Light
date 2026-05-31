using UnityEngine;

public abstract class EnemyStateBase : IEnemyState
{
    // Métodos obligatorios para cada estado concreto
    public abstract void Enter(EnemyStateMachine sm);
    public abstract void Update(EnemyStateMachine sm, float dt);
    public abstract void Exit(EnemyStateMachine sm);

    // 🔽 Helper seguro para transiciones
    protected void Transition(EnemyStateMachine sm, EnemyState targetState)
    {
        if (sm == null)
        {
            Debug.LogError("Intento de transición con StateMachine nulo.");
            return;
        }
        sm.ChangeState(targetState);
    }
}