using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Enemy/Config")]
public class EnemyConfig : ScriptableObject
{
    [Header("Movement")]
    public float idleDuration = 2f;
    public float walkDuration = 3f;
    public float walkRadius = 8f;
    public float moveSpeed = 3.5f;

    [Header("Detection")]
    public float detectionRadius = 10f;
    public float chaseExitMultiplier = 1.5f;
    public LayerMask targetLayer;
    public bool useOverlapSphere = true;

    [Header("Prefab")]
    public GameObject enemyPrefab;
}