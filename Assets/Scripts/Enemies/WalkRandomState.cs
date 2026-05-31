using UnityEngine;

public class WalkRandomState : EnemyStateBase
{
    private float _stateTimer;
    private float _targetChangeInterval;
    private Vector3 _currentTarget;

    public override void Enter(EnemyStateMachine sm)
    {
        _stateTimer = 0f;

        // Fallback seguro por si los campos no están en el SO
        float min = sm.Config.walkMinInterval > 0 ? sm.Config.walkMinInterval : 20f;
        float max = sm.Config.walkMaxInterval > min ? sm.Config.walkMaxInterval : min + 20f;
        _targetChangeInterval = Random.Range(min, max);

        sm.Agent.isStopped = false;
        sm.Agent.speed = sm.Config.moveSpeed;

        SetNewRandomTarget(sm);
    }

    public override void Update(EnemyStateMachine sm, float dt)
    {
        _stateTimer += dt;

        // ✅ Mismo patrón
        if (sm.IsDetected) { Transition(sm, EnemyState.Chase); return; }

        if (_stateTimer >= _targetChangeInterval)
        {
            SetNewRandomTarget(sm);
            _stateTimer = 0f;
            _targetChangeInterval = Random.Range(sm.Config.walkMinInterval, sm.Config.walkMaxInterval);
        }
    }

    public override void Exit(EnemyStateMachine sm) { }

    private void SetNewRandomTarget(EnemyStateMachine sm, int attempts = 0)
    {
        if (attempts >= 5)
        {
            Debug.LogWarning($"[{sm.name}] No se encontró punto navegable tras 5 intentos.");
            return;
        }

        Vector3 randomPoint = sm.transform.position + Random.insideUnitSphere * sm.Config.walkRadius;
        randomPoint.y = sm.transform.position.y;

        bool found = UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out var hit, sm.Config.walkRadius, UnityEngine.AI.NavMesh.AllAreas);

        if (found)
        {
            _currentTarget = hit.position;
            sm.SetDestinationSafe(_currentTarget);
        }
        else
        {
            SetNewRandomTarget(sm, attempts + 1);
        }
    }
}