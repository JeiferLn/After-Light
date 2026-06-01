using UnityEngine;
using UnityEngine.AI;

public class WalkRandomState : EnemyStateBase
{
    private float _targetChangeTimer;
    private float _targetChangeInterval;
    private float _idleWaitTimer;
    private Vector3 _currentTarget;
    private bool _hasReachedDestination;

    private const int ARRIVAL_CONFIRM_FRAMES = 3;
    private int _arrivalFrameCount;

    public override void Enter(EnemyStateMachine sm)
    {
        _targetChangeTimer = 0f;
        _idleWaitTimer = 0f;
        _hasReachedDestination = false;
        _arrivalFrameCount = 0;
        _targetChangeInterval = GetRandomInterval(sm);

        sm.Agent.isStopped = false;
        sm.Agent.speed = sm.Config.moveSpeed;

        sm.animator?.CrossFade("ZombieFemale_Walk01Forward", 0.1f);

        SetNewRandomTarget(sm);
    }

    public override void Update(EnemyStateMachine sm, float dt)
    {
        // 1️⃣ Detección tiene prioridad absoluta
        if (sm.IsDetected)
        {
            Transition(sm, EnemyState.Chase);
            return;
        }

        // 2️⃣ Mientras espera en destino
        if (_hasReachedDestination)
        {
            _idleWaitTimer += dt;
            if (_idleWaitTimer >= sm.Config.idleDuration)
                Transition(sm, EnemyState.Idle);
            return;
        }

        // 3️⃣ Detección de llegada
        bool noPath = !sm.Agent.pathPending && !sm.Agent.hasPath;
        bool arrived = !sm.Agent.pathPending
                    && sm.Agent.hasPath
                    && sm.Agent.remainingDistance <= sm.Agent.stoppingDistance
                    && sm.Agent.velocity.sqrMagnitude < 0.04f;

        bool looksDone = arrived || noPath;

        if (looksDone)
        {
            _arrivalFrameCount++;
            if (_arrivalFrameCount >= ARRIVAL_CONFIRM_FRAMES)
            {
                _hasReachedDestination = true;
                _idleWaitTimer = 0f;
                _arrivalFrameCount = 0;
                sm.Agent.isStopped = true;
                sm.animator?.CrossFade("ZombieFemale_Idle01", 0.2f);
            }
            return;
        }
        else
        {
            _arrivalFrameCount = 0;
        }

        // 4️⃣ Sigue caminando — timer de cambio de target
        _targetChangeTimer += dt;
        if (_targetChangeTimer >= _targetChangeInterval)
        {
            SetNewRandomTarget(sm);
            _targetChangeTimer = 0f;
            _targetChangeInterval = GetRandomInterval(sm);
        }
    }

    public override void Exit(EnemyStateMachine sm)
    {
        sm.Agent.isStopped = false;
        sm.animator?.CrossFade("ZombieFemale_Idle01", 0.1f);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private float GetRandomInterval(EnemyStateMachine sm)
    {
        float min = sm.Config.walkMinInterval > 0 ? sm.Config.walkMinInterval : 20f;
        float max = sm.Config.walkMaxInterval > min ? sm.Config.walkMaxInterval : min + 20f;
        return Random.Range(min, max);
    }

    private void SetNewRandomTarget(EnemyStateMachine sm, int attempts = 0)
    {
        if (attempts >= 5)
        {
            // Sin punto válido — forzar idle directamente
            _hasReachedDestination = true;
            _idleWaitTimer = 0f;
            sm.Agent.isStopped = true;
            sm.animator?.CrossFade("ZombieFemale_Idle01", 0.2f);
            return;
        }

        Vector3 randomDir = Random.insideUnitSphere * sm.Config.walkRadius;
        randomDir.y = 0f;
        Vector3 randomPoint = sm.transform.position + randomDir;

        float sampleRadius = Mathf.Max(sm.Config.walkRadius, 3f);
        if (NavMesh.SamplePosition(randomPoint, out var hit, sampleRadius, NavMesh.AllAreas))
        {
            _currentTarget = hit.position;
            _hasReachedDestination = false;
            _arrivalFrameCount = 0;
            sm.Agent.isStopped = false;
            sm.SetDestinationSafe(_currentTarget);
            sm.animator?.CrossFade("ZombieFemale_Walk01Forward", 0.1f);
        }
        else
        {
            SetNewRandomTarget(sm, attempts + 1);
        }
    }
}