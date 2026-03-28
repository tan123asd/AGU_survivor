using System.Collections;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    [Header("Normal Enemy Settings")]
    public GameObject[] enemyPrefabs;
    private int waveNumber = 5;
    public Camera mainCamera;

    [Header("Boss Settings")]
    [SerializeField] private GameObject bossPrefab; // Prefab của boss
    [SerializeField] private MapConfig mapConfig; // Reference đến MapConfig

    private Coroutine normalSpawnCoroutine;
    private Coroutine bossSpawnCoroutine;
    private bool isBossPhase = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // ✅ CHỈ CHẠY KHI Ở PLAY MODE
        if (!Application.isPlaying) return;

        // Tìm camera nếu chưa assign
        if (mainCamera == null)
            mainCamera = Camera.main;

        // Bắt đầu spawn enemy thường
        SpawnWave(waveNumber);
        normalSpawnCoroutine = StartCoroutine(SpawnWaves());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator SpawnWaves()
    {
        while (!isBossPhase) // Chỉ spawn khi chưa vào boss phase
        {
            yield return new WaitForSeconds(5f);

            // Kiểm tra xem có đang ở boss phase không
            if (MapManager.Instance != null && MapManager.Instance.IsBossPhase)
            {
                isBossPhase = true;
                break;
            }

            SpawnWave(waveNumber);
        }
    }

    private void SpawnWave(int waveNumber)
    {
        for (int i = 0; i < waveNumber; i++)
        {
            int randomIndex = Random.Range(0, enemyPrefabs.Length);
            Instantiate(enemyPrefabs[randomIndex], RandomPosition(), enemyPrefabs[randomIndex].transform.rotation);
        }
        waveNumber++;
    }

    /// <summary>
    /// Bắt đầu spawn boss (được gọi bởi MapManager khi hết time limit).
    /// </summary>
    public void StartBossSpawn()
    {
        if (isBossPhase) return;

        isBossPhase = true;

        // Spawn boss đầu tiên
        int initialBossCount = mapConfig != null ? mapConfig.InitialBossCount : 1;
        for (int i = 0; i < initialBossCount; i++)
        {
            SpawnSingleBoss();
        }

        Debug.Log($"Boss phase started! Spawned {initialBossCount} initial boss(es)");
    }

    /// <summary>
    /// Spawn 1 boss tại vị trí ngẫu nhiên.
    /// </summary>
    public void SpawnSingleBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogError("Boss prefab is not assigned!");
            return;
        }

        Vector2 spawnPos = RandomPosition();
        GameObject bossObj = Instantiate(bossPrefab, spawnPos, Quaternion.identity);

        Debug.Log($"Spawned BOSS at position {spawnPos}");
    }

    private Vector2 RandomPosition()
    {
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        // ✅ LẤY VỊ TRÍ CAMERA (sửa bug spawn ở (0,0))
        Vector2 cameraPos = mainCamera.transform.position;

        int side = Random.Range(0, 4);
        Vector2 spawnPosition = Vector2.zero;

        switch (side)
        {
            case 0: // Top
                spawnPosition = cameraPos + new Vector2(Random.Range(-cameraWidth / 2, cameraWidth / 2), cameraHeight / 2 + 1);
                break;
            case 1: // Bottom
                spawnPosition = cameraPos + new Vector2(Random.Range(-cameraWidth / 2, cameraWidth / 2), -cameraHeight / 2 - 1);
                break;
            case 2: // Left 
                spawnPosition = cameraPos + new Vector2(-cameraWidth / 2 - 1, Random.Range(-cameraHeight / 2, cameraHeight / 2));
                break;
            case 3: // Right
                spawnPosition = cameraPos + new Vector2(cameraWidth / 2 + 1, Random.Range(-cameraHeight / 2, cameraHeight / 2));
                break;
        }

        return spawnPosition;
    }
}
