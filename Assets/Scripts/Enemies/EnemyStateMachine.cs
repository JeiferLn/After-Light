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
        RegisterStates(); // Llenar diccionario con instancias de estados
    }

    private void OnEnable()
    {
        EnemyManager.Instance?.Register(this);
        // Pequeño delay para que NavMeshAgent esté listo tras activar desde pool
        StartCoroutine(InitializeAsync());
    }

    private void OnDisable()
    {
        EnemyManager.Instance?.Unregister(this);
        _isInitialized = false;
    }

    private IEnumerator InitializeAsync()
    {
        yield return null; // Esperar 1 frame para setup interno de Unity
        
        // Validar que estamos sobre NavMesh
        if (!Agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out var hit, 2f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                Agent.Warp(hit.position);
            }
            else
            {
                Debug.LogWarning($"[{name}] No está sobre NavMesh. Posición: {transform.position}");
                yield break;
            }
        }
        
        _isInitialized = true;
        ChangeState(EnemyState.Idle); // Estado inicial por defecto
    }

    private void Update()
    {
        // Guardrails: no actualizar si no está listo o le faltan referencias
        if (!_isInitialized || Player == null || Config == null || CurrentState == null) return;

        UpdateDetection();
        CurrentState.Update(this, Time.deltaTime); // Delegar lógica al estado actual
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
        // _states[EnemyState.Attack] = new AttackState();
        // _states[EnemyState.Flee] = new FleeState();
        // ... etc
    }

    public void ChangeState(EnemyState newState)
    {
        if (CurrentState != null)
            CurrentState.Exit(this); // Hook de salida del estado anterior

        if (_states.TryGetValue(newState, out var state))
        {
            CurrentState = state;
            CurrentStateEnum = newState; // Para debug/UI
            CurrentState.Enter(this);    // Hook de entrada del nuevo estado
            Debug.Log($"[{name}] → {newState}");
        }
        else
        {
            Debug.LogError($"Estado {newState} no registrado en {name}. Revisa RegisterStates().");
        }
    }

    // === CONFIGURACIÓN INICIAL (inyectada desde fuera) ===
    public void SetConfig(EnemyConfig config)
    {
        Config = config;
        // Configurar agente con valores del SO
        if (Agent != null)
        {
            Agent.speed = config.moveSpeed;
            Agent.acceleration = 8f;
            Agent.angularSpeed = 90f;
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