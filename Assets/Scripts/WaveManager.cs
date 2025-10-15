using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Used for sorting and summing

public class WaveComposition
{
    public List<GameObject> TanksToSpawn = new List<GameObject>();
    public List<GameObject> OtherEnemiesToSpawn = new List<GameObject>();
}

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    public enum WaveState { Preparation, Combat, WaveComplete }
    public WaveState CurrentState { get; private set; }

    [Header("Wave Configuration")]
    [SerializeField] private float preparationTime = 30f;
    [SerializeField] private int baseWaveCredits = 50;
    [SerializeField] private float tankHeadstartDelay = 2.0f; // Delay after tanks spawn
    [SerializeField] private List<EnemySpawnData> enemyTypes = new List<EnemySpawnData>();
    
    //A private list to dynamically hold the registered spawn points.
    private List<Transform> spawnPoints = new List<Transform>();

    private int waveNumber = 0;
    private float prepCountdown;
    private List<BaseEnemy> activeEnemies = new List<BaseEnemy>();

    // FIX: This will store the wave composition that is generated at the START of the prep phase.
    private WaveComposition nextWaveComposition;

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

            // Check if activeEnemies is not null before checking count to be safe.
            if (activeEnemies != null && activeEnemies.Count == 0)
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

        // FIX: Generate the wave here, based on the state of the board *before* the prep phase.
        // The result is stored and will be used when the combat phase begins.
        nextWaveComposition = GenerateWave();
    }

    private void StartCombatPhase()
    {
        CurrentState = WaveState.Combat;
        UIManager.Instance.ShowStartWaveButton(false);
        UIManager.Instance.UpdateCountdownText(0);
        Debug.Log($"Starting Combat for Wave {waveNumber}");

        // FIX: The wave has already been generated. Spawn the pre-calculated wave.
        if (nextWaveComposition != null)
        {
            StartCoroutine(SpawnWaveRoutine(nextWaveComposition));
        }
        else
        {
            Debug.LogError("Next wave composition was not generated. Cannot start combat phase.");
        }
    }

    private void EndCombatPhase()
    {
        CurrentState = WaveState.WaveComplete;
        Debug.Log($"Wave {waveNumber} Complete!");
        // Add a small delay before starting the next prep phase
        Invoke("StartPreparationPhase", 3f);
    }
    
    private WaveComposition GenerateWave()
    {
        int totalCredits = CalculateWaveCredits(); 
        Dictionary<GameObject, float> adjustedWeights = CalculateEnemyWeights();

        // The list of enemies we will "purchase"
        List<GameObject> purchasedEnemies = new List<GameObject>();
        
        if (enemyTypes == null || enemyTypes.Count == 0)
        {
            Debug.LogError("No enemy types configured in WaveManager!");
            return new WaveComposition(); // Return an empty wave
        }
        var sortedEnemyTypes = enemyTypes.OrderByDescending(e => e.cost).ToList(); // Prioritise Expensive units
        int cheapestCost = enemyTypes.Min(e => e.cost); // So we dont compare totalCredits to 0

        while (totalCredits >= cheapestCost)
        {
            GameObject enemyToSpawn = GetRandomEnemyWeighted(adjustedWeights);
            EnemySpawnData enemyData = enemyTypes.First(e => e.enemyPrefab == enemyToSpawn);

            if (totalCredits >= enemyData.cost)
            {
                purchasedEnemies.Add(enemyToSpawn);
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
                        purchasedEnemies.Add(cheaperEnemy.enemyPrefab);
                        totalCredits -= cheaperEnemy.cost;
                        foundCheaper = true;
                        break;
                    }
                }
                if (!foundCheaper) break; // Cant afford anything else
            }
        }
        // Logged the count from the correct list 'purchasedEnemies'.
        Debug.Log($"Generated wave with {purchasedEnemies.Count} enemies.");
        
        // Now, sort the purchased enemies into tanks and others.
        WaveComposition finalComposition = new WaveComposition();
        foreach (var enemyPrefab in purchasedEnemies)
        {
            if (enemyPrefab.GetComponent<TankEnemy>())
            {
                finalComposition.TanksToSpawn.Add(enemyPrefab);
            }
            else
            {
                finalComposition.OtherEnemiesToSpawn.Add(enemyPrefab);
            }
        }
        return finalComposition;
    }

    private int CalculateWaveCredits()
    {
        int towerThreat = CalculateTowerThreat();
        int totalCredits = (baseWaveCredits * waveNumber) + towerThreat;
        Debug.Log($"Wave has {totalCredits} credits. (Base: {baseWaveCredits * waveNumber}, Tower Threat: {towerThreat})");
        return totalCredits;
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
        //CALCULATE TOTAL THREAT PER CATEGORY
        // This dynamically calculates threat by iterating through towers,using their  'threatValue'
        float totalResourceThreat = 0;
        float totalMortarThreat = 0;
        float totalLaserThreat = 0;

        BaseTower[] allTowers = FindObjectsOfType<BaseTower>();
        foreach(var tower in allTowers)
        {
            if (tower is ResourceTower) totalResourceThreat += tower.threatValue;
            else if (tower is AoETower) totalMortarThreat += tower.threatValue;
            else if (tower is LaserTower) totalLaserThreat += tower.threatValue;
        }

        // The total of all specialist threats. This is the denominator.
        float grandTotalThreat = totalResourceThreat + totalMortarThreat + totalLaserThreat;
        if (grandTotalThreat < 1) grandTotalThreat = 1; // Prevent division by zero

    
        // CALCULATE A DIRECT, PROPORTIONAL BONUS FOR EACH COUNTER
        Dictionary<GameObject, float> weights = new Dictionary<GameObject, float>();
    
        // This value determines how strongly the AI reacts to your strategy.
        const float COUNTER_POTENCY = 500f; 

        foreach (var enemyData in enemyTypes)
        {
            float counterBonus = 0;
        
            // Fast Enemies get a bonus directly proportional to the Resource Tower threat.
            if (enemyData.enemyPrefab.GetComponent<FastEnemy>())
            {
                float resourceRatio = totalResourceThreat / grandTotalThreat;
                counterBonus = COUNTER_POTENCY * resourceRatio;
                Debug.Log($"FastEnemy Bonus Calculation: {COUNTER_POTENCY} * ({totalResourceThreat} / {grandTotalThreat}) = {counterBonus}");
            }
            // Tank Enemies get a bonus directly proportional to the Mortar Tower threat.
            else if (enemyData.enemyPrefab.GetComponent<TankEnemy>())
            {
                float mortarRatio = totalMortarThreat / grandTotalThreat;
                counterBonus = COUNTER_POTENCY * mortarRatio;
                Debug.Log($"TankEnemy Bonus Calculation: {COUNTER_POTENCY} * ({totalMortarThreat} / {grandTotalThreat}) = {counterBonus}");
            }
            // Basic Enemies get a bonus directly proportional to the Laser Tower threat.
            else if (enemyData.enemyPrefab.GetComponent<BasicEnemy>())
            {
                float laserRatio = totalLaserThreat / grandTotalThreat;
                counterBonus = COUNTER_POTENCY * laserRatio;
                Debug.Log($"BasicEnemy Bonus Calculation: {COUNTER_POTENCY} * ({totalLaserThreat} / {grandTotalThreat}) = {counterBonus}");
            }

            // The final weight is simply the base + its calculated bonus.
            float finalWeight = enemyData.baseSpawnWeight + counterBonus;
            weights[enemyData.enemyPrefab] = Mathf.Max(1f, finalWeight);
        }
    
        // Log the final weights for debugging
        foreach(var w in weights) { Debug.Log($"--- Final Weight for {w.Key.name}: {w.Value}"); }

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

    private IEnumerator SpawnWaveRoutine(WaveComposition wave)
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogError("No spawn points have been registered with the WaveManager");
            yield break; // Stop the coroutine
        }

        // SPAWN TANKS
        Debug.Log($"Spawning {wave.TanksToSpawn.Count} tanks...");
        foreach (GameObject enemyPrefab in wave.TanksToSpawn)
        {
            Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            GameObject enemyGO = Instantiate(enemyPrefab, randomSpawnPoint.position, randomSpawnPoint.rotation);
            activeEnemies.Add(enemyGO.GetComponent<BaseEnemy>());
        }

        // WAIT (if necessary)
        // only wait if there were tanks AND there are other enemies to spawn.
        if (wave.TanksToSpawn.Count > 0 && wave.OtherEnemiesToSpawn.Count > 0)
        {
            // FIX: Corrected comment to match the code below it.
            Debug.Log($"Spawning {wave.OtherEnemiesToSpawn.Count} other enemies after a delay...");
            yield return new WaitForSeconds(tankHeadstartDelay);
        }

        //SPAWN THE REST
        foreach (GameObject enemyPrefab in wave.OtherEnemiesToSpawn)
        {
            Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            GameObject enemyGO = Instantiate(enemyPrefab, randomSpawnPoint.position, randomSpawnPoint.rotation);
            activeEnemies.Add(enemyGO.GetComponent<BaseEnemy>());
        }
    }
}