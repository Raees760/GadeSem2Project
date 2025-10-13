using UnityEngine;

public class ProjectileShooterTower : BaseTower
{
    [Header("Projectile Tower")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 50f;
    
    // Override the base FireRate property
    //protected override float fireRate => 2f; // Shoots 2 times per second

    protected override void Attack()
    {
        GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectile = projectileGO.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Seek(target, projectileSpeed);
        }
    }
}