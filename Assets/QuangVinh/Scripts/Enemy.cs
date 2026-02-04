using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    public int speed = 2;
    private GameObject player;
    private PlayerHealth playerHealth;
    private Animator enemyAnim;
    private bool isHit = false;
    public int health = 3;
    private float damageCooldown = 1.0f;
    private float lastDamageTime = 0f;
    public GameObject[] expSpawn;
    private bool isDead = false;

    // Wandering variables
    private Vector2 wanderTarget;
    private float wanderTimer = 0f;
    private float wanderInterval = 2f; // Thời gian đổi hướng
    private float wanderRadius = 5f; // Phạm vi di chuyển tự do
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        enemyAnim = GetComponent<Animator>();

        // Tìm PlayerHealth - NÓ LÀ GAMEOBJECT CON, không phải component!
        if (player != null)
        {
            // Tìm GameObject con tên "PlayerHealth"
            Transform playerHealthTransform = player.transform.Find("PlayerHealth");
            if (playerHealthTransform != null)
            {
                playerHealth = playerHealthTransform.GetComponent<PlayerHealth>();
                Debug.Log("Enemy found PlayerHealth on child GameObject!");
            }
            else
            {
                Debug.LogError("Cannot find PlayerHealth child GameObject!");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Nếu Player chết rồi thì di chuyển tự do
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
        if (player == null) return;

        Vector2 direction = (player.transform.position - transform.position).normalized;
        if (direction.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        if (health <= 0)
        {
            if (!isDead)
            {
                isDead = true;
                Die();
            }
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
        }
    }

    public void TakeDamage(int damage)
    {
        if(isDead) return;

        // Implementation of TakeDamage method
        lastDamageTime = Time.time;
        health -= damage;
        isHit = true;
        enemyAnim.SetTrigger("Hit");
        Debug.Log("Enemy Health: " + health);
    }

    public void Die()
    {
        enemyAnim.SetBool("Dead", true);
        Invoke(nameof(SpawnExp), 0.8f);
        Destroy(gameObject, 1.0f);
    }

    private void SpawnExp()
    {
        int index = Random.Range(0, expSpawn.Length);
        Instantiate(expSpawn[index], transform.position, transform.rotation);
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
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (direction.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Tìm PlayerHealth nếu chưa có
            if (playerHealth == null && player != null)
            {
                Transform playerHealthTransform = player.transform.Find("PlayerHealth");
                if (playerHealthTransform != null)
                {
                    playerHealth = playerHealthTransform.GetComponent<PlayerHealth>();
                }
            }

            // Không tấn công nếu Player đã chết
            if (playerHealth != null && playerHealth.IsDead)
            {
                Debug.Log("Enemy collision but Player is dead - skipping damage");
                return;
            }

            if (playerHealth == null)
            {
                Debug.LogError("PlayerHealth is NULL in Enemy OnTriggerEnter2D!");
                return;
            }

            if (Time.time - lastDamageTime < damageCooldown)
            {
                return;
            }

            lastDamageTime = Time.time;

            // Tìm IDamageable trên GameObject Player hoặc các con của nó
            IDamageable playerDamageable = collision.GetComponent<IDamageable>();
            if (playerDamageable == null)
                playerDamageable = collision.GetComponentInChildren<IDamageable>();

            if (playerDamageable != null)
            {
                playerDamageable.TakeDamage(10); // Player mất 10 máu
                Debug.Log("Enemy damaged Player!");
            }

            // Enemy cũng nhận damage
            if (!isHit)
            {
                TakeDamage(1);
            }
        }
    }


}
