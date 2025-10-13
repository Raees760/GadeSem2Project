using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Used for sorting and summing

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    public enum WaveState { Preparation, Combat, WaveComplete }
    public WaveState CurrentState { get; private set; }

    [Header("Wave Configuration")]
    [SerializeField] private float preparationTime = 30f;
    [SerializeField] private int baseWaveCredits = 50;
    [SerializeField] private List<EnemySpawnData> enemyTypes = new List<EnemySpawnData>();
    [SerializeField] private TowerWeightModifier weightModifiers;
    
//A private list to dynamically hold the registered spawn points.
    private List<Transform> spawnPoints = new List<Transform>();

    private int waveNumber = 0;
    private float prepCountdown;
    private List<BaseEnemy> activeEnemies = new List<BaseEnemy>();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Start the game with the first preparation phase
        StartPreparationPhase();
    }

    void Update()
    {
        if (CurrentState == WaveState.Preparation)
        {
            prepCountdown -= Time.deltaTime;
            UIManager.Instance.UpdateCountdownText(prepCountdown);

            if (prepCountdown <= 0)
            {
                StartCombatPhase();
            }
        }
        else if (CurrentState == WaveState.Combat)
        {
            // Clean up list of destroyed enemies
            activeEnemies.RemoveAll(enemy => enemy == null);

            if (activeEnemies.Count == 0)
            {
                EndCombatPhase();
            }
        }
    }
    
    /// Allows  TerrainGenerator to register a spawn point at runtime
    public void RegisterSpawnPoint(Transform spawnPoint)
    {
        if (spawnPoint != null && !spawnPoints.Contains(spawnPoint))
        {
            spawnPoints.Add(spawnPoint);
            Debug.Log($"Spawn point registered: {spawnPoint.name}");
        }
    }

    public void StartNextWaveButton()
    {
        if (CurrentState == WaveState.Preparation)
        {
            StartCombatPhase();
        }
    }

    private void StartPreparationPhase()
    {
        CurrentState = WaveState.Preparation;
        prepCountdown = preparationTime;
        waveNumber++;
        UIManager.Instance.UpdateWaveText(waveNumber);
        UIManager.Instance.UpdateCountdownText(prepCountdown);
        UIManager.Instance.ShowStartWaveButton(true);
        Debug.Log($"Starting Preparation for Wave {waveNumber}");
    }

    private void StartCombatPhase()
    {
        CurrentState = WaveState.Combat;
        UIManager.Instance.ShowStartWaveButton(false);
        UIManager.Instance.UpdateCountdownText(0);
        Debug.Log($"Starting Combat for Wave {waveNumber}");

        // Generate and spawn the wave
        List<GameObject> waveComposition = GenerateWave();
        SpawnWave(waveComposition);
    }

    private void EndCombatPhase()
    {
        CurrentState = WaveState.WaveComplete;
        Debug.Log($"Wave {waveNumber} Complete!");
        // Optional: Add a small delay before starting the next prep phase
        Invoke("StartPreparationPhase", 3f);
    }
    
    private List<GameObject> GenerateWave()
    {
        // Calculate Credits
        int towerThreat = CalculateTowerThreat();
        int totalCredits = (baseWaveCredits * waveNumber) + towerThreat;
        Debug.Log($"This wave has {totalCredits} credits. (Base: {baseWaveCredits * waveNumber}, Tower Threat: {towerThreat})");

        // Calculate Weights
        Dictionary<GameObject, float> adjustedWeights = CalculateEnemyWeights();

        // Purchase enemies
        List<GameObject> waveComposition = new List<GameObject>();
        
        // Need to sort for when we have 19 or less credits to prevent wastage of credits
        var sortedEnemyTypes = enemyTypes.OrderByDescending(e => e.cost).ToList(); // Prioritize expensive units
        int cheapestCost = enemyTypes.Min(e => e.cost);

        while (totalCredits >= cheapestCost)
        {
            GameObject enemyToSpawn = GetRandomEnemyWeighted(adjustedWeights);
            EnemySpawnData enemyData = enemyTypes.First(e => e.enemyPrefab == enemyToSpawn);

            if (totalCredits >= enemyData.cost)
            {
                waveComposition.Add(enemyToSpawn);
                totalCredits -= enemyData.cost;
            }
            else
            {
                // Can't afford the chosen one, try to find one we CAN afford
                bool foundCheaper = false;
                foreach (var cheaperEnemy in sortedEnemyTypes)
                {
                    if (totalCredits >= cheaperEnemy.cost)
                    {
                        waveComposition.Add(cheaperEnemy.enemyPrefab);
                        totalCredits -= cheaperEnemy.cost;
                        foundCheaper = true;
                        break;
                    }
                }
                if (!foundCheaper) break; // Can't afford anything else
            }
        }
        Debug.Log($"Generated wave with {waveComposition.Count} enemies.");
        return waveComposition;
    }

    private int CalculateTowerThreat()
    {
        int totalThreat = 0;
        BaseTower[] allTowers = FindObjectsOfType<BaseTower>();
        foreach (BaseTower tower in allTowers)
        {
            totalThreat += tower.threatValue;
        }
        return totalThreat;
    }

    private Dictionary<GameObject, float> CalculateEnemyWeights()
    {
        // Count tower types from the previous wave
        int resourceTowers = FindObjectsOfType<ResourceTower>().Length;
        int mortarTowers = FindObjectsOfType<AoETower>().Length;
        int laserTowers = FindObjectsOfType<LaserTower>().Length;

        Dictionary<GameObject, float> weights = new Dictionary<GameObject, float>();
        foreach(var enemyData in enemyTypes)
        {
            float currentWeight = enemyData.baseSpawnWeight;

            // Apply modifiers based on tower counts
            if (enemyData.enemyPrefab.GetComponent<FastEnemy>())
            {
                if (resourceTowers > 2) currentWeight *= weightModifiers.vsResourceTowers_FastEnemy_Modifier;
                if (mortarTowers > 1) currentWeight *= weightModifiers.vsMortarTowers_FastEnemy_Reducer;
                if (laserTowers > 1) currentWeight *= weightModifiers.vsLaserTowers_Swarm_Modifier;
            }
            else if (enemyData.enemyPrefab.GetComponent<TankEnemy>())
            {
                if (mortarTowers > 1) currentWeight *= weightModifiers.vsMortarTowers_TankEnemy_Modifier;
                if (laserTowers > 2) currentWeight *= weightModifiers.vsLaserTowers_TankEnemy_Reducer;
            }
            else if (enemyData.enemyPrefab.GetComponent<BasicEnemy>())
            {
                 if (laserTowers > 1) currentWeight *= weightModifiers.vsLaserTowers_Swarm_Modifier;
            }
            weights[enemyData.enemyPrefab] = currentWeight;
        }
        return weights;
    }

    private GameObject GetRandomEnemyWeighted(Dictionary<GameObject, float> weights)
    {
        float totalWeight = weights.Sum(x => x.Value);
        float randomPoint = Random.Range(0, totalWeight);

        foreach (var entry in weights)
        {
            if (randomPoint < entry.Value)
            {
                return entry.Key;
            }
            else
            {
                randomPoint -= entry.Value;
            }
        }
        return weights.Keys.First(); // Fallback
    }

    private void SpawnWave(List<GameObject> waveComposition)
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogError("No spawn points assigned in WaveManager!");
            return;
        }

        foreach (GameObject enemyPrefab in waveComposition)
        {
            Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            GameObject enemyGO = Instantiate(enemyPrefab, randomSpawnPoint.position, randomSpawnPoint.rotation);
            activeEnemies.Add(enemyGO.GetComponent<BaseEnemy>());
        }
    }
}