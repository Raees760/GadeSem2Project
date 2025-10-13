// BaseEnemy.cs

using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BaseEnemy : MonoBehaviour
{
    [Header("Base Enemy Stats")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected float attackRate = 1f;
    protected float health;

    [Header("Enemy Rewards")]
    [SerializeField] protected int moneyReward = 10;

    [Header("UI")]
    [SerializeField] private HealthBar healthBar;

    protected NavMeshAgent agent;
    protected Transform currentTarget;
    private float attackCooldown = 0f;

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        health = maxHealth;
    }

    protected virtual void Update()
    {
        // If we don't have a target, or our target was destroyed, find a new one.
        if (currentTarget == null)
        {
            currentTarget = FindNewTarget();
            if (currentTarget != null)
            {
                agent.SetDestination(currentTarget.position);
                agent.isStopped = false; // Ensure we are moving towards the new target
            }
            else
            {
                // No valid targets found, stop moving.
                agent.isStopped = true;
                return;
            }
        }

        attackCooldown -= Time.deltaTime;

        // Check distance to the current target
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

        if (distanceToTarget <= attackRange)
        {
            agent.isStopped = true; // Stop moving to attack
            if (attackCooldown <= 0)
            {
                Attack(currentTarget.GetComponent<BaseTower>());
                attackCooldown = 1f / attackRate;
            }
        }
        else
        {
            // If we were stopped but are now out of range, resume moving.
            if (agent.isStopped)
            {
                agent.isStopped = false;
                agent.SetDestination(currentTarget.position); // Re-affirm destination in case target moved
            }
        }
    }

    /// <summary>
    /// This is the core of the new logic. Child classes will override this
    /// to implement their unique targeting priorities.
    /// The default behavior is to find the closest "OffensiveTower".
    /// </summary>
    /// <returns>The transform of the new target, or null if no valid target is found.</returns>
    protected virtual Transform FindNewTarget()
    {
        // Default behavior: Target the closest offensive tower.
        GameObject[] offensiveTowers = GameObject.FindGameObjectsWithTag("OffensiveTower");
        Transform nearestTower = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject towerObject in offensiveTowers)
        {
            float distance = Vector3.Distance(transform.position, towerObject.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestTower = towerObject.transform;
            }
        }

        // If no offensive towers are left, target the main tower as a last resort.
        if (nearestTower == null)
        {
            MainTower mainTower = FindObjectOfType<MainTower>();
            if (mainTower != null)
            {
                return mainTower.transform;
            }
        }

        return nearestTower;
    }


    protected virtual void Attack(BaseTower tower)
    {
        if (tower != null)
        {
            tower.TakeDamage(damage);
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        healthBar.UpdateHealth(health, maxHealth);
        if (health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}