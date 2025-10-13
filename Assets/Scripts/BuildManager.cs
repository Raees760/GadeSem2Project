using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    [Header("Tower Prefabs")]
    [SerializeField] private GameObject projectileShooterPrefab;
    [SerializeField] private GameObject resourceTowerPrefab;
    [SerializeField] private GameObject laserTowerPrefab;
    [SerializeField] private GameObject aoeTowerPrefab;
    // Space for other tower prefabs here in the future
    
    public GameObject SelectedTowerPrefab { get; private set; }
    public int SelectedTowerCost { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // This method called by UI buttons
    public void SelectProjectileShooter()
    {
        SelectedTowerPrefab = projectileShooterPrefab;
        SelectedTowerCost = GameManager.Instance.projectileTowerCost;
        Debug.Log("Projectile Tower Selected");
    }
    public void SelectResourceTower()
    {
        SelectedTowerPrefab = resourceTowerPrefab;
        SelectedTowerCost = GameManager.Instance.resourceTowerCost;
        Debug.Log("Resource Tower Selected");
    }
    
    public void SelectLaserTower() 
    {
        SelectedTowerPrefab = laserTowerPrefab;
        SelectedTowerCost = GameManager.Instance.laserTowerCost;
        Debug.Log("Laser Tower Selected");
    }

    public void SelectAoeTower()
    {
        SelectedTowerPrefab = aoeTowerPrefab;
        SelectedTowerCost = GameManager.Instance.aoeTowerCost;
        Debug.Log("AoE Tower Selected");
    }
    // Create methods for other towers
    // public void SelectLaserTower() { ... }
}