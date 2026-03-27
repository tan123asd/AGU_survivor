using Photon.Pun;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    public int speed = 2;

    // ─── Target (multi-player aware) ──────────────────────────────────────────
    // Instead of a single cached GameObject, we query PlayerController each
    // frame so we always chase the nearest ALIVE player.
    protected PlayerEntity targetPlayer;
    protected PlayerHealth playerHealth; // kept for wander / death check

    protected Animator enemyAnim;
    protected bool isHit = false;
    public int health = 3;
    private float damageCooldown = 1.0f;
    private float lastDamageTime = 0f;

    // EXP drop: use the prefab NAME (string) so PhotonNetwork.Instantiate can find
    // it in Resources/. Leave empty to disable network EXP spawn.
    [Tooltip("Names of EXP prefabs inside Assets/Resources/. MasterClient spawns one on death.")]
    public string[] expPrefabNames;
    [HideInInspector] public GameObject[] expSpawn; // legacy — kept so old Inspector refs don't break

    private bool isDead = false;

    // ─── Photon ───────────────────────────────────────────────────────────────
    private PhotonView _photonView;

    // Wandering variables
    protected Vector2 wanderTarget;
    protected float wanderTimer = 0f;
    protected float wanderInterval = 2f; // Thời gian đổi hướng
    protected float wanderRadius = 5f; // Phạm vi di chuyển tự do

    // Scale gốc để giữ khi flip sprite
    protected Vector3 originalScale;

    protected virtual void Awake()
    {
        _photonView = GetComponent<PhotonView>();
    }

    void Start()
    {
        originalScale = transform.localScale;
        enemyAnim = GetComponent<Animator>();

        // Initial target: find nearest player via PlayerController.
        // Works for any number of players (1-N).
        RefreshTarget();
    }

    // ─── Target helper ────────────────────────────────────────────────────────
    /// <summary>Queries PlayerController for the nearest alive player and caches references.</summary>
    protected void RefreshTarget()
    {
        if (PlayerController.Instance == null) return;

        targetPlayer = PlayerController.Instance.GetNearestPlayer(transform.position);
        if (targetPlayer != null)
            playerHealth = targetPlayer.PlayerHealth;
        else
            playerHealth = null;
    }

    // ─── Update (MasterClient only) ───────────────────────────────────────────
    void Update()
    {
        // In multiplayer, only MasterClient runs enemy AI.
        // Non-master clients receive enemy positions via PhotonTransformView.
        if (_photonView != null && !PhotonNetwork.IsMasterClient) return;

        // Re-acquire nearest target every frame (handles player deaths / new joins)
        RefreshTarget();

        // If the current target is dead, wander
        if (playerHealth != null && playerHealth.IsDead)
        {
            WanderAround();
            return;
        }

        ChasePlayer();
    }

    void LateUpdate()
    {
        isHit = false;
    }

    private void ChasePlayer()
    {
        if (targetPlayer == null) return;

        Vector3 targetPos = targetPlayer.RootTransform != null
            ? targetPlayer.RootTransform.position
            : targetPlayer.transform.position;

        Vector2 direction = (targetPos - transform.position).normalized;
        transform.localScale = direction.x < 0
            ? new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z)
            : new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);

        if (health <= 0)
        {
            if (!isDead) { isDead = true; Die(); }
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
        }
    }

    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;

        // Implementation of TakeDamage method
        lastDamageTime = Time.time;
        health -= damage;
        isHit = true;
        enemyAnim.SetTrigger("Hit");
        Debug.Log("Enemy Health: " + health);
    }

    public virtual void Die()
    {
        enemyAnim.SetBool("Dead", true);
        Invoke(nameof(SpawnExp), 0.8f);

        // Use PhotonNetwork.Destroy so the object is removed on all clients.
        // Falls back to Destroy when offline.
        if (_photonView != null)
            PhotonNetwork.Destroy(gameObject);
        else
            Destroy(gameObject, 1.0f);
    }

    protected virtual void SpawnExp()
    {
        if (!PhotonNetwork.IsMasterClient) return;  // Chỉ Master spawn

        if (expPrefabNames == null || expPrefabNames.Length == 0)
        {
            Debug.LogWarning($"{gameObject.name} has no expPrefabNames!");
            return;
        }

        string prefabName = expPrefabNames[Random.Range(0, expPrefabNames.Length)];

        // Debug để kiểm tra
        Debug.Log($"[Master] Spawning network EXP: {prefabName} at {transform.position}");

        // Dùng InstantiateRoomObject cho EXP orb (room-owned)
        GameObject spawned = PhotonNetwork.InstantiateRoomObject(prefabName, transform.position, Quaternion.identity);

        if (spawned == null)
        {
            Debug.LogError($"Spawn failed for {prefabName}! Check Resources/ folder and prefab name.");
            return;
        }

        PhotonView pv = spawned.GetComponent<PhotonView>();
        if (pv != null)
        {
            Debug.Log($"[Spawn OK] EXP ViewID: {pv.ViewID} (should > 0)");
        }
        else
        {
            Debug.LogError("Spawned EXP but NO PhotonView! Add PhotonView to prefab root.");
        }
    }

    private void WanderAround()
    {
        wanderTimer -= Time.deltaTime;

        // Chọn điểm ngẫu nhiên mới khi hết thời gian
        if (wanderTimer <= 0)
        {
            // Tạo vị trí ngẫu nhiên quanh vị trí hiện tại
            Vector2 randomDirection = Random.insideUnitCircle * wanderRadius;
            wanderTarget = (Vector2)transform.position + randomDirection;
            wanderTimer = wanderInterval;
        }

        // Di chuyển đến điểm mục tiêu
        transform.position = Vector2.MoveTowards(transform.position, wanderTarget, speed * 0.5f * Time.deltaTime);

        // Flip sprite theo hướng di chuyển
        Vector2 direction = (wanderTarget - (Vector2)transform.position).normalized;
        if (direction.x < 0)
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        else
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
    }

    /// <summary>
    /// Override để boss có thể gây damage khác nhau.
    /// </summary>
    protected virtual int GetDamageAmount()
    {
        return 10; // Damage mặc định
    }

    protected virtual bool ShouldProcessContactOnThisClient()
    {
        // Only MasterClient applies damage — prevents double-damage from all clients.
        return _photonView == null || PhotonNetwork.IsMasterClient;
    }

    protected virtual bool CanDealContactDamage(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return false;

        if (Time.time - lastDamageTime < damageCooldown) return false;
        lastDamageTime = Time.time;

        return true;
    }

    protected virtual PlayerHealth ResolveHitPlayerHealth(Collider2D collision)
    {
        // Get PlayerHealth from the hit collider
        PlayerHealth hitPlayerHealth = collision.GetComponent<PlayerHealth>();
        if (hitPlayerHealth == null) hitPlayerHealth = collision.GetComponentInChildren<PlayerHealth>();
        if (hitPlayerHealth == null) hitPlayerHealth = collision.GetComponentInParent<PlayerHealth>();
        return hitPlayerHealth;
    }

    protected virtual void DealContactDamage(PlayerHealth hitPlayerHealth)
    {
        if (hitPlayerHealth != null && !hitPlayerHealth.IsDead)
        {
            // RPC → all clients reduce HP simultaneously
            hitPlayerHealth.SendTakeDamageRPC(GetDamageAmount());
            Debug.Log($"Enemy damaged Player for {GetDamageAmount()}!");
        }
    }

    protected virtual void OnPostContactWithPlayer(Collider2D collision)
    {
        // Enemy also takes 1 damage on contact
        if (!isHit)
            TakeDamage(1);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (!ShouldProcessContactOnThisClient()) return;

        if (!CanDealContactDamage(collision)) return;

        PlayerHealth hitPlayerHealth = ResolveHitPlayerHealth(collision);
        DealContactDamage(hitPlayerHealth);
        OnPostContactWithPlayer(collision);
    }


}
