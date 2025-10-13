// Projectile.cs

using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform target;
    private float speed;
    [SerializeField] protected float damage = 10f; // Can be set from the tower later

    public void Seek(Transform _target, float _speed)
    {
        target = _target;
        speed = _speed;
    }

    void Update()
    {
        if (target == null)// Incase bullets have fired but enemy is already dead
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = target.position - transform.position; // Bullet direction subtly changes direction to guarantee hit
        float distanceThisFrame = speed * Time.deltaTime;
        
        //More performant than collision detection
        if (dir.magnitude <= distanceThisFrame) // if inside target
        {
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
    }

    
    public float GetDamage()
    {
        return damage;
    }
    
    void HitTarget()
    {
        BaseEnemy e = target.GetComponent<BaseEnemy>();
        if(e != null)
        {
            e.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}