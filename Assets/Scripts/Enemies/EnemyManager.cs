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

        // Altura aproximada del jugador (pecho/cabeza) para que el raycast no roce el suelo
        Vector3 playerEyePos = playerPos + Vector3.up * 1.5f;

        foreach (var sm in enemies)
        {
            if (sm == null || sm.Config == null) continue;

            // 1️⃣ Distancia (barato, filtro inicial)
            float distSqr = (sm.transform.position - playerPos).sqrMagnitude;
            float detRadSqr = sm.Config.detectionRadius * sm.Config.detectionRadius;

            if (distSqr > detRadSqr)
            {
                sm.IsDetected = false;
                continue;
            }

            // 2️⃣ Field of View (opcional pero recomendado)
            Vector3 dirToPlayer = (playerPos - sm.transform.position);
            dirToPlayer.y = 0f; // Ignorar desnivel para el ángulo
            if (dirToPlayer.sqrMagnitude > 0.01f &&
                Vector3.Angle(sm.transform.forward, dirToPlayer) > sm.Config.detectionAngle * 0.5f)
            {
                sm.IsDetected = false;
                continue;
            }

            // 3️⃣ Line of Sight (Raycast contra obstáculos)
            Vector3 enemyEyePos = sm.transform.position + Vector3.up * sm.Config.eyeHeightOffset;
            Vector3 rayDir = playerEyePos - enemyEyePos;
            float rayDist = rayDir.magnitude;

            // Debug visual (descomentar solo en editor para depurar)
            // Debug.DrawLine(enemyEyePos, playerEyePos, sm.IsDetected ? Color.green : Color.red, 0.1f);

            // Retorna TRUE si GOLPEA un obstáculo en el LayerMask
            bool hitsObstacle = Physics.Raycast(enemyEyePos, rayDir.normalized, rayDist, sm.Config.obstacleLayer);

            sm.IsDetected = !hitsObstacle;
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