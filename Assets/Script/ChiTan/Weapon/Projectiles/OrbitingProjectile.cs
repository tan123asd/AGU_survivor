using UnityEngine;

/// <summary>
/// Makes a projectile orbit around a center transform at a given angular speed and radius.
/// Attach to instantiated projectile prefabs when using spinning weapon mode.
/// </summary>
public class OrbitingProjectile : MonoBehaviour
{
    public Transform center;
    public float angularSpeed = 180f; // degrees per second
    public float radius = 1f;
    public float lifetime = 5f;

    private float angleDeg;
    private float timer;
    private Rigidbody2D rb;
    private bool initialized = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Prevent physics from moving the orbiting projectile
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;
            rb.gravityScale = 0f;
        }
    }

    private void Start()
    {
        // If Initialize(...) was called earlier, don't override center/radius
        if (!initialized)
        {
            if (center == null)
            {
                // If no center assigned, try to find a Player transform
                var player = GameObject.FindWithTag("Player");
                if (player != null) center = player.transform;
            }

            // Initialize angle based on current position relative to center
            if (center != null)
            {
                Vector2 dir = (transform.position - center.position);
                if (dir.sqrMagnitude > 0.0001f)
                {
                    angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    // only set radius if it wasn't explicitly provided (radius <= 0 means uninitialized)
                    if (radius <= 0f) radius = dir.magnitude;
                }
                else
                {
                    // default angle
                    angleDeg = 0f;
                }
            }
        }

        // Schedule lifetime destroy if requested
        if (lifetime > 0f)
            Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (center == null) return;

        angleDeg += angularSpeed * Time.deltaTime;
        float rad = angleDeg * Mathf.Deg2Rad;
        Vector3 pos = center.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * radius;
        transform.position = pos;

        timer += Time.deltaTime;
        if (lifetime > 0f && timer >= lifetime)
            Destroy(gameObject);
    }

    /// <summary>
    /// Initialize orbiting parameters. Call immediately after Instantiate to ensure proper radius/center.
    /// </summary>
    public void Initialize(Transform centerTransform, float orbitRadius, float angSpeed, float life)
    {
        center = centerTransform;
        radius = orbitRadius;
        angularSpeed = angSpeed;
        lifetime = life;

        // compute initial angle based on current position relative to center
        if (center != null)
        {
            Vector2 dir = (transform.position - center.position);
            angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }

        initialized = true;

        // schedule lifetime destroy if requested (Start may not run immediately)
        if (lifetime > 0f)
            Destroy(gameObject, lifetime);
    }

    private void OnDestroy()
    {
        Debug.Log($"OrbitingProjectile.OnDestroy: {name} | radius={radius} | lifetime={lifetime}");
    }
}
