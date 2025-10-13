using UnityEngine;

public abstract class BaseTower : MonoBehaviour
{
    [Header("Base Tower Stats")]
    [SerializeField] protected float maxHealth= 100f;
    [SerializeField] protected float attackRange = 15f;
    [SerializeField] protected float fireRate = 1f; // 1 shot per second
    [SerializeField] protected float rotationSpeed = 10f;
    [SerializeField] protected Transform partToRotate; // e.g., the turret head
    [SerializeField] public int threatValue = 5; // How much this tower adds to the next wave's credit pool
    protected float health ;
    
    [Header("UI")]
    [SerializeField] private HealthBar healthBar; 

    protected Transform target;
    protected float attackCooldown = 0f;
    
    // This can be overridden by specific tower types
    //protected virtual float fireRate { get { return 1f; } }
    protected virtual void Start()
    {
        health = maxHealth;
    }


    protected virtual void Update()
    {
        if (target == null)
        {
            FindTarget();
        }
        else
        {
            TrackTarget();
            if (attackCooldown <= 0f)
            {
                Attack();
                attackCooldown = 1f / fireRate;
            }
        }
        
        attackCooldown -= Time.deltaTime;
    }

    protected virtual void FindTarget()
    {
        //Find the closest enemy
        BaseEnemy[] enemies = FindObjectsOfType<BaseEnemy>();
        float shortestDistance = Mathf.Infinity;
        BaseEnemy nearestEnemy = null;

        foreach (BaseEnemy enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && shortestDistance <= attackRange)
        {
            target = nearestEnemy.transform;
        }
        else
        {
            target = null;
        }
    }
    
    protected virtual void TrackTarget()
    {
        Vector3 dir = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * rotationSpeed).eulerAngles;
        partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);

        // Target out of range
        if (Vector3.Distance(transform.position, target.position) > attackRange)
        {
            target = null;
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
    
    // To be implemented by child classes
    protected abstract void Attack();
}