using UnityEngine;

public class IdleState : EnemyStateBase
{
    private float _timer;

    public override void Enter(EnemyStateMachine sm)
    {
        _timer = 0f;
        sm.Agent.isStopped = true;
        // sm.Animator?.SetTrigger("Idle"); // Descomenta si usas Animator
    }

    public override void Update(EnemyStateMachine sm, float dt)
    {
        _timer += dt;

        // ✅ Usa el valor ya calculado, no recalcula distancia
        if (sm.IsDetected) { Transition(sm, EnemyState.Chase); return; }
        if (_timer >= sm.Config.idleDuration) Transition(sm, EnemyState.WalkRandom);
    }

    public override void Exit(EnemyStateMachine sm) { }
}