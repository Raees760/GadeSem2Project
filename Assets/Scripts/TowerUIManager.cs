using System.Text;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TowerUIManager : MonoBehaviour
{
    public static TowerUIManager Instance;

    [Header("UI Panel Components")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private Button upgradeButton;

    private BaseTower selectedTower;
    public bool IsPanelOpen => upgradePanel.activeSelf;

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
        upgradePanel.SetActive(false);
        upgradeButton.onClick.AddListener(OnUpgradeButtonPressed);
    }

    public void ShowUpgradePanel(BaseTower tower)
    {
        selectedTower = tower;
        UpdatePanel();
        upgradePanel.SetActive(true);
    }

    public void HideUpgradePanel()
    {
        selectedTower = null;
        upgradePanel.SetActive(false);
    }
    public void UpdatePanel()
    {
        if (selectedTower == null) return;

        // Use a StringBuilder for efficient string creation
        StringBuilder sb = new StringBuilder();
    
        // --- Universal Stats (always shown) ---
        sb.AppendLine($"Level: {selectedTower.CurrentUpgradeLevel + 1}");
        sb.AppendLine($"Health: {selectedTower.Health:F0} / {selectedTower.MaxHealth:F0}");
    
        // --- Context-Specific Stats ---
        if (selectedTower is ResourceTower)
        {
            // We need to get the goldPerInterval value. Let's add a public property to ResourceTower.
            ResourceTower resourceTower = selectedTower as ResourceTower;
            sb.AppendLine($"Gold/10s: {resourceTower.GoldPerInterval}");
        }
        else if (selectedTower is BaseTower) // Catches all other towers (Projectile, Laser, AoE, Main)
        {
            sb.AppendLine($"Fire Rate: {selectedTower.FireRate:F2}");
            sb.AppendLine($"Range: {selectedTower.AttackRange:F1}");
        }
    
        statsText.text = sb.ToString();

        // Check for next upgrade and display its info
        UpgradeLevel nextUpgrade = selectedTower.GetNextUpgrade();
        if (nextUpgrade != null)
        {
            upgradeCostText.text = $"{nextUpgrade.upgradeCost}g";
            upgradeButton.interactable = true;
        }
        else
        {
            upgradeCostText.text = "MAX";
            upgradeButton.interactable = false;
        }
    }
    private void OnUpgradeButtonPressed()
    {
        if (selectedTower != null)
        {
            selectedTower.Upgrade();
        }
    }
}