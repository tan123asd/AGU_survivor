using UnityEngine;
using Photon.Pun;

/// <summary>
/// Đạn đặc biệt cho vũ khí kết hợp Inferno Storm: khi va chạm sẽ gây AOE, đốt cháy kẻ địch trong vùng.
/// </summary>
public class InfernoStormProjectile : Projectile
{
    [Header("Inferno Storm Settings")]
    public float explosionRadius = 2.5f;
    public float aoeDamageMultiplier = 1.0f;
    public float burnDuration = 3f;
    public float burnDamagePerSecond = 10f;
    public LayerMask enemyLayer;

    protected override void OnHitEnemy(Collider2D enemyCollider)
    {
        // Gây damage AOE tại vị trí va chạm
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayer);
        foreach (var col in hits)
        {
            IDamageable dmg = col.GetComponent<IDamageable>();
            if (dmg != null)
            {
                dmg.TakeDamage((int)(damage * aoeDamageMultiplier));
                // Nếu có hiệu ứng đốt cháy
                BurningEffect burn = col.GetComponent<BurningEffect>();
                if (burn != null)
                {
                    burn.Initialize((int)burnDamagePerSecond, burnDuration);
                }
            }
        }
        // Hiệu ứng nổ (nếu có)
        DebugDrawSphere(transform.position, explosionRadius, Color.red, 2f);
        // TODO: Instantiate explosion VFX here
        base.OnHitEnemy(enemyCollider);
    }

    private void DebugDrawSphere(Vector3 position, float radius, Color color, float duration)
    {
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
