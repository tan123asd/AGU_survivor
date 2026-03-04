using System.Collections.Generic;
using UnityEngine;

// Run before CameraDirector (-1) and Enemy (0) so the player is registered
// in PlayerController before any other Start() tries to find it.
[DefaultExecutionOrder(-50)]
public class PlayerSpawner : MonoBehaviour
{
    // ─── Inspector ────────────────────────────────────────────────────────────
    [Header("Player Prefabs")]
    [Tooltip("Drag your Player prefab(s) here. One will be chosen per player slot.")]
    [SerializeField] private GameObject[] playerPrefabs;

    [Header("Health Bar UI")]
    [Tooltip("Drag your HealthBar Canvas prefab here. It will be instantiated at runtime "
           + "and parented to the Main Camera so it stays fixed on screen.")]
    [SerializeField] private GameObject healthBarCanvasPrefab;

    [Tooltip("Parent for the spawned HealthBar canvas. Leave empty to auto-use Main Camera.")]
    [SerializeField] private Transform healthBarParent;

    [Header("Spawn Count")]
    [Tooltip("How many players to spawn (1 = single-player, 2+ = local co-op).")]
    [SerializeField] private int playerCount = 1;

    [Header("Spawn Points (Optional)")]
    [Tooltip("If assigned, players spawn at these transforms (shuffled). " +
             "Leave empty to use random positions inside spawnRadius.")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Random Spawn Fallback")]
    [Tooltip("Used only when no spawnPoints are assigned.")]
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private Vector2 mapCenter = Vector2.zero;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    // ─── Lifecycle ────────────────────────────────────────────────────────────
    private void Start()
    {
        SpawnPlayers(playerCount);
    }

    // ─── Spawn Logic ──────────────────────────────────────────────────────────

    /// <summary>Spawns <paramref name="count"/> players using the configured prefabs and positions.</summary>
    public void SpawnPlayers(int count)
    {
        if (playerPrefabs == null || playerPrefabs.Length == 0)
        {
            Debug.LogError("[PlayerSpawner] No playerPrefabs assigned! Please assign at least one Player prefab.");
            return;
        }

        List<Vector3> positions = BuildSpawnPositions(count);

        for (int i = 0; i < count; i++)
        {
            GameObject prefabToSpawn = playerPrefabs[i % playerPrefabs.Length];
            Vector3 spawnPos = positions[i];

            // ── 1. Spawn HealthBar Canvas ─────────────────────────────────────
            HealthBar healthBar = null;
            if (healthBarCanvasPrefab != null)
            {
                // Instantiate at scene ROOT (no parent).
                // We force Screen Space - Overlay from code so the canvas always
                // renders on top of the screen regardless of camera position/Z.
                GameObject canvasObj = Instantiate(healthBarCanvasPrefab);
                canvasObj.name = $"HealthBarCanvas_Player{i}";

                // Force overlay mode so it works even without a parent
                Canvas canvas = canvasObj.GetComponent<Canvas>();
                if (canvas == null) canvas = canvasObj.GetComponentInChildren<Canvas>();
                if (canvas != null)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvas.sortingOrder = 10; // Draw above everything
                }

                healthBar = canvasObj.GetComponentInChildren<HealthBar>(true);
                if (healthBar == null)
                    Debug.LogWarning("[PlayerSpawner] healthBarCanvasPrefab has no HealthBar component!");
                else if (showDebugLogs)
                    Debug.Log($"[PlayerSpawner] HealthBarCanvas spawned for Player_{i}");
            }
            else
            {
                Debug.LogWarning("[PlayerSpawner] No healthBarCanvasPrefab assigned — HealthBar UI will not show.");
            }

            // ── 2. Spawn Player ───────────────────────────────────────────────
            GameObject playerObj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            playerObj.name = $"Player_{i}";

            Player player = playerObj.GetComponent<Player>();
            if (player == null) player = playerObj.GetComponentInChildren<Player>();

            if (player != null)
            {
                player.SetPlayerIndex(i);

                // ── FIX: always anchor root transform to the instantiated object
                // (in case Player.cs is on a child object inside the prefab)
                player.SetRootTransform(playerObj.transform);

                // Wire HealthBar directly into PlayerHealth
                if (healthBar != null)
                {
                    PlayerHealth ph = playerObj.GetComponentInChildren<PlayerHealth>();
                    if (ph == null) ph = playerObj.GetComponent<PlayerHealth>();
                    if (ph != null)
                    {
                        ph.SetHealthBar(healthBar);
                        if (showDebugLogs)
                            Debug.Log($"[PlayerSpawner] HealthBar wired to {playerObj.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"[PlayerSpawner] No PlayerHealth found on {playerObj.name}!");
                    }
                }

                if (showDebugLogs)
                    Debug.Log($"[PlayerSpawner] Spawned {playerObj.name} at {spawnPos} (index {i})");
            }
            else
            {
                Debug.LogWarning($"[PlayerSpawner] '{playerObj.name}' has no Player component!");
            }
        }
    }

    // ─── Position helpers ─────────────────────────────────────────────────────

    private List<Vector3> BuildSpawnPositions(int count)
    {
        List<Vector3> result = new List<Vector3>();

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            // Shuffle spawn points to add variety
            List<Transform> shuffled = new List<Transform>(spawnPoints);
            ShuffleList(shuffled);

            for (int i = 0; i < count; i++)
            {
                // Wrap around if more players than points
                Transform pt = shuffled[i % shuffled.Count];
                result.Add(pt != null ? pt.position : GetRandomPosition());
            }
        }
        else
        {
            // No points assigned — use random inside radius
            for (int i = 0; i < count; i++)
                result.Add(GetRandomPosition());
        }

        return result;
    }

    private Vector3 GetRandomPosition()
    {
        Vector2 offset = Random.insideUnitCircle * spawnRadius;
        return new Vector3(mapCenter.x + offset.x, mapCenter.y + offset.y, 0f);
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // ─── Gizmos ───────────────────────────────────────────────────────────────
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            // Show random radius
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.3f);
            Gizmos.DrawSphere(new Vector3(mapCenter.x, mapCenter.y, 0f), spawnRadius);
            Gizmos.color = new Color(0f, 1f, 0.5f, 1f);
            Gizmos.DrawWireSphere(new Vector3(mapCenter.x, mapCenter.y, 0f), spawnRadius);
        }
    }
#endif
}
