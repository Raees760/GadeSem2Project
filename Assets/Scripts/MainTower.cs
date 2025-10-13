using UnityEngine;

public class MainTower : ProjectileShooterTower // Inherits from Projectile tower
{
    protected override void Die()
    {
        GameManager.Instance.GameOver();
        base.Die();
    }
}