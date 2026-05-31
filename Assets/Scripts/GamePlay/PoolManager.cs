using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [Header("Pool Settings")]
    [SerializeField] private int defaultPreWarmSize = 10;
    [SerializeField] private Transform poolRoot; // Para instancias dinámicas

    [Header("Custom Pool (Scene Placed)")]
    [Tooltip("Si se activa, escanea el contenedor y registra los enemigos como ACTIVOS. NO los desactiva.")]
    [SerializeField] private bool useCustomPool = false;

    [ShowIf("@useCustomPool")]
    [Tooltip("Padre que contiene los enemigos pre-colocados en posiciones fijas.")]
    [SerializeField] private Transform customPoolContainer;

    [ShowIf("@useCustomPool"), ReadOnly]
    public int PrePlacedCount { get; private set; }

    // 🔹 Cola de enemigos INACTIVOS (listos para ser spawneados)
    private readonly Dictionary<EnemyConfig, Queue<EnemyStateMachine>> _inactivePools = new();

    // 🔹 Conjunto de enemigos ACTIVOS (incluye pre-colocados + spawneados)
    private readonly HashSet<EnemyStateMachine> _activeEnemies = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (poolRoot == null) poolRoot = new GameObject("EnemyPoolRoot").transform;
    }

    private void Start()
    {
        if (useCustomPool)
            InitializeCustomPool();
    }

    private void InitializeCustomPool()
    {
        if (customPoolContainer == null) return;

        var enemies = customPoolContainer.GetComponentsInChildren<EnemyStateMachine>(includeInactive: true);
        PrePlacedCount = enemies.Length;

        foreach (var enemy in enemies)
        {
            if (enemy.Config == null)
            {
                Debug.LogWarning($"[PoolManager] '{enemy.name}' sin EnemyConfig. Se omite.");
                continue;
            }
            // ✅ Solo registrar. NO desactivar. NO asignar config.
            RegisterActive(enemy);
        }
        Debug.Log($"[PoolManager] ✅ {PrePlacedCount} enemigos pre-colocados registrados como ACTIVOS.");
    }

    public EnemyStateMachine GetEnemy(EnemyConfig config, Vector3 pos, Quaternion rot)
    {
        if (config == null || config.enemyPrefab == null) return null;

        // Si no hay cola para este tipo, crearla (fallback/dinámico)
        if (!_inactivePools.ContainsKey(config))
            WarmPool(config, defaultPreWarmSize);

        // Saca de la cola inactiva o crea uno nuevo si se vació
        EnemyStateMachine enemy = _inactivePools[config].Count > 0
            ? _inactivePools[config].Dequeue()
            : CreateInstance(config);

        enemy.transform.SetPositionAndRotation(pos, rot);
        enemy.gameObject.SetActive(true); // 🔁 Dispara OnEnable → RegisterActive
        return enemy;
    }

    public void ReturnToPool(EnemyStateMachine enemy)
    {
        if (enemy == null || enemy.Config == null) return;

        // 🔁 Desactivar y pasar a cola inactiva (válido para pre-colocados y spawneados)
        enemy.gameObject.SetActive(false);

        if (!_inactivePools.ContainsKey(enemy.Config))
            _inactivePools[enemy.Config] = new Queue<EnemyStateMachine>();

        _inactivePools[enemy.Config].Enqueue(enemy);
    }

    public void RegisterActive(EnemyStateMachine enemy)
    {
        if (enemy != null) _activeEnemies.Add(enemy);
    }

    public void UnregisterActive(EnemyStateMachine enemy)
    {
        if (enemy != null) _activeEnemies.Remove(enemy);
    }

    public void WarmPool(EnemyConfig config, int count)
    {
        if (config == null) return;
        _inactivePools[config] = new Queue<EnemyStateMachine>(count);
        for (int i = 0; i < count; i++)
        {
            var e = CreateInstance(config);
            e.gameObject.SetActive(false);
            _inactivePools[config].Enqueue(e);
        }
    }

    private EnemyStateMachine CreateInstance(EnemyConfig config)
    {
        GameObject obj = Instantiate(config.enemyPrefab, poolRoot);
        var sm = obj.GetComponent<EnemyStateMachine>();
        if (sm == null) { Debug.LogError("Prefab sin EnemyStateMachine"); Destroy(obj); return null; }
        return sm;
    }

    public IReadOnlyCollection<EnemyStateMachine> ActiveEnemies => _activeEnemies;
    public int ActiveCount => _activeEnemies.Count;
}