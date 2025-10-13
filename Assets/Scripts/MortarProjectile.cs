// MortarProjectile.cs

using System.Collections.Generic;
using UnityEngine;

public class MortarProjectile : MonoBehaviour
{
    [Header("AoE Stats")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float arcHeight = 30f; // How high the projectile flies

    [Header("Effects")]
    [SerializeField] private GameObject explosionEffectPrefab;

    private Vector3 startPosition;
    private Vector3 targetPosition;  // For firing at a fixed point (fast enemies)
    private Transform seekTarget;    // For seeking a moving target (slow enemies)
    private float speed;
    private float journeyProgress = 0f;
    private Vector3 lastPosition;

    /// Launches the projectile to continuously seek a moving target.
    public void LaunchAtTarget(Transform _target, float _speed)
    {
        seekTarget = _target;
        speed = _speed;
        startPosition = transform.position;
        lastPosition = startPosition;
    }

    /// Launches the projectile towards a fixed point in space.
    public void LaunchAtPoint(Vector3 _position, float _speed)
    {
        targetPosition = _position;
        speed = _speed;
        startPosition = transform.position;
        lastPosition = startPosition;
    }

    void Update()
    {
        // Determine the current destination
        Vector3 currentDestination;
        if (seekTarget != null)
        {
            currentDestination = seekTarget.position;
        }
        else if (targetPosition != Vector3.zero)
        {
            currentDestination = targetPosition;
        }
        else
        {
            // If there's no target, destroy the projectile
            //Destroy(gameObject);
            return;
        }

        // Calculate travel distance and update progress
        float totalDistance = Vector3.Distance(startPosition, currentDestination);
        if (totalDistance > 0)
        {
            journeyProgress += (speed * Time.deltaTime) / totalDistance;
        }

        if (journeyProgress >= 1f)
        {
            Explode();
            return;
        }

        // Calculate the position on the arc
        Vector3 currentPos = Vector3.Lerp(startPosition, currentDestination, journeyProgress);
        
        // This formula creates a parabola that starts and ends at y=0
        float arc = arcHeight * 4 * journeyProgress * (1 - journeyProgress);
        currentPos.y += arc;

        transform.position = currentPos;
        
        // Make the projectile face its direction of travel
        if (lastPosition != transform.position)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - lastPosition);
            lastPosition = transform.position;
        }
    }

    void Explode()
    {
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // A list to track enemies that have already taken damage from this single explosion.
        List<BaseEnemy> damagedEnemies = new List<BaseEnemy>();
        
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hitCollider  in colliders)
        {
            BaseEnemy enemy = hitCollider.GetComponentInParent<BaseEnemy>();
            
            // Check if we found an enemy AND if we haven't already damaged it.
            if (enemy != null && !damagedEnemies.Contains(enemy))
            {
                enemy.TakeDamage(damage);
                damagedEnemies.Add(enemy); // Add the enemy to the list to prevent re-damaging.
            }
        }

        Destroy(gameObject);
    }
}