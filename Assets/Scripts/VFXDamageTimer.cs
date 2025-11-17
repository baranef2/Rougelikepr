using UnityEngine;
using System.Collections;

/// <summary>
/// Bu script, bir VFX tuzaðýnýn hasar verme süresini
/// görsel efekt süresiyle senkronize etmek için kullanýlýr.
/// Particle System'in 'Stop Action: Disable' ile çalýþmak üzere tasarlanmýþtýr.
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(TrapDamage))] //
public class VFXDamageTimer : MonoBehaviour
{
    [Tooltip("Görsel efektin 'tehlikeli' olduðu süre (saniye). Duman vs. hariç, sadece ateþin sürdüðü zaman.")]
    [SerializeField] private float damageDuration = 1.5f;

    private Collider _collider;
    private TrapDamage _trapDamage; //

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _trapDamage = GetComponent<TrapDamage>(); //
    }

    private void OnEnable()
    {
        // 1. Döngüye hazýrlan: Hasar component'lerini AÇ
        // (Bir önceki döngüden kapalý kalmýþ olabilirler)
        if (_collider != null)
            _collider.enabled = true;

        if (_trapDamage != null)
            _trapDamage.enabled = true; //

        // 2. Hasarý durdurmak için zamanlayýcýyý baþlat
        StartCoroutine(DisableDamageCoroutine());
    }

    private IEnumerator DisableDamageCoroutine()
    {
        // 3. Hasar süresi boyunca bekle
        yield return new WaitForSeconds(damageDuration);

        // 4. Süre doldu, hasarý KES
        if (_collider != null)
            _collider.enabled = false; // OnTriggerStay'i durdurur

        if (_trapDamage != null)
            _trapDamage.enabled = false; // (Ýþi garantiye alýr)
    }

    // Not: Bu script OnDisable'da hiçbir þey yapmaz.
    // Ana obje (Particle System'in 'Stop Action: Disable' ayarý sayesinde)
    // kendi kendini kapatacak ve bu script de onunla birlikte pasif hale gelecektir.
}