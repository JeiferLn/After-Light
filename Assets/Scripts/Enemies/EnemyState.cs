using UnityEngine;
using UnityEngine.AI;

public interface IEnemyState
{
    void Enter(EnemyStateMachine sm);
    void Update(EnemyStateMachine sm, float dt);
    void Exit(EnemyStateMachine sm);
}

public abstract class EnemyStateBase : IEnemyState
{
    public abstract void Enter(EnemyStateMachine sm);
    public abstract void Update(EnemyStateMachine sm, float dt);
    public abstract void Exit(EnemyStateMachine sm);

    protected void Transition(EnemyStateMachine sm, EnemyState target) => sm.ChangeState(target);
}

// --- ESTADOS CONCRETOS (Copia este patrón para los demás) ---
public class IdleState : EnemyStateBase
{
    private float _timer;
    public override void Enter(EnemyStateMachine sm) { _timer = 0; sm.Agent.isStopped = true; }
    public override void Update(EnemyStateMachine sm, float dt)
    {
        _timer += dt;
        if (sm.IsDetected) { Transition(sm, EnemyState.Chase); return; }
        if (_timer >= sm.Config.idleDuration) Transition(sm, EnemyState.WalkRandom);
    }
    public override void Exit(EnemyStateMachine sm) { }
}


public class WalkRandomState : EnemyStateBase
{
    private float _stateTimer;
    private float _targetChangeInterval; // 20-40 segundos
    private Vector3 _currentTarget;

    public override void Enter(EnemyStateMachine sm)
    {
        _stateTimer = 0f;
        _targetChangeInterval = Random.Range(20f, 40f); // 🔀 Rango configurable

        sm.Agent.isStopped = false;
        sm.Agent.speed = sm.Config.moveSpeed;

        SetNewRandomTarget(sm);
    }

    public override void Update(EnemyStateMachine sm, float dt)
    {
        _stateTimer += dt;

        // 🎯 Prioridad 1: Si detecta al jugador → Chase (inmediato)
        if (sm.IsDetected)
        {
            Transition(sm, EnemyState.Chase);
            return;
        }

        // 🎯 Prioridad 2: Timer para cambiar de dirección (20-40s)
        if (_stateTimer >= _targetChangeInterval)
        {
            SetNewRandomTarget(sm);
            _stateTimer = 0f;
            _targetChangeInterval = Random.Range(20f, 40f); // Re-roll para variabilidad
            return;
        }

        // 🎯 Prioridad 3: Si llegó al destino actual, esperar (no cambiar inmediatamente)
        if (sm.Agent.hasPath && sm.Agent.remainingDistance <= sm.Agent.stoppingDistance)
        {
            // Opcional: pausar movimiento visualmente mientras espera el próximo target
            // sm.Agent.isStopped = true; // Descomentar si quieres que "descanse" al llegar
        }
    }

    public override void Exit(EnemyStateMachine sm)
    {
        // Cleanup si es necesario
    }

    private void SetNewRandomTarget(EnemyStateMachine sm, int attempts = 0)
    {
        // 🔒 Límite de seguridad para evitar StackOverflow
        if (attempts >= 5)
        {
            Debug.LogWarning($"[{sm.name}] No se encontró punto navegable tras 5 intentos.");
            return;
        }

        Vector3 randomPoint = sm.transform.position + Random.insideUnitSphere * sm.Config.walkRadius;
        randomPoint.y = sm.transform.position.y;

        // ✅ Aquí se declara 'found' y se evalúa el NavMesh
        bool found = NavMesh.SamplePosition(randomPoint, out var hit, sm.Config.walkRadius, NavMesh.AllAreas);

        if (found)
        {
            _currentTarget = hit.position;
            sm.SetDestinationSafe(_currentTarget); // Usa tu método seguro
        }
        else
        {
            // Reintentar con contador incrementado
            SetNewRandomTarget(sm, attempts + 1);
        }
    }
}

public class ChaseState : EnemyStateBase
{
    public override void Enter(EnemyStateMachine sm)
    {
        sm.Agent.isStopped = false; sm.Agent.speed = sm.Config.moveSpeed * 1.2f;
        sm.Agent.SetDestination(sm.Player.position);
    }
    public override void Update(EnemyStateMachine sm, float dt)
    {
        if (!sm.IsDetected) { Transition(sm, EnemyState.Idle); return; }
        sm.Agent.SetDestination(sm.Player.position);
        // Aquí iría lógica de ataque si distance < attackRange
    }
    public override void Exit(EnemyStateMachine sm) { }
}