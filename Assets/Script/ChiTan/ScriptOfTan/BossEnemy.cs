using Photon.Pun;
using UnityEngine;
public class BossEnemy : Enemy
{
    [Header("Boss Settings")]
    [SerializeField] private int bossHealthMultiplier = 10;
    [SerializeField] private int bossDamageMultiplier = 3;
    [SerializeField] private float bossSpeedMultiplier = 1.2f;
    [Header("Attack & Cast Settings")]
    [SerializeField] private float attackRange = 3.5f;
    [SerializeField] private float castRange = 9f;
    [SerializeField] private float attackCooldown = 2.2f;
    [SerializeField] private float castCooldownMin = 10f;
    [SerializeField] private float castCooldownMax = 18f;
    [Header("Spell Settings")]
    [SerializeField] private GameObject spellProjectilePrefab;
    [SerializeField] private Transform spellSpawnPoint;
    [SerializeField] private int spellDamage = 30;
    [SerializeField] private float spellSpeed = 7f;
    private Animator bossAnimator;
    private float lastAttackTime = -100f;
    private float nextCastTime = 0f;
    protected override void Awake()
    {
        base.Awake();
        bossAnimator = GetComponent<Animator>();
        if (bossAnimator == null)
            bossAnimator = GetComponentInChildren<Animator>();
        // Tăng stats
        health *= bossHealthMultiplier;
        speed = Mathf.RoundToInt(speed * bossSpeedMultiplier);
        // Thời gian cast phép lần đầu ngẫu nhiên
        nextCastTime = Time.time + Random.Range(6f, 12f);
        Debug.Log($"BOSS SPAWNED! Health: {health}, Speed: {speed}");
    }
    // Override Update để thêm boss logic
    void Update()
    {
        // MasterClient only
        PhotonView pv = GetComponent<PhotonView>();
        if (pv != null && !PhotonNetwork.IsMasterClient) return;
        if (health <= 0) return;
        // Refresh target
        RefreshTarget();
        // If no target or dead, wander (gọi base class)
        if (targetPlayer == null || playerHealth == null || playerHealth.IsDead)
        {
            DoWander();
            return;
        }
        // Boss attack logic
        Vector3 targetPos = targetPlayer.RootTransform != null
            ? targetPlayer.RootTransform.position
            : targetPlayer.transform.position;
        float distance = Vector2.Distance(transform.position, targetPos);
        Vector2 direction = (targetPos - transform.position).normalized;
        // Flip sprite
        transform.localScale = direction.x < 0
            ? new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z)
            : new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        // === Priority 1: ATTACK ===
        if (distance <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            if (bossAnimator != null)
                bossAnimator.SetTrigger("isAttacking");
            Debug.Log("Boss → Attack!");
            return;
        }
        // === Priority 2: CAST ===
        if (distance <= castRange && Time.time >= nextCastTime)
        {
            if (bossAnimator != null)
                bossAnimator.SetTrigger("isCasting");
            nextCastTime = Time.time + Random.Range(castCooldownMin, castCooldownMax);
            Debug.Log("Boss → Cast Spell!");
            return;
        }
        bossAnimator.SetBool("isWalking", true);
        // === Default: CHASE ===
        if (distance > 0.5f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
        }
    }
    // Wander method từ Enemy
    private void DoWander()
    {
        bossAnimator.SetBool("isWalking", true);
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0)
        {
            Vector2 randomDirection = Random.insideUnitCircle * wanderRadius;
            wanderTarget = (Vector2)transform.position + randomDirection;
            wanderTimer = wanderInterval;
        }
        transform.position = Vector2.MoveTowards(transform.position, wanderTarget, speed * 0.5f * Time.deltaTime);
        Vector2 direction = (wanderTarget - (Vector2)transform.position).normalized;
        if (direction.x < 0)
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        else
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
    }
    // Override TakeDamage để boss có animation hurt riêng
    public override void TakeDamage(int damage)
    {
        if (health <= 0) return;
        health -= damage;
        Debug.Log($"Boss Health: {health}");
        if (bossAnimator != null)
            bossAnimator.SetTrigger("isHurt");
        if (health <= 0)
            Die();
    }
    // Override Die để boss có animation death riêng
    public override void Die()
    {
        if (bossAnimator != null)
            bossAnimator.SetTrigger("isDeath");
        Debug.Log("=== BOSS DEFEATED! ===");
        Invoke(nameof(SpawnExp), 0.8f);
        Invoke(nameof(DestroyBoss), 3.0f); // Cho animation death chạy xong rồi mới destroy
    }

    private void DestroyBoss()
    {
        // Use base class Die logic
        PhotonView pv = GetComponent<PhotonView>();
        if (pv != null)
            PhotonNetwork.Destroy(gameObject);
        else
            Destroy(gameObject);
    }
    // Gọi từ Animation Event trong animation Cast
    public void SpawnSpell()
    {
        Debug.Log("=== CastSpell EVENT ĐÃ ĐƯỢC GỌI ===");
        Debug.Log($"Spell Prefab: {(spellProjectilePrefab != null ? "OK" : "NULL")}");
        Debug.Log($"Spawn Point: {(spellSpawnPoint != null ? "OK" : "NULL")}");

        if (spellProjectilePrefab == null || spellSpawnPoint == null || targetPlayer == null)
        {
            Debug.LogError("Thiếu prefab hoặc spawn point!");
            return;
        }

        GameObject spell = Instantiate(spellProjectilePrefab, spellSpawnPoint.position, Quaternion.identity);
        Debug.Log("Projectile đã Instantiate thành công tại vị trí: " + spell.transform.position);

        SpellProjectile proj = spell.GetComponent<SpellProjectile>();
        if (proj != null)
        {
            proj.Setup(targetPlayer.transform, spellDamage, spellSpeed);
            Debug.Log("Boss spawned spell projectile! SUCCESS");
        }
        else
        {
            Debug.LogError("Không tìm thấy SpellProjectile component trên prefab!");
        }
    }
    // Override GetDamageAmount để boss gây damage khác
    protected override int GetDamageAmount()
    {
        return 10 * bossDamageMultiplier;
    }
}