using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Único MonoBehaviour con Update() que gobierna todos los enemigos activos.
/// Centraliza: detección, cache de vecinos para flocking, y tick de estados.
/// </summary>
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    // ── Configuración ─────────────────────────────────────────────────────────
    [Header("Optimización")]
    [Tooltip("Cuántos enemigos se actualizan por frame. 0 = todos.")]
    [SerializeField] private int _batchSizePerFrame = 0;

    [Tooltip("Cada cuántos frames recalcular vecinos para flocking (1 = cada frame)")]
    [SerializeField] private int _flockingUpdateInterval = 3;

    // ── Estado interno ────────────────────────────────────────────────────────
    private int _flockingFrameCounter;
    private int _batchIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        IReadOnlyCollection<EnemyStateMachine> enemies = PoolManager.Instance?.ActiveEnemies;
        if (enemies == null || enemies.Count == 0) return;

        float dt = Time.deltaTime;

        UpdateDetectionBatch(enemies);

        _flockingFrameCounter++;
        if (_flockingFrameCounter >= _flockingUpdateInterval)
        {
            _flockingFrameCounter = 0;
            UpdateFlockingNeighborsBatch(enemies);
        }

        // Sin batching: foreach directo sobre IReadOnlyCollection
        foreach (var sm in enemies)
            sm?.ManagedUpdate(dt);
    }

    // ─── Detección centralizada ───────────────────────────────────────────────
    private void UpdateDetectionBatch(IReadOnlyCollection<EnemyStateMachine> enemies)
    {
        Vector3 playerPos = GameManager.Instance?.PlayerTransform != null
            ? GameManager.Instance.PlayerTransform.position
            : GameManager.Instance.PlayerPosition;

        foreach (var sm in enemies)
        {
            if (sm == null || sm.Config == null) continue;

            float detRadSqr = sm.Config.detectionRadius * sm.Config.detectionRadius;
            float distSqr = (sm.transform.position - playerPos).sqrMagnitude;
            sm.IsDetected = distSqr <= detRadSqr;
        }
    }


    private EnemyStateMachine[] _flockingBuffer = new EnemyStateMachine[64];

    private void UpdateFlockingNeighborsBatch(IReadOnlyCollection<EnemyStateMachine> enemies)
    {
        // Limpiar listas previas
        foreach (var sm in enemies)
            sm?.NearbyEnemies.Clear();

        // Copiar a buffer cacheado sin alocación (crece solo si hace falta)
        int count = enemies.Count;
        if (_flockingBuffer.Length < count)
            _flockingBuffer = new EnemyStateMachine[count * 2];

        int idx = 0;
        foreach (var sm in enemies)
            _flockingBuffer[idx++] = sm;

        // O(n²) con simetría
        for (int i = 0; i < count; i++)
        {
            var a = _flockingBuffer[i];
            if (a == null) continue;

            float radiusSqr = a.Config.flockRadius * a.Config.flockRadius;

            for (int j = i + 1; j < count; j++)
            {
                var b = _flockingBuffer[j];
                if (b == null) continue;

                float distSqr = (a.transform.position - b.transform.position).sqrMagnitude;
                if (distSqr < radiusSqr)
                {
                    a.NearbyEnemies.Add(b);
                    b.NearbyEnemies.Add(a);
                }
            }
        }
    }
}