using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuController : MonoBehaviour
{
    [Header("Menu Panelleri")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject languagePanel;
    public GameObject creditsPanel;

    // --- TEMEL BUTONLAR ---
    public void PlayButton()
    {
        // NOT: Genellikle menü sahnesi 0 olduðu için, oyun sahnesi 1 veya adý ile yüklenir.
        // Bu örnekte 1. indeksteki sahneyi yüklüyoruz.
        Debug.Log("Oyun Baþlatýlýyor...");
        SceneManager.LoadScene(1);
    }

    public void QuitButton()
    {
        Debug.Log("Oyundan Çýkýþ Yapýlýyor...");
        Application.Quit();

        // **EKLEME:** Bu blok, oyun Unity Editör'ünde çalýþýrken çýkýþ iþlemini simüle eder.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // --- ALT MENÜ AÇMA ---
    public void OpenSettings()
    {
        Debug.Log("Ayarlar Menüsü Açýldý.");
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false); // Ana menüyü kapat
        if (settingsPanel != null) settingsPanel.SetActive(true);  // Ayarlarý aç
    }

    public void OpenCredits()
    {
        Debug.Log("Jenerik Menüsü Açýldý.");
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false); // Ana menüyü kapat
        if (creditsPanel != null) creditsPanel.SetActive(true);    // Jeneriði aç
    }

    // --- GERÝ DÖNÜÞ (BackButton'larýn çaðýrdýðý fonksiyondur) ---
    public void ReturnToMain()
    {
        Debug.Log("Ana Menüye Dönülüyor.");

        // Tüm alt panelleri kapat (Hangi panel açýksa onu kapatýr, diðerlerini kontrol eder)
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (languagePanel != null) languagePanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);

        // Ana menüyü aç
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }
}