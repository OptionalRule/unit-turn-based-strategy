using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    private Vector3 targetPosition;
    private Vector3 direction;
    private float speed = 100f;
    public void Setup(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
        direction = (targetPosition - transform.position).normalized;
    }

    public void Update()
    {
        float startDistance = Vector3.Distance(transform.position, targetPosition);
        transform.position += direction * speed * Time.deltaTime;
        float endDistance = Vector3.Distance(transform.position, targetPosition);

        if(endDistance > startDistance)
        {
            Destroy(gameObject);
        }
    }
    
}
