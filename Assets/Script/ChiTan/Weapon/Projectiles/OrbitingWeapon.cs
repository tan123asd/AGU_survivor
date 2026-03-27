
    using UnityEngine;

/// <summary>
/// Vũ khí xoay quanh player, gây damage cho enemy khi va chạm nhưng không bị biến mất.
/// </summary>

public class OrbitingWeapon : Projectile
{
    [Header("Orbit Settings")]
    public Transform target; // Player để xoay quanh
    public float orbitRadius = 2f;
    public float orbitSpeed = 180f; // Độ/giây
    private float angle;

    [Header("Spawn Settings")]
    public GameObject orbitingWeaponPrefab; // Prefab để spawn lại
    public bool autoRespawn = true; // Có tự động spawn lại không

    private void Start()
    {
        persistentOnHit = true; // Không bị destroy khi va chạm enemy
        disableLifetime = true; // Không tự hủy
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
        // Khởi tạo vị trí ban đầu trên quỹ đạo
        if (target != null)
        {
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * orbitRadius;
            transform.position = (Vector2)target.position + offset;
        }
    }

    private void Update()
    {
        if (target == null) return;
        // Tính toán vị trí mới trên quỹ đạo
        angle += orbitSpeed * Mathf.Deg2Rad * Time.deltaTime;
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * orbitRadius;
        transform.position = (Vector2)target.position + offset;
        // Xoay mũi vũ khí theo hướng di chuyển
        Vector2 dir = offset.normalized;
        if (dir != Vector2.zero)
        {
            float rotAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, rotAngle);
        }
        // Tự hủy sau 2 vòng (720 độ = 4 * PI radian)
        if (angle >= 4 * Mathf.PI)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Spawn lại vũ khí mới nếu được bật autoRespawn và có prefab
        if (autoRespawn && orbitingWeaponPrefab != null && target != null)
        {
            // Tạo vũ khí mới tại vị trí player
            GameObject newWeapon = Instantiate(orbitingWeaponPrefab, target.position, Quaternion.identity);
            OrbitingWeapon orbitScript = newWeapon.GetComponent<OrbitingWeapon>();
            if (orbitScript != null)
            {
                orbitScript.target = target;
                orbitScript.orbitingWeaponPrefab = orbitingWeaponPrefab;
                orbitScript.autoRespawn = autoRespawn;
            }
        }
    }

    // Không destroy khi va chạm enemy
    protected override void OnHitEnemy(Collider2D enemyCollider)
    {
        // Có thể thêm hiệu ứng đặc biệt ở đây nếu muốn
    }

    // Bỏ qua logic destroy trong OnTriggerEnter2D của Projectile
    private new void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            bool canApplyDamage = true;
#if PHOTON_UNITY_NETWORKING
            canApplyDamage = !Photon.Pun.PhotonNetwork.IsConnected || Photon.Pun.PhotonNetwork.IsMasterClient;
#endif
            if (canApplyDamage)
            {
                IDamageable damageable = other.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damage);
                }
                OnHitEnemy(other);
            }
            // KHÔNG destroy(gameObject)
        }
    }
}
