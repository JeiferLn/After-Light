using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Transform playerTransform;
    public Transform PlayerTransform => playerTransform;
    public Vector3 PlayerPosition => playerTransform != null ? playerTransform.position : Vector3.zero;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        Debug.Log($"[GameManager] 🟢 Instancia creada. PlayerTransform asignada: {playerTransform != null}");
        if (playerTransform == null)
            Debug.LogError("[GameManager] ⛔ PlayerTransform NO está asignada en Inspector!");
    }
}