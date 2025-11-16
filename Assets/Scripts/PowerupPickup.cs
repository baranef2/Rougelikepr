using UnityEngine;

public class PowerupPickup : MonoBehaviour
{
    
    [SerializeField] private PowerupType type;
    [SerializeField] private float amount = 2f;
    [SerializeField] private float duration = 5f;


    [SerializeField] private string playerTag = "Player";
    [SerializeField] private GameObject pickupVFX;
    [SerializeField] private float rotationspeed = 60f;
    [SerializeField] private float despawnTime = 15f;



    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if(!col.isTrigger) col.isTrigger = true;
        Destroy(gameObject, despawnTime);
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationspeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag(playerTag))
        {
            if(other.TryGetComponent<PlayerController>(out PlayerController playerController))
            {
                playerController.ApplyPowerup(type, amount , duration);

                if(pickupVFX != null)
                {
                    Instantiate(pickupVFX , transform.position, Quaternion.identity);
                }
                Destroy(gameObject);
            }


        }

    }






}
