using UnityEngine;
using System.Collections;

public class FallingSymbol : MonoBehaviour
{
    
    [SerializeField] private GameObject barrierObject; 

    [SerializeField] private GameObject pathObjectToOpen; // Açýlacak veya kapanacak engel
    [SerializeField] private bool activatePathObject = false; 
    [SerializeField] private float fallSpeed = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float yOffset = 0.5f;

    private bool _isFalling = false;
    private Vector3 _targetPosition;

    
    public void DropSymbol()
    {
        if (barrierObject != null) barrierObject.SetActive(false);
        DetectGroundAndStartFall();
    }

    private void DetectGroundAndStartFall()
    {
        RaycastHit hit;
        
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 50f, groundLayer))
        {
            _targetPosition = hit.point + Vector3.up * yOffset;
            StartCoroutine(FallRoutine());
        }
    }

    private IEnumerator FallRoutine()
    {
        _isFalling = true;
        while (Vector3.Distance(transform.position, _targetPosition) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, fallSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = _targetPosition;
        _isFalling = false;
    }

    
    public void OpenPath()
    {
        
        if (pathObjectToOpen != null)
        {
            // Eðer köprü spawn olsun istiyorsan inspector'da 'activatePathObject'i tikle.
            // Eðer duvar yok olsun istiyorsan tiki kaldýr.
            pathObjectToOpen.SetActive(activatePathObject);
            Debug.Log("Yol objesi durumu deðiþtirildi: " + activatePathObject);
        }
        else
        {
            Debug.LogError("HATA: 'Path Object To Open' kutusu boþ! Inspector'dan objeyi sürüklemedin.");
        }

        
        Debug.Log("Sembol yok ediliyor.");
        Destroy(gameObject);
    }
}