using System.Collections;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    private int waveNumber = 5;
    public Camera mainCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnWave(waveNumber);
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(SpawnWaves());
    }

    IEnumerator SpawnWaves()
    {
        yield return new WaitForSeconds(5f);
        SpawnWave(waveNumber);
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

    private Vector2 RandomPosition()
    {
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        int side = Random.Range(0, 4);
        Vector2 spawnPosition = Vector2.zero;

        switch (side)
        {
            case 0: // Top
                spawnPosition = new Vector2(Random.Range(-cameraWidth / 2, cameraWidth / 2), cameraHeight / 2 + 1);
                break;
            case 1: // Bottom
                spawnPosition = new Vector2(Random.Range(-cameraWidth / 2, cameraWidth / 2), -cameraHeight / 2 - 1);
                break;
            case 2: // Left 
                spawnPosition = new Vector2(-cameraWidth / 2 - 1, Random.Range(-cameraHeight / 2, cameraHeight / 2));
                break;
            case 3: // Right
                spawnPosition = new Vector2(cameraWidth / 2 + 1, Random.Range(-cameraHeight / 2, cameraHeight / 2));
                break;
        }

        return spawnPosition;
    }
}
