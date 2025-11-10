// EliteEnemy.cs

using UnityEngine;
using UnityEngine.Serialization;

public class EliteEnemy : BaseEnemy
{
    private enum EliteArchetype { Juggernaut, GlassCannon, Speedster, Balanced }
    
    [FormerlySerializedAs("eliteRenderer")]
    [Header("Elite Visuals")]
    [SerializeField] private ParticleSystem speedVFX; 
    [SerializeField] private Color baseColor = Color.grey;
    [SerializeField] private Color highDamageColor = Color.red;

    private Renderer[] eliteRenderers;
    private float baselineHealth = 50f;
    private float baselineDamage = 15f;
    private float baselineSpeed = 3.5f;

    protected override void Awake()
    {
        base.Awake();
        
        if (eliteRenderers == null || eliteRenderers.Length == 0)
        {
            eliteRenderers = GetComponentsInChildren<Renderer>();
        }
        
    }
    /// This is the core method for procedural generation.
    /// The WaveManager will call this after instantiating the Elite.
    public void GenerateStatsAndVisuals(int statBudget)
    {
        //RANDOMLY CHOOSE AN ARCHETYPE
        EliteArchetype chosenArchetype = (EliteArchetype)Random.Range(0, System.Enum.GetValues(typeof(EliteArchetype)).Length);
        Debug.Log($"--- ELITE GENERATED --- Archetype: {chosenArchetype}");

        float healthPoints = 0;
        float damagePoints = 0;
        float speedPoints = 0;

        //  Distribute the Stat Budget BASED ON THE CHOSEN ARCHETYPE
        switch (chosenArchetype)
        {
            case EliteArchetype.Juggernaut:
                // Focus heavily on health, with minimal damage and speed.
                healthPoints = statBudget * Random.Range(0.60f, 0.90f); // 80-90%
                damagePoints = statBudget * Random.Range(0.01f, 0.10f); // 1-10%
                break;

            case EliteArchetype.GlassCannon:
                // Focus heavily on damage, with minimal health and speed.
                healthPoints = statBudget * Random.Range(0.01f, 0.25f); // 1-25%
                damagePoints = statBudget * Random.Range(0.30f, 0.75f); // 30-75%
                break;

            case EliteArchetype.Speedster:
                // Focus heavily on speed, with moderate health and low damage.
                healthPoints = statBudget * Random.Range(0.20f, 0.30f); // 10-30%
                damagePoints = statBudget * Random.Range(0.20f, 0.30f); // 1-15%
                break;

            case EliteArchetype.Balanced:
                // This is your old, middle-of-the-road logic.
                healthPoints = statBudget * Random.Range(0.30f, 0.50f); // 30-50%
                damagePoints = statBudget * Random.Range(0.20f, 0.40f); // 20-40%
                break;
        }
        
        // The remaining budget is always allocated to the last stat (speed in most cases).
        speedPoints = statBudget - healthPoints - damagePoints;


        //Calculate Final Stats 
        maxHealth = baselineHealth + (healthPoints * 25f);
        damage = baselineDamage + (damagePoints * 2f);
        float finalSpeed = baselineSpeed + (speedPoints * 0.1f);
        
        health = maxHealth;
        if(agent != null) agent.speed = finalSpeed;

        Debug.Log($"Budget: {statBudget} | Health: {maxHealth} | Damage: {damage} | Speed: {finalSpeed}");

        // Update Visuals Based on Stats 
        // Health affects Scale
        float scaleMultiplier = healthPoints/statBudget;
        transform.localScale = Vector3.one * scaleMultiplier; // Adjusted clamp

        // Damage affects Color and Emission
        if (eliteRenderers != null && eliteRenderers.Length > 0)
        {
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            float damageRatio = Mathf.Clamp01(damagePoints / (statBudget * 0.5f));
            Color finalColor = Color.Lerp(baseColor, highDamageColor, damageRatio);
            
            foreach (var renderer in eliteRenderers)
            {
                renderer.GetPropertyBlock(propBlock);
                propBlock.SetColor("_BaseColor", finalColor);
                propBlock.SetFloat("_EmissiveIntensity", damageRatio * 5f);
                renderer.SetPropertyBlock(propBlock);
            }
        }
        
        // Speed affects VFX
        if (speedVFX != null)
        {
            var emission = speedVFX.emission;
            var main = speedVFX.main;
            emission.rateOverTime = speedPoints * 8f;
            main.startSpeed = finalSpeed;
        }
        
        healthBar.UpdateHealth(health, maxHealth);
    }

    // Elites are single-minded and only target the Main Tower.
   /* protected override Transform FindNewTarget()
    {
        base.FindNewTarget();
        // MainTower mainTower = FindObjectOfType<MainTower>();
        //return mainTower != null ? mainTower.transform : null;
    }*/

    protected override void Die()
    {
        // Elites give a larger reward for being defeated
        GameManager.Instance.AddMoney(moneyReward * 5); // 5x the normal reward
        base.Die();
    }
}