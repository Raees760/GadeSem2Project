using System.Collections;
using UnityEngine;

public abstract class BaseTower : MonoBehaviour
{
    [Header("Upgrade Configuration")]
    [SerializeField] private TowerUpgradePath upgradePath;
    [SerializeField] protected Transform visualsParent; // Parent object for the tower's 3D model

    // These are now controlled by the Upgrade Path
    public float MaxHealth { get; private set; }
    public float FireRate { get; private set; } // 1 shot per second
    public float AttackRange { get; private set; }
    public int ThreatValue { get; private set; } // How much this tower adds to the next wave's credit pool
    
    [Header("Base Tower Logic")]
    [SerializeField] protected float rotationSpeed = 10f;
    [SerializeField] protected Transform partToRotate; // e.g., the turret head

    [Header("UI")]
    [SerializeField] private HealthBar healthBar;
    
    public float Health { get; private set; } 
    public int CurrentUpgradeLevel { get; private set; } 
    protected Transform target;
    protected float attackCooldown = 0f;
    private MaterialPropertyBlock propBlock;
    private Renderer[] objectRenderers;
    private Coroutine flashCoroutine;
    
    // This can be overridden by specific tower types
    //protected virtual float fireRate { get { return 1f; } }
    protected virtual void Start()
    {
        // Apply stats for the initial level (Level 0). This will also cache the renderers.
        ApplyUpgrade(0);
        
    }


    protected virtual void Update()
    {
        if (target == null)
        {
            FindTarget();
        }
        else
        {
            TrackTarget();
            if (attackCooldown <= 0f)
            {
                Attack();
                attackCooldown = 1f / FireRate;
            }
        }
        
        attackCooldown -= Time.deltaTime;
    }

    //This method is called when the tower is clicked on.
    void OnMouseDown()
    {
        //If the click is on the UI or not on the object, don't select the tower.
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        //Tell the ui Manager to show the panel for this specific tower.
        TowerUIManager.Instance.ShowUpgradePanel(this);
        //TO DO: Add indicator which tower is selected
    }

    public void Upgrade()
    {
        // Check if there is a next level to upgrade to.
        if (CurrentUpgradeLevel + 1 >= upgradePath.levels.Length)
        {
            UIManager.Instance.ShowFeedbackMessage("Fully Upgraded!");
            return;
        }

        UpgradeLevel nextLevel = upgradePath.levels[CurrentUpgradeLevel + 1];

        // Check if the player has enough money.
        if (GameManager.Instance.SpendMoney(nextLevel.upgradeCost))
        {
            ApplyUpgrade(CurrentUpgradeLevel + 1);
            TowerUIManager.Instance.UpdatePanel(); // Refresh the UI panel
        }
        else
        {
            UIManager.Instance.ShowFeedbackMessage("Not enough gold!");
        }
    }

    private void ApplyUpgrade(int level)
    {
        CurrentUpgradeLevel = level;
        UpgradeLevel newStats = upgradePath.levels[level];

        //Apply new stats
        MaxHealth = newStats.maxHealth;
        FireRate = newStats.fireRate;
        AttackRange = newStats.attackRange;
        ThreatValue = newStats.threatValue;

        //for the first level, health is set to max. For subsequent upgrades, it's healed.
        if (level == 0)
        {
            Health = MaxHealth;
        }
        else
        {
            //Heal the tower to full on upgrade
            Health = MaxHealth; 
        }
        healthBar.UpdateHealth(Health, MaxHealth);

        // Swap the visual model
        
        //destroy the old visuals
        if (visualsParent.childCount > 0)
        {
            Destroy(visualsParent.GetChild(0).gameObject);
        }
        
        // Instantiate the new visuals and keep a reference to it
        GameObject newVisualsGO = Instantiate(newStats.visualPrefab, visualsParent.position, visualsParent.rotation, visualsParent);
    
        // Search the new visuals for the part to rotate
        TowerPartToRotate foundPart = newVisualsGO.GetComponentInChildren<TowerPartToRotate>();
        if (foundPart != null)
        {
            this.partToRotate = foundPart.transform;
        }
        else
        {
            // If no rotating part is found (like for a ResourceTower), set it to null.
            this.partToRotate = null;
        }
    
        // Notify child classes that the visuals have changed so they can find their own parts.
        OnUpgradeApplied(newVisualsGO);
        
        Debug.Log($"{gameObject.name} upgraded to Level {level + 1}");
        CacheRenderers();
    }
    private void CacheRenderers()
    {
        propBlock = new MaterialPropertyBlock();
        // Get all Renderer components in the children of this tower.
        objectRenderers = GetComponentsInChildren<Renderer>();
    }
    
    // Helping method for the ui to get next upgrade info
    public UpgradeLevel GetNextUpgrade()
    {
        if (CurrentUpgradeLevel + 1 < upgradePath.levels.Length)
        {
            return upgradePath.levels[CurrentUpgradeLevel + 1];
        }
        return null; // No more upgrades
    }

    protected virtual void FindTarget()
    {
        //Find the closest enemy
        BaseEnemy[] enemies = FindObjectsOfType<BaseEnemy>();
        float shortestDistance = Mathf.Infinity;
        BaseEnemy nearestEnemy = null;

        foreach (BaseEnemy enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && shortestDistance <= AttackRange)
        {
            target = nearestEnemy.transform;
        }
        else
        {
            target = null;
        }
    }
    
    protected virtual void TrackTarget()
    {
        if (partToRotate == null) return;

        Vector3 dir = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * rotationSpeed).eulerAngles;
        partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);

        // Target out of range
        if (Vector3.Distance(transform.position, target.position) > AttackRange)
        {
            target = null;
        }
    }

    protected virtual void OnUpgradeApplied(GameObject newVisuals)
    {
        // Base implementation is empty.
    }
    
    public void TakeDamage(float amount)
    {
        Health -= amount;
        healthBar.UpdateHealth(Health, MaxHealth); 
        
        if (Health <= 0)
        {
            Die();
        }
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    protected virtual void Die()
    {
        
        if (TowerUIManager.Instance != null)
        {
            TowerUIManager.Instance.HideUpgradePanel(); // Hide panel if the selected tower is destroyed
        }
        Destroy(gameObject);
    }
    
    private IEnumerator FlashRoutine()
    {
        // Set the flash amount to 1 for ALL renderers.
        foreach (var renderer in objectRenderers)
        {
            renderer.GetPropertyBlock(propBlock);
            propBlock.SetFloat("_FlashAmount", 1f);
            renderer.SetPropertyBlock(propBlock);
        }
        // Fade back to normal over a short time
        float duration = 0.25f;
        float time = 0;
        while(time < duration)
        {
            float flashAmount = Mathf.Lerp(1f, 0f, time / duration);
            propBlock.SetFloat("_FlashAmount", flashAmount);
            foreach (var renderer in objectRenderers)
            {
                propBlock.SetFloat("_FlashAmount", flashAmount);
                renderer.SetPropertyBlock(propBlock);
            }
            time += Time.deltaTime;
            yield return null;
        }
    
        // Ensure it's fully reset
        foreach (var renderer in objectRenderers)
        {
            propBlock.SetFloat("_FlashAmount", 0f);
            renderer.SetPropertyBlock(propBlock);
        }
    }
    // To be implemented by child classes
    protected abstract void Attack();
}