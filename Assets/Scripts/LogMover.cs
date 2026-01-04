using UnityEngine;

public class LogMover : MonoBehaviour
{
    
    public float speed = 10f; 
    public float lifeTime = 10f; 

    
    public float direction = -1f;

    void Start()
    {
       
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
       
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime);
    }
}