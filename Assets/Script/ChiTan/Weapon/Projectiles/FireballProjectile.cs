using UnityEngine;

/// <summary>
/// Fireball projectile - Gây bỏng cho enemy (1 damage/giây trong 5 giây)
/// </summary>
public class FireballProjectile : Projectile
{
    [Header("Fireball Special Effect")]
    [SerializeField] private int burnDamagePerSecond = 1;
    [SerializeField] private float burnDuration = 5f;

    protected override void OnHitEnemy(Collider2D enemyCollider)
    {
        // Gây damage bình thường
        base.OnHitEnemy(enemyCollider);

        // Thêm burning effect
        ApplyBurningEffect(enemyCollider.gameObject);
    }

    private void ApplyBurningEffect(GameObject enemy)
    {
        // Kiểm tra xem enemy đã có burning effect chưa
        BurningEffect existingBurn = enemy.GetComponent<BurningEffect>();
        
        if (existingBurn != null)
        {
            // Nếu đã có → Refresh duration
            existingBurn.RefreshDuration();
            Debug.Log($"🔥 Burning effect refreshed on {enemy.name}!");
        }
        else
        {
            // Nếu chưa có → Add new burning effect
            BurningEffect newBurn = enemy.AddComponent<BurningEffect>();
            newBurn.Initialize(burnDamagePerSecond, burnDuration);
            Debug.Log($"🔥 Applied burning effect to {enemy.name}!");
        }
    }
}
