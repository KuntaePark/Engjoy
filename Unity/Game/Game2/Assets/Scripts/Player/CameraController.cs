using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; //����ٴ� �÷��̾�
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0,0,-10); //ī�޶�� �÷��̾��� �Ÿ� ������

    private void LateUpdate()
    {
        if(target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }
}
