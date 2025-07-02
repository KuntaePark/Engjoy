using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public string Id { get; private set; }
    private Vector3 targetPosition; //투사체 위치
    private float positionLerpFactor = 15f; //투사체 속도

    public void Initialize(string id, Vector3 initialPosition)
    {
        Id = id;
        transform.position = initialPosition;
        targetPosition = initialPosition;
    }

    public void UpdateState(Vector3 newPosition)
    {
        targetPosition = newPosition;
    }

    private void Update()
    {
         transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionLerpFactor);
    }
}
