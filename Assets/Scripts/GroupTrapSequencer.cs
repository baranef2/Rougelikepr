using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Listenin durumunu kontrol etmek için (Any)

/// <summary>
/// Bu yardýmcý sýnýf, Inspector'da "Aktivasyon Gruplarý" oluþturmamýzý saðlar.
/// </summary>
[System.Serializable]
public class TrapActivationGroup
{
    [Tooltip("Inspector'da grubu tanýman için bir isim (örn: Grup A)")]
    public string groupName;

    [Tooltip("Bu grupta AYNI ANDA tetiklenecek tuzaklar")]
    public List<ControllableTrap> trapsInThisGroup;
}


/// <summary>
/// Belirlenen tuzak gruplarýný sýrayla tetikler.
/// Bir grup bittikten sonra diðerine geçer.
/// </summary>
public class GroupTrapSequencer : MonoBehaviour
{
    [Header("Tuzak Gruplarý")]
    [Tooltip("Sýrayla tetiklenecek tuzak gruplarýnýn listesi")]
    [SerializeField] private List<TrapActivationGroup> activationGroups;

    [Header("Zamanlama Ayarlarý")]
    [Tooltip("Tüm gruplar bittikten sonra (tam döngü) baþa dönmek için bekleme süresi")]
    [SerializeField] private float delayAfterFullCycle = 3.0f;
    [SerializeField] private float delayBetweenGroups = 0.5f;

    [Tooltip("Oyun baþlar baþlamaz sekans otomatik baþlasýn mý?")]
    [SerializeField] private bool startOnAwake = true;

    private void Start()
    {
        if (startOnAwake)
        {
            StartCoroutine(SequenceCoroutine());
        }
    }

    private IEnumerator SequenceCoroutine()
    {
        while (true) // Bütün sekansý (Grup 1, Grup 2, ...) sonsuz döngüye al
        {
            // Tanýmlanan her bir grup için sýrayla...
            foreach (var group in activationGroups)
            {
                if (group.trapsInThisGroup == null || group.trapsInThisGroup.Count == 0)
                {
                    continue; // Bu grup boþ, atla
                }

                // --- 1. ADIM: Gruptaki TÜM tuzaklarý AYNI ANDA aktive et ---
                foreach (var trap in group.trapsInThisGroup)
                {
                    if (trap != null)
                    {
                        // ControllableTrap script'indeki döngüyü baþlat
                        trap.ActivateTrap();
                    }
                }

                // --- 2. ADIM: Bu gruptaki BÜTÜN tuzaklarýn hareketinin bitmesini bekle ---
                // Gruptaki tuzaklardan 'Herhangi Biri' (Any) hala hareketliyse (IsMoving == true)
                // bir sonraki kareye kadar bekle.
                yield return new WaitUntil(() =>
                    !group.trapsInThisGroup.Any(trap => trap != null && trap.IsMoving)
                );
                yield return new WaitForSeconds(delayBetweenGroups);

                // (Not: Ýsteðe baðlý olarak buraya gruplar arasý
                // 'yield return new WaitForSeconds(0.5f);' gibi kýsa bir bekleme ekleyebilirsin.)
            }

            // --- 3. ADIM: Bütün gruplar bitti. Baþa dönmeden önce bekle ---
            yield return new WaitForSeconds(delayAfterFullCycle);
        }
    }
}