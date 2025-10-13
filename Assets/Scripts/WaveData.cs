using UnityEngine;

// This makes these classes visible and editable in the Unity Inspector
[System.Serializable]
public class EnemySpawnData
{
    public string name; // For easy identification in the Inspector
    public GameObject enemyPrefab;
    public int cost;
    [Range(0f, 100f)]
    public float baseSpawnWeight;
}

[System.Serializable]
public class TowerWeightModifier
{
    // Define how each tower type affects the spawn weights
    public float vsResourceTowers_FastEnemy_Modifier = 1.5f; // Multiplier for Fast Enemy weight
    public float vsMortarTowers_TankEnemy_Modifier = 1.5f;   // Multiplier for Tank Enemy weight
    public float vsMortarTowers_FastEnemy_Reducer = 0.5f;    // Multiplier to reduce Fast Enemy weight
    public float vsLaserTowers_TankEnemy_Reducer = 0.5f;     // Multiplier to reduce Tank Enemy weight
    public float vsLaserTowers_Swarm_Modifier = 1.25f;       // Multiplier for Basic/Fast weights
}