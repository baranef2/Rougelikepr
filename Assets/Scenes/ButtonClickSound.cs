using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonClickSound : MonoBehaviour
{
    private AudioManager audioManager;
    private Button button;

    [Header("Opsiyonel: Özel Ses Klipi (boþsa AudioManager'daki buttonClick sesi çalar)")]
    public AudioClip customClip;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        // Panel yeniden aktif olduðunda AudioManager'ý tekrar bul
        if (audioManager == null)
            audioManager = Object.FindFirstObjectByType<AudioManager>();


        // Her enable olduðunda listener'ý yenile (çift eklenmeyi önler)
        if (button != null)
        {
            button.onClick.RemoveListener(PlayClickSound);
            button.onClick.AddListener(PlayClickSound);
        }
    }

    private void PlayClickSound()
    {
        if (audioManager == null)
        {
            audioManager = Object.FindFirstObjectByType<AudioManager>();

            if (audioManager == null)
            {
                Debug.LogWarning("AudioManager bulunamadý! Ses çalýnamadý.");
                return;
            }
        }

        // Özel klip varsa onu çal, yoksa varsayýlan buton sesi
        if (customClip != null)
            audioManager.PlaySFX(customClip);
        else
            audioManager.PlaySFX(audioManager.buttonClick);
    }
}
