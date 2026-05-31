using UnityEngine;
using UnityEngine.AI;

public class ChaseState : EnemyStateBase
{
    private float _pathUpdateTimer;
    private float _flankTimer;
    private Vector3 _lastPlayerPos;

    // Bloqueo / Espera
    private float _stuckTimer;
    private bool _isBlocked;
    private const float STUCK_THRESHOLD = 1.2f;
    private const float VELOCITY_EPSILON = 0.05f;

    public override void Enter(EnemyStateMachine sm)
    {
        _pathUpdateTimer = 0f;
        _flankTimer = 0f;
        _stuckTimer = 0f;
        _isBlocked = false;

        sm.Agent.avoidancePriority = Random.Range(20, 90);
        sm.Agent.autoBraking = false;
        sm.Agent.isStopped = false;
        sm.Agent.speed = sm.Config.moveSpeed * 1.2f;

        UpdateDestination(sm);
    }

    public override void Update(EnemyStateMachine sm, float dt)
    {
        Vector3 playerPos = sm.Player != null
            ? sm.Player.position
            : GameManager.Instance.PlayerPosition;

        float distSqr = (sm.transform.position - playerPos).sqrMagnitude;
        float attackRangeSqr = sm.Config.attackRange * sm.Config.attackRange;

        // 1️⃣ Atacar — orientar al jugador ANTES de transicionar
        if (distSqr <= attackRangeSqr)
        {
            FacePlayer(sm, playerPos, dt, instant: true); // ← FIX: giro instantáneo
            Transition(sm, EnemyState.Attack);
            return;
        }

        // 2️⃣ Abandonar persecución
        float exitDistSqr = Mathf.Pow(
            sm.Config.detectionRadius * sm.Config.chaseExitMultiplier, 2);
        if (distSqr > exitDistSqr) { Transition(sm, EnemyState.Idle); return; }

        // 3️⃣ Detección de bloqueo
        bool tryingToMove = sm.Agent.hasPath
            && sm.Agent.remainingDistance > sm.Agent.stoppingDistance;
        bool actuallyMoving = sm.Agent.velocity.sqrMagnitude > VELOCITY_EPSILON;

        if (tryingToMove && !actuallyMoving) _stuckTimer += dt;
        else _stuckTimer = Mathf.Max(0f, _stuckTimer - dt * 0.5f); // ← FIX: decay suave

        if (_stuckTimer >= STUCK_THRESHOLD && !_isBlocked)
        {
            _isBlocked = true;
            sm.Agent.isStopped = true;
        }

        // 🛑 Comportamiento mientras está bloqueado
        if (_isBlocked)
        {
            FacePlayer(sm, playerPos, dt); // ← FIX: método reutilizable

            bool pathResolved = sm.Agent.pathStatus == NavMeshPathStatus.PathComplete;
            if (pathResolved || _stuckTimer >= 2.5f)
            {
                _isBlocked = false;
                _stuckTimer = 0f;
                sm.Agent.isStopped = false;
                UpdateDestination(sm);
            }
            return;
        }

        // 4️⃣ Movimiento + Flocking (actualizado cada 0.4s)
        _pathUpdateTimer += dt;
        if (_pathUpdateTimer >= 0.4f)
        {
            _pathUpdateTimer = 0f;
            bool playerMoved = Vector3.SqrMagnitude(playerPos - _lastPlayerPos) > 1f;
            _flankTimer += 0.4f;
            bool shouldReFlank = _flankTimer >= Random.Range(1f, 2.5f);

            if (playerMoved || shouldReFlank)
            {
                UpdateDestination(sm);
                _lastPlayerPos = playerPos;
                if (shouldReFlank) _flankTimer = 0f;
            }
        }
    }

    public override void Exit(EnemyStateMachine sm)
    {
        sm.Agent.isStopped = true;
        sm.Agent.autoBraking = true;
    }

    // ─── Orientación al jugador reutilizable ──────────────────────────────────
    private void FacePlayer(EnemyStateMachine sm, Vector3 playerPos,
                            float dt, bool instant = false)
    {
        Vector3 dir = (playerPos - sm.transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion target = Quaternion.LookRotation(dir.normalized);
        sm.transform.rotation = instant
            ? target
            : Quaternion.Slerp(sm.transform.rotation, target, dt * 6f);
    }

    // ─── Destino con Flocking ─────────────────────────────────────────────────
    private void UpdateDestination(EnemyStateMachine sm)
    {
        Vector3 playerPos = sm.Player != null
            ? sm.Player.position
            : GameManager.Instance.PlayerPosition;

        Vector3 flockOffset = CalculateFlocking(sm, playerPos);

        // FIX: limitar cuánto puede desviarse el destino del jugador
        flockOffset = Vector3.ClampMagnitude(flockOffset, sm.Config.attackRange * 0.8f);

        sm.SetDestinationSafe(playerPos + flockOffset);
    }

    // ─── Boids adaptado ──────────────────────────────────────────────────────
    private Vector3 CalculateFlocking(EnemyStateMachine sm, Vector3 playerPos)
    {
        // ← Ahora usa la lista precalculada por EnemyManager, no itera todo el pool
        var neighbors = sm.NearbyEnemies;
        if (neighbors.Count == 0) return Vector3.zero;

        Vector3 separation = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        Vector3 alignment = Vector3.zero;

        for (int i = 0; i < neighbors.Count; i++)
        {
            var other = neighbors[i];
            if (other == null || other.Agent == null) continue;

            Vector3 diff = sm.transform.position - other.transform.position;
            float distSqr = diff.sqrMagnitude;
            if (distSqr < 0.01f) continue;

            float dist = Mathf.Sqrt(distSqr);
            separation += (diff / dist) * (sm.Config.flockRadius / dist);
            cohesion += other.transform.position;
            alignment += other.Agent.velocity;
        }

        int n = neighbors.Count;
        cohesion = (cohesion / n) - sm.transform.position;
        alignment = alignment / n;

        Vector3 result = (separation * sm.Config.separationWeight) +
                         (cohesion * sm.Config.cohesionWeight) +
                         (alignment * sm.Config.alignmentWeight);

        return Vector3.ClampMagnitude(result, sm.Config.attackRange * 0.8f);
    }
}