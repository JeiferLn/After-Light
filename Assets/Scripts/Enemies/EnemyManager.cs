using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // Delegación directa a PoolManager
    public EnemyStateMachine GetEnemy(EnemyConfig config, Vector3 pos, Quaternion rot) => 
        PoolManager.Instance.GetEnemy(config, pos, rot);

    public void ReturnToPool(EnemyStateMachine enemy) => 
        PoolManager.Instance.ReturnToPool(enemy);

    public IReadOnlyCollection<EnemyStateMachine> ActiveEnemies => 
        PoolManager.Instance.ActiveEnemies;

    public int ActiveCount => PoolManager.Instance.ActiveCount;
}