using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; //따라다닐 플레이어
    public float smoothSpeed = 10.0f;
    public Vector3 offset = new Vector3(0,0,-10); //카메라와 플레이어의 거리 오프셋

    private Camera cam;
    private float originalSize;
    [SerializeField] private float zoomSize = 4f; //줌인되었을 때의 카메라 크기
    [SerializeField] private float zoomSpeed = 1f; //줌아웃 속도
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
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
            transform.position = smoothedPosition;
        }

        float targetSize = isReviveZooming ? zoomSize : originalSize;
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);
    }
}
