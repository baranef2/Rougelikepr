using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Paneller")]
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;

    [Header("Referanslar")]
    public SettingsManager settingsManager;

    private bool isPaused = false;

    void Start()
    {
        // Ayar paneli bağlantısı eksikse otomatik bul
        if (settingsManager == null)
            settingsManager = FindAnyObjectByType<SettingsManager>();

        // Başlangıçta kapalı tut
        if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
                BackToPauseMenu(); // Ayarlardayken geri döner
            else
                TogglePause(); // Normal ESC davranışı
        }
    }

    private void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (pauseMenuPanel) pauseMenuPanel.SetActive(true);
        if (settingsPanel) settingsPanel.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    // 🔹 ESC menüsünden Ayarlara geçiş
    public void OpenSettings()
    {
        if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(true);
    }

    // 🔹 Ayarlardan ESC menüsüne geri dönüş
    public void BackToPauseMenu()
    {
        if (settingsPanel) settingsPanel.SetActive(false);
        if (pauseMenuPanel) pauseMenuPanel.SetActive(true);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}
