using UnityEngine;

public class RisingTrap : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] private float riseSpeed = 2f;
    [SerializeField] private bool isActive = false;
    [SerializeField] private float lifeTime = 10f;
    private void Update()
    {
        if (isActive)
        {
            // Space.World ekleyerek objenin yönü ne olursa olsun DÜNYA EKSENÝNDE yukarý çýkmasýný saðlýyoruz
            transform.Translate(Vector3.up * riseSpeed * Time.deltaTime, Space.World);

            // Hem hýzý hem konumu yazdýralým ki sorunu görelim
            Debug.Log($"Hýz: {riseSpeed} | Konum Y: {transform.position.y}");
        }
    }

    public void StartRising()
    {
        isActive = true;
        // Eðer yanlýþlýkla Inspector'da 0 kaldýysa, kodla zorla düzeltmeyi deneyebilirsin (geçici çözüm):
        if (riseSpeed == 0) riseSpeed = 2f;
        Destroy(gameObject, lifeTime);
        Debug.Log("Tuzak Tetiklendi! Duman yükseliyor...");
    }
}