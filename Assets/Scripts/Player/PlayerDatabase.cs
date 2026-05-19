using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDatabase : MonoBehaviour
{
    public static PlayerDatabase Instance { get; private set; }

    [SerializeField] private CharacterStats baseStats;

    public float CurrentHealth { get; private set; }
    public float CurrentCrystalCharge { get; private set; }
    public float MaxHealth => baseStats != null ? baseStats.maxHealth : 100f;
    public float MaxCrystalCharge => baseStats != null ? baseStats.maxCrystalCharge : 100f;

    public ItemData EquippedConsumable { get; private set; }

    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnCrystalChargeChanged;
    public event Action<ItemData> OnEquippedConsumableChanged;

    private InventoryUI cachedInventory;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        CurrentHealth = MaxHealth;
        CurrentCrystalCharge = 0f;

        Debug.Log($"[PlayerDatabase] Inicializado — Health: {CurrentHealth}/{MaxHealth} | Cristal: {CurrentCrystalCharge}/{MaxCrystalCharge}");
        Debug.Log("[PlayerDatabase] Consumible equipado: ninguno");
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;
        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        Debug.Log($"[PlayerDatabase] Daño recibido: {amount} | Health: {CurrentHealth}/{MaxHealth}");
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void Heal(float amount)
    {
        if (amount <= 0f) return;
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        Debug.Log($"[PlayerDatabase] Curado: {amount} | Health: {CurrentHealth}/{MaxHealth}");
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void DrainCrystalCharge(float amount)
    {
        if (amount <= 0f) return;
        float previous = CurrentCrystalCharge;
        CurrentCrystalCharge = Mathf.Max(0f, CurrentCrystalCharge - amount);

        if (Mathf.FloorToInt(previous) != Mathf.FloorToInt(CurrentCrystalCharge))
            Debug.Log($"[PlayerDatabase] Cristal drenando — Carga: {CurrentCrystalCharge:F1}/{MaxCrystalCharge}");

        OnCrystalChargeChanged?.Invoke(CurrentCrystalCharge, MaxCrystalCharge);
    }

    public void AddCrystalCharge(float amount)
    {
        if (amount <= 0f) return;
        CurrentCrystalCharge = Mathf.Min(MaxCrystalCharge, CurrentCrystalCharge + amount);
        Debug.Log($"[PlayerDatabase] Cristal recargado: +{amount} | Carga: {CurrentCrystalCharge}/{MaxCrystalCharge}");
        OnCrystalChargeChanged?.Invoke(CurrentCrystalCharge, MaxCrystalCharge);
    }

    public void OnInventoryItemAdded(ItemData item)
    {
        if (item == null) return;
        if (EquippedConsumable == null && item.itemType == ItemType.Crystal)
            SetEquippedConsumable(item, "auto-equip al recoger primer cristal");
    }

    public void OnInventoryItemConsumed(ItemData item)
    {
        if (item == null || item != EquippedConsumable) return;

        InventoryUI inv = ResolveInventory();
        if (inv == null) return;

        if (inv.CountItem(EquippedConsumable) > 0) return;

        List<ItemData> crystals = inv.GetItemsByType(ItemType.Crystal);
        if (crystals.Count == 0)
        {
            SetEquippedConsumable(null, "sin existencias y sin alternativas");
            return;
        }

        SetEquippedConsumable(crystals[0], $"auto-ciclo (se acabó {item.itemName})");
    }

    public void CycleEquippedConsumable()
    {
        InventoryUI inv = ResolveInventory();
        if (inv == null)
        {
            Debug.LogWarning("[PlayerDatabase] InventoryUI no encontrado al ciclar.");
            return;
        }

        List<ItemData> crystals = inv.GetItemsByType(ItemType.Crystal);
        if (crystals.Count == 0)
        {
            SetEquippedConsumable(null, "no hay cristales en el inventario");
            return;
        }

        int currentIdx = EquippedConsumable != null ? crystals.IndexOf(EquippedConsumable) : -1;
        int nextIdx = (currentIdx + 1) % crystals.Count;
        SetEquippedConsumable(crystals[nextIdx], "ciclo manual");
    }

    private void SetEquippedConsumable(ItemData item, string reason)
    {
        EquippedConsumable = item;

        InventoryUI inv = ResolveInventory();
        int count = (item != null && inv != null) ? inv.CountItem(item) : 0;
        string name = item != null ? item.itemName : "ninguno";

        Debug.Log($"[PlayerDatabase] Equipado: {name} (x{count} en inventario) — {reason}");
        OnEquippedConsumableChanged?.Invoke(item);
    }

    private InventoryUI ResolveInventory()
    {
        if (cachedInventory == null)
            cachedInventory = FindFirstObjectByType<InventoryUI>();
        return cachedInventory;
    }
}
