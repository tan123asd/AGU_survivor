using System.Collections;
using UnityEngine;

public class SpawnObjects : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] chestPrefabs;
    public GameObject[] destructiveObjectPrefabs;

    [Header("Spawn Settings")]
    [Tooltip("Time between each spawn wave (seconds)")]
    public float spawnInterval = 15f;

    [Tooltip("How many chests to spawn per wave")]
    public int chestsPerWave = 1;

    [Tooltip("How many destructive objects to spawn per wave")]
    public int destructivesPerWave = 2;

    [Tooltip("Min distance from player to spawn")]
    public float minSpawnDistance = 3f;

    [Tooltip("Max distance from player to spawn")]
    public float maxSpawnDistance = 8f;

    [Tooltip("Max objects alive at the same time")]
    public int maxChests = 5;
    public int maxDestructives = 10;

    private Transform playerTransform;
    private int currentChestCount;
    private int currentDestructiveCount;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (playerTransform == null) continue;

            // Spawn chests
            for (int i = 0; i < chestsPerWave; i++)
            {
                if (currentChestCount >= maxChests) break;
                if (chestPrefabs.Length == 0) break;

                int index = Random.Range(0, chestPrefabs.Length);
                Vector2 pos = RandomPositionAroundPlayer();
                GameObject obj = Instantiate(chestPrefabs[index], pos, Quaternion.identity);
                currentChestCount++;
                TrackDestruction(obj, true);
            }

            // Spawn destructive objects
            for (int i = 0; i < destructivesPerWave; i++)
            {
                if (currentDestructiveCount >= maxDestructives) break;
                if (destructiveObjectPrefabs.Length == 0) break;

                int index = Random.Range(0, destructiveObjectPrefabs.Length);
                Vector2 pos = RandomPositionAroundPlayer();
                GameObject obj = Instantiate(destructiveObjectPrefabs[index], pos, Quaternion.identity);
                currentDestructiveCount++;
                TrackDestruction(obj, false);
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

    private void TrackDestruction(GameObject obj, bool isChest)
    {
        // Use a helper component to decrement count when object is destroyed
        var tracker = obj.AddComponent<SpawnedObjectTracker>();
        tracker.Init(this, isChest);
    }

    public void OnSpawnedObjectDestroyed(bool isChest)
    {
        if (isChest)
            currentChestCount--;
        else
            currentDestructiveCount--;
    }
}
