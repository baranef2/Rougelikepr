using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[System.Serializable]
public struct LootDrop
{
    public GameObject itemPrefab;
    [Range(0f, 100f)]
    public float dropChancePercentage;
}

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Animator))]
public class EnemyAI : MonoBehaviour
{
    [Header("Referanslar")]
    protected NavMeshAgent _agent;
    protected Transform _player;
    protected Health _playerHealth;
    protected Animator _animator;
    protected Health _health;

    [Header("AI Ayarlarý")]
    [SerializeField] protected float detectionRange = 15f;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected float attackDamage = 10f;
    [SerializeField] protected float attackCooldown = 1.5f;
    [SerializeField] protected float attackPreparationTime = 0.3f;
    [SerializeField] private GameObject hitBloodVFX;
    [SerializeField] private Vector3 hitVFXOffset = new Vector3(0, 1.0f, 0);
    [SerializeField] private GameObject enemyDeathVFX;

    [Header("Hasar Tepkisi (Slow) Ayarlarý")]
    [SerializeField] private float slowDuration = 0.5f; 
    [SerializeField] private float slowedSpeed = 0.5f;  
    private float _originalSpeed; 
    private bool _isHit = false;  

    private float _lastAttackTime = -999f;

    private int _animIDSpeed;
    private int _animIDDamage;

    [SerializeField] private LootDrop[] lootTable;
    [SerializeField] private Vector3 lootSpawnOffset = new Vector3(0, 0.5f, 0);

    private enum DüþmanDurumu { Bekleme, Takip, Saldýrý }
    private DüþmanDurumu _mevcutDurum;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _health = GetComponent<Health>();

        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDDamage = Animator.StringToHash("Damage");

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
            _playerHealth = playerObj.GetComponent<Health>();
        }
    }

    private void Start()
    {
        _mevcutDurum = DüþmanDurumu.Bekleme;

        
        if (_agent != null)
        {
            _originalSpeed = _agent.speed;
        }
    }

    private void Update()
    {
        

        if (_agent != null && _animator != null)
        {
            float currentSpeed = _agent.velocity.magnitude;
            _animator.SetFloat(_animIDSpeed, currentSpeed);
        }

        if (_player == null || _playerHealth == null)
        {
            _agent.isStopped = true;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

        
        if (distanceToPlayer <= attackRange)
        {
            if (_mevcutDurum != DüþmanDurumu.Saldýrý)
            {
                _lastAttackTime = Time.time - attackCooldown + attackPreparationTime;
            }
            _mevcutDurum = DüþmanDurumu.Saldýrý;
        }
        else if (distanceToPlayer <= detectionRange)
        {
            _mevcutDurum = DüþmanDurumu.Takip;
        }
        else
        {
            _mevcutDurum = DüþmanDurumu.Bekleme;
        }

        
        switch (_mevcutDurum)
        {
            case DüþmanDurumu.Bekleme:
                BeklemeDavranýþý();
                break;
            case DüþmanDurumu.Takip:
                TakipDavranýþý();
                break;
            case DüþmanDurumu.Saldýrý:
                SaldýrýDavranýþý();
                break;
        }
    }

    private void BeklemeDavranýþý()
    {
        _agent.isStopped = true;
    }

    private void TakipDavranýþý()
    {
        
        _agent.isStopped = false;
        _agent.SetDestination(_player.position);
    }

    private void SaldýrýDavranýþý()
    {
       
        _agent.isStopped = true;

        Vector3 lookDir = _player.position - transform.position;
        lookDir.y = 0;
        transform.rotation = Quaternion.LookRotation(lookDir);

        
        if (!_isHit && Time.time > _lastAttackTime + attackCooldown)
        {
            PerformAttack();
            _lastAttackTime = Time.time;
        }
    }

    protected virtual void PerformAttack()
    {
        _animator.SetTrigger("Attack");
        if (_playerHealth != null) _playerHealth.TakeDamage(attackDamage);
    }

    
    private void HandleDamage(float currentHealthPercent)
    {
        if (currentHealthPercent <= 0) return;
        if (hitBloodVFX != null)
        {
            // Efekti düþmanýn pozisyonunda + biraz yukarýda (offset) oluþtur
            Vector3 spawnPos = transform.position + hitVFXOffset;

            
            Quaternion rotation = Quaternion.identity;

            GameObject vfx = Instantiate(hitBloodVFX, spawnPos, rotation);

            // Efekti 2 saniye sonra sahneden sil (Performans için önemli)
            Destroy(vfx, 2f);

            StartCoroutine(SlowRoutine());
        }
    }

    private IEnumerator SlowRoutine()
    {
        _isHit = true;

        // 1. Animasyonu oynat
        _animator.SetTrigger(_animIDDamage);

        // 2. Hýzý düþür (Örn: 3.5 -> 0.5)
        _agent.speed = slowedSpeed;

        // 3. Belirlenen süre kadar bekle (Örn: 0.5 saniye)
        yield return new WaitForSeconds(slowDuration);

        // 4. Hýzý eski haline getir (Örn: 0.5 -> 3.5)
        _agent.speed = _originalSpeed;

        _isHit = false;
    }

    private void HandleDeath()
    {
        TryDropLoot();
        if (enemyDeathVFX != null)
        {
            GameObject vfx = Instantiate(enemyDeathVFX, transform.position, Quaternion.identity);
            Destroy(vfx, 2.2f);
        }
        _animator.SetTrigger("Death");
        _agent.isStopped = true;
        this.enabled = false;
        if (GetComponent<Collider>() != null) GetComponent<Collider>().enabled = false;
        Destroy(gameObject, 2.9f);
    }

    private void TryDropLoot()
    {
        if (lootTable == null || lootTable.Length == 0) return;
        foreach (var drop in lootTable)
        {
            if (Random.Range(0f, 100f) <= drop.dropChancePercentage)
            {
                Instantiate(drop.itemPrefab, transform.position + lootSpawnOffset, Quaternion.identity);
            }
        }
    }

    private void OnEnable()
    {
        if (_health != null)
        {
            _health.OnDeath.AddListener(HandleDeath);
            _health.OnTakeDamage.AddListener(HandleDamage);
        }
    }

    private void OnDisable()
    {
        if (_health != null)
        {
            _health.OnDeath.RemoveListener(HandleDeath);
            _health.OnTakeDamage.RemoveListener(HandleDamage);
        }
    }
}