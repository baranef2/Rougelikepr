using UnityEngine;

public class PowerupPickup : MonoBehaviour
{
    [Header("Powerup Settings")]
    [SerializeField] private PowerupType type;
    [SerializeField] private float amount = 2f;
    [SerializeField] private float duration = 5f;

    [Header("Visuals & Audio")]
    [SerializeField] private GameObject pickupVFX;
    [SerializeField] private AudioClip pickupSound; // YENÝ: Ses dosyasý için alan
    [SerializeField, Range(0f, 1f)] private float soundVolume = 1f; // YENÝ: Ses þiddeti ayarý

    [Header("Setup")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float rotationspeed = 60f;
    [SerializeField] private float despawnTime = 15f;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        
        if (!col.isTrigger) col.isTrigger = true;

        Destroy(gameObject, despawnTime);
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationspeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (other.TryGetComponent<PlayerController>(out PlayerController playerController))
            {
                
                playerController.ApplyPowerup(type, amount, duration);

                
                if (pickupVFX != null)
                {
                    Instantiate(pickupVFX, transform.position, Quaternion.identity);
                }

                
                if (pickupSound != null)
                {
                    
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position, soundVolume);
                }
                

                
                Destroy(gameObject);
            }
        }
    }
}