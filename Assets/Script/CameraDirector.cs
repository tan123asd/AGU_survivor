using UnityEngine;

/// <summary>
/// Smooth-follow camera for the local player.
/// Priority: manually assigned [SerializeField] target → PlayerController.GetLocalPlayer().RootTransform
/// </summary>
[DefaultExecutionOrder(-1)]   // After PlayerSpawner(-50), before Enemy(0)
public class CameraDirector : MonoBehaviour
{
    [Header("Follow Target")]
    [Tooltip("Assign a Transform to follow. If left empty, the local player is found automatically via PlayerController.")]
    [SerializeField] private Transform target;

    [Header("Camera Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float smoothTime = 0.3f;

    private Vector3 velocity = Vector3.zero;

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    private void Start()
    {
        // Auto-assign if nothing set in Inspector
        if (target == null)
            TryAssignLocalPlayer();
    }

    private void Update()
    {
        // Re-acquire if target was destroyed (player died and was removed)
        if (target == null)
            TryAssignLocalPlayer();

        if (target == null) return;

        Vector3 targetPos = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private void TryAssignLocalPlayer()
    {
        if (PlayerController.Instance == null) return;

        PlayerEntity local = PlayerController.Instance.GetLocalPlayer();
        if (local != null)
        {
            // Use RootTransform — always the root GameObject, even if PlayerEntity.cs is on a child
            target = local.RootTransform;
            Debug.Log($"[CameraDirector] Auto-assigned target: {local.name} (root: {target.name})");
        }
    }

    /// <summary>Manually override the follow target at runtime.</summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
