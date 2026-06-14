using UnityEngine;

public class IdleState : EnemyStateBase
{
    private float _timer;

    public override void Enter(EnemyStateMachine sm)
    {
        _timer = 0f;
        sm.Agent.isStopped = true;
        sm.animator?.CrossFade("ZombieFemale_Idle01",0.1f);
        Debug.Log("Idle Set");
    }

    public override void Update(EnemyStateMachine sm, float dt)
    {
        _timer += dt;

        // ✅ Usa el valor ya calculado, no recalcula distancia
        if (sm.IsDetected) { Transition(sm, EnemyState.Chase); return; }
        if (_timer >= sm.Config.idleDuration) Transition(sm, EnemyState.WalkRandom);
    }

    public override void Exit(EnemyStateMachine sm)
    {
        Debug.Log("Salio del modo Idle");
    }
}