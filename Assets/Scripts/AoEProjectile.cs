// UNUSED SCRIPT
using UnityEngine;

public class AoEProjectile : Projectile // Inherits from your existing Projectile class
{
    [Header("AoE Specifics")]
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private GameObject explosionEffectPrefab; // Visual effect for the explosion

    // We override HitTarget to add the explosion logic
    void HitTarget()
    {
        // Instantiate visual effect at the target's last known position
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // Find all colliders within the explosion radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider collider in colliders)
        {
            // Check if the collider belongs to an enemy
            BaseEnemy enemy = collider.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                // Damage every enemy in the radius
                enemy.TakeDamage(GetDamage());
            }
        }

        Destroy(gameObject);
    }
    
    public float GetDamage()
    {
        return damage; 
    }
}