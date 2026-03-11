using UnityEngine;

/// <summary>
/// ScriptableObject chứa config cho từng loại vũ khí
/// Tạo asset: Right Click → Create → Weapons → Weapon Data
/// </summary>
[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Weapon Data", order = 1)]
public class WeaponData : ScriptableObject
{
    [Header("Basic Info")]
    [Tooltip("ID unique của weapon (e.g., fireball, laser, sword)")]
    public string weaponId = "weapon_id";
    
    [Tooltip("Tên hiển thị (e.g., Fireball, Lightning Bolt)")]
    public string displayName = "New Weapon";
    
    [Tooltip("Mô tả weapon")]
    [TextArea(2, 4)]
    public string description = "Weapon description";
    
    [Tooltip("Icon hiển thị trong UI")]
    public Sprite icon;
    
    [Header("Weapon Prefab")]
    [Tooltip("Prefab weapon (phải có component Weapon)")]
    public GameObject weaponPrefab;
    
    [Header("Base Stats")]
    [Tooltip("Damage cơ bản")]
    public int baseDamage = 10;
    
    [Tooltip("Thời gian giữa các lần attack (giây)")]
    public float baseAttackInterval = 2f;
    
    [Tooltip("Phạm vi tấn công")]
    public float baseRange = 10f;
    
    [Header("Level Scaling")]
    [Tooltip("Level tối đa")]
    public int maxLevel = 5;
    
    [Tooltip("% tăng damage mỗi level (0.1 = 10%)")]
    public float damagePerLevel = 0.1f;
    
    [Tooltip("% giảm cooldown mỗi level (0.05 = 5%)")]
    public float cooldownReductionPerLevel = 0.05f;
    
    [Tooltip("% tăng range mỗi level (0.1 = 10%)")]
    public float rangePerLevel = 0.1f;
    
    [Header("Unlock Requirements")]
    [Tooltip("Level tối thiểu để unlock weapon này")]
    public int minPlayerLevel = 1;
    
    [Tooltip("Weapon cần có trước khi unlock (optional)")]
    public WeaponData requiredWeapon;
    
    /// <summary>
    /// Tính damage ở level cụ thể
    /// </summary>
    public int GetDamageAtLevel(int level)
    {
        float multiplier = 1 + (level - 1) * damagePerLevel;
        return Mathf.RoundToInt(baseDamage * multiplier);
    }
    
    /// <summary>
    /// Tính attack interval ở level cụ thể
    /// </summary>
    public float GetAttackIntervalAtLevel(int level)
    {
        float reduction = 1 - (level - 1) * cooldownReductionPerLevel;
        return baseAttackInterval * Mathf.Max(0.5f, reduction); // Min 50% cooldown
    }
    
    /// <summary>
    /// Tính range ở level cụ thể
    /// </summary>
    public float GetRangeAtLevel(int level)
    {
        float multiplier = 1 + (level - 1) * rangePerLevel;
        return baseRange * multiplier;
    }
    
    /// <summary>
    /// Validate config
    /// </summary>
    private void OnValidate()
    {
        // Auto format weaponId
        if (!string.IsNullOrEmpty(weaponId))
        {
            weaponId = weaponId.ToLower().Replace(" ", "_");
        }
        
        // Validate stats
        baseDamage = Mathf.Max(1, baseDamage);
        baseAttackInterval = Mathf.Max(0.1f, baseAttackInterval);
        baseRange = Mathf.Max(1f, baseRange);
        maxLevel = Mathf.Clamp(maxLevel, 1, 10);
    }
}
