using UnityEngine;

/// <summary>
/// Vũ khí bắn đạn tự động vào enemy gần nhất
/// </summary>
public class ProjectileWeapon : Weapon
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab; // Kéo prefab viên đạn vào đây
    [SerializeField] private Transform firePoint; // Vị trí spawn đạn (optional)
    private int projectileNumber;

    protected override void Attack()
    {
        if (PlayerStats.Instance == null)
        {
            Debug.LogWarning("[ProjectileWeapon] PlayerStats.Instance is null! Make sure a PlayerStats GameObject exists in the scene. Defaulting to 1 projectile.");
            projectileNumber = 1;
        }
        else
        {
            projectileNumber = PlayerStats.Instance.numberOfProjectiles;
        }
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
        Vector2 mainDirection = (target.transform.position - (Vector3)spawnPosition).normalized;
        float mainAngle = Mathf.Atan2(mainDirection.y, mainDirection.x) * Mathf.Rad2Deg;

        int referenceMax = 12;
        float progress = Mathf.Clamp01((float)projectileNumber / referenceMax);
        float totalSpreadAngle = 360f * progress; // Spread tối đa 360 độ khi đạt 12 viên đạn
        float stepAngle = totalSpreadAngle / projectileNumber; // Chia đều góc giữa các viên đạn

        // Spawn viên đạn
        if (projectilePrefab != null)
        {
            for (int i = 0; i < projectileNumber; i++)
            {
                float offset = ((i + 0.05f) / projectileNumber - 0.5f) * totalSpreadAngle;// Tính offset góc cho viên đạn
                float bulletAngle = mainAngle + offset;
                float spawnRadius = 0.4f + projectileNumber * 0.01f; // Tăng dần bán kính spawn để tránh chồng lên nhau

                Vector2 spawnOffset = new Vector2(
                    spawnPosition.x + Mathf.Cos(bulletAngle * Mathf.Deg2Rad) * spawnRadius,
                    spawnPosition.y + Mathf.Sin(bulletAngle * Mathf.Deg2Rad) * spawnRadius
                );

                Vector2 bulletDirection = new Vector2(
                    Mathf.Cos(bulletAngle * Mathf.Deg2Rad),
                    Mathf.Sin(bulletAngle * Mathf.Deg2Rad)
                ).normalized;

                GameObject projectile = Instantiate(projectilePrefab, spawnOffset, Quaternion.identity);

                // Khởi tạo viên đạn
                Projectile projectileScript = projectile.GetComponent<Projectile>();
                if (projectileScript != null)
                {
                    projectileScript.Initialize(bulletDirection, damage);
                }

                if (debugMode)
                {
                    Debug.Log($"Fired projectile at {target.name}!");
                }
            }
        }
        else
        {
            Debug.LogError("ProjectilePrefab is not assigned!");
        }
    }
}
