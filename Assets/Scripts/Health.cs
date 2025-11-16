using UnityEngine;
using UnityEngine.Events;
using System;
public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 3f;
    [SerializeField] private float _currentHealth;

    public UnityEvent<float> OnHealthChanged;
    public UnityEvent OnDeath;
    public UnityEvent <float>OnTakeDamage;

    public bool isDead = false;
    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    public void TakeDamage (float amount)
    {   
        if (_currentHealth <= 0) return;

        _currentHealth -= amount;
        OnHealthChanged?.Invoke(_currentHealth / maxHealth); // UI update için normalleþtirilmiþ deðer
        OnTakeDamage?.Invoke(_currentHealth/maxHealth);
        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            OnDeath?.Invoke();
            Debug.Log(gameObject.name + " öldü ");
            //Destroy(gameObject, 0f);
        }
    }

    public void Heal(float amount)
    {
        if (_currentHealth <= 0) return;
        _currentHealth += amount;

        if (_currentHealth >= maxHealth)
        {
            _currentHealth = maxHealth;
        }

        OnHealthChanged?.Invoke(_currentHealth/maxHealth);
        Debug.Log(gameObject.name + " " +  amount + " Kadar iyileþti."); 
    }


}
