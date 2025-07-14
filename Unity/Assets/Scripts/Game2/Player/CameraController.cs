using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; //����ٴ� �÷��̾�
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0,0,-10); //ī�޶�� �÷��̾��� �Ÿ� ������

    private Camera cam;
    private float originalSize;
    [SerializeField] private float zoomSize = 4f; //���εǾ��� ���� ī�޶� ũ��
    [SerializeField] private float zoomSpeed = 1f; //�ܾƿ� �ӵ�
    private bool isReviveZooming = false;

    private void Start()
    {
        cam = GetComponent<Camera>();
        originalSize = cam.orthographicSize;
    }

    public void SetReviveZoom(bool isZooming)
    {
        this.isReviveZooming = isZooming;
    }

    private void LateUpdate()
    {
        if(target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }

        float targetSize = isReviveZooming ? zoomSize : originalSize;
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);
    }
}
