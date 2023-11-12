using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    private Vector3 targetPosition;
    [SerializeField] private float speed = 200f;
    public void Setup(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
    }

    public void Update()
    {
        float distanceBefore = Vector3.Distance(transform.position, targetPosition);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        float distanceAfter = Vector3.Distance(transform.position, targetPosition);

        if(distanceBefore < distanceAfter)
        {
            Destroy(gameObject);
        }
    }
    
}
