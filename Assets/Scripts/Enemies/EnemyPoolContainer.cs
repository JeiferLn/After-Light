using UnityEngine;

public class EnemyPoolContainer : MonoBehaviour
{
    [SerializeField] private bool forceRegisterOnStart = true;

    private void Start() => InitializeChildren();

    private void InitializeChildren()
    {
        var enemies = GetComponentsInChildren<EnemyStateMachine>(includeInactive: false);
        Debug.Log($"[PoolContainer] 📦 Escaneando {enemies.Length} hijos...");

        foreach (var enemy in enemies)
        {
            if (enemy.Config == null)
            {
                Debug.LogWarning($"[PoolContainer] ⚠️ {enemy.name} sin Config. Omitido.");
                continue;
            }

            if (forceRegisterOnStart)
                PoolManager.Instance?.RegisterActive(enemy);

            // ✅ Elige aleatoriamente entre Idle y WalkRandom
            EnemyState initialState = Random.value > 0.5f
                ? EnemyState.Idle
                : EnemyState.WalkRandom;

            enemy.ChangeState(initialState);
            Debug.Log($"[PoolContainer]  {enemy.name} → Estado inicial: {initialState}");
        }
    }
}