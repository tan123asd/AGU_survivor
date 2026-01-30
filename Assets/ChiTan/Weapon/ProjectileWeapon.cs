using UnityEngine;

/// <summary>
/// Vũ khí bắn đạn tự động vào enemy gần nhất
/// </summary>
public class ProjectileWeapon : Weapon
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab; // Kéo prefab viên đạn vào đây
    [SerializeField] private Transform firePoint; // Vị trí spawn đạn (optional)
    
    protected override void Attack()
    {
        // Tìm enemy gần nhất
        GameObject target = FindNearestEnemy();
        
        if (target == null)
        {
            if (debugMode)
            {
                Debug.Log("No enemy in range!");
            }
            return;
        }
        
        // Tính hướng bắn
        Vector2 spawnPosition = firePoint != null ? firePoint.position : player.position;
        Vector2 direction = (target.transform.position - (Vector3)spawnPosition).normalized;
        
        // Spawn viên đạn
        if (projectilePrefab != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
            
            // Khởi tạo viên đạn
            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.Initialize(direction, damage);
            }
            
            if (debugMode)
            {
                Debug.Log($"Fired projectile at {target.name}!");
            }
        }
        else
        {
            Debug.LogError("ProjectilePrefab is not assigned!");
        }
    }
}
