using UnityEngine;
using System.Collections;

public class LogPatternTrapManager : MonoBehaviour
{
    
    public GameObject[] logPatterns; 
    public Transform spawnPoint;     

    
    public float spawnCooldown = 2.0f; 
    public bool isActive = false;      

    private Coroutine spawnCoroutine;

    
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("Player") && !isActive)
        {
            isActive = true;
            spawnCoroutine = StartCoroutine(SpawnLogs());
            Debug.Log("Tuzak sistemi aktifleþti!");
        }
    }

   

    IEnumerator SpawnLogs()
    {
        while (isActive)
        {
           
            int randomIndex = Random.Range(0, logPatterns.Length);

            
            Instantiate(logPatterns[randomIndex], spawnPoint.position, Quaternion.identity);

            
            yield return new WaitForSeconds(spawnCooldown);
        }
    }
}