using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserTower : BaseTower
{
    [Header("Laser Tower Attributes")]
    [SerializeField] private float damageOverTime = 30f;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float targetAcquisitionDelay = 1f; // The delay before finding a new target. This makes target weak to swarms
    [SerializeField] private ParticleSystem impactEffect; // particle effect at point of impact

    private LineRenderer lineRenderer;
    private float searchCooldown; // The internal timer for the delay

    protected override void Start()
    {
        base.Start();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
        searchCooldown = 0f;
    }

    protected override void Update()
    {
        // We still need to find and track a target
        if (target == null)
        {
            if (lineRenderer.enabled)
            {
                lineRenderer.enabled = false;
                if(impactEffect != null) impactEffect.Stop();
            }
            // If we are in the search delay period, count down and do nothing else.
            if (searchCooldown > 0f)
            {
                searchCooldown -= Time.deltaTime;
                return; // Stop further execution this frame
            }
            
            // If the cooldown is over, we are allowed to find a new target.
            FindTarget();
        }
        else
        {
            // Prime the search cooldown. This ensures that the moment we lose this target,
            // the delay will be active for its full duration.
            searchCooldown = targetAcquisitionDelay;
            
            TrackTarget();
            Laser();
        }
    }

    private void Laser()
    {
        // Apply damage over time
        target.GetComponent<BaseEnemy>().TakeDamage(damageOverTime * Time.deltaTime);

        // Visuals
        if (!lineRenderer.enabled)
        {
            lineRenderer.enabled = true;
            if(impactEffect != null) impactEffect.Play();
        }

        lineRenderer.SetPosition(0, firePoint.position);
        lineRenderer.SetPosition(1, target.position);
        
        if (impactEffect != null)
        {
            impactEffect.transform.position = target.position;
        }
    }

    protected override void Attack()
    {
        // Intentionally left blank because the laser logic is in Update()
    }

    // Custom FindTarget logic to handle turning off the laser when out of range
    private new void FindTarget()
    {
        base.FindTarget(); // Calls the base class FindTarget method
        if (target == null && lineRenderer.enabled)
        {
            lineRenderer.enabled = false;
            if (impactEffect != null) impactEffect.Stop();
        }
    }
     // Custom TrackTarget logic to handle turning off the laser when out of range
    private new void TrackTarget()
    {
        base.TrackTarget(); // Calls the base class TrackTarget method
        if (target == null && lineRenderer.enabled)
        {
            lineRenderer.enabled = false;
            if (impactEffect != null) impactEffect.Stop();
        }
    }
}