using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private int baseNumberOfProjectiles = 1;

    private Dictionary<string, List<IStatModifier>> statModifiers = new();

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
        }
    }

    public void AddModifier(string statKey, IStatModifier modifier)
    {
        if (!statModifiers.ContainsKey(statKey))
        {
            statModifiers[statKey] = new List<IStatModifier>();
        }
        statModifiers[statKey].Add(modifier);

        statModifiers[statKey].Sort((a, b) => a.Order.CompareTo(b.Order));
    }

    public float GetStat(string statKey, float baseValue)
    {
        if (!statModifiers.TryGetValue(statKey, out var mods) || mods.Count == 0)
        {
            return baseValue;
        }
        float finalValue = baseValue;
        foreach (var mod in mods)
        {

            finalValue = mod.Apply(finalValue);
        }

        Debug.Log($"Stat {statKey} calculated: {finalValue}");

        return finalValue;
    }

    public float moveSpeed => GetStat("baseMoveSpeed", baseMoveSpeed);
    public float damage => GetStat("baseDamage", baseDamage);
    public int numberOfProjectiles => (int)GetStat("baseNumberOfProjectiles", baseNumberOfProjectiles);
}
