using UnityEngine;
using Unity.Cinemachine; 

public class ModeSwitcher : MonoBehaviour
{
    
    public PlayerController.MovementType targetMode;

    
    public bool activateOnStart = false;

    
    
    public CinemachineCamera targetCamera;

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

        
        player.currentMovementType = targetMode;

        
        if (targetCamera != null)
        {
           
            var allCameras = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
            foreach (var cam in allCameras)
            {
                cam.Priority = 10;
            }
            
            targetCamera.Priority = 20;
        }

        Debug.Log($"Mod Deðiþtirildi: {targetMode}");
    }
}