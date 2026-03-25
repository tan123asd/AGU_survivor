using UnityEngine;

public class ChoosePlayer : MonoBehaviour
{
    [Tooltip("UI panel that contains the character selection buttons")] 
    public GameObject selectionPanel;

    [Tooltip("Optional default prefab name to spawn if none selected")] 
    public string defaultPrefabName;

    private const string PrefKey = "SelectedCharacterPrefab";

    private void Start()
    {
        // Show panel at start
        if (selectionPanel != null)
            selectionPanel.SetActive(true);
    }

    /// <summary>
    /// Called by UI buttons. Stores the chosen prefab name and triggers the spawner.
    /// </summary>
    public void SelectCharacter(string prefabName)
    {
        if (string.IsNullOrEmpty(prefabName))
        {
            Debug.LogError("[ChoosePlayer] prefabName is empty");
            return;
        }

        Debug.Log($"[ChoosePlayer] SelectCharacter called: {prefabName}");

        PlayerPrefs.SetString(PrefKey, prefabName);
        PlayerPrefs.Save();

        // Hide selection UI
        if (selectionPanel != null)
            selectionPanel.SetActive(false);

        // Prefer NetworkPlayerSpawner if present (handles Photon join queuing)
        var netSpawner = FindObjectOfType<NetworkPlayerSpawner>();
        if (netSpawner != null)
        {
            Debug.Log($"[ChoosePlayer] Using NetworkPlayerSpawner to spawn '{prefabName}'");
            netSpawner.SpawnSelectedNetworkPlayer(prefabName);
            return;
        }

        // Fallback: PlayerSpawner
        var spawner = FindObjectOfType<PlayerSpawner>();
        if (spawner != null)
        {
            Debug.Log($"[ChoosePlayer] Using PlayerSpawner to spawn '{prefabName}'");
            spawner.SpawnSelectedPlayerFromName(prefabName);
        }
        else
        {
            Debug.LogWarning("[ChoosePlayer] No PlayerSpawner or NetworkPlayerSpawner found in scene. Make sure a spawner exists.");
        }
    }

    /// <summary>
    /// Call to spawn previously selected prefab (if any) or default.
    /// Useful when reconnecting or skipping selection.
    /// </summary>
    public void SpawnPreviouslySelected()
    {
        string prefab = PlayerPrefs.GetString(PrefKey, defaultPrefabName);
        if (string.IsNullOrEmpty(prefab))
        {
            Debug.LogWarning("[ChoosePlayer] No previously selected prefab and no default set.");
            return;
        }

        // Hide UI
        if (selectionPanel != null)
            selectionPanel.SetActive(false);

        var netSpawner = FindObjectOfType<NetworkPlayerSpawner>();
        if (netSpawner != null)
        {
            netSpawner.SpawnSelectedNetworkPlayer(prefab);
            return;
        }

        var spawner = FindObjectOfType<PlayerSpawner>();
        if (spawner != null)
            spawner.SpawnSelectedPlayerFromName(prefab);
        else
            Debug.LogWarning("[ChoosePlayer] No PlayerSpawner or NetworkPlayerSpawner found in scene.");
    }
}
