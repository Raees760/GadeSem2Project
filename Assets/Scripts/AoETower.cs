using UnityEngine;

public class AoETower : BaseTower
{
    [Header("AoE Tower")]
    [SerializeField] private GameObject mortarProjectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 5f; // Slower projectile for visual effect

    //protected override float fireRate => 0.5f; // A mortar, thus should have a slower fire rate (once every 2 seconds

    protected override void Attack()
    {
        if (target == null) return;

        // Instantiate the mortar shell, no rotation needed as it orients itself
        GameObject projectileGO = Instantiate(mortarProjectilePrefab, firePoint.position, Quaternion.identity);
        MortarProjectile projectile = projectileGO.GetComponent<MortarProjectile>();

        if (projectile != null)
        {
            // Check if the target is a "fast" enemy
            if (target.GetComponent<FastEnemy>() != null)
            {
                // For fast enemies, fire at their current position and don't seek
                // this creates a situation where the projectile might miss if the enemy moves
                projectile.LaunchAtPoint(target.position, projectileSpeed);
            }
            else
            {
                // For slow and basic enemies, seek them to guarantee a hit.
                projectile.LaunchAtTarget(target, projectileSpeed);
            }
        }
    }
}