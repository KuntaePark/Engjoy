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

    private bool inMatch = false;

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
            if(inMatch)
            {                 //매칭 취소 요청
                matchClient.Send("match_cancel", "");
                inMatch = false;
                gameStartButtonText.text = "게임 시작!";
                return;
            }
            else
            {
                inMatch = true;
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
                        //실패 시 행동 여기에 추가
                    }
                }));
            }
        });
    }

    public void loadGameStartUI()
    {
        //서버에게 해당 유저의 정보 요청, 5초까지 기다림
        StartCoroutine(DataManager.Instance.getUserData((userGameData) =>
        {
            if (userGameData != null)
            {
                //유저의 게임 시작 UI 설정
                setGameStartUI(userGameData.game1Score, userGameData.ranking);
            }
            else
            {
                Debug.Log("유저 정보 로드 실패");
            }
        }));
    }

    private void setGameStartUI(int score, long ranking)
    {
        scoreText.text = score.ToString();
        rankingText.text = ranking.ToString();
    }
}
