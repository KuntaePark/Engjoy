using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    [SerializeField]
    private CanvasGroup loadingCanvasGroup;
    [SerializeField]
    private float fadeDuration = 0.5f; // 페이드 아웃/인 시간

    private string loadSceneName;

    private bool sceneLoaded = false;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }


    public void loadScene(string sceneName)
    {
        Debug.Log("Loading scene: " + sceneName);
        gameObject.SetActive(true);
        sceneLoaded = false;
        loadSceneName = sceneName;
        SceneManager.sceneLoaded += LoadSceneEnd;
        StartCoroutine(Load(sceneName));
    }

    private IEnumerator Load(string sceneName)
    {
        yield return StartCoroutine(Fade(true));

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = true; //webgl 조건으로 강제 true 설정
        
        float minDelay = 0.5f; // 최소 대기 시간
        float elapsedTime = 0f;

        while(elapsedTime < minDelay || !sceneLoaded)
        {
            elapsedTime += Time.deltaTime;
            yield return null; // 매 프레임 대기
        }
        
        Debug.Log("Scene loaded: " + sceneName);
        yield return StartCoroutine(Fade(false)); // 페이드 인 시작
        

    }


    private void LoadSceneEnd(Scene scene, LoadSceneMode loadSceneMode)
    {
        if(scene.name == loadSceneName)
        {
            sceneLoaded = true; // 씬이 로드되었음을 표시
            SceneManager.sceneLoaded -= LoadSceneEnd; // 이벤트 핸들러 제거
        }
    }

    private IEnumerator Fade(bool isFadeOut)
    {
        if (isFadeOut)
        {
            loadingCanvasGroup.alpha = 0f;
            loadingCanvasGroup.blocksRaycasts = true; //클릭 차단
            while (loadingCanvasGroup.alpha < 1f)
            {
                loadingCanvasGroup.alpha += Time.deltaTime / fadeDuration; // 1초 동안 페이드 아웃
                yield return null;
            }
        }
        else
        {
            while (loadingCanvasGroup.alpha > 0f)
            {
                loadingCanvasGroup.alpha -= Time.deltaTime / fadeDuration; // 1초 동안 페이드 인
                yield return null;
            }
            loadingCanvasGroup.blocksRaycasts = false; // 클릭 차단 해제
            gameObject.SetActive(false); // 페이드 인이 끝나면 오브젝트 비활성화
        }
    }

}
