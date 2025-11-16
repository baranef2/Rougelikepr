using UnityEngine;


public class TrapDamage : MonoBehaviour
{
    #region Trap Variables
    [SerializeField] private float damageAmount = 10f;    
    [SerializeField] private float damageCooldown = 0.8f; 
    [SerializeField] private string targetTag = "Player"; 
    
    
    private float _lastDamageTime;
    #endregion
    private void Awake()
    {
        
        _lastDamageTime = -damageCooldown;
    }

    
    
    
    private void OnTriggerStay(Collider other)
    {
        
        if (other.CompareTag(targetTag))
        {
            
            if (Time.time > _lastDamageTime + damageCooldown)
            {
                
                if (other.TryGetComponent<Health>(out Health targetHealth))
                {
                    
                    targetHealth.TakeDamage(damageAmount);

                    
                    _lastDamageTime = Time.time;
                }
            }
        }
    }

    
    private void OnTriggerEnter(Collider other)
    {
       

        OnTriggerStay(other); // Ýçeri girdiði an da bir kez kontrol et
    }
}