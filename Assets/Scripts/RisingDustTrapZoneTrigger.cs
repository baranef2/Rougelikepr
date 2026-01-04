using UnityEngine;
using UnityEngine.Events;

public class RisingDustTrapZoneTrigger : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private bool oneTimeUse = true; // Sadece bir kere mi çalýþsýn?

    [Header("Olaylar")]
    public UnityEvent onTriggerEnter; // Buraya Dumaný baðlayacaðýz

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            // Olayý tetikle (Dumaný baþlat)
            onTriggerEnter?.Invoke();

            // Tek seferlikse collider'ý kapat (Objeyi yok etmiyoruz, sadece pasif yapýyoruz)
            if (oneTimeUse)
            {
                GetComponent<Collider>().enabled = false;
            }
        }
    }
}