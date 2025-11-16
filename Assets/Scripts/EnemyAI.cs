using UnityEngine;
using UnityEngine.AI; // NavMeshAgent için gerekli

[System.Serializable]
public struct LootDrop
{
    public GameObject itemPrefab;
    [Range(0f, 100f)]
    public float dropChancePercentage;
}


[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))] // Can sistemi de zorunlu
[RequireComponent(typeof(Animator))]
public class EnemyAI : MonoBehaviour
{
    [Header("Referanslar")]
    private NavMeshAgent _agent;
    private Transform _player;
    private Health _playerHealth; // Oyuncunun can sistemi
    private Animator _animator;
    private Health _health;

    [Header("AI Ayarlarý")]
    [SerializeField] private float detectionRange = 15f; // Oyuncuyu fark etme mesafesi
    [SerializeField] private float attackRange = 2f;    // Saldýrý mesafesi
    [SerializeField] private float attackDamage = 10f;  // Vereceði hasar
    [SerializeField] private float attackCooldown = 1.5f; // Saniyede kaç kez vurabileceði
    [SerializeField] private GameObject enemyDeathVFX;
    private float _lastAttackTime = -999f;
    private int _animIDSpeed;
    [SerializeField] private LootDrop[] lootTable;
    [SerializeField] private Vector3 lootSpawnOffset = new Vector3(0, 0.5f, 0);
    // FSM (Durum Makinesi)
    private enum DüþmanDurumu { Bekleme, Takip, Saldýrý }
    private DüþmanDurumu _mevcutDurum;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _health = GetComponent<Health>();

        _animIDSpeed = Animator.StringToHash("Speed");

        // Oyuncuyu dinamik olarak bul (veya bir manager ile ata)
        // Bu en basit yöntemdir, daha geliþmiþ sistemler kurabilirsiniz.
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
    }

    private void Update()
    {
        if (_agent != null && _animator != null)
        {
            // velocity.magnitude, agent'ýn saniyedeki dünya birimi cinsinden hýzýdýr.
            // Bu, bizim Animator'de "Speed" (float) parametresi için tam aradýðýmýz þey.
            float currentSpeed = _agent.velocity.magnitude;
            _animator.SetFloat(_animIDSpeed, currentSpeed);
        }

        // OLMASI GEREKEN DOÐRU BLOK:
        if (_player == null || _playerHealth == null)
        {
           
            _agent.isStopped = true;
            return;
        }

        // Oyuncuyla mesafeyi hesapla
        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

        // --- Durum Geçiþleri ---
        if (distanceToPlayer <= attackRange)
        {
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

        // --- Durum Davranýþlarý ---
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
        // Navigasyonu durdur
        _agent.isStopped = true;

        // (Opsiyonel) Etrafa bakýnma, idle animasyonu oynatma vb.
    }

    private void TakipDavranýþý()
    {
        // Navigasyonu baþlat
        _agent.isStopped = false;
        // Hedefi oyuncu olarak ayarla
        _agent.SetDestination(_player.position);

        // (Opsiyonel) Koþma animasyonunu oynat
    }

    private void SaldýrýDavranýþý()
    {
        // Olduðu yerde dur
        _agent.isStopped = true;

        // Düþmanýn yüzünün oyuncuya dönmesini saðla (anýnda)
        Vector3 lookDir = _player.position - transform.position;
        lookDir.y = 0; // Düþmanýn havaya bakmasýný engelle
        transform.rotation = Quaternion.LookRotation(lookDir);

        // Saldýrý Cooldown kontrolü
        if (Time.time > _lastAttackTime + attackCooldown)
        {
            PerformAttack();
            _lastAttackTime = Time.time;
        }
    }

    private void PerformAttack()
    {
        // (Opsiyonel) Saldýrý animasyonunu tetikle
        // _animator.SetTrigger("Attack");

        Debug.Log("Düþman saldýrdý!");

        // Oyuncunun canýný azalt
        if (_playerHealth != null)
        {
            _playerHealth.TakeDamage(attackDamage);
        }
    }

    private void HandleDeath()
    {
        TryDropLoot();


        if (enemyDeathVFX != null)
        {
            Instantiate(enemyDeathVFX, transform.position, Quaternion.identity); 
        }

        _animator.SetTrigger("Death");
        _agent.isStopped = true;
        this.enabled = false;
        if (GetComponent<Collider>() != null)
        {
            GetComponent<Collider>().enabled = false;
        }
        Destroy(gameObject, 2f);
    }

    private void TryDropLoot()
    {
        if (lootTable == null || lootTable.Length == 0) return;

        foreach (var drop in lootTable)
        {
            float randomRoll = Random.Range(0f, 100f);

            if (randomRoll <= drop.dropChancePercentage)
            {
                Vector3 spawnPosition = transform.position + lootSpawnOffset;
                Instantiate(drop.itemPrefab, spawnPosition, Quaternion.identity);
            }
        }
    }

    private void OnEnable()
    {
        // Kendi can sistemimizin OnDeath event'ine HandleDeath metodunu baðla
        if (_health != null)
        {
            _health.OnDeath.AddListener(HandleDeath);
        }
    }

    private void OnDisable()
    {
        // Obje kapandýðýnda veya yok edildiðinde event baðlantýsýný kes (hafýza sýzýntýsýný önler)
        if (_health != null)
        {
            _health.OnDeath.RemoveListener(HandleDeath);
        }
    }
}
