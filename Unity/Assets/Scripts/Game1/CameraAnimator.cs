using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAnimator : MonoBehaviour
{
    [SerializeField]
    private Transform[] targetLocations = new Transform[2];
    [SerializeField]
    private float targetSize = 2.0f;
    [SerializeField]
    private float duration = 1.0f;

    public float slowTimeScale = 0.2f; // 슬로우 모션 비율
    public float slowDuration = 2.0f;  // 슬로우 모션 지속 시간 (현실 시간 기준)


    private Camera cam;
    private float originalSize;
    private Vector3 originalPos;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        originalSize = cam.orthographicSize;
        originalPos = cam.transform.position;
    }

    // Update is called once per frame

    public IEnumerator finalBlowCameraMovement(Animator animator, int targetIdx, Action onEnd)
    {
        Transform target = targetLocations[targetIdx];
        float time = 0f;

        Vector3 startPos = cam.transform.position;
        Vector3 targetPos = new Vector3(target.position.x, target.position.y, startPos.z);

        Time.timeScale = slowTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale; // FixedUpdate도 느려지게

        animator.SetTrigger("dead");

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = time / duration;

            cam.orthographicSize = Mathf.Lerp(originalSize, targetSize, t);
            cam.transform.position = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        cam.orthographicSize = targetSize;
        cam.transform.position = targetPos;

        yield return new WaitForSecondsRealtime(slowDuration);

        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;
        onEnd?.Invoke();
    }


}
