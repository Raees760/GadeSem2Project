using UnityEngine;

public class TankEnemy : BaseEnemy
{
    [Header("Tank Specifics")]
    [SerializeField] private float speedModifier = 0.5f; // Moves slow

    protected override void Start()
    {
        base.Start();
        // Tanks are slower but have more health. The health is set in the inspector on the prefab
        if (agent != null)
        {
            agent.speed *= speedModifier;
        }
    }
    // Tanks have one goal: destroy the Main Tower. They ignore all other defenders.
    protected override Transform FindNewTarget()
    {
        MainTower mainTower = FindObjectOfType<MainTower>();
        if (mainTower != null)
        {
            return mainTower.transform;
        }
        return null; // No other targets are valid for the tank
    }
}