using UnityEngine;

public class TrapAudioController : MonoBehaviour
{
    [Header("Ayarlar")]
    public AudioSource mainAudioSource; 
    public AudioClip hitSound;          

    void Start()
    {
        
        if (mainAudioSource == null)
            mainAudioSource = GetComponent<AudioSource>();

       
        mainAudioSource.loop = true;

        
        if (!mainAudioSource.isPlaying)
            mainAudioSource.Play();
    }

    
    private void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayDamageSound();
        }
    }

    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayDamageSound();
        }
    }

    void PlayDamageSound()
    {
        
        if (hitSound != null)
        {
            mainAudioSource.PlayOneShot(hitSound);
        }
    }
}