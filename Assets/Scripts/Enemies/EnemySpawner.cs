using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyConfig config;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int enemiesPerWave = 3;
    [SerializeField] private float waveInterval = 5f;
    [SerializeField] private float spawnStagger = 0.5f;
    [SerializeField] private Transform playerTarget; // Asignar manualmente o por GameManager

    private bool _isSpawning;

    private void Start() => StartCoroutine(SpawnLoop());

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return SpawnWave();
            yield return new WaitForSeconds(waveInterval);
        }
    }

    private IEnumerator SpawnWave()
    {
        _isSpawning = true;
        for (int i = 0; i < enemiesPerWave; i++)
        {
            if (spawnPoints.Length == 0) break;
            Transform pt = spawnPoints[Random.Range(0, spawnPoints.Length)];
            
            var enemy = EnemyManager.Instance.GetEnemy(config, pt.position, pt.rotation);
            enemy.Player = playerTarget; // Inyección de referencia

            yield return new WaitForSeconds(spawnStagger);
        }
        _isSpawning = false;
    }

    public void TriggerWave() => StartCoroutine(SpawnWave());
}