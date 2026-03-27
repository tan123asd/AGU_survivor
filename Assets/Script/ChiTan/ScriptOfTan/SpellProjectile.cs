using UnityEngine;

public class SpellProjectile : MonoBehaviour
{
    [Header("Spell Settings")]
    public float speed = 8f;
    public int damage = 30;
    public float lifetime = 10f;           // Thời gian tồn tại tối đa nếu không trúng

    private Transform target;
    private bool hasTarget = false;

    // Hàm này sẽ được gọi khi spawn spell từ Boss
    public void Setup(Transform playerTarget, int spellDamage, float spellSpeed)
    {
        target = playerTarget;
        damage = spellDamage;
        speed = spellSpeed;
        hasTarget = true;

        // Tự hủy sau một thời gian nếu không trúng
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (!hasTarget || target == null)
        {
            // Nếu mất target thì rơi thẳng xuống
            transform.Translate(Vector2.down * speed * Time.deltaTime);
            return;
        }

        // Bay về phía người chơi
        Vector2 direction = (target.position - transform.position).normalized;
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Tìm PlayerHealth
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>()
                                     ?? other.GetComponentInChildren<PlayerHealth>()
                                     ?? other.GetComponentInParent<PlayerHealth>();

            if (playerHealth != null && !playerHealth.IsDead)
            {
                playerHealth.SendTakeDamageRPC(damage);
                Debug.Log($"Spell trúng Player gây {damage} damage!");
            }

        }
    }

    // Optional: Nếu muốn spell gây damage cả khi va chạm với tường hoặc ground
    // private void OnCollisionEnter2D(Collision2D collision)
    // {
    //     if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Wall"))
    //         Destroy(gameObject);
    // }
}