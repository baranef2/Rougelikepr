using UnityEngine;
using System.Collections;
using System.Collections.Generic;


 [RequireComponent(typeof(Collider))]
public class SimpleVFXSequencer : MonoBehaviour
{
    [Header("VFX Tuzak Listesi")]
    [Tooltip("Sýrayla aktive edilecek VFX GameObjelerinin listesi")]
    [SerializeField] private List<GameObject> vfxTraps;

    [Header("Zamanlama Ayarlarý")]
    [Tooltip("Bir tuzak bittikten sonra diðeri baþlamadan önce beklenecek süre")]
    [SerializeField] private float delayBetweenTraps = 0.2f;

    [Tooltip("Tüm döngü bittikten sonra baþa dönmeden önce beklenecek süre")]
    [SerializeField] private float delayAfterFullCycle = 1.0f;

    [Tooltip("Oyun baþlar baþlamaz sekans otomatik baþlasýn mý?")]
    [SerializeField] private bool startOnAwake = false;
    private bool _hasStarted=false;

    private void Start()
    {
        
        foreach (var trap in vfxTraps)
        {
            if (trap != null)
                trap.SetActive(false);
        }

        if (startOnAwake)
        {
            StartCoroutine(SequenceCoroutine());
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        
        if (_hasStarted || !other.CompareTag("Player"))
        {
            return;
        }

        // Döngüyü baþlat
        StartTrapSequence();
    }

    public void StartTrapSequence() 
    {
        if (_hasStarted) return; 

        _hasStarted = true;
        StartCoroutine(SequenceCoroutine());
    }

    private IEnumerator SequenceCoroutine()
    {
        while (true) 
        {
            
            for (int i = 0; i < vfxTraps.Count; i++)
            {
                GameObject currentTrap = vfxTraps[i];
                if (currentTrap == null) continue;

                
                currentTrap.SetActive(true);

                
                yield return new WaitUntil(() => !currentTrap.activeInHierarchy);

                
                yield return new WaitForSeconds(delayBetweenTraps);
            }

            
            yield return new WaitForSeconds(delayAfterFullCycle);
        }
    }
}