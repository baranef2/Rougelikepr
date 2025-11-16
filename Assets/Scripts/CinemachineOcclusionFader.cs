using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;

/// <summary>
/// Bir Cinemachine Virtual Camera eklentisi (Extension).
/// Kameradan 'Follow' hedefine (oyuncu) ýþýn atar ve aradaki
/// 'OccluderFader' component'ine sahip objeleri saydamlaþtýrýr.
/// (Cinemachine 3.0 / Unity 6 uyumlu)
/// </summary>
[AddComponentMenu("")] // Component menüsünde görünmez, VCam'in "Add Extension" listesinden eklenir
[SaveDuringPlay] // Oyun çalýþýrken yapýlan deðiþiklikleri kaydeder
[ExecuteInEditMode] // Editörde de çalýþýr
public class CinemachineOcclusionFader : CinemachineExtension
{
    #region SETTINGS
    [Header("Raycast Ayarlarý")]
    [Tooltip("Hangi layer'daki objelerin saydamlaþacaðýný belirler.")]
    [SerializeField] private LayerMask occluderLayer;

    [Tooltip("Iþýnýn ne kadar kalýn olacaðý (0 = Raycast, >0 = SphereCast)")]
    [SerializeField] private float raycastRadius = 0.1f;

    [Tooltip("Iþýn oyuncunun ne kadar üzerinden geçsin (pivot noktasý genellikle ayaktadýr)")]
    [SerializeField] private Vector3 targetOffset = Vector3.up;
    #endregion

    #region STATE
    // O an araya giren objelerin listesi
    private HashSet<OccluderFader> _currentlyOccluding = new HashSet<OccluderFader>();
    // Bir önceki karede araya giren objelerin listesi (FadeIn yapmak için)
    private HashSet<OccluderFader> _previousOccluding = new HashSet<OccluderFader>();

    // Raycast için önbellek
    private RaycastHit[] _hits = new RaycastHit[10];
    #endregion

    // Extension'ýn ana döngüsü. Kamera her güncellendiðinde burasý çalýþýr.
    // Extension'ýn ana döngüsü. Kamera her güncellendiðinde burasý çalýþýr.
    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        // Sadece en son aþamada, kamera pozisyonu netleþtiðinde çalýþ
        if (stage == CinemachineCore.Stage.Finalize)
        {
            // === DÜZELTME 1: 'Follow' yerine 'vcam.Follow' ===
            if (vcam.Follow == null) return; // 'Follow' -> 'vcam.Follow' olarak deðiþti

            // Mevcut listeyi temizlemeden önce referansýný sakla
            // ve 'currentlyOccluding' için yeni bir set oluþtur (ya da clear() kullan)
            (_previousOccluding, _currentlyOccluding) = (_currentlyOccluding, _previousOccluding);
            _currentlyOccluding.Clear();

            // === DÜZELTME 1: 'Follow.position' yerine 'vcam.Follow.position' ===
            Vector3 playerPos = vcam.Follow.position + targetOffset; // 'Follow' -> 'vcam.Follow' olarak deðiþti

            // === DÜZELTME 2: 'state.FinalPosition' yerine 'state.Position' ===
            Vector3 camPos = state.GetFinalPosition(); 

            Vector3 dir = (playerPos - camPos);
            float distance = dir.magnitude;

            int hitCount = 0;

            // Iþýn at
            if (raycastRadius > 0f)
            {
                hitCount = Physics.SphereCastNonAlloc(camPos, raycastRadius, dir.normalized, _hits, distance, occluderLayer, QueryTriggerInteraction.Ignore);
            }
            else
            {
                hitCount = Physics.RaycastNonAlloc(camPos, dir.normalized, _hits, distance, occluderLayer, QueryTriggerInteraction.Ignore);
            }


            // Iþýnýn çarptýðý tüm objeleri iþle
            for (int i = 0; i < hitCount; i++)
            {
                // Çarpan objenin 'OccluderFader' script'i var mý?
                if (_hits[i].collider.TryGetComponent<OccluderFader>(out var fader))
                {
                    // Varsa, onu "saydamlaþ" listesine ekle ve 'FadeOut' komutu ver
                    _currentlyOccluding.Add(fader);
                    fader.FadeOut();
                }
            }

            // Þimdi, bir önceki karede listede olup BU karede listede OLMAYANLARI bul
            // Bunlar artýk araya girmeyen objelerdir.
            foreach (var fader in _previousOccluding)
            {
                if (!_currentlyOccluding.Contains(fader))
                {
                    // 'FadeIn' (opaklaþ) komutu ver
                    fader.FadeIn();
                }
            }
        }
    }
}