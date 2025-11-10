using UnityEngine;

public class ResourceTower : BaseTower
{
    [Header("Resource Generation")]
    [SerializeField] private int goldPerInterval = 5;
    [SerializeField] private float generationInterval = 10f;
    [SerializeField] private GameObject goldBurstEffectPrefab;
    
    private float generationCooldown;
    public int GoldPerInterval => goldPerInterval;
    
    protected override void Start()
    {
        base.Start(); 
        generationCooldown = generationInterval;
    }

    // by NOT calling base.Update(), we prevent this tower from ever
    // running the FindTarget() or TrackTarget() logic.
    protected override void Update()
    {
        // Only tick down the cooldown if the game is in the Combat phase.
        if (WaveManager.Instance != null && WaveManager.Instance.CurrentState == WaveManager.WaveState.Combat)
        {
            // Instead of attacking, we run our own generation logic.
            generationCooldown -= Time.deltaTime;
            if (generationCooldown <= 0f)
            {
                GenerateResources();
                generationCooldown = generationInterval;
            }
        }
        else
        {
            // Optional: If not in combat, ensure the cooldown is reset for the start of the next wave.
            generationCooldown = generationInterval;
        }
    }

    private void GenerateResources()
    {
        GameManager.Instance.AddMoney(goldPerInterval);
        Instantiate(goldBurstEffectPrefab, transform.position, Quaternion.identity);
        
        Debug.Log($"Resource Tower generated {goldPerInterval} gold.");
    }

    protected override void Attack()
    {
        // Intentionally left blank becuase not an attacking tower
    }
    
}