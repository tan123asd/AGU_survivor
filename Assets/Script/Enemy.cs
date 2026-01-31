using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int speed = 2;
    private GameObject player;
    private Animator enemyAnim;
    private bool isHit = false;
    public int health = 3;
    private float damageCooldown = 1.0f;
    private float lastDamageTime = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        enemyAnim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        ChasePlayer();
        
    }

    void LateUpdate()
    {
        isHit = false;
    }

    private void ChasePlayer()
    {
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
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
        }
    }

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
    }
}
