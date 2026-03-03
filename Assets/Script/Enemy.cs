using UnityEngine;

<<<<<<< Updated upstream
public class Enemy : MonoBehaviour
=======
/// <summary>
/// Enemy AI: chases the nearest alive player using PlayerController.GetNearestPlayer().
/// Multiplayer-aware — automatically switches target as players die or move.
/// </summary>
public class Enemy : MonoBehaviour, IDamageable
>>>>>>> Stashed changes
{
    [Header("Stats")]
    public int speed = 2;
<<<<<<< Updated upstream
    private GameObject player;
=======
    public int health = 3;

    [Header("Damage")]
    [SerializeField] private int damageToPlayer = 10;
    [SerializeField] private float damageCooldown = 1.0f;

    // ─── Private state ────────────────────────────────────────────────────────
    private Player targetPlayer;
>>>>>>> Stashed changes
    private Animator enemyAnim;
    private bool isHit = false;
    private float lastDamageTime = 0f;
<<<<<<< Updated upstream
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
=======

    // Re-evaluate target every N seconds instead of every frame
    private float targetRefreshInterval = 1f;
    private float targetRefreshTimer = 0f;

    // Wandering (when all players dead)
    private Vector2 wanderTarget;
    private float wanderTimer = 0f;
    private float wanderInterval = 2f;
    private float wanderRadius = 5f;

    // ─── Lifecycle ────────────────────────────────────────────────────────────
    private void Start()
>>>>>>> Stashed changes
    {
        enemyAnim = GetComponent<Animator>();
<<<<<<< Updated upstream
=======
        RefreshTarget(); // Find nearest player at spawn
>>>>>>> Stashed changes
    }

    private void Update()
    {
<<<<<<< Updated upstream
=======
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

>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
        Vector2 direction = (player.transform.position - transform.position).normalized;
        if(direction.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        if (health <= 0)
        {
            enemyAnim.SetBool("Dead", true);
            Destroy(gameObject, 1.0f);
=======
        if (targetPlayer == null) return;

        if (health <= 0)
        {
            Die();
            return;
>>>>>>> Stashed changes
        }

        Vector2 direction = (targetPlayer.transform.position - transform.position).normalized;

        // Flip sprite
        transform.localScale = direction.x < 0
            ? new Vector3(-1, 1, 1)
            : new Vector3(1, 1, 1);

        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPlayer.transform.position,
            speed * Time.deltaTime
        );
    }

<<<<<<< Updated upstream
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && !isHit)
        {
            if(Time.time - lastDamageTime < damageCooldown)
            {
                return;
            }
            else
            {
                lastDamageTime = Time.time;
                health--;
                isHit = true;
                enemyAnim.SetTrigger("Hit");
                Debug.Log("Enemy Health: " + health);
            }
             
        }
=======
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

        Debug.Log($"[Enemy] Health: {health}");

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
        if (!collision.CompareTag("Player")) return;

        // Damage cooldown
        if (Time.time - lastDamageTime < damageCooldown) return;

        // Validate target is alive
        IDamageable damageable = collision.GetComponent<IDamageable>();
        if (damageable == null)
            damageable = collision.GetComponentInChildren<IDamageable>();

        if (damageable == null) return;

        // Check if the collided player is alive via PlayerHealth
        PlayerHealth ph = collision.GetComponent<PlayerHealth>();
        if (ph == null) ph = collision.GetComponentInChildren<PlayerHealth>();
        if (ph != null && ph.IsDead) return;

        lastDamageTime = Time.time;
        damageable.TakeDamage(damageToPlayer);
        Debug.Log("[Enemy] Damaged player!");

        // Enemy takes a hit from the collision
        if (!isHit)
            TakeDamage(1);
>>>>>>> Stashed changes
    }
}
