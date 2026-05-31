using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;

public class EnemyStateMachine : MonoBehaviour
{
    // Estado actual (público para debug/UI)
    public EnemyState CurrentStateEnum { get; private set; } = EnemyState.Idle;

    // Referencias externas
    public IEnemyState CurrentState { get; private set; }
    public EnemyConfig Config { get; private set; }
    public NavMeshAgent Agent { get; private set; }
    public Transform Player { get; set; }
    public bool IsDetected { get; private set; }

    // Diccionario de estados: Enum → Lógica del estado
    private readonly Dictionary<EnemyState, IEnemyState> _states = new();

    // Flags de inicialización
    private bool _isInitialized;
    private bool _isInTrigger;

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
        Agent.speed = Config.moveSpeed;
        Agent.acceleration = 20f;
        Agent.angularSpeed = 200f;

        Debug.Log($"[SM] 🟡 {name} despierto. States registrados: {_states.Count}");
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
        yield return null; // Espera 1 frame para que NavMesh/Physics terminen setup

        if (!Agent.isOnNavMesh)
        {
            Debug.LogWarning($"[{name}] No está sobre NavMesh al iniciar. Corrigiendo posición...");
            if (NavMesh.SamplePosition(transform.position, out var hit, 2f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                Agent.Warp(hit.position);
            }
        }

        _isInitialized = true; // ✅ Siempre true, no bloquea el loop
        Debug.Log($"[SM] 🟢 {name} inicializado. Update loop activo.");
    }

    private void Update()
    {
        if (!_isInitialized) return;

        // 🔍 DIAGNÓSTICO DETALLADO
        if (GameManager.Instance == null)
        {
            Debug.LogWarning($"[{name}] ⚠️ GameManager.Instance es NULL. Esperando...");
            return;
        }

        if (GameManager.Instance.PlayerTransform == null)
        {
            Debug.LogWarning($"[{name}] ⚠️ GameManager.PlayerTransform es NULL. ¿Está asignado en Inspector?");
            return;
        }

        // Auto-asignar si no se inyectó manualmente
        if (Player == null)
            Player = GameManager.Instance.PlayerTransform;

        if (Player == null)
        {
            Debug.LogWarning($"[{name}] ⛔ Player sigue siendo null tras auto-asignación.");
            return;
        }

        if (Config == null)
        {
            Debug.LogError($"[{name}] ⛔ Config es null.");
            return;
        }
        if (CurrentState == null)
        {
            Debug.LogError($"[{name}] ⛔ CurrentState es null.");
            return;
        }

        UpdateDetection();
        CurrentState.Update(this, Time.deltaTime);
    }

    // === DETECCIÓN HÍBRIDA (Trigger + OverlapSphere) ===
    private void UpdateDetection()
    {
        if (Config == null) return;

        bool sphereDetected = false;
        if (Config.useOverlapSphere)
        {
            sphereDetected = Physics.CheckSphere(
                transform.position,
                Config.detectionRadius,
                Config.targetLayer
            );
        }
        IsDetected = _isInTrigger || sphereDetected;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Config == null) return;
        if ((Config.targetLayer.value == -1 || (Config.targetLayer & (1 << other.gameObject.layer)) != 0)
            || other.CompareTag("Player"))
        {
            _isInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (Config == null) return;
        if ((Config.targetLayer.value == -1 || (Config.targetLayer & (1 << other.gameObject.layer)) != 0)
            || other.CompareTag("Player"))
        {
            _isInTrigger = false;
        }
    }

    // === GESTIÓN DE ESTADOS (State Pattern) ===
    private void RegisterStates()
    {
        // Registrar cada estado concreto. Añade más aquí según necesites.
        _states[EnemyState.Idle] = new IdleState();
        _states[EnemyState.WalkRandom] = new WalkRandomState();
        _states[EnemyState.Chase] = new ChaseState();
        _states[EnemyState.Attack] = new AttackState();
        // _states[EnemyState.Flee] = new FleeState();
        // ... etc
    }

    public void ChangeState(EnemyState newState)
    {
        Debug.Log($"[SM] 🔄 {name} → Solicitando estado: {newState} (Actual: {CurrentStateEnum})");

        if (CurrentState != null) CurrentState.Exit(this);

        if (_states.TryGetValue(newState, out var state))
        {
            CurrentState = state;
            CurrentStateEnum = newState;
            CurrentState.Enter(this);
            Debug.Log($"[SM] ✅ {name} → Estado activo confirmado: {newState}");
        }
        else
        {
            Debug.LogError($"[SM] ❌ {name} → Estado '{newState}' NO registrado en RegisterStates().");
        }
    }

    // === UTILIDADES PARA LOS ESTADOS ===
    public void SetDestinationSafe(Vector3 target)
    {
        if (!Agent.isOnNavMesh) return;
        if (NavMesh.SamplePosition(target, out var hit, 1f, NavMesh.AllAreas))
            Agent.SetDestination(hit.position);
    }
}