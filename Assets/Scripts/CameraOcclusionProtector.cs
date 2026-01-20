using UnityEngine;
using System.Collections.Generic;

public class CameraOcclusionProtector : MonoBehaviour
{
    [Header("Settings")]
    public Transform target; // Karakterin (Player) Transform'u
    public LayerMask obstacleLayer; // Sadece "Wall" layer'ýný seç
    public float checkRadius = 0.5f; // Iþýnýn kalýnlýðý

    private List<OccluderFader> _activeFaders = new List<OccluderFader>();

    void LateUpdate() // Kamera hareketinden sonra çalýþmasý için LateUpdate
    {
        if (target == null) return;

        // Kameradan karaktere olan yön ve mesafe
        Vector3 direction = target.position - transform.position;
        float distance = direction.magnitude;

        // Iþýn yolla (SphereCast daha iyidir, ince duvarlarý kaçýrmaz)
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, checkRadius, direction, distance, obstacleLayer);

        // Yeni çarpanlarý tutacak geçici liste
        HashSet<OccluderFader> currentFaders = new HashSet<OccluderFader>();

        foreach (var hit in hits)
        {
            // Eðer çarpan objede OccluderFader varsa
            if (hit.transform.TryGetComponent<OccluderFader>(out var fader))
            {
                currentFaders.Add(fader);
                if (!_activeFaders.Contains(fader))
                {
                    fader.FadeOut(); // Görüþü kapatýyorsa þeffaflaþtýr
                    _activeFaders.Add(fader);
                }
            }
        }

        // Artýk araya girmeyen objeleri eski haline getir
        for (int i = _activeFaders.Count - 1; i >= 0; i--)
        {
            OccluderFader fader = _activeFaders[i];
            if (!currentFaders.Contains(fader))
            {
                fader.FadeIn(); // Artýk engel deðilse opak yap
                _activeFaders.RemoveAt(i);
            }
        }
    }
}