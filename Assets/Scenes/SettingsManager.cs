using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.U2D;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    [Header("Sekme İçerik Panelleri")]
    public GameObject displayContentPanel;
    public GameObject controlsContentPanel;
    public GameObject audioContentPanel;

    [Header("Sekme Butonları")]
    public Button displayTabButton;
    public Button controlsTabButton;
    public Button audioTabButton;

    [Header("SES AYARLARI")]
    public AudioMixer mainMixer;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("GÖRÜNTÜ AYARLARI")]
    public Toggle vsyncToggle;
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown fullscreenDropdown;
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown fpsDropdown;

    private Resolution[] resolutions;

    private void OnEnable()
    {
        OpenTab(displayContentPanel);
        LoadAllSettingsFromPrefs();
    }

    // ----------------- TAB YÖNETİMİ -----------------
    public void OpenTab(GameObject targetPanel)
    {
        displayContentPanel.SetActive(false);
        controlsContentPanel.SetActive(false);
        audioContentPanel.SetActive(false);

        targetPanel.SetActive(true);
        UpdateTabButtonVisuals(targetPanel);

        if (targetPanel == displayContentPanel)
            LoadDisplaySettingsFromPrefs();

        if (targetPanel == audioContentPanel)
            LoadAudioSettingsFromPrefs();
    }

    private void UpdateTabButtonVisuals(GameObject activePanel)
    {
        Color normal = Color.white;
        Color active = Color.yellow;

        displayTabButton.image.color = (activePanel == displayContentPanel) ? active : normal;
        controlsTabButton.image.color = (activePanel == controlsContentPanel) ? active : normal;
        audioTabButton.image.color = (activePanel == audioContentPanel) ? active : normal;
    }

    // ----------------- AYAR YÜKLEME -----------------
    private void LoadAllSettingsFromPrefs()
    {
        LoadAudioSettingsFromPrefs();
        LoadDisplaySettingsFromPrefs();
    }

    private void LoadAudioSettingsFromPrefs()
    {
        float def = 0.5f;

        float mv = PlayerPrefs.GetFloat("MasterVolumeValue", def);
        float mus = PlayerPrefs.GetFloat("MusicVolumeValue", def);
        float sfx = PlayerPrefs.GetFloat("SFXVolumeValue", def);

        masterVolumeSlider.value = mv;
        musicVolumeSlider.value = mus;
        sfxVolumeSlider.value = sfx;

        ApplyMasterVolume(mv);
        ApplyMusicVolume(mus);
        ApplySFXVolume(sfx);
    }

    private void LoadDisplaySettingsFromPrefs()
    {
        // --- ÇÖZÜNÜRLÜK ---
        if (resolutionDropdown)
        {
            resolutions = Screen.resolutions;
            resolutionDropdown.ClearOptions();

            List<string> options = new();
            for (int i = 0; i < resolutions.Length; i++)
            {
                string lbl = $"{resolutions[i].width}x{resolutions[i].height}";
                if (!options.Contains(lbl))
                    options.Add(lbl);
            }

            resolutionDropdown.AddOptions(options);

            int savedIndex = PlayerPrefs.GetInt("ResolutionIndex", GetDefaultResolutionIndex());
            resolutionDropdown.value = Mathf.Clamp(savedIndex, 0, options.Count - 1);
        }

        // --- KALİTE ---
        if (qualityDropdown)
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new List<string>(QualitySettings.names));

            int q = PlayerPrefs.GetInt("QualityLevel", GetHighQualityIndex());
            qualityDropdown.value = Mathf.Clamp(q, 0, QualitySettings.names.Length - 1);
        }

        // --- FPS ---
        if (fpsDropdown)
        {
            List<string> fpsOpts = new() { "Sınırsız", "144 FPS", "60 FPS", "30 FPS" };
            fpsDropdown.ClearOptions();
            fpsDropdown.AddOptions(fpsOpts);

            int f = PlayerPrefs.GetInt("FPSLimitIndex", 0);
            fpsDropdown.value = Mathf.Clamp(f, 0, fpsOpts.Count - 1);
        }

        // --- FULLSCREEN MODU ---
        if (fullscreenDropdown)
        {
            List<string> modes = new() { "Tam Ekran", "Çerçevesiz", "Pencereli" };
            fullscreenDropdown.ClearOptions();
            fullscreenDropdown.AddOptions(modes);

            int m = PlayerPrefs.GetInt("FullscreenMode", 0);
            fullscreenDropdown.value = Mathf.Clamp(m, 0, modes.Count - 1);
        }

        // --- VSYNC ---
        if (vsyncToggle)
        {
            vsyncToggle.isOn = PlayerPrefs.GetInt("VSync", 1) == 1;
        }
    }

    // ----------------- SES UYGULAMA -----------------
    private void ApplyMasterVolume(float v)
        => mainMixer?.SetFloat("MasterVolume", Mathf.Log10(Mathf.Max(v, 0.0001f)) * 20);

    private void ApplyMusicVolume(float v)
        => mainMixer?.SetFloat("MusicVolume", Mathf.Log10(Mathf.Max(v, 0.0001f)) * 20);

    private void ApplySFXVolume(float v)
        => mainMixer?.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(v, 0.0001f)) * 20);

    // ----------------- KAYDET (GÖRÜNTÜYÜ UYGULAMAZ!) -----------------
    public void ApplySettings()
    {
        // 🔊 SADECE SESİ UYGULA
        float m = masterVolumeSlider.value;
        float mu = musicVolumeSlider.value;
        float s = sfxVolumeSlider.value;

        ApplyMasterVolume(m);
        ApplyMusicVolume(mu);
        ApplySFXVolume(s);

        PlayerPrefs.SetFloat("MasterVolumeValue", m);
        PlayerPrefs.SetFloat("MusicVolumeValue", mu);
        PlayerPrefs.SetFloat("SFXVolumeValue", s);

        // 🖥️ GÖRÜNTÜ AYARLARINI ANINDA UYGULAMA!
        // SADECE KAYDET → Böylece ekran bozulmaz.
        PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
        PlayerPrefs.SetInt("QualityLevel", qualityDropdown.value);
        PlayerPrefs.SetInt("FPSLimitIndex", fpsDropdown.value);
        PlayerPrefs.SetInt("FullscreenMode", fullscreenDropdown.value);
        PlayerPrefs.SetInt("VSync", vsyncToggle.isOn ? 1 : 0);

        PlayerPrefs.Save();
    }

    // ----------------- RESET / CANCEL -----------------
    public void ResetSettings()
    {
        if (displayContentPanel.activeSelf)
        {
            resolutionDropdown.value = GetDefaultResolutionIndex();
            qualityDropdown.value = GetHighQualityIndex();
            fullscreenDropdown.value = 0;
            fpsDropdown.value = 0;
            vsyncToggle.isOn = true;
        }
        else if (audioContentPanel.activeSelf)
        {
            masterVolumeSlider.value = 0.5f;
            musicVolumeSlider.value = 0.5f;
            sfxVolumeSlider.value = 0.5f;
        }
    }

    public void CancelSettings()
    {
        LoadAllSettingsFromPrefs();
    }

    // ----------------- YARDIMCI -----------------
    private int GetDefaultResolutionIndex()
    {
        resolutions = Screen.resolutions;
        int fallback = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == 1920 && resolutions[i].height == 1080)
                return i;

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
                fallback = i;
        }
        return fallback;
    }

    private int GetHighQualityIndex()
    {
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            string n = QualitySettings.names[i].ToLower();
            if (n.Contains("yüksek") || n.Contains("high"))
                return i;
        }
        return QualitySettings.GetQualityLevel();
    }
}
