using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic; // Listeleri kullanmak için gerekli

public class NewModeSwitcher : MonoBehaviour
{
    [Header("Oyun Modu Ayarlarý")]
    public PlayerController.MovementType targetMode;
    public bool activateOnStart = false;

    [Header("Kamera Ayarlarý")]
    public CinemachineCamera targetCamera;

    [Header("Duvar/Obje Yönetimi")]
    [Tooltip("Bu triggera girince YOK olacak duvarlar/objeler")]
    public List<GameObject> objectsToHide;

    [Tooltip("Bu triggera girince GERÝ GELECEK duvarlar/objeler")]
    public List<GameObject> objectsToShow;

    private void Start()
    {
        if (activateOnStart)
        {
            SetModeAndCamera(FindFirstObjectByType<PlayerController>());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                SetModeAndCamera(player);
            }
        }
    }

    private void SetModeAndCamera(PlayerController player)
    {
        if (player == null) return;

        // 1. Movement Modunu Deðiþtir
        player.currentMovementType = targetMode;

        // 2. Kamerayý Deðiþtir
        if (targetCamera != null)
        {
            var allCameras = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
            foreach (var cam in allCameras)
            {
                cam.Priority = 10;
            }
            targetCamera.Priority = 20;
        }

        // 3. Duvarlarý/Objeleri Yönet (GÝZLE)
        if (objectsToHide != null)
        {
            foreach (var obj in objectsToHide)
            {
                if (obj != null) obj.SetActive(false);
            }
        }

        // 4. Duvarlarý/Objeleri Yönet (GÖSTER)
        if (objectsToShow != null)
        {
            foreach (var obj in objectsToShow)
            {
                if (obj != null) obj.SetActive(true);
            }
        }

        Debug.Log($"Mod Deðiþtirildi: {targetMode} | Gizlenen Obje Sayýsý: {objectsToHide?.Count}");
    }
}