using UnityEngine;

/// <summary>
/// Bomb projectile flies to a target point, then explodes with AOE damage.
/// </summary>
public class BombProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 9f;
    [SerializeField] private float explodeDistance = 0.15f;
    [SerializeField] private float maxLifetime = 2f;

    [Header("Explosion")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float baseExplosionRadius = 3f;
    [SerializeField] private float radiusPerLevel = 0.15f;
    [SerializeField] private float baseStunDuration = 0.5f;
    [SerializeField] private float stunBonusPerLevelAfter3 = 0.2f;

    private float explosionRadius;
    private int damage = 10;
    private float stunDuration;

    private Vector3 targetPosition;
    private bool initialized;
    private bool exploded;
    private float lifeTimer;

    public void Initialize(Vector3 target, int damageValue, int weaponLevel, float weaponBaseRange)
    {
        targetPosition = target;
        damage = damageValue;

        // Explosion radius scales with level, not with weapon baseRange
        explosionRadius = baseExplosionRadius + Mathf.Max(0, weaponLevel - 1) * radiusPerLevel;
        explosionRadius = Mathf.Max(0.5f, explosionRadius); // Min 0.5 to avoid tiny explosions

        if (weaponLevel >= 3)
            stunDuration = Mathf.Max(0f, baseStunDuration + (weaponLevel - 3) * stunBonusPerLevelAfter3);
        else
            stunDuration = Mathf.Max(0f, baseStunDuration);

        initialized = true;

        Vector2 direction = (targetPosition - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Update()
    {
        if (!initialized || exploded) return;

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= maxLifetime)
        {
            Explode();
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        if (Vector2.Distance(transform.position, targetPosition) <= explodeDistance)
            Explode();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (exploded || !initialized) return;
        if (other.CompareTag("Enemy"))
            Explode();
    }

    private void Explode()
    {
        if (exploded) return;
        exploded = true;

        // Debug visualization of explosion radius
        Debug.Log($"🔴 Bomb exploded at {transform.position}, Radius: {explosionRadius}", gameObject);
        DebugDrawSphere(transform.position, explosionRadius, Color.red, 3f);

        if (explosionPrefab != null)
            Destroy(Instantiate(explosionPrefab, transform.position, Quaternion.identity), 1f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            IDamageable enemy = hit.GetComponent<IDamageable>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);

                if (stunDuration > 0f)
                {
                    Enemy enemyScript = hit.GetComponent<Enemy>();
                    if (enemyScript != null)
                    {
                        // enemyScript.Stun(stunDuration);
                    }
                }
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Show explosion radius in editor when bomb is selected
        Gizmos.color = Color.yellow;
        float displayRadius = explosionRadius > 0 ? explosionRadius : baseExplosionRadius;
        Gizmos.DrawWireSphere(transform.position, displayRadius);
    }

    private void DebugDrawSphere(Vector3 position, float radius, Color color, float duration)
    {
        // Draw a circle to visualize the explosion radius during gameplay
        int lineCount = 32;
        for (int i = 0; i < lineCount; i++)
        {
            float angle1 = (i / (float)lineCount) * 360f * Mathf.Deg2Rad;
            float angle2 = ((i + 1) / (float)lineCount) * 360f * Mathf.Deg2Rad;

            Vector3 p1 = position + new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1)) * radius;
            Vector3 p2 = position + new Vector3(Mathf.Cos(angle2), Mathf.Sin(angle2)) * radius;

            Debug.DrawLine(p1, p2, color, duration);
        }
    }
}
