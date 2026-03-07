using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Database quản lý tất cả weapon configs
/// Tạo: Right Click → Create → Weapons → Weapon Database
/// </summary>
[CreateAssetMenu(fileName = "WeaponDatabase", menuName = "Weapons/Weapon Database", order = 0)]
public class WeaponDatabase : ScriptableObject
{
    [Header("All Weapons")]
    [Tooltip("Kéo tất cả WeaponData vào đây")]
    public List<WeaponData> allWeapons = new List<WeaponData>();
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    // Singleton pattern
    private static WeaponDatabase instance;
    public static WeaponDatabase Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<WeaponDatabase>("WeaponDatabase");
                if (instance == null)
                {
                    Debug.LogError("WeaponDatabase not found in Resources folder! Create one and place in Assets/Resources/");
                }
            }
            return instance;
        }
    }
    
    /// <summary>
    /// Tìm weapon theo ID
    /// </summary>
    public WeaponData GetWeaponById(string weaponId)
    {
        WeaponData weapon = allWeapons.FirstOrDefault(w => w.weaponId == weaponId);
        
        if (weapon == null && showDebugLogs)
        {
            Debug.LogWarning($"Weapon '{weaponId}' not found in database!");
        }
        
        return weapon;
    }
    
    /// <summary>
    /// Tìm weapon theo tên
    /// </summary>
    public WeaponData GetWeaponByName(string displayName)
    {
        return allWeapons.FirstOrDefault(w => w.displayName == displayName);
    }
    
    /// <summary>
    /// Lấy tất cả weapons có thể unlock ở player level hiện tại
    /// </summary>
    public List<WeaponData> GetAvailableWeapons(int playerLevel, List<string> ownedWeaponIds)
    {
        return allWeapons.Where(w => 
            w.minPlayerLevel <= playerLevel &&
            !ownedWeaponIds.Contains(w.weaponId) &&
            (w.requiredWeapon == null || ownedWeaponIds.Contains(w.requiredWeapon.weaponId))
        ).ToList();
    }
    
    /// <summary>
    /// Lấy tất cả weapons player đang sở hữu và có thể upgrade
    /// </summary>
    public List<WeaponData> GetUpgradeableWeapons(List<string> ownedWeaponIds, Dictionary<string, int> weaponLevels)
    {
        List<WeaponData> upgradeableWeapons = new List<WeaponData>();
        
        foreach (var weaponId in ownedWeaponIds)
        {
            WeaponData weapon = GetWeaponById(weaponId);
            if (weapon != null)
            {
                int currentLevel = weaponLevels.ContainsKey(weaponId) ? weaponLevels[weaponId] : 1;
                if (currentLevel < weapon.maxLevel)
                {
                    upgradeableWeapons.Add(weapon);
                }
            }
        }
        
        return upgradeableWeapons;
    }
    
    /// <summary>
    /// Validate database (gọi từ Unity Editor)
    /// </summary>
    private void OnValidate()
    {
        // Check duplicate IDs
        var duplicates = allWeapons.GroupBy(w => w.weaponId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);
        
        foreach (var duplicateId in duplicates)
        {
            Debug.LogError($"Duplicate weapon ID: '{duplicateId}' in WeaponDatabase!");
        }
        
        // Check missing prefabs
        foreach (var weapon in allWeapons)
        {
            if (weapon.weaponPrefab == null)
            {
                Debug.LogWarning($"WeaponData '{weapon.displayName}' has no prefab assigned!");
            }
        }
    }
    
    /// <summary>
    /// Debug: Print all weapons
    /// </summary>
    [ContextMenu("Print All Weapons")]
    public void PrintAllWeapons()
    {
        Debug.Log($"=== WeaponDatabase contains {allWeapons.Count} weapons ===");
        foreach (var weapon in allWeapons)
        {
            Debug.Log($"[{weapon.weaponId}] {weapon.displayName} - Damage: {weapon.baseDamage}, Cooldown: {weapon.baseAttackInterval}s");
        }
    }
}
