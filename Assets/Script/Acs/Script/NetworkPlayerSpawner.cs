using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

/// <summary>
/// Multiplayer-aware player spawner for the PlayerControllerScene.
///
/// HOW IT WORKS:
///   - Each client calls PhotonNetwork.Instantiate() for its OWN player only.
///   - The prefab MUST be inside Assets/Resources/ named "NetworkPlayer".
///   - Use SpawnPoint components in the scene to define spawn positions.
///   - After spawning, only the local (IsMine) player gets the camera wired.
///   - HealthBar canvas is also instantiated locally for the owning client only.
///
/// SETUP IN UNITY:
///   1. Create a prefab named "NetworkPlayer" and place it in Assets/Resources/.
///   2. Add PhotonView + PhotonTransformView to that prefab.
///   3. Place SpawnPoint GameObjects in the scene and drag them into spawnPoints[].
///   4. Attach this script to a GameObject in PlayerControllerScene.
///   5. (Optional) Assign healthBarCanvasPrefab to show a local health bar.
/// </summary>
public class NetworkPlayerSpawner : MonoBehaviourPunCallbacks
{
    // ─── Inspector ────────────────────────────────────────────────────────────

    [Header("Network Player Prefab")]
    [Tooltip("Name of the player prefab inside Assets/Resources/. Must match exactly.")]
    [SerializeField] private string networkPlayerPrefabName = "NetworkPlayer";

    [Header("Health Bar UI (Local Only)")]
    [Tooltip("HealthBar canvas prefab instantiated only for the owning client.")]
    [SerializeField] private GameObject healthBarCanvasPrefab;

    [Header("Spawn Points")]
    [Tooltip("Drag SpawnPoint GameObjects here. Each client picks a different one.")]
    [SerializeField] private SpawnPoint[] spawnPoints;

    [Header("Random Spawn Fallback")]
    [Tooltip("Radius around mapCenter used when no spawnPoints are assigned.")]
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private Vector2 mapCenter = Vector2.zero;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    // ─── State ────────────────────────────────────────────────────────────────

    /// <summary>The local (IsMine) player GameObject after spawning.</summary>
    private GameObject _localPlayerObj;

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    private void Start()
    {
        // If already in the room, spawn straight away.
        // If not yet joined (rare race condition), wait via OnJoinedRoom.
        if (PhotonNetwork.InRoom)
        {
            SpawnLocalPlayer();
        }
        else
        {
            if (showDebugLogs)
                Debug.Log("[NetworkPlayerSpawner] Not in room yet — waiting for OnJoinedRoom callback.");
        }
    }

    /// <summary>
    /// Fallback: if Start() fires before the client is fully in the room,
    /// this callback will trigger the spawn.
    /// </summary>
    public override void OnJoinedRoom()
    {
        if (_localPlayerObj == null)
            SpawnLocalPlayer();
    }

    // ─── Spawn Logic ──────────────────────────────────────────────────────────

    private void SpawnLocalPlayer()
    {
        // ── 1. Pick spawn position ────────────────────────────────────────────
        Vector3 spawnPos = GetSpawnPosition();

        // ── 2. Network-instantiate the player prefab ──────────────────────────
        // PhotonNetwork.Instantiate creates the prefab on ALL clients,
        // but only this client owns it (photonView.IsMine == true here).
        _localPlayerObj = PhotonNetwork.Instantiate(
            networkPlayerPrefabName,
            spawnPos,
            Quaternion.identity
        );

        if (_localPlayerObj == null)
        {
            Debug.LogError(
                $"[NetworkPlayerSpawner] PhotonNetwork.Instantiate failed! " +
                $"Make sure '{networkPlayerPrefabName}' exists in Assets/Resources/."
            );
            return;
        }

        if (showDebugLogs)
            Debug.Log($"[NetworkPlayerSpawner] Spawned local player at {spawnPos}");

        // ── 3. Local-only setup (camera, health bar) ──────────────────────────
        WireCamera(_localPlayerObj);
        WireHealthBar(_localPlayerObj);
    }

    // ─── Camera Wiring ────────────────────────────────────────────────────────

    /// <summary>
    /// Tells CameraDirector to follow the local player's transform.
    /// Only runs for the owning client.
    /// </summary>
    private void WireCamera(GameObject playerObj)
    {
        CameraDirector cam = FindObjectOfType<CameraDirector>();
        if (cam == null)
        {
            Debug.LogWarning("[NetworkPlayerSpawner] No CameraDirector found in scene.");
            return;
        }

        cam.SetTarget(playerObj.transform);

        if (showDebugLogs)
            Debug.Log($"[NetworkPlayerSpawner] CameraDirector now follows: {playerObj.name}");
    }

    // ─── HealthBar Wiring ─────────────────────────────────────────────────────

    /// <summary>
    /// Instantiates a local HealthBar canvas and wires it to the PlayerHealth
    /// component on the spawned player. Runs only on the owning client.
    /// </summary>
    private void WireHealthBar(GameObject playerObj)
    {
        if (healthBarCanvasPrefab == null)
        {
            Debug.LogWarning("[NetworkPlayerSpawner] No healthBarCanvasPrefab assigned — HealthBar UI will not show.");
            return;
        }

        // Spawn canvas at scene root
        GameObject canvasObj = Instantiate(healthBarCanvasPrefab);
        canvasObj.name = "HealthBarCanvas_LocalPlayer";

        // Force Screen Space - Overlay so it always renders on top
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        if (canvas == null) canvas = canvasObj.GetComponentInChildren<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
        }

        // Wire HealthBar → PlayerHealth
        HealthBar healthBar = canvasObj.GetComponentInChildren<HealthBar>(true);
        if (healthBar == null)
        {
            Debug.LogWarning("[NetworkPlayerSpawner] healthBarCanvasPrefab has no HealthBar component!");
            return;
        }

        PlayerHealth ph = playerObj.GetComponentInChildren<PlayerHealth>();
        if (ph == null) ph = playerObj.GetComponent<PlayerHealth>();

        if (ph != null)
        {
            ph.SetHealthBar(healthBar);
            if (showDebugLogs)
                Debug.Log("[NetworkPlayerSpawner] HealthBar wired to local player.");
        }
        else
        {
            Debug.LogWarning("[NetworkPlayerSpawner] No PlayerHealth found on spawned player — HealthBar not wired.");
        }
    }

    // ─── Position Helpers ─────────────────────────────────────────────────────

    /// <summary>
    /// Picks a spawn position for this client.
    /// Each client uses its Photon actor number to pick a different SpawnPoint,
    /// so players don't stack on top of each other.
    /// Falls back to a random position inside spawnRadius if no points are set.
    /// </summary>
    private Vector3 GetSpawnPosition()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            // Use actor number (1-based) to spread players across spawn points
            int actorIndex = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % spawnPoints.Length;
            SpawnPoint point = spawnPoints[actorIndex];

            if (point != null)
            {
                Vector3 pos = point.transform.position;
                pos.z = -1f;
                return pos;
            }
        }

        // Fallback: random within radius
        Vector2 offset = Random.insideUnitCircle * spawnRadius;
        return new Vector3(mapCenter.x + offset.x, mapCenter.y + offset.y, -1f);
    }

    // ─── Photon Callbacks ─────────────────────────────────────────────────────

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (showDebugLogs)
            Debug.Log($"[NetworkPlayerSpawner] Player entered room: {newPlayer.NickName}");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (showDebugLogs)
            Debug.Log($"[NetworkPlayerSpawner] Player left room: {otherPlayer.NickName}");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        // Enemy AI automatically transfers: Enemy.Update() checks
        // PhotonNetwork.IsMasterClient every frame, so the new master
        // immediately takes control of all enemy movement.
        if (showDebugLogs)
            Debug.Log($"[NetworkPlayerSpawner] MasterClient switched to: {newMasterClient.NickName}. Enemy AI transferred automatically.");
    }

    public override void OnLeftRoom()
    {
        // Clean up local player when we leave
        if (_localPlayerObj != null)
        {
            PhotonNetwork.Destroy(_localPlayerObj);
            _localPlayerObj = null;
        }
    }

    // ─── Gizmos (Editor Only) ─────────────────────────────────────────────────
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.3f);
            Gizmos.DrawSphere(new Vector3(mapCenter.x, mapCenter.y, 0f), spawnRadius);
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 1f);
            Gizmos.DrawWireSphere(new Vector3(mapCenter.x, mapCenter.y, 0f), spawnRadius);
        }
    }
#endif
}
