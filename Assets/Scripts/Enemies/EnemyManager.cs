using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [SerializeField] private int poolSizePerType = 10;
    [SerializeField] private Transform poolRoot;

    private readonly Dictionary<EnemyConfig, Queue<EnemyStateMachine>> _pools = new();
    private readonly List<EnemyStateMachine> _activeEnemies = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (poolRoot == null) poolRoot = new GameObject("EnemyPool").transform;
    }

    public EnemyStateMachine GetEnemy(EnemyConfig config, Vector3 pos, Quaternion rot)
    {
        if (!_pools.ContainsKey(config)) InitPool(config, poolSizePerType);

        EnemyStateMachine enemy = _pools[config].Count > 0 
            ? _pools[config].Dequeue() 
            : CreateInstance(config);

        enemy.transform.SetPositionAndRotation(pos, rot);
        enemy.gameObject.SetActive(true);
        return enemy;
    }

    public void ReturnToPool(EnemyStateMachine enemy)
    {
        enemy.gameObject.SetActive(false);
        if (_pools.TryGetValue(enemy.Config, out var queue)) queue.Enqueue(enemy);
    }

    private EnemyStateMachine CreateInstance(EnemyConfig config)
    {
        GameObject obj = Instantiate(config.enemyPrefab, poolRoot);
        var sm = obj.GetComponent<EnemyStateMachine>();
        if (sm == null) Debug.LogError("Prefab sin EnemyStateMachine");
        sm.SetConfig(config);
        return sm;
    }

    private void InitPool(EnemyConfig config, int size)
    {
        _pools[config] = new Queue<EnemyStateMachine>(size);
        for (int i = 0; i < size; i++)
        {
            var enemy = CreateInstance(config);
            enemy.gameObject.SetActive(false);
            _pools[config].Enqueue(enemy);
        }
    }

    public void Register(EnemyStateMachine enemy)
    {
        if (enemy != null && !_activeEnemies.Contains(enemy)) _activeEnemies.Add(enemy);
    }

    public void Unregister(EnemyStateMachine enemy) => _activeEnemies.Remove(enemy);
    public IReadOnlyList<EnemyStateMachine> ActiveEnemies => _activeEnemies.AsReadOnly();
}