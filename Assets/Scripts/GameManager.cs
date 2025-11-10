using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Resources")]
    [SerializeField] private int startingMoney = 100;
    public int CurrentMoney { get; private set; }

    [Header("Tower Costs")]
    public int projectileTowerCost = 50;
    public int resourceTowerCost = 75; 
    public int laserTowerCost = 125;
    public int aoeTowerCost = 150;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Check if the upgrade panel is open and the click was NOT on a UI element.
            if (TowerUIManager.Instance != null && TowerUIManager.Instance.IsPanelOpen && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                // Fire a ray from the camera to see what we clicked on.
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                // We only care if we hit *something*. If we hit a tower, OnMouseDown on that tower will handle it.
                // if we hit anything else (terrain, or nothing at all), we close the panel.
                // a simple way is to check if we DIDN'T hit a tower.
                if (!Physics.Raycast(ray, out RaycastHit hit) || hit.collider.GetComponent<BaseTower>() == null)
                {
                    TowerUIManager.Instance.HideUpgradePanel();
                }
            }
        }
    }

    void Start()
    {
        CurrentMoney = startingMoney;
        UIManager.Instance.UpdateMoneyText(CurrentMoney);
    }

    public void AddMoney(int amount)
    {
        CurrentMoney += amount;
        UIManager.Instance.UpdateMoneyText(CurrentMoney);
    }

    public bool SpendMoney(int amount)
    {
        if (amount <= CurrentMoney)
        {
            CurrentMoney -= amount;
            UIManager.Instance.UpdateMoneyText(CurrentMoney);
            return true;
        }
        else
        {
            Debug.Log("Not enough money!");
            return false;
        }
    }

    public void GameOver()
    {
        Debug.Log("GAME OVER!");
        // Trigger the game over UI
        UIManager.Instance.ShowGameOverScreen();
        Time.timeScale = 0f; // Pause the game
    }
}