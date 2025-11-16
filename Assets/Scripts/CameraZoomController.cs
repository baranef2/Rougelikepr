using UnityEngine;
using UnityEngine.InputSystem; // Input System için gerekli
using Unity.Cinemachine;     // Cinemachine için gerekli

// Unity 6 (Cinemachine 3.x) için doðru sýnýf: CinemachineCamera
[RequireComponent(typeof(CinemachineCamera))]
public class CameraZoomController : MonoBehaviour
{
    // === AYARLAR (Ýsteðinize göre güncellendi) ===
    [Header("Zoom Ayarlarý")]
    [Tooltip("En fazla ne kadar YAKINLAÞABÝLECEÐÝ (en düþük Orthographic Size deðeri)")]
    [SerializeField] private float minZoom = 1f; // <-- 1 olarak deðiþtirildi

    [Tooltip("En fazla ne kadar UZAKLAÞABÝLECEÐÝ (en yüksek Orthographic Size deðeri)")]
    [SerializeField] private float maxZoom = 5f; // <-- 5 olarak deðiþtirildi

    [Tooltip("Zoom'un ne kadar hýzlý olacaðý")]
    [SerializeField] private float zoomSpeed = 1f;


    // === REFERANSLAR ===
    private CinemachineCamera _virtualCamera;
    private InputSystem_Actions _playerInputActions;
    private float _currentTargetZoom;

    private void Awake()
    {
        // Referanslarý al
        _virtualCamera = GetComponent<CinemachineCamera>();
        _playerInputActions = new InputSystem_Actions();

        // Baþlangýç zoom deðerini kameranýn mevcut ayarýndan al
        if (_virtualCamera.Lens.Orthographic)
        {
            _currentTargetZoom = _virtualCamera.Lens.OrthographicSize;
        }
        else
        {
            _currentTargetZoom = _virtualCamera.Lens.FieldOfView;
        }
    }

    private void OnEnable()
    {
        _playerInputActions.Player.Enable();
        _playerInputActions.Player.Zoom.performed += OnZoom;
    }

    private void OnDisable()
    {
        _playerInputActions.Player.Zoom.performed -= OnZoom;
        _playerInputActions.Player.Disable();
    }

    private void OnZoom(InputAction.CallbackContext context)
    {

        float scrollValue = context.ReadValue<Vector2>().y;

        if (scrollValue == 0) return;

        float zoomDirection = -Mathf.Sign(scrollValue);
        _currentTargetZoom += zoomDirection * zoomSpeed;
        _currentTargetZoom = Mathf.Clamp(_currentTargetZoom, minZoom, maxZoom);

        // Kameranýn türüne (Orthographic veya Perspective) göre doðru ayarý deðiþtir
        if (_virtualCamera.Lens.Orthographic)
        {
            _virtualCamera.Lens.OrthographicSize = _currentTargetZoom;
        }
        else
        {
            _virtualCamera.Lens.FieldOfView = _currentTargetZoom;
        }
    }
}