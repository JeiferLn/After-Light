using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class EnemyStateMachine : MonoBehaviour
{
    public EnemyState CurrentStateEnum { get; private set; } = EnemyState.Idle;
    public IEnemyState CurrentState { get; private set; }
    public EnemyConfig Config { get; private set; }
    public NavMeshAgent Agent { get; private set; }
    public Transform Player { get; set; }
    public bool IsDetected { get; set; } // ← setter público: lo escribe EnemyManager

    private readonly Dictionary<EnemyState, IEnemyState> _states = new();
    private bool _isInitialized;

    // ─── Cache de vecinos para flocking — lo rellena EnemyManager ─────────────
    // Evita que cada enemigo itere ActiveEnemies por su cuenta (O(n²) → O(n))
    [System.NonSerialized] public List<EnemyStateMachine> NearbyEnemies = new();

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        var enemyComp = GetComponent<Enemy>();
        if (enemyComp == null || enemyComp.Config == null)
        {
            Debug.LogError($"[{name}] Falta componente 'Enemy' o 'EnemyConfig'.");
            enabled = false; return;
        }

        Config = enemyComp.Config;
        RegisterStates();
        Agent.speed        = Config.moveSpeed;
        Agent.acceleration = 20f;
        Agent.angularSpeed = 200f;
    }

    private void OnEnable()
    {
        StartCoroutine(InitializeAsync());
        PoolManager.Instance?.RegisterActive(this);
    }

    private void OnDisable()
    {
        PoolManager.Instance?.UnregisterActive(this);
        _isInitialized = false;
    }

    private IEnumerator InitializeAsync()
    {
        yield return null;

        if (!Agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out var hit, 2f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                Agent.Warp(hit.position);
            }
        }
        _isInitialized = true;
    }

    // ─── Update del engine DESACTIVADO ────────────────────────────────────────
    // EnemyManager llama ManagedUpdate() en su propio Update centralizado.
    private void Update() { }  // Vacío intencionalmente — o usa [DisableDomainReload]

    // ─── Llamado por EnemyManager cada frame ──────────────────────────────────
    public void ManagedUpdate(float dt)
    {
        if (!_isInitialized || CurrentState == null || Config == null) return;

        if (Player == null)
        {
            Player = GameManager.Instance?.PlayerTransform;
            if (Player == null) return;
        }

        CurrentState.Update(this, dt);
    }

    // ─── Estado ───────────────────────────────────────────────────────────────
    private void RegisterStates()
    {
        _states[EnemyState.Idle]       = new IdleState();
        _states[EnemyState.WalkRandom] = new WalkRandomState();
        _states[EnemyState.Chase]      = new ChaseState();
        _states[EnemyState.Attack]     = new AttackState();
    }

    public void ChangeState(EnemyState newState)
    {
        if (CurrentState != null) CurrentState.Exit(this);

        if (_states.TryGetValue(newState, out var state))
        {
            CurrentState = state;
            CurrentStateEnum = newState;
            CurrentState.Enter(this);
        }
        else
        {
            Debug.LogError($"[SM] Estado '{newState}' no registrado.");
        }
    }

    public void SetDestinationSafe(Vector3 target)
    {
        if (!Agent.isOnNavMesh) return;
        if (NavMesh.SamplePosition(target, out var hit, 1f, NavMesh.AllAreas))
            Agent.SetDestination(hit.position);
    }
}