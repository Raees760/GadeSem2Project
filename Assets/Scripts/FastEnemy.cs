using UnityEngine;

public class FastEnemy : BaseEnemy
{
    [Header("Fast Enemy Specifics")]
    [SerializeField] private float speedModifier = 2.0f; // Moves at double the speed

    protected override void Start()
    {
        base.Start();
        // Fast enemies are quicker but have less health.
        if (agent != null)
        {
            agent.speed *= speedModifier;
        }
    }
    // Fast enemies attack all towers, regardless of its type (including resource towers).
    protected override Transform FindNewTarget()
    {
        BaseTower[] allTowers = FindObjectsOfType<BaseTower>();
        Transform nearestTower = null;
        float closestDistance = Mathf.Infinity;

        if (allTowers.Length == 0) return null;

        foreach (BaseTower tower in allTowers)
        {
            float distance = Vector3.Distance(transform.position, tower.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestTower = tower.transform;
            }
        }
        return nearestTower;
    }
}