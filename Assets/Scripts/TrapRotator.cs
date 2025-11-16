using UnityEngine;

/// <summary>
/// Bu component, eklendiði objeyi belirlenen eksen ve hýzda
/// sürekli olarak döndürür.
/// </summary>
public class TrapRotator : MonoBehaviour
{
    #region ROTATION_SETTINGS

    [Header("Rotation Tweakables")]
    [SerializeField] private float rotationSpeed = 90f; // Saniyede derece cinsinden hýz
    [SerializeField] private Vector3 rotationAxis = Vector3.up; // (0, 1, 0) -> Yatay (Yaw) ekseni

    #endregion

    #region UNITY_METHODS

    private void Update()
    {
        // Objeyi her kare, saniyedeki hýza (Time.deltaTime) baðlý olarak döndür.
        // Bu, oyunun kare hýzýndan (FPS) baðýmsýz, stabil bir dönüþ saðlar.
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
    }

    #endregion
}