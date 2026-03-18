using UnityEngine;

/// <summary>
/// VŨ KHÍ 3: LASER - Tăng range cực lớn, damage tăng mạnh
/// Hiệu ứng: Mỗi level tăng 20% range và 15% damage (thay vì 10%)
/// </summary>
public class LaserWeapon : ProjectileWeapon
{
    [Header("Laser Special")]
    [SerializeField] private float bonusRangeMultiplier = 0.1f; // Extra 10% range per level
    [SerializeField] private float bonusDamageMultiplier = 0.05f; // Extra 5% damage per level
    
    protected override void Start()
    {
        weaponName = "laser";
        base.Start();
    }
    
    protected override void CalculateStats()
    {
        base.CalculateStats();
        
        // Laser đặc biệt: Range và Damage tăng NHIỀU HƠN
        float extraRangeBonus = (weaponLevel - 1) * bonusRangeMultiplier;
        float extraDamageBonus = (weaponLevel - 1) * bonusDamageMultiplier;
        
        range *= (1 + extraRangeBonus);
        damage = Mathf.RoundToInt(damage * (1 + extraDamageBonus));
        
        if (debugMode)
        {
            Debug.Log($"🔫 Laser bonus: +{extraRangeBonus*100}% range, +{extraDamageBonus*100}% damage!");
        }
    }
    
    protected override void OnUpgrade()
    {
        Debug.Log($"🔫 LASER UPGRADE! Level {weaponLevel} - LONG RANGE SNIPER!");
        
        // Optional: Visual effect - laser sight
        // ShowLaserSight(range);
    }
}
