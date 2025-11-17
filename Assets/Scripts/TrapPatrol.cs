using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PatrolTrap : MonoBehaviour
{
    
    [SerializeField] private List<Transform> waypoints;

    
    [SerializeField] private float moveSpeed = 5f;

    private int _waypointIndex = 0; 
    private int _direction = 1;     

    private void Start()
    {
        
        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogError(gameObject.name + " için waypoint ayarlanmamýþ! Script devre dýþý býrakýlýyor.", this);
            this.enabled = false;
            return;
        }

        
        if (waypoints.Count == 1)
        {
            transform.position = waypoints[0].position; 
            this.enabled = false; 
            return;
        }

        
        transform.position = waypoints[0].position;

        
        _waypointIndex = 1;
        _direction = 1;
    }

    private void Update()
    {
        
        Vector3 targetPosition = waypoints[_waypointIndex].position;

        
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            

            if (_direction == 1) 
            {
                _waypointIndex++; 

                
                if (_waypointIndex >= waypoints.Count)
                {
                    
                    _waypointIndex = waypoints.Count - 2; 
                    _direction = -1; 
                }
            }
            else 
            {
                _waypointIndex--; 

                
                if (_waypointIndex < 0)
                {
                    
                    _waypointIndex = 1; 
                    _direction = 1; 
                }
            }
        }
    }
}