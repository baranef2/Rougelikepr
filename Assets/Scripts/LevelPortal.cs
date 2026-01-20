using UnityEngine;
using UnityEngine.SceneManagement; // Sahne geçiþi için gerekli kütüphane

public class LevelPortal : MonoBehaviour
{
    [Header("Ayarlar")]
    public string gidilecekSahneAdi; // Ýkinci bölümün tam adý (Örn: Level2)
    public GameObject uyariYazisiObjesi; // "E'ye Bas" yazýsý (UI Objesi)

    private bool oyuncuYakininda = false;

    // Oyuncu alana girdiðinde
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Çarpan obje "Player" etiketine sahip mi?
        {
            oyuncuYakininda = true;
            if (uyariYazisiObjesi != null)
                uyariYazisiObjesi.SetActive(true); // Yazýyý aç
        }
    }

    // Oyuncu alandan çýktýðýnda
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            oyuncuYakininda = false;
            if (uyariYazisiObjesi != null)
                uyariYazisiObjesi.SetActive(false); // Yazýyý kapat
        }
    }

    void Update()
    {
        // Oyuncu yakýndaysa VE E tuþuna basarsa
        if (oyuncuYakininda && Input.GetKeyDown(KeyCode.E))
        {
            SahneDegistir();
        }
    }

    void SahneDegistir()
    {
        // Hata almamak için sahne adýný kontrol edelim
        if (!string.IsNullOrEmpty(gidilecekSahneAdi))
        {
            SceneManager.LoadScene(gidilecekSahneAdi);
        }
        else
        {
            Debug.LogError("Sahne adý girilmemiþ!");
        }
    }
}