using UnityEngine;
using System.Collections; 

public class RangedEnemyAI : EnemyAI
{
    
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float fireDelay = 1.5f; 

    
    protected override void PerformAttack()
    {
        
        _animator.SetTrigger("Attack");

        
        StartCoroutine(SpawnProjectileWithDelay());
    }

   
    private IEnumerator SpawnProjectileWithDelay()
    {
        
        yield return new WaitForSeconds(fireDelay);

        
        if (projectilePrefab != null && firePoint != null && _player != null)
        {
            
            Vector3 targetPos = _player.position + Vector3.up * 1.0f;
            Vector3 dir = (targetPos - firePoint.position).normalized;

            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(dir));

            EnemyProjectile projectileScript = proj.GetComponent<EnemyProjectile>();
            if (projectileScript != null)
            {
                projectileScript.Init(dir, projectileSpeed, attackDamage);
            }
        }
    }
}