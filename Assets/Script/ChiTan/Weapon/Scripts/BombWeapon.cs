using UnityEngine;
using System.Collections;

/// <summary>
/// VŨ KHÍ 4: BOMB - Spawn AOE explosion, damage nhiều enemies
/// Hiệu ứng: Level cao → AOE lớn hơn + thời gian stun
/// </summary>
public class BombWeapon : Weapon
{
    [Header("Bomb Settings")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float stunDuration = 0.5f;
    
    protected override void CalculateStats()
    {
        base.CalculateStats();
        
        // Bomb đặc biệt: AOE radius tăng theo level
        explosionRadius = baseRange * (1 + (weaponLevel - 1) * 0.15f); // +15% per level
    }
    
    protected override void OnUpgrade()
    {
        Debug.Log($"💣 BOMB UPGRADED! Level {weaponLevel} - BIGGER EXPLOSION (Radius: {explosionRadius})");
        
        // Bonus effect: Level 3+ có stun effect
        if (weaponLevel >= 3)
        {
            stunDuration = 0.5f + (weaponLevel - 3) * 0.2f;
            Debug.Log($"💥 Stun effect: {stunDuration}s!");
        }
        
        // Optional: Test explosion at player position
        // SpawnExplosion(player.position);
    }
    
    protected override void Attack()
    {
        GameObject target = FindNearestEnemy();
        if (target == null) return;
        
        // Spawn bomb at enemy position
        SpawnExplosion(target.transform.position);
    }
    
    private void SpawnExplosion(Vector3 position)
    {
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
            
            // Damage tất cả enemies trong AOE
            Collider2D[] hits = Physics2D.OverlapCircleAll(position, explosionRadius);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    IDamageable enemy = hit.GetComponent<IDamageable>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(damage);
                        
                        // Stun effect (nếu level đủ cao)
                        if (weaponLevel >= 3)
                        {
                            Enemy enemyScript = hit.GetComponent<Enemy>();
                            if (enemyScript != null)
                            {
                                // enemyScript.Stun(stunDuration);
                            }
                        }
                    }
                }
            }
            
            Destroy(explosion, 1f);
        }
    }
}
