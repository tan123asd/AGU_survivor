using UnityEngine;

/// <summary>
/// VŨ KHÍ 2: LIGHTNING - Giảm cooldown nhiều hơn, tấn công cực nhanh
/// Hiệu ứng: Mỗi level giảm thêm 10% cooldown (thay vì 5% mặc định)
/// </summary>
public class LightningWeapon : ProjectileWeapon
{
    [Header("Lightning Special")]
    [SerializeField] private float extraCooldownReduction = 0.05f; // Extra 5% per level
    
    protected override void CalculateStats()
    {
        // Gọi base calculation first
        base.CalculateStats();
        
        // Lightning đặc biệt: Cooldown giảm NHIỀU HƠN
        float extraReduction = (weaponLevel - 1) * extraCooldownReduction;
        attackInterval *= (1 - extraReduction);
        
        if (debugMode)
        {
            Debug.Log($"⚡ Lightning bonus: Extra {extraReduction * 100}% cooldown reduction!");
        }
    }
    
    protected override void OnUpgrade()
    {
        Debug.Log($"⚡ LIGHTNING STRIKE! Level {weaponLevel} - ULTRA FAST ATTACK!");
        
        // Optional: Screen shake effect
        // CameraShake.Instance.Shake(0.2f, 0.1f);
        
        // Optional: Lightning particle around player
        // ParticleEffects.PlayAt(player.position, "lightning_aura");
    }
}
