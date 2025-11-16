using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 30f;
    [SerializeField] private float lifeTime = 5f;
    //[SerializeField] private float damage = 25f;
    private float _damage;
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private LayerMask wallMask;
    private Vector3 _dir;

    // --- YENÝ EKLENEN DEÐÝÞKENLER ---
    private TrailRenderer _trail;
    private bool _isDying = false; // "Ölme" durumunu takip etmek için
    private float _trailTime;
    // --- BÝTÝÞ ---

    public void Init(Vector3 worldDirection, float damageAmount)
    {
        _dir = worldDirection.normalized;
        transform.rotation = Quaternion.LookRotation(_dir, Vector3.up);
        _damage = damageAmount;
        
        _trail = GetComponentInChildren<TrailRenderer>();
        if (_trail != null)
        {
            _trailTime = _trail.time; // Trail'in ömrünü kaydet
        }
       
    }

    private void Update()
    {
        // --- YENÝ EKLENEN KOD ---
        // Eðer "ölme" durumundaysak, hareketi durdur
        if (_isDying) return;
        // --- BÝTÝÞ ---

        transform.position += _dir * speed * Time.deltaTime;
        lifeTime -= Time.deltaTime;

        // ESKÝ KOD: if (lifeTime <= 0f) Destroy(gameObject);
        // --- YENÝ KOD ---
        if (lifeTime <= 0f)
        {
            StartDeathSequence();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // --- YENÝ EKLENEN KOD ---
        // Zaten ölüyorsak, tekrar çarpýþmayý tetikleme
        if (_isDying) return;
        // --- BÝTÝÞ ---

        if (other.CompareTag(enemyTag))
        {
            if (other.TryGetComponent<Health>(out Health enemyHealth))
            {
                enemyHealth.TakeDamage(_damage);
            }

            // ESKÝ KOD: Destroy(gameObject);
            // --- YENÝ KOD ---
            StartDeathSequence();
        }
        else if (wallMask == (wallMask | (1 << other.gameObject.layer)))
        {
            // Çarptýðýmýz objenin katmaný, wallMask'ýn içindeyse
            // Bu bir duvardýr. Mermiyi yok et.
            StartDeathSequence();
        }
    }

    // --- YENÝ EKLENEN METOD ---
    /// <summary>
    /// Mermiyi anýnda yok etmek yerine, trail'in kaybolmasý için ölme sürecini baþlatýr.
    /// </summary>
    private void StartDeathSequence()
    {
        if (_isDying) return; // Süreç zaten baþladýysa tekrar baþlatma
        _isDying = true;

        // Mermiyi durdur
        speed = 0;

        // Ýsteðe baðlý: Merminin kendi 3D modelini veya partikülünü gizle
        // if (GetComponent<MeshRenderer>() != null) GetComponent<MeshRenderer>().enabled = false;
        // if (GetComponent<ParticleSystem>() != null) GetComponent<ParticleSystem>().Stop();

        if (_trail != null)
        {
            // Trail'in yeni parça üretmesini durdur
            _trail.emitting = false;

            // Objeyi, trail'in kaybolma süresi + küçük bir tampon süre sonra yok et
            Destroy(gameObject, _trailTime + 0.1f);
        }
        else
        {
            // Trail yoksa, eskisi gibi hemen yok et
            Destroy(gameObject);
        }
    }
}