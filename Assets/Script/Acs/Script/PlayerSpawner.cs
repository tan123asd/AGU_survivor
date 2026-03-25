using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// Run before CameraDirector (-1) and Enemy (0) so the player is registered
// in PlayerController before any other Start() tries to find it.
[DefaultExecutionOrder(-50)]
public class PlayerSpawner : MonoBehaviourPunCallbacks
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

    [Header("Options")]
    [Tooltip("If true, PlayerSpawner will wait until SpawnSelectedPlayerFromName() is called instead of spawning in Start().")]
    [SerializeField] private bool waitForSelection = true; // default changed to true

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    // Pending spawn when selection happens before joining Photon room          
    private string pendingSpawnPrefabName = null;
    private int pendingSpawnCount = 1;

    // ─── Lifecycle ───────────────────────────────────────────────────────────
    private void Start()
    {
        if (!waitForSelection)
            SpawnPlayers(playerCount);
        else if (showDebugLogs)
            Debug.Log("[PlayerSpawner] Waiting for player selection before spawning. Set 'waitForSelection' to false to auto-spawn.");
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

            // ── 2. Spawn Player (network-aware) ───────────────────────────────
            GameObject playerObj = SpawnOne(prefabToSpawn, spawnPos);
            if (playerObj == null)
            {
                Debug.LogError($"[PlayerSpawner] Failed to spawn player prefab: {prefabToSpawn.name}");
                continue;
            }

            playerObj.name = $"Player_{i}";

            PlayerEntity player = playerObj.GetComponent<PlayerEntity>();
            if (player == null) player = playerObj.GetComponentInChildren<PlayerEntity>();

            if (player != null)
            {
                player.SetPlayerIndex(i);

                // ── FIX: always anchor root transform to the instantiated object
                // (in case PlayerEntity.cs is on a child object inside the prefab)
                player.SetRootTransform(playerObj.transform);

                if (showDebugLogs)
                    Debug.Log($"[PlayerSpawner] Spawned {playerObj.name} at {spawnPos} (index {i})");
            }
            else
            {
                Debug.LogWarning($"[PlayerSpawner] '{playerObj.name}' has no PlayerEntity component! Camera follow and PlayerController registration will not work.");
            }

            // ── Wire HealthBar — always runs, regardless of Player component ──────
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
        }
    }

    /// <summary>
    /// Spawns a single player prefab at the position. Uses PhotonNetwork.Instantiate when connected and in a room.
    /// Returns the spawned GameObject (or null on failure).
    /// </summary>
    private GameObject SpawnOne(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return null;

        // If Photon is connected and in a room, prefer PhotonNetwork.Instantiate so networked players are created across clients.
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            try
            {
                if (showDebugLogs) Debug.Log($"[PlayerSpawner] PhotonNetwork.Instantiate: {prefab.name} at {position}");
                GameObject netObj = PhotonNetwork.Instantiate(prefab.name, position, Quaternion.identity);
                return netObj;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[PlayerSpawner] Photon Instantiate failed for '{prefab.name}': {ex.Message}. Falling back to local Instantiate.");
                return Instantiate(prefab, position, Quaternion.identity);
            }
        }

        // Offline fallback or not yet in room
        if (showDebugLogs) Debug.Log($"[PlayerSpawner] Local Instantiate: {prefab.name} at {position}");
        return Instantiate(prefab, position, Quaternion.identity);
    }

    /// <summary>
    /// External API: spawn a selected prefab by name. This is intended to be called
    /// after a player chooses a character in the UI. If the prefab name matches one
    /// of the configured playerPrefabs, that prefab will be used; otherwise, the
    /// method will attempt to load the prefab from Resources by name.
    /// If the client is not yet in a Photon room, the spawn will be queued and executed when OnJoinedRoom fires.
    /// </summary>
    public void SpawnSelectedPlayerFromName(string prefabName)
    {
        if (string.IsNullOrEmpty(prefabName))
        {
            Debug.LogError("[PlayerSpawner] SpawnSelectedPlayerFromName called with empty name.");
            return;
        }

        // Find matching prefab in the configured list
        GameObject selected = null;
        if (playerPrefabs != null)
        {
            foreach (var p in playerPrefabs)
            {
                if (p != null && p.name == prefabName)
                {
                    selected = p;
                    break;
                }
            }
        }

        // Try Resources load as fallback
        if (selected == null)
        {
            GameObject res = Resources.Load<GameObject>(prefabName);
            if (res != null)
            {
                selected = res;
                if (showDebugLogs) Debug.Log($"[PlayerSpawner] Loaded prefab '{prefabName}' from Resources.");
            }
        }

        if (selected == null)
        {
            Debug.LogError($"[PlayerSpawner] Could not find player prefab named '{prefabName}'. Make sure it's assigned in playerPrefabs or located in Resources/.");
            return;
        }

        // If Photon is connected but not yet in a room, queue spawn until OnJoinedRoom.
        if (PhotonNetwork.IsConnected && !PhotonNetwork.InRoom)
        {
            pendingSpawnPrefabName = prefabName;
            pendingSpawnCount = 1;
            if (showDebugLogs) Debug.Log($"[PlayerSpawner] Queued spawn for '{prefabName}' until OnJoinedRoom.");
            return;
        }

        // If not connected at all but waitForSelection is true, still spawn locally.
        if (!PhotonNetwork.IsConnected)
        {
            if (showDebugLogs) Debug.Log("[PlayerSpawner] Photon not connected — spawning locally.");
            SpawnPlayersWithSpecificPrefab(selected, 1);
            return;
        }

        // Otherwise spawn immediately (either offline fallback or already in room)
        SpawnPlayersWithSpecificPrefab(selected, 1);
    }

    private void SpawnPlayersWithSpecificPrefab(GameObject prefab, int count)
    {
        if (prefab == null) return;
        List<Vector3> positions = BuildSpawnPositions(count);

        for (int i = 0; i < count; i++)
        {
            GameObject playerObj = SpawnOne(prefab, positions[i]);
            if (playerObj == null) continue;

            playerObj.name = $"Player_{i}";
            PlayerEntity player = playerObj.GetComponent<PlayerEntity>() ?? playerObj.GetComponentInChildren<PlayerEntity>();
            if (player != null)
            {
                player.SetPlayerIndex(i);
                player.SetRootTransform(playerObj.transform);
            }
        }
    }

    // ─── Photon Callbacks ─────────────────────────────────────────────────────
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        if (showDebugLogs) Debug.Log("[PlayerSpawner] OnJoinedRoom fired.");

        if (!string.IsNullOrEmpty(pendingSpawnPrefabName))
        {
            string prefabName = pendingSpawnPrefabName;
            pendingSpawnPrefabName = null;
            if (showDebugLogs) Debug.Log($"[PlayerSpawner] Processing pending spawn: {prefabName}");
            SpawnSelectedPlayerFromName(prefabName);
        }
    }

    // ─── Position helpers ─────────────────────────────────────────────────────────

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
                Vector3 ptPos = pt != null ? pt.position : GetRandomPosition();
                ptPos.z = -1f;
                result.Add(ptPos);
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
        return new Vector3(mapCenter.x + offset.x, mapCenter.y + offset.y, -1f);
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
