using UnityEngine;

// This class  holds the stats for a single upgrade level.
[System.Serializable]
public class UpgradeLevel
{
    public int upgradeCost;
    public float maxHealth;
    public float fireRate;
    public float attackRange;
    public int threatValue;
    public GameObject visualPrefab; // The new 3D model for this level
}

// Lets us create assets from this class in the Unity Editor
[CreateAssetMenu(fileName = "New Tower Upgrade Path", menuName = "Towers/Upgrade Path")]
public class TowerUpgradePath : ScriptableObject
{
    // Level 0 is the base tower
    public UpgradeLevel[] levels;
}