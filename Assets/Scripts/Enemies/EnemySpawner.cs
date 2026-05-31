using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

public class EnemySpawner : MonoBehaviour
{
    [Header("Global Limits")]
    [Range(1, 1000)]
    [Tooltip("Límite absoluto de enemigos en esta escena. Al alcanzarse, el spawner se detiene permanentemente.")]
    public int globalLimit = 10;

    [Header("Spawn Mode")]
    [Tooltip("ON: Spawneo aleatorio desde la lista. OFF: Reglas específicas por tipo con límite individual.")]
    public bool isRandomMode = false;

    [HideIf("@isRandomMode")]
    [Tooltip("Lista de enemigos con límite individual por tipo.")]
    public List<EnemySpawnRule> specificRules = new();

    [ShowIf("@isRandomMode")]
    [Tooltip("Lista de candidatos para spawneo aleatorio. Se ignora el límite individual, pero se respeta el global.")]
    public List<EnemyConfig> randomPool = new();

    [Header("Scene Setup")]
    public Transform[] spawnPoints;
    public Transform playerTarget;

    [Header("Timing")]
    public float waveInterval = 5f;
    public float spawnStagger = 0.5f;

    // 🔒 Runtime tracking (no serializado)
    private Dictionary<EnemyConfig, int> _spawnedCounts = new();
    private int _totalSpawned = 0;
    private Coroutine _spawnRoutine;

    [System.Serializable]
    public class EnemySpawnRule
    {
        public EnemyConfig config;
        [Range(1, 100), LabelText("Max Count")]
        public int maxCount = 1;
    }

    private void Start() => _spawnRoutine = StartCoroutine(SpawnLoop());

    private IEnumerator SpawnLoop()
    {
        while (_totalSpawned < globalLimit)
        {
            EnemyConfig next = GetNextConfig();
            if (next == null) break; // Todos los límites alcanzados

            TrySpawn(next);
            yield return new WaitForSeconds(spawnStagger);
        }
        Debug.Log($"[Spawner] ✅ Límite global ({globalLimit}) alcanzado. Spawner detenido.");
    }

    private EnemyConfig GetNextConfig()
    {
        if (_totalSpawned >= globalLimit) return null;

        if (isRandomMode)
        {
            return randomPool.Count > 0 ? randomPool[Random.Range(0, randomPool.Count)] : null;
        }

        // Modo específico: elige aleatoriamente SOLO entre los que aún tienen cupo
        var available = specificRules.Where(r =>
            (_spawnedCounts.TryGetValue(r.config, out int c) ? c : 0) < r.maxCount
        ).ToList();

        return available.Count > 0 ? available[Random.Range(0, available.Count)].config : null;
    }

    private void TrySpawn(EnemyConfig config)
    {
        if (spawnPoints.Length == 0 || playerTarget == null) return;

        Transform pt = spawnPoints[Random.Range(0, spawnPoints.Length)];
        var enemy = PoolManager.Instance.GetEnemy(config, pt.position, pt.rotation);

        if (enemy != null)
        {
            enemy.Player = playerTarget;
            enemy.ChangeState(EnemyState.Chase); // ✅ Fuerza persecución inmediata
            Debug.Log($"[Spawner] 🔴 {enemy.name} → Spawneado y forzado a CHASE.");

            _totalSpawned++;
            _spawnedCounts[config] = (_spawnedCounts.TryGetValue(config, out int c) ? c : 0) + 1;
        }
    }

    // 🔧 Utilidades externas
    public void ForceStop() => StopCoroutine(_spawnRoutine);
    public void ResetSpawner()
    {
        StopCoroutine(_spawnRoutine);
        _totalSpawned = 0;
        _spawnedCounts.Clear();
        _spawnRoutine = StartCoroutine(SpawnLoop());
    }
}