using UnityEngine;

/// <summary>
/// VŨ KHÍ 4: BOMB - Spawn AOE explosion, damage nhiều enemies
/// Hiệu ứng: Level cao → AOE lớn hơn + thời gian stun
/// </summary>
public class BombWeapon : Weapon
{
    [Header("Bomb Settings")]
    [SerializeField] private GameObject bombProjectilePrefab;
    [SerializeField] private Transform firePoint;
    
    protected override void OnUpgrade()
    {
        Debug.Log($"💣 BOMB UPGRADED! Level {weaponLevel} - projectile config is driven by BombProjectile prefab.");
    }
    
    protected override void Attack()
    {
        GameObject target = FindNearestEnemy();
        if (target == null) return;

        Vector3 spawnPosition = firePoint != null ? firePoint.position : player.position;
        if (bombProjectilePrefab == null)
        {
            Debug.LogWarning("BombWeapon: bombProjectilePrefab is not assigned.");
            return;
        }

        GameObject bomb = Instantiate(bombProjectilePrefab, spawnPosition, Quaternion.identity);
        BombProjectile projectile = bomb.GetComponent<BombProjectile>();
        if (projectile == null)
        {
            Destroy(bomb);
            Debug.LogWarning("BombWeapon: bombProjectilePrefab has no BombProjectile component.");
            return;
        }

        projectile.Initialize(target.transform.position, damage, weaponLevel, baseRange);
    }
}
