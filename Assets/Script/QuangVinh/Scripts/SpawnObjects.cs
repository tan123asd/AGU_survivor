using System.Collections;
using UnityEngine;

public class SpawnObjects : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] healthPotionPrefabs;

    [Header("Spawn Settings")]
    [Tooltip("Time between each spawn wave (seconds)")]
    public float spawnInterval = 15f;

    [Tooltip("How many health potions to spawn per wave")]
    public int healthPotionsPerWave = 1;

    [Tooltip("Min distance from player to spawn")]
    public float minSpawnDistance = 3f;

    [Tooltip("Max distance from player to spawn")]
    public float maxSpawnDistance = 8f;

    [Tooltip("Max health potions alive at the same time")]
    public int maxHealthPotions = 3;

    private Transform playerTransform;
    private int currentHealthPotionCount;

    private bool TryResolvePlayerTransform()
    {
        if (playerTransform != null) return true;

        if (PlayerController.Instance != null)
        {
            PlayerEntity localPlayer = PlayerController.Instance.GetLocalPlayer();
            if (localPlayer != null)
            {
                playerTransform = localPlayer.RootTransform != null
                    ? localPlayer.RootTransform
                    : localPlayer.transform;
                return true;
            }
        }

        // Backward-compatible fallback for scenes without PlayerController setup.
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            return true;
        }

        return false;
    }

    void Start()
    {
        TryResolvePlayerTransform();

        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (!TryResolvePlayerTransform()) continue;

            // Spawn health potions
            for (int i = 0; i < healthPotionsPerWave; i++)
            {
                if (currentHealthPotionCount >= maxHealthPotions) break;
                if (healthPotionPrefabs.Length == 0) break;

                int index = Random.Range(0, healthPotionPrefabs.Length);
                Vector2 pos = RandomPositionAroundPlayer();
                GameObject obj = Instantiate(healthPotionPrefabs[index], pos, Quaternion.identity);
                currentHealthPotionCount++;
                TrackDestruction(obj);
            }
        }
    }

    private Vector2 RandomPositionAroundPlayer()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(minSpawnDistance, maxSpawnDistance);

        Vector2 playerPos = playerTransform.position;
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;

        return playerPos + offset;
    }

    private void TrackDestruction(GameObject obj)
    {
        var tracker = obj.AddComponent<SpawnedObjectTracker>();
        tracker.Init(this);
    }

    public void OnSpawnedObjectDestroyed()
    {
        currentHealthPotionCount--;
    }
}
