using UnityEngine;

/// <summary>
/// Enemy AI: chases the nearest alive player using PlayerController.GetNearestPlayer().
/// Multiplayer-aware — automatically switches target as players die or move.
/// </summary>
public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int speed = 2;
    public int health = 3;

    [Header("Damage")]
    [SerializeField] private int damageToPlayer = 10;
    [SerializeField] private float damageCooldown = 1.0f;

    // ─── Private state ────────────────────────────────────────────────────────
    private Player targetPlayer;
    private Animator enemyAnim;
    private bool isHit = false;
    private float lastDamageTime = 0f;

    // Re-evaluate target every N seconds instead of every frame
    private float targetRefreshInterval = 1f;
    private float targetRefreshTimer = 0f;

    // Wandering (when all players dead)
    private Vector2 wanderTarget;
    private float wanderTimer = 0f;
    private float wanderInterval = 2f;
    private float wanderRadius = 5f;

    // ─── Lifecycle ────────────────────────────────────────────────────────────
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        enemyAnim = GetComponent<Animator>();
        RefreshTarget(); // Find nearest player at spawn
    }

    // Update is called once per frame
    private void Update()
    {
        // Refresh target periodically (cheap polling instead of per-frame Find)
        targetRefreshTimer -= Time.deltaTime;
        if (targetRefreshTimer <= 0f)
        {
            RefreshTarget();
            targetRefreshTimer = targetRefreshInterval;
        }

        // All players dead → wander
        if (targetPlayer == null)
        {
            WanderAround();
            return;
        }

        ChasePlayer();
    }

    private void LateUpdate()
    {
        isHit = false;
    }

    // ─── Target ───────────────────────────────────────────────────────────────

    private void RefreshTarget()
    {
        if (PlayerController.Instance == null) return;

        Player nearest = PlayerController.Instance.GetNearestPlayer(transform.position);

        // Accept new target only if it's alive
        if (nearest != null && nearest.PlayerHealth != null && !nearest.PlayerHealth.IsDead)
            targetPlayer = nearest;
        else
            targetPlayer = null;
    }

    // ─── Chase ────────────────────────────────────────────────────────────────

    private void ChasePlayer()
    {
        if (targetPlayer == null) return;

        if (health <= 0)
        {
            Die();
            return;
        }

        Vector2 direction = (targetPlayer.transform.position - transform.position).normalized;

        // Flip sprite
        if (direction.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = new Vector3(1, 1, 1);

        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPlayer.transform.position,
            speed * Time.deltaTime
        );
    }

    // ─── Wander ───────────────────────────────────────────────────────────────

    private void WanderAround()
    {
        wanderTimer -= Time.deltaTime;

        if (wanderTimer <= 0)
        {
            wanderTarget = (Vector2)transform.position + Random.insideUnitCircle * wanderRadius;
            wanderTimer = wanderInterval;
        }

        transform.position = Vector2.MoveTowards(
            transform.position, wanderTarget, speed * 0.5f * Time.deltaTime);

        Vector2 dir = (wanderTarget - (Vector2)transform.position).normalized;
        if (dir.x < 0)       transform.localScale = new Vector3(-1, 1, 1);
        else if (dir.x > 0)  transform.localScale = new Vector3(1, 1, 1);
    }

    // ─── Damage ───────────────────────────────────────────────────────────────

    public void TakeDamage(int damage)
    {
        lastDamageTime = Time.time;
        health -= damage;
        isHit = true;

        if (enemyAnim != null)
            enemyAnim.SetTrigger("Hit");

        Debug.Log("Enemy Health: " + health);

        if (health <= 0)
            Die();
    }

    public void Die()
    {
        if (enemyAnim != null)
            enemyAnim.SetBool("Dead", true);
        Destroy(gameObject, 1.0f);
    }

    // ─── Collision ────────────────────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isHit)
        {
            if (Time.time - lastDamageTime < damageCooldown)
                return;

            lastDamageTime = Time.time;
            isHit = true;

            // Damage cooldown
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable == null)
                damageable = collision.GetComponentInChildren<IDamageable>();

            // Check if the collided player is alive
            PlayerHealth ph = collision.GetComponent<PlayerHealth>();
            if (ph == null) ph = collision.GetComponentInChildren<PlayerHealth>();
            if (ph != null && ph.IsDead) return;

            if (damageable != null)
            {
                damageable.TakeDamage(damageToPlayer);
                Debug.Log("Enemy damaged Player!");
            }

            // Enemy takes 1 damage on contact
            health--;
            enemyAnim.SetTrigger("Hit");
            Debug.Log("Enemy Health: " + health);
        }
    }
}
