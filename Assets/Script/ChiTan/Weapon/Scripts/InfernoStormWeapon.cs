using UnityEngine;

/// <summary>
/// Fusion weapon example: combines projectile burst + bomb AOE + burning effect.
/// Suggested fusion pair: fireball + bomb.
/// </summary>
public class InfernoStormWeapon : Weapon
{
    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private int bonusProjectiles = 1;

    [Header("Explosion")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionRadius = 2.5f;
    [SerializeField] private float aoeDamageMultiplier = 0.75f;

    [Header("Burning")]
    [SerializeField] private int burnDamagePerSecond = 1;
    [SerializeField] private float burnDuration = 4f;

    protected override void CalculateStats()
    {
        base.CalculateStats();

        // Fusion weapon scales explosion radius a bit stronger than default range scaling.
        explosionRadius = Mathf.Max(1f, baseRange * (1f + (weaponLevel - 1) * 0.12f));
    }

    protected override void OnUpgrade()
    {
        Debug.Log($"Inferno Storm upgraded to level {weaponLevel}");
    }

    protected override void Attack()
    {
        GameObject target = FindNearestEnemy();
        if (target == null) return;

        Vector2 spawnPosition = firePoint != null ? firePoint.position : (Vector2)player.position;
        Vector2 mainDirection = ((Vector2)target.transform.position - spawnPosition).normalized;
        float mainAngle = Mathf.Atan2(mainDirection.y, mainDirection.x) * Mathf.Rad2Deg;

        int projectileCount = 1;
        if (PlayerStats.Instance != null)
            projectileCount = Mathf.Max(1, PlayerStats.Instance.numberOfProjectiles + bonusProjectiles);

        SpawnProjectileBurst(spawnPosition, mainAngle, projectileCount, target.transform.position);
        SpawnExplosion(target.transform.position);
    }

    private void SpawnProjectileBurst(Vector2 spawnPosition, float mainAngle, int projectileCount, Vector3 targetPosition)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("InfernoStormWeapon: projectilePrefab is not assigned.");
            return;
        }

        int referenceMax = 12;
        float progress = Mathf.Clamp01((float)projectileCount / referenceMax);
        float totalSpreadAngle = 360f * progress;

        for (int i = 0; i < projectileCount; i++)
        {
            float offset = ((i + 0.05f) / projectileCount - 0.5f) * totalSpreadAngle;
            float bulletAngle = mainAngle + offset;
            float spawnRadius = 0.45f + projectileCount * 0.01f;

            Vector2 spawnOffset = new Vector2(
                spawnPosition.x + Mathf.Cos(bulletAngle * Mathf.Deg2Rad) * spawnRadius,
                spawnPosition.y + Mathf.Sin(bulletAngle * Mathf.Deg2Rad) * spawnRadius
            );

            Vector2 bulletDirection = new Vector2(
                Mathf.Cos(bulletAngle * Mathf.Deg2Rad),
                Mathf.Sin(bulletAngle * Mathf.Deg2Rad)
            ).normalized;

            GameObject projectileObj = Instantiate(projectilePrefab, spawnOffset, Quaternion.identity);
            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Initialize(bulletDirection, damage);
            }

            if (debugMode)
                Debug.Log($"Inferno Storm fired at {targetPosition}");
        }
    }

    private void SpawnExplosion(Vector3 position)
    {
        if (explosionPrefab != null)
            Destroy(Instantiate(explosionPrefab, position, Quaternion.identity), 1f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(position, explosionRadius);
        int aoeDamage = Mathf.Max(1, Mathf.RoundToInt(damage * aoeDamageMultiplier));

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
                damageable.TakeDamage(aoeDamage);

            ApplyBurningEffect(hit.gameObject);
        }
    }

    private void ApplyBurningEffect(GameObject enemy)
    {
        BurningEffect existingBurn = enemy.GetComponent<BurningEffect>();
        if (existingBurn != null)
        {
            existingBurn.RefreshDuration();
            return;
        }

        BurningEffect newBurn = enemy.AddComponent<BurningEffect>();
        newBurn.Initialize(burnDamagePerSecond, burnDuration);
    }
}
