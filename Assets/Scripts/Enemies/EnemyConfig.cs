using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Enemy/Config")]
public class EnemyConfig : ScriptableObject
{
    [Header("Movement")]
    public float idleDuration = 2f;
    public float walkDuration = 3f;
    public float walkRadius = 8f;
    public float moveSpeedWalk = 0.5f;
    public float moveSpeedChase = 3.5f;

    [Header("Vision / Detection")]
    [Tooltip("Layer para paredes/obstáculos que bloquean la visión")]
    public LayerMask obstacleLayer;

    [Tooltip("Ángulo de visión en grados (180 = mitad delantera, 360 = 360°)")]
    [Range(30f, 360f)]
    public float detectionAngle = 120f;

    [Tooltip("Altura del 'ojo' del enemigo para el raycast")]
    public float eyeHeightOffset = 1.2f;

    [Header("Movement Tactics")]
    [Tooltip("Radio alrededor del jugador para flanquear (evitar fila india). 0 = línea recta.")]
    public float flankRadius = 1f;

    [Header("Flocking / Group Behavior")]
    [Tooltip("Radio de detección de aliados para comportamiento grupal")]
    public float flockRadius = 3.5f;

    [Range(0, 5)]
    [Tooltip("Fuerza de repulsión entre aliados (evita solapamiento)")]
    public float separationWeight = 2.5f;

    [Range(0, 2)]
    [Tooltip("Fuerza de atracción al centro del grupo")]
    public float cohesionWeight = 0.6f;

    [Range(0, 2)]
    [Tooltip("Fuerza de alineación de dirección con aliados")]
    public float alignmentWeight = 0.8f;

    [Header("WalkRandom Behavior")]
    [Tooltip("Tiempo mínimo entre cambios de dirección (segundos)")]
    public float walkMinInterval = 20f;

    [Tooltip("Tiempo máximo entre cambios de dirección (segundos)")]
    public float walkMaxInterval = 40f;


    [Header("Detection")]
    public float detectionRadius = 10f;
    public float chaseExitMultiplier = 1.5f;
    public LayerMask targetLayer;
    public bool useOverlapSphere = true;

    [Header("Combat")]
    [Tooltip("Distancia a la que el enemigo deja de moverse y ataca")]
    public float attackRange = 1.5f;

    [Tooltip("Tiempo entre ataques (segundos)")]
    public float attackCooldown = 1.2f;

    [Tooltip("Daño base por golpe")]
    public int attackDamage = 10;

    [Header("Prefab")]
    public GameObject enemyPrefab;
}