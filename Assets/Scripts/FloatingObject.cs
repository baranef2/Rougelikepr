using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    [Header("Hareket Ayarlarý")]
    public float donmeHizi = 50f;
    public float inmeCikmaHizi = 1f;
    public float inmeCikmaMesafesi = 0.5f;

    private Vector3 baslangicPozisyonu;

    void Start()
    {
        baslangicPozisyonu = transform.position;
    }

    void Update()
    {
        // Kendi etrafýnda dönme
        transform.Rotate(Vector3.up * donmeHizi * Time.deltaTime);

        // Yukarý aþaðý süzülme (Sinüs dalgasý kullanarak)
        float yeniY = baslangicPozisyonu.y + Mathf.Sin(Time.time * inmeCikmaHizi) * inmeCikmaMesafesi;
        transform.position = new Vector3(transform.position.x, yeniY, transform.position.z);
    }
}