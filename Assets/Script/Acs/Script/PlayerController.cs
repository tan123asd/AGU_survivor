using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton manager that tracks ALL active PlayerEntity instances.
/// Provides a central API for enemy AI, camera, and game logic.
/// Multiplayer-ready: supports any number of registered players.
/// </summary>
public class PlayerController : MonoBehaviour
{
    // ─── Singleton ────────────────────────────────────────────────────────────
    private static PlayerController _instance;
    public static PlayerController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<PlayerController>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("PlayerController (Auto-Created)");
                    _instance = go.AddComponent<PlayerController>();
                    DontDestroyOnLoad(go);
                    Debug.LogWarning("[PlayerController] No PlayerController found in scene — created one automatically.");
                }
            }
            return _instance;
        }
    }

    // ─── Events ───────────────────────────────────────────────────────────────
    /// <summary>Fired when any player dies.</summary>
    public UnityEvent<PlayerEntity> OnPlayerDied = new UnityEvent<PlayerEntity>();

    /// <summary>Fired when ALL registered players are dead.</summary>
    public UnityEvent OnAllPlayersDied = new UnityEvent();

    // ─── Internal state ───────────────────────────────────────────────────────
    private readonly List<PlayerEntity> _players = new List<PlayerEntity>();

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    // ─── Lifecycle ────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    /// <summary>
    /// Clears the player list on scene load so stale references
    /// from a previous session don't linger (e.g. after returning to lobby).
    /// PlayerEntity.Awake() re-registers each player in the new scene.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _players.Clear();
        if (showDebugLogs)
            Debug.Log($"[PlayerController] Scene loaded '{scene.name}' → player list cleared.");
    }

    // ─── Registration ─────────────────────────────────────────────────────────

    /// <summary>Called by PlayerEntity.Awake() to register itself.</summary>
    public void RegisterPlayer(PlayerEntity player)
    {
        if (!_players.Contains(player))
        {
            _players.Add(player);
            if (showDebugLogs)
                Debug.Log($"[PlayerController] Registered: {player.name} (index {player.PlayerIndex}) | Total players: {_players.Count}");
        }
    }

    /// <summary>Called by PlayerEntity.OnDestroy() to unregister itself.</summary>
    public void UnregisterPlayer(PlayerEntity player)
    {
        if (_players.Remove(player))
        {
            if (showDebugLogs)
                Debug.Log($"[PlayerController] Unregistered: {player.name} | Remaining players: {_players.Count}");
        }
    }

    /// <summary>
    /// Called by a Player when it dies.
    /// Fires OnPlayerDied and, if no players remain alive, OnAllPlayersDied.
    /// </summary>
    public void NotifyPlayerDied(PlayerEntity player)
    {
        OnPlayerDied.Invoke(player);

        if (showDebugLogs)
            Debug.Log($"[PlayerController] Player died: {player.name}");

        // Check if every registered player is dead
        bool anyAlive = false;
        foreach (PlayerEntity p in _players)
        {
            if (p != null && !p.PlayerHealth.IsDead)
            {
                anyAlive = true;
                break;
            }
        }

        if (!anyAlive)
        {
            if (showDebugLogs)
                Debug.Log("[PlayerController] ALL PLAYERS DIED → OnAllPlayersDied event fired!");
            OnAllPlayersDied.Invoke();
        }
    }

    // ─── Queries ──────────────────────────────────────────────────────────────

    /// <summary>Returns a read-only snapshot of all registered players.</summary>
    public IReadOnlyList<PlayerEntity> GetAllPlayers() => _players.AsReadOnly();

    /// <summary>
    /// Returns the local player (index 0).
    /// In a future LAN setup, this would be the local machine's owned player.
    /// </summary>
    public PlayerEntity GetLocalPlayer()
    {
        // Multiplayer: return the player owned by THIS client (PhotonView.IsMine).
        // Single-player: IsLocalPlayer is always true, so first registered wins.
        foreach (PlayerEntity p in _players)
            if (p != null && p.IsLocalPlayer) return p;
        // Fallback: first alive player (should not reach here in normal flow)
        return _players.Count > 0 ? _players[0] : null;
    }

    /// <summary>
    /// Returns the nearest ALIVE player to the given world position.
    /// Used by Enemy AI to decide who to chase.
    /// </summary>
    public PlayerEntity GetNearestPlayer(Vector2 position)
    {
        PlayerEntity nearest = null;
        float minDist = float.MaxValue;

        foreach (PlayerEntity p in _players)
        {
            if (p == null) continue;
            if (p.PlayerHealth != null && p.PlayerHealth.IsDead) continue;

            float dist = Vector2.Distance(position, p.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = p;
            }
        }

        return nearest;
    }

    /// <summary>Enables/disables input for all registered players (e.g. on game-over cutscene).</summary>
    public void SetAllPlayersInputEnabled(bool enabled)
    {
        foreach (PlayerEntity p in _players)
        {
            if (p != null && p.PlayerMovement != null)
                p.PlayerMovement.SetInputEnabled(enabled);
        }
    }
}
