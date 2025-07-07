using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DataForm;
using Newtonsoft.Json;
using UnityEngine.Rendering.Universal.Internal;

public class GameStartUI : MonoBehaviour
{
    private BrowserRequest browserRequest;
    public MatchClient matchClient;
    
    //UI 요소
    public Button gameStartButton;
    public Text gameStartButtonText;
    public Text scoreText;
    public Text rankingText;

    private void Awake()
    {
        browserRequest = new BrowserRequest();
        gameStartButtonText.text = "게임 시작!";
    }

    // Start is called before the first frame update
    void Start()
    {
        gameStartButton.onClick.AddListener(() =>
        {
            int requestId = browserRequest.StartRequest("POST", "/game/match/join");
            StartCoroutine(browserRequest.waitForResponse(requestId, 5.0f, (response) =>
            {
                if (response != null)
                {
                    long userId = JsonConvert.DeserializeObject<long>(response.body);
                    //매칭 요청 인증이 완료되었으므로 매칭 서버 연결 시작
                    //매칭 중 UI로 변경
                    gameStartButtonText.text = "매칭 중...\n(여기를 눌러 취소)";
                    DataManager.Instance.id = userId;
                    matchClient.startConnection();
                }
                else
                {
                    Debug.Log("정보 조회에 실패했습니다.");
                }
            }));
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void loadGameStartUI()
    {
        //서버에게 해당 유저의 정보 요청, 5초까지 기다림
        int requestId = browserRequest.StartRequest("GET", "/game/test");
        StartCoroutine(browserRequest.waitForResponse(requestId, 5.0f, (response) =>
        {
            if (response != null)
            {
                var userGameData = JsonConvert.DeserializeObject<UserGameData>(response.body);
                setGameStartUI(userGameData.game1Score, userGameData.ranking);
            }
            else
            {
                Debug.Log("정보 조회에 실패했습니다.");
            }

        }));
    }

    private void setGameStartUI(int score, long ranking)
    {
        scoreText.text = score.ToString();
        rankingText.text = ranking.ToString();
    }
}
