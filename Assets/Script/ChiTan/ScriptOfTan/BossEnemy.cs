using UnityEngine;
using Photon.Pun;
/// <summary>
/// Boss Enemy - Enemy đặc biệt với stats cao hơn nhiều.
/// Kế thừa từ Enemy để tái sử dụng logic cơ bản.
/// </summary>
public class BossEnemy : Enemy
{
    [Header("Boss Settings")]
    [SerializeField] private int bossHealthMultiplier = 10; // Boss có máu gấp 10
    [SerializeField] private int bossDamageMultiplier = 3; // Boss damage gấp 3
    [SerializeField] private float bossSpeedMultiplier = 0.7f; // Boss chậm hơn 30%
    [SerializeField] private float bossScaleMultiplier = 10f; // Boss to gấp bao nhiêu lần (1.5 = to gấp 1.5 lần)
    [SerializeField] private Color bossColor = Color.red; // Màu đặc trưng

    [Header("Boss UI")]
    [SerializeField] private GameObject bossHealthBarPrefab; // Health bar riêng cho boss (optional)
    private SpriteRenderer spriteRenderer;

    protected override void Awake()
    {
        base.Awake();
        
        // Tăng stats cho boss
        health *= bossHealthMultiplier;
        speed = Mathf.RoundToInt(speed * bossSpeedMultiplier);
        
        // Thay đổi màu sắc để dễ nhận biết
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = bossColor;
        }

        // Scale lớn hơn để nổi bật
        transform.localScale *= bossScaleMultiplier;

        Debug.Log($"BOSS SPAWNED! Health: {health}, Speed: {speed}, Scale: {bossScaleMultiplier}x");
    }

    /// <summary>
    /// Override TakeDamage để boss khó giết hơn (optional).
    /// </summary>
    public override void TakeDamage(int damage)
    {
        // Boss có thể giảm damage nhận vào
        int reducedDamage = Mathf.Max(1, damage / 2); // Giảm 50% damage
        base.TakeDamage(reducedDamage);
        
        Debug.Log($"Boss took {reducedDamage} damage! Remaining health: {health}");
    }

    /// <summary>
    /// Override Die để có hiệu ứng đặc biệt khi boss chết.
    /// </summary>
    public override void Die()
    {
        Debug.Log("=== BOSS DEFEATED! ===");
        
        // TODO: Thêm hiệu ứng đặc biệt, drop loot tốt hơn...
        // DropBossLoot();
        // PlayBossDeathAnimation();
        
        base.Die();
    }

    /// <summary>
    /// Boss có thể gây damage cao hơn.
    /// </summary>
    protected override int GetDamageAmount()
    {
        return 10 * bossDamageMultiplier; // 30 damage thay vì 10
    }

    protected override bool ShouldProcessContactOnThisClient()
    {
        // Match existing boss behavior: process contact when offline,
        // and only on MasterClient when online.
        return !PhotonNetwork.IsConnected || PhotonNetwork.IsMasterClient;
    }

    protected override bool CanDealContactDamage(Collider2D collision)
    {
        // Keep previous boss behavior: no local cooldown gate here.
        return collision.CompareTag("Player");
    }

    protected override PlayerHealth ResolveHitPlayerHealth(Collider2D collision)
    {
        PlayerHealth hitHealth = collision.GetComponent<PlayerHealth>();
        if (hitHealth == null) hitHealth = collision.GetComponentInParent<PlayerHealth>();
        if (hitHealth == null)
        {
            Transform t = collision.transform.Find("PlayerHealth");
            if (t != null) hitHealth = t.GetComponent<PlayerHealth>();
        }

        return hitHealth;
    }

    protected override void DealContactDamage(PlayerHealth hitHealth)
    {
        if (hitHealth != null && !hitHealth.IsDead)
        {
            // Use RPC so all clients apply boss damage
            hitHealth.SendTakeDamageRPC(GetDamageAmount());
            Debug.Log($"Boss hit Player for {GetDamageAmount()} damage!");
        }
    }

    protected override void OnPostContactWithPlayer(Collider2D collision)
    {
        // Keep previous boss behavior: no self-damage on contact.
    }
}
