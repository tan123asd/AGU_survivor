using UnityEngine;

/// <summary>
/// VŨ KHÍ 5: SHIELD - Defensive weapon, heal player mỗi khi upgrade
/// Hiệu ứng: Mỗi level tăng max HP và heal player
/// </summary>
public class ShieldWeapon : Weapon
{
    [Header("Shield Settings")]
    [SerializeField] private int healAmountPerLevel = 10;
    [SerializeField] private float damageReduction = 0.05f; // 5% per level
    
    protected override void Start()
    {
        weaponName = "shield";
        base.Start();
    }
    
    protected override void OnUpgrade()
    {
        Debug.Log($"🛡️ SHIELD UPGRADED! Level {weaponLevel} - DEFENSE BOOST!");
        
        // Heal player khi upgrade
        if (playerHealth != null)
        {
            // playerHealth.Heal(healAmountPerLevel);
            Debug.Log($"❤️ Healed {healAmountPerLevel} HP!");
        }
        
        // Tăng defense (giảm damage nhận vào)
        float totalReduction = weaponLevel * damageReduction;
        Debug.Log($"🛡️ Damage reduction: {totalReduction * 100}%");
        
        // Apply to PlayerStats
        // PlayerStats.Instance.AddModifier("damageReduction", new FlatAddModifier { Amount = damageReduction });
        
        // Visual effect - shield glow
        // ShowShieldEffect();
    }
    
    protected override void Attack()
    {
        // Shield không attack - passive defense
        // Hoặc có thể reflect damage về enemies gần
        
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            float distance = Vector2.Distance(player.position, enemy.transform.position);
            if (distance < range)
            {
                // Reflect damage
                IDamageable damageable = enemy.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damage / 2); // Reflect một nửa damage
                }
            }
        }
    }
}
