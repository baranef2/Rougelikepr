using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HieroglyphWallPuzzle : MonoBehaviour
{
    [System.Serializable]
    public struct TrapSetup
    {
        public GameObject decalObject;
        public GameObject fireTrapObject;
    }

    
    [SerializeField] private GameObject entryFireWall;
    [SerializeField] private GameObject exitFireWall;

    
    [SerializeField] private List<TrapSetup> availableTraps;
    [SerializeField] private int totalRounds = 5;

    
    [SerializeField] private float warningDuration = 1.0f;
    [SerializeField] private float fireDuration = 1.5f;
    [SerializeField] private float timeBetweenRounds = 1.0f;
    [SerializeField] private float wallActivationDelay = 1.0f; 

    private bool _isPuzzleActive = false;
    private bool _isPuzzleCompleted = false;

    private void Start()
    {
        if (entryFireWall != null) entryFireWall.SetActive(false);
        if (exitFireWall != null) exitFireWall.SetActive(false);

        foreach (var trap in availableTraps)
        {
            if (trap.decalObject != null) trap.decalObject.SetActive(false);
            if (trap.fireTrapObject != null) trap.fireTrapObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (!_isPuzzleCompleted && !_isPuzzleActive && other.CompareTag("Player"))
        {
            StartCoroutine(PuzzleRoutine());
        }
    }

    
    private IEnumerator PuzzleRoutine()
    {
        _isPuzzleActive = true;
        yield return new WaitForSeconds(wallActivationDelay);
        // Giriþi Kilitle
        if (entryFireWall != null) entryFireWall.SetActive(true);
        if (exitFireWall != null) exitFireWall.SetActive(true);

        
        

        for (int i = 0; i < totalRounds; i++)
        {
            
            int index1 = Random.Range(0, availableTraps.Count);
            int index2 = Random.Range(0, availableTraps.Count);

            
            while (index2 == index1)
            {
                index2 = Random.Range(0, availableTraps.Count);
            }

            TrapSetup trap1 = availableTraps[index1];
            TrapSetup trap2 = availableTraps[index2];

            yield return new WaitForSeconds(warningDuration);
            if (trap1.decalObject != null) trap1.decalObject.SetActive(true);
            if (trap2.decalObject != null) trap2.decalObject.SetActive(true);

            yield return new WaitForSeconds(warningDuration);

            
            if (trap1.fireTrapObject != null) trap1.fireTrapObject.SetActive(true);
            if (trap2.fireTrapObject != null) trap2.fireTrapObject.SetActive(true);

            yield return new WaitForSeconds(fireDuration);

            
            if (trap1.decalObject != null) trap1.decalObject.SetActive(false);
            if (trap1.fireTrapObject != null) trap1.fireTrapObject.SetActive(false);

            if (trap2.decalObject != null) trap2.decalObject.SetActive(false);
            if (trap2.fireTrapObject != null) trap2.fireTrapObject.SetActive(false);

            
            yield return new WaitForSeconds(timeBetweenRounds);
        }

        PuzzleCompleted();
    }
    

    private void PuzzleCompleted()
    {
        _isPuzzleCompleted = true;
        _isPuzzleActive = false;

        if (entryFireWall != null) entryFireWall.SetActive(false);
        if (exitFireWall != null) exitFireWall.SetActive(false);

        GetComponent<Collider>().enabled = false;
        Debug.Log("Bulmaca tamamlandý");
    }
}