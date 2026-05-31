using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Configuración base del enemigo. Debe asignarse en el prefab.")]
    [SerializeField] private EnemyConfig config;

    public EnemyConfig Config => config;

    private void Awake()
    {
        if (config == null)
            Debug.LogWarning($"[{name}] 'EnemyConfig' no asignado. El enemigo no funcionará correctamente.");
    }
}