using UnityEngine;
using UnityEngine.SceneManagement; 


public class FallDetector : MonoBehaviour
{
    [Header("Düþme Ayarý")]
    [Tooltip("Bu Y seviyesinin altýna düþünce sahne yeniden baþlar")]
    [SerializeField] private float deathThresholdY = -5f;

    
    private bool _isRestarting = false;

    private void Update()
    {
        
        if (transform.position.y < deathThresholdY && !_isRestarting)
        {
            StartRestart();
        }
    }

    private void StartRestart()
    {
        
        _isRestarting = true;

        Debug.Log(gameObject.name + " eþiðin altýna düþtü. Sahne yeniden baþlatýlýyor...");

        
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}