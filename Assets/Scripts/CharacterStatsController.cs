using UnityEngine;

public class CharacterStatsController : MonoBehaviour
{
    [SerializeField] private CharacterStats baseStats;

    private float currentHealth;

    void Start()
    {
        currentHealth = baseStats.maxHealth;
    }
}
