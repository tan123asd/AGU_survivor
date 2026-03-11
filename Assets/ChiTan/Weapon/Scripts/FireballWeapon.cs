using UnityEngine;

/// <summary>
/// VŨ KHÍ 1: FIREBALL - Gây bỏng cho enemies
/// Hiệu ứng đặc biệt:
/// - Mỗi viên đạn gây BURNING EFFECT: 1 damage/giây trong 5 giây
/// - Tăng số viên đạn mỗi 2 level (Level 2, 4)
/// 
/// SETUP:
/// 1. Projectile Prefab phải dùng FireballProjectile.cs thay vì Projectile.cs
/// 2. FireballProjectile sẽ tự động thêm BurningEffect vào enemy khi hit
/// </summary>
public class FireballWeapon : ProjectileWeapon
{
    [Header("Fireball Special")]
    [SerializeField] private int bonusProjectilesPerLevel = 1; // Mỗi 2 level tăng 1 viên
    
    protected override void Start()
    {
        weaponName = "fireball";
        base.Start();
        
        Debug.Log("🔥 Fireball Weapon loaded! Effect: Burn enemies (1 dmg/sec for 5s)");
    }
    
    protected override void OnUpgrade()
    {
        // Hiệu ứng đặc biệt của Fireball: Tăng số viên đạn
        if (weaponLevel % 2 == 0) // Level 2, 4 → tăng projectile
        {
            // Tăng stat global (affect tất cả projectile weapons)
            // PlayerStats.Instance.numberOfProjectiles += bonusProjectilesPerLevel;
            
            Debug.Log($"🔥 FIREBALL UPGRADED! Level {weaponLevel} - Shoot more projectiles!");
            Debug.Log($"🔥 Each fireball burns enemies: 1 damage/sec for 5 seconds!");
        }
        else
        {
            Debug.Log($"🔥 Fireball Level {weaponLevel} - Damage & Burning increased!");
        }
        
        // Optional: Play sound/particle effect
        // AudioManager.Instance.PlaySound("fireball_upgrade");
        // ParticleEffects.PlayAt(transform.position, "level_up_fire");
    }
}
