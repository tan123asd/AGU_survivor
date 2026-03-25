using System.Collections.Generic;
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
    [SerializeField] private bool isSpinning; // Nếu true, sẽ bắn theo vòng tròn quanh player

    [Header("Spinning Settings")]
    [SerializeField] private float spinRadius = 1.5f;
    [SerializeField] private float spinAngularSpeed = 180f; // degrees per second
    [SerializeField] private float spinLifetime = 0f; // 0 = persistent until stopped
    [Tooltip("Extra radius added per projectile when multiple orbiting projectiles are spawned (prevents overlap)")]
    [SerializeField] private float spinRadiusPerProjectile = 0.12f;
    [SerializeField] private float minSpinRadius = 1.5f;

    // Active orbiting projectiles (spawned once while spinning active)
    private List<GameObject> activeOrbitProjectiles = new List<GameObject>();
    private bool spinningActive = false;

    protected override void Start()
    {
        // Set weaponName để WeaponController có thể tìm được
        weaponName = "DefaultWeapon"; // Khớp với UpgradeData

        base.Start(); // Gọi base.Start() để tính stats
    }

    protected override void OnUpgrade()
    {
        // Effect khi upgrade: có thể thêm particle, sound, hoặc bonus stat
        Debug.Log($"\uD83C\uDF1F {weaponName} upgraded to level {weaponLevel}!");

        // When upgrading while spinning, refresh orbiting projectiles to match new count immediately
        if (isSpinning && spinningActive)
        {
            if (debugMode) Debug.Log("[ProjectileWeapon] Upgrade detected while spinning - refreshing orbiting projectiles.");
            RefreshSpinning();
        }
    }

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

        Vector2 spawnPosition = firePoint != null ? firePoint.position : player.position;

        if (isSpinning)
        {
            // If already active and count matches, do nothing
            if (spinningActive && activeOrbitProjectiles.Count > 0)
            {
                if (activeOrbitProjectiles.Count == projectileNumber)
                {
                    if (debugMode) Debug.Log("[ProjectileWeapon] Spinning already active with matching count - skipping spawn.");
                    return;
                }

                // Different count → refresh immediately
                if (debugMode) Debug.Log("[ProjectileWeapon] Projectile count changed - refreshing spinning projectiles.");
                RefreshSpinning();
                return;
            }

            // Not active yet => spawn now
            SpawnOrbitingProjectiles(projectileNumber, spawnPosition);
            return;
        }

        // Non-spinning behavior: target nearest enemy
        GameObject target = FindNearestEnemy();

        if (target == null)
        {
            if (debugMode)
            {
                Debug.Log("No enemy in range!");
            }
            return;
        }

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

    private void SpawnOrbitingProjectiles(int count, Vector2 spawnPosition)
    {
        if (projectilePrefab == null) return;

        // Ensure any previous orbiting objects are destroyed
        StopSpinning();

        // Calculate an appropriate orbit radius to avoid spawning on top of player
        float extra = Mathf.Max(0f, (count - 1)) * spinRadiusPerProjectile;
        float actualRadius = Mathf.Max(minSpinRadius + extra, 0.1f);

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            float angleDeg = t * 360f;
            float rad = angleDeg * Mathf.Deg2Rad;

            Vector3 pos = new Vector3(
                spawnPosition.x + Mathf.Cos(rad) * actualRadius,
                spawnPosition.y + Mathf.Sin(rad) * actualRadius,
                0f
            );

            GameObject projectile = Instantiate(projectilePrefab, pos, Quaternion.identity);

            // Attach orbiting behaviour and initialize explicitly
            var orbit = projectile.GetComponent<OrbitingProjectile>();
            if (orbit == null) orbit = projectile.AddComponent<OrbitingProjectile>();
            orbit.Initialize(player, actualRadius, spinAngularSpeed, spinLifetime);

            // If projectile has regular Projectile script, set it persistent and initialize without direction
            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                // Make it persist on hit and disable auto lifetime when orbiting
                projectileScript.persistentOnHit = true;
                projectileScript.disableLifetime = true;
                projectileScript.Initialize(Vector2.zero, damage);
            }

            activeOrbitProjectiles.Add(projectile);

            if (debugMode)
            {
                Debug.Log($"Spawned spinning projectile #{i} at angle {angleDeg} with radius {actualRadius}");
            }
        }

        spinningActive = true;
    }

    /// <summary>
    /// Refresh spinning projectiles to match current projectile count immediately.
    /// </summary>
    private void RefreshSpinning()
    {
        int newCount = PlayerStats.Instance != null ? PlayerStats.Instance.numberOfProjectiles : 1;
        Vector2 spawnPosition = firePoint != null ? firePoint.position : player.position;
        SpawnOrbitingProjectiles(newCount, spawnPosition);
    }

    /// <summary>
    /// Stop spinning mode and destroy any active orbiting projectiles.
    /// </summary>
    public void StopSpinning()
    {
        if (debugMode) Debug.Log($"[ProjectileWeapon] StopSpinning called. active count={activeOrbitProjectiles.Count}");
        for (int i = activeOrbitProjectiles.Count - 1; i >= 0; i--)
        {
            var go = activeOrbitProjectiles[i];
            if (go != null)
            {
                if (debugMode) Debug.Log($"[ProjectileWeapon] Destroying orbit projectile: {go.name}");
                Destroy(go);
            }
        }
        activeOrbitProjectiles.Clear();
        spinningActive = false;
        if (debugMode) Debug.Log("[ProjectileWeapon] Stopped spinning and cleared orbiting projectiles.");
    }

    private void OnDisable()
    {
        // Clean up if weapon is disabled
        StopSpinning();
    }
}
