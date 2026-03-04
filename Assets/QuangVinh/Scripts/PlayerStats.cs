using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    private static PlayerStats _instance;
    public static PlayerStats Instance
    {
        get
        {
            if (_instance == null)
            {
                // Try to find one already in the scene
                _instance = FindObjectOfType<PlayerStats>();

                // Still null? Create one automatically
                if (_instance == null)
                {
                    GameObject go = new GameObject("PlayerStats (Auto-Created)");
                    _instance = go.AddComponent<PlayerStats>();
                    DontDestroyOnLoad(go);
                    Debug.LogWarning("[PlayerStats] No PlayerStats found in scene — created one automatically. Consider adding it to your scene manually.");
                }
            }
            return _instance;
        }
    }

    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private int baseNumberOfProjectiles = 1;

    private Dictionary<string, List<IStatModifier>> statModifiers = new();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
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
