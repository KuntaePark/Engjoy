using System;
using System.Collections;
using System.Collections.Specialized;
using DataForm;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class ResultUIManager : MonoBehaviour
{
    private BrowserRequest browserRequest = new BrowserRequest();

    public static ResultUIManager Instance { get; private set; }

    [Header("Effect & Main Panels")]
    public Image screenEffectImage; //게임오버 이펙트용 이미지
    public GameObject gameOverText; //게임오버 텍스트
    public GameObject resultPanel;

    [Header("Page 1 UI")]
    public GameObject page1;
    public TextMeshProUGUI stageText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI goldText;
    public Button nextPageButton;
    public Button lobbyButton;

    [Header("Page 2 UI")]
    public GameObject page2;
    public GameObject sentenceItemPrefab;
    public Transform sentenceListContent; //문장들 들어갈 Content
    //public Button backToLobbyButton; //로비로 버튼
    //public Button againButton; //한판더 버튼

    [Header("Sound Effects")]
    [SerializeField] private AudioClip gameOverSound; // 게임오버 텍스트 등장 시 재생할 사운드
    private AudioSource audioSource;



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        //버튼 이벤트 연결
        if (nextPageButton != null) nextPageButton.onClick.AddListener(ShowNextPage);
        lobbyButton.onClick.AddListener(() =>
        {
            int requestId = browserRequest.StartRequest("POST", "/game/lobby/join", "");
            StartCoroutine(browserRequest.waitForResponse(requestId, 10.0f, (response) =>
            {
                if (response != null && response.status == 200)
                {
                    Debug.Log("Lobby enter successful: " + response.body);
                    //로비 요청 승인 완료, 로비 접속 시도
                    disableAllUI();
                    removeAllInstances();
                    //데이터 강제 로드 위해 데이터 초기화
                    DataManager.Instance.resetUserData();
                    StartCoroutine(DataManager.Instance.getUserData((userGameData) => { SceneController.Instance.loadScene("LobbyScene"); }));
                    ;
                }
                else
                {
                    Debug.LogError("Lobby enter failed: " + (response != null ? response.body : "No response received."));
                }
            }));

        });
        //로비 버튼, 한판더 버튼 이벤트 연결

    }

    public void disableAllUI()
    {
        //게임 오버되면 인게임 UI 싹 끄기
        if (UIManager.Instance != null)
        {
            UIManager.Instance.matchingRoomPanel.SetActive(false);
            UIManager.Instance.playingPanel.SetActive(false);
            UIManager.Instance.Inventory.SetActive(false);
            UIManager.Instance.MyStatusUI.SetActive(false);
            UIManager.Instance.playerStatusLayout.gameObject.SetActive(false);
        }


        //시작 시 모든 관련 UI를 끈 상태로 초기화
        if (resultPanel != null) resultPanel.SetActive(false);
        if (gameOverText != null) gameOverText.SetActive(false);
        if (screenEffectImage != null) screenEffectImage.color = new Color(0, 0, 0, 0);
    }

    public void removeAllInstances()
    {
        //해당 게임의 instance 모두 제거
        Destroy(WsClient.Instance.gameObject);
        Destroy(PlayerManager.Instance.gameObject);
        Destroy(KeywordManager.Instance.gameObject);
        Destroy(GameManager.Instance.gameObject);
        Destroy(ExitUIManager.Instance.gameObject);
        Destroy(Instance.gameObject);
    }

    public void StartGameOverSequence(GameState finalState)
    {
        disableAllUI();

        StartCoroutine(GameOverSequenceCoroutine(finalState));
    }



    private IEnumerator GameOverSequenceCoroutine(GameState finalState)
    {

        if (screenEffectImage != null) screenEffectImage.gameObject.SetActive(true);

        //화면 연출
        screenEffectImage.color = new Color(1, 1, 1, 0.5f);
        yield return new WaitForSeconds(0.1f);

        float fadeOutDuration = 0.5f;
        float timer = 0f;
        Color startColor = screenEffectImage.color;
        Color targetColor = new Color(0.2f, 0.2f, 0.2f, 0.7f);

        if (gameOverText != null) gameOverText.SetActive(false);

        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            screenEffectImage.color = Color.Lerp(startColor, targetColor, timer / fadeOutDuration);
            yield return null;

        }
        screenEffectImage.color = targetColor; //화면 잿빛으로

        yield return new WaitForSeconds(1.0f); //1초 대기 후 Game Over 텍스트 표시

        //Game Over 텍스트 띄우기
        if (gameOverText != null)
        {
            if (audioSource != null && gameOverSound != null)
            {
                audioSource.PlayOneShot(gameOverSound);
            }

            gameOverText.SetActive(true);
            yield return StartCoroutine(ShakeObjectCoroutine(gameOverText, 0.15f, 15f));
        }
        yield return new WaitForSeconds(2.3f);

        //서서히 검은 화면으로(2.7초동안)
        timer = 0f;
        startColor = screenEffectImage.color;
        fadeOutDuration = 2.7f;

        if (gameOverText != null)
        {
           gameOverText.SetActive(false);
        }

            while (timer < fadeOutDuration)
            {
                timer += Time.deltaTime;
                screenEffectImage.color = Color.Lerp(startColor, Color.black, timer / fadeOutDuration);
                yield return null;
            }
            screenEffectImage.color = Color.black;
            yield return new WaitForSeconds(1.0f);

            //결과창 보여주기
            //gameOverText.SetActive(false);
            resultPanel.SetActive(true);
            UpdateResultUI(finalState);
            GameManager.Instance.NotifyResultVisible(); //결과창 뜸!

        
    }

   private IEnumerator ShakeObjectCoroutine(GameObject targetObject, float duration, float magnitude)
    {
        Vector3 originalPos = targetObject.transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            float y = UnityEngine.Random.Range(-1f, 1f) * magnitude;

            targetObject.transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }
        targetObject.transform.localPosition = originalPos;
    }


    //결과창 내용 띄워주기
    void UpdateResultUI(GameState finalState)
    {
        //페이지1 내용 채우기
        if (stageText != null) stageText.text = $"CLEARED STAGE: {finalState.gameLevel - 1}";
        if (scoreText != null) scoreText.text = $"TOTAL SCORE: {finalState.score}";
        if (goldText != null) goldText.text = $"EARNED GOLD: {finalState.gold}";

        //페이지2_문장 리스트 내용 채우기
        //기존에 있던 리스트 삭제
        if (sentenceListContent != null)
        {
            foreach (Transform child in sentenceListContent)
            {
                Destroy(child.gameObject);
            }
        }

        //서버에서 받은 문장 목록으로 새로 생성
        if (sentenceItemPrefab != null && finalState.completedSentences != null)
        {
            foreach (SentenceData sentence in finalState.completedSentences)
            {
                GameObject itemGO = Instantiate(sentenceItemPrefab, sentenceListContent);
                //프리팹에 있는 Text 컴포넌트 찾아서 내용 채워넣기 (영문장 텍스트, 해설문 넣어주기)
                TextMeshProUGUI[] texts = itemGO.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length >= 2)
                {
                    texts[0].text = sentence.text;
                    texts[1].text = sentence.meaning;
                }
            }
        }

        //초기엔 1페이지만 보여주기
        if (page1 != null) page1.SetActive(true);
        if (page2 != null) page2.SetActive(false);
    }


    //다음 페이지 보기 버튼
    void ShowNextPage()
    {
        if (page1 != null) page1.SetActive(false);
        if (page2 != null) page2.SetActive(true);
    }

    //로비로 돌아가기 버튼

    //한 판 더 버튼
}
