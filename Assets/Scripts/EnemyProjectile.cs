using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private float speed;
    private float damage;
    private Vector3 direction;

    public void Init(Vector3 dir, float moveSpeed, float dmg)
    {
        direction = dir;
        speed = moveSpeed;
        damage = dmg;
        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            Destroy(gameObject); 
        }
        
        else if (!other.CompareTag("Enemy") && !other.CompareTag("Projectile"))
        {
            Destroy(gameObject);
        }
    }
}