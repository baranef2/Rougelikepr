using UnityEngine;
using Unity.Cinemachine;
public class WallZoneManager : MonoBehaviour
{
    #region AYARLAR
    [SerializeField] private CinemachineCamera WallCamera;
    [SerializeField] private CinemachineCamera mainIsoCamera;
    [SerializeField] private GameObject hieroglyphDecal;
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out PlayerController Player))
        {
            if (WallCamera != null) WallCamera.Priority = 20;
            Player.is2DMode = true; 
            if(hieroglyphDecal != null) hieroglyphDecal.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.TryGetComponent<PlayerController>(out PlayerController player))
        {
            if (WallCamera != null) WallCamera.Priority = 0;

            player.is2DMode = false;    

            if(hieroglyphDecal != null) hieroglyphDecal.SetActive(false);
        }
    }
}
