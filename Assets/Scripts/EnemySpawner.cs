using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class EnemySpawner : MonoBehaviour
{
    [FormerlySerializedAs("enemyPrefab")] [SerializeField] private GameObject[] enemyPrefabs; // This is now an array for p2
    [SerializeField] private float initialSpawnInterval = 5f;
    [SerializeField] private float minimumSpawnInterval = 0.5f; // Max difficulty
    [SerializeField] private float difficultyMultiplier = 0.90f; // Every spawn, interval becomes 90% of what it was

    private float currentSpawnInterval;

    void Start()
    {
        currentSpawnInterval = initialSpawnInterval;
        StartCoroutine(SpawnEnemyRoutine());
    }

    private IEnumerator SpawnEnemyRoutine()
    {
        while (true) 
        {
            yield return new WaitForSeconds(currentSpawnInterval);

            if (enemyPrefabs.Length > 0)
            {
                GameObject enemyToSpawn = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                Instantiate(enemyToSpawn, transform.position, Quaternion.identity);
            }
            
            // Increase difficulty
            currentSpawnInterval *= difficultyMultiplier;
            currentSpawnInterval = Mathf.Max(currentSpawnInterval, minimumSpawnInterval);
        }
    }
}