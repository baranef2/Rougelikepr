using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Mixer")]
    public AudioMixer mainMixer;

    [Header("Sources")]
    public AudioSource musicSource;     // Arka plan müziği
    public AudioSource sfxSource;       // Oyun SFX
    public AudioSource uiClickSource;   // UI buton sesi (YENİ)

    [Header("Clips")]
    public AudioClip mainMusic;
    public AudioClip buttonClick;
    public AudioClip walk;
    public AudioClip jump;
    public AudioClip knifeDraw;
    public AudioClip knifeThrow;

    private Dictionary<string, AudioClip> sfxClips = new();
    private bool isReady = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeClips();
            FixUISourceSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // =============================
    //    CLIP KAYIT & HAZIRLAMA
    // =============================
    private void InitializeClips()
    {
        sfxClips.Clear();

        if (buttonClick) sfxClips["ButtonClick"] = buttonClick;
        if (walk) sfxClips["Walk"] = walk;
        if (jump) sfxClips["Jump"] = jump;
        if (knifeDraw) sfxClips["KnifeDraw"] = knifeDraw;
        if (knifeThrow) sfxClips["KnifeThrow"] = knifeThrow;

        if (mainMusic && musicSource)
        {
            musicSource.clip = mainMusic;
            musicSource.loop = true;
            musicSource.Play();
        }

        isReady = true;
    }

    // =============================
    //     UI CLICK FIX (YENİ)
    // =============================
    private void FixUISourceSettings()
    {
        if (uiClickSource == null) return;

        uiClickSource.playOnAwake = false;
        uiClickSource.spatialBlend = 0f; // 2D ses
        uiClickSource.priority = 0;      // En öncelikli
        uiClickSource.loop = false;
    }

    // =============================
    //         UI CLICK SESİ
    // =============================
    public void PlayUIClick()
    {
        if (uiClickSource == null || buttonClick == null) return;

        uiClickSource.PlayOneShot(buttonClick, 1f);
    }

    // =============================
    //     OYUN SFX ÇALMA
    // =============================
    public void PlaySFX(string soundName)
    {
        if (!isReady || sfxSource == null) return;
        if (sfxClips.ContainsKey(soundName))
            sfxSource.PlayOneShot(sfxClips[soundName]);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }

    // =============================
    //          MÜZİK
    // =============================
    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }
}
