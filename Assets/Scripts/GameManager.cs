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