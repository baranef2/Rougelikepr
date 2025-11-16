using UnityEngine;
using System.Collections;

/// <summary>
/// Bu script, bir tuzaðýn belirlenen bir hedefe doðru hareket etmesini,
/// orada beklemesini ve baþlangýç pozisyonuna geri dönmesini yönetir.
/// TrapSequencer tarafýndan tetiklenmek üzere tasarlanmýþtýr.
/// </summary>
[RequireComponent(typeof(TrapDamage))]
public class ControllableTrap : MonoBehaviour
{
    [Header("Hareket Ayarlarý")]
    [SerializeField] private Transform targetTransform; // Tuzaðýn uzayacaðý son nokta (Boþ bir GameObject olabilir)
    [SerializeField] private float extendSpeed = 5f;    // Ýleri hareket hýzý
    [SerializeField] private float retractSpeed = 3f;   // Geri çekilme hýzý
    [SerializeField] private float extendedDuration = 1.0f; // Tuzak dýþarýda ne kadar kalacak?

    [Header("Referanslar")]
    [SerializeField] private TrapDamage _trapDamage; // Hasar veren component

    private Vector3 _startPosition;
    private bool _isMoving = false; // Tuzaðýn zaten hareket halinde olup olmadýðýný kontrol eder
    public bool IsMoving => _isMoving; // Tuzaðýn meþgul olup olmadýðýný dýþarýya bildiren public özellik

    private void Awake()
    {
        _startPosition = transform.position;

        // TrapDamage component'ýný bul
        if (_trapDamage == null)
        {
            _trapDamage = GetComponent<TrapDamage>();
        }

        // Baþlangýçta hasar vermeyi kapat
        if (_trapDamage != null)
        {
            _trapDamage.enabled = false;
        }

        if (targetTransform == null)
        {
            Debug.LogError(gameObject.name + " için bir 'Target Transform' atanmamýþ!", this);
        }
    }

    /// <summary>
    /// TrapSequencer bu metodu çaðýrarak tuzaðý aktive eder.
    /// </summary>
    public void ActivateTrap()
    {
        // Eðer tuzak zaten aktif bir döngüdeyse, tekrar baþlatma
        if (!_isMoving && targetTransform != null)
        {
            StartCoroutine(RunTrapCycle());
        }
    }

    private IEnumerator RunTrapCycle()
    {
        _isMoving = true;

        // 1. Hedefe Doðru Ýlerle (Extend)
        yield return StartCoroutine(MoveToPosition(targetTransform.position, extendSpeed));

        // 2. Hedefte Bekle ve Hasar Ver
        if (_trapDamage != null)
        {
            _trapDamage.enabled = true; // Hasar vermeyi aç
        }

        yield return new WaitForSeconds(extendedDuration);

        if (_trapDamage != null)
        {
            _trapDamage.enabled = false; // Hasar vermeyi kapat
        }

        // 3. Baþlangýç Pozisyonuna Geri Dön (Retract)
        yield return StartCoroutine(MoveToPosition(_startPosition, retractSpeed));

        _isMoving = false;
    }

    /// <summary>
    /// Obje'yi belirlenen pozisyona MoveTowards ile hareket ettirir.
    /// </summary>
    private IEnumerator MoveToPosition(Vector3 target, float speed)
    {
        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null; // Bir sonraki kareye kadar bekle
        }
        // Tam hedefe oturt
        transform.position = target;
    }
}