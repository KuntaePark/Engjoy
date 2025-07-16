using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DataForm;
using TMPro;

public class UIController : MonoBehaviour
{

    public Game1Manager game1Manager; //게임 매니저 스크립트 참조

    public PlayerPanel[] playerPanels = new PlayerPanel[2]; //플레이어 패널 배열
    public WordPanel wordPanel; //단어 패널

    private BrowserRequest browserRequest = new BrowserRequest();

    //시간
    public Slider TimeBar;
    public TextMeshProUGUI TimeText;

    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText; //게임 오버 텍스트

    //게임 오버 버튼
    public Button lobbyButton;

    [SerializeField]
    private GameObject countdownUI;
    [SerializeField]
    private Text countdownText;

    private Animator mainUIAnimator;
    private bool beforeGameStart = true; //게임 시작 전 상태

    // Start is called before the first frame update
    void Start()
    {
        gameOverPanel.SetActive(false); //게임 오버 패널 비활성화
        countdownUI.SetActive(false); //카운트다운 UI 비활성화
        TimeBar.maxValue = Game1Manager.timeLimit * 1000; //슬라이더 최대값 설정
        TimeBar.value = Game1Manager.timeLimit * 1000; //슬라이더 초기값 설정
        mainUIAnimator = gameObject.GetComponent<Animator>();

        lobbyButton.onClick.AddListener(() => 
        {
            int requestId = browserRequest.StartRequest("POST", "/game/lobby/join", "");
            StartCoroutine(browserRequest.waitForResponse(requestId, 10.0f, (response) =>
            {
                if (response != null && response.status == 200)
                {
                    Debug.Log("Lobby enter successful: " + response.body);
                    //로비 요청 승인 완료, 로비 접속 시도
                    SceneController.Instance.loadScene("LobbyScene");
                }
                else
                {
                    Debug.LogError("Lobby enter failed: " + (response != null ? response.body : "No response received."));
                }
            }));

        });
    }

    // Update is called once per frame
    void Update()
    {
        if(game1Manager.gameState.state == "countdown")
        {
            countdownUI.SetActive(true);
            long countdownTime = game1Manager.getCountdownTimeLeft() / 1000;
            if (countdownTime > 0)
            {
                countdownText.text = countdownTime.ToString();
            }
            else
            {
                if(beforeGameStart)
                {
                    beforeGameStart = false; //게임 시작 전 상태 변경
                    mainUIAnimator.SetTrigger("startGame"); //애니메이션 트리거 설정
                }
                countdownText.text = "게임 시작!";
            }
            return;
        }


        if (game1Manager.gameState.state == "start")
        {
            try
            {
                countdownUI.SetActive(false); //카운트다운 UI 비활성화
                var players = game1Manager.gameState.players;
                for (int i = 0; i < 2; i++)
                {
                    playerPanels[i].showPlayerInfo(players[i]);
                    if (i != game1Manager.myIdx)
                    {
                        //상대방의 액션 선택은 서버와 동기화
                        string action = players[i].currentAction;
                        switch (action)
                        {
                            case "ATTACK":
                                playerPanels[i].selected = 0; //공격
                                break;
                            case "DEFENSE":
                                playerPanels[i].selected = 1; //방어
                                break;
                            case "SPECIAL":
                                playerPanels[i].selected = 2; //스페셜
                                break;
                            default:
                                playerPanels[i].selected = 0; //선택 안함
                                break;
                        }
                    }
                }

                TimeBar.value = game1Manager.getTimesLeft();
                TimeText.text = $"{game1Manager.getTimesLeft() / 1000}";

                //단어 선택 UI
                var myInfo = players[game1Manager.myIdx];
                if (myInfo.isActionSelected)
                {
                    wordPanel.activateOptions();
                    wordPanel.showWord(myInfo);
                    playerPanels[game1Manager.myIdx].setButtonText(true);
                }
                else
                {
                    wordPanel.deactivateOptions();
                    playerPanels[game1Manager.myIdx].setButtonText(false);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error in UIController Update: " + e.Message);
            }
        }
    }

    public void setAction(int selected)
    {
        playerPanels[game1Manager.myIdx].selected = selected;
    }

    public void showGameOver(int winnerIdx)
    {
        gameOverPanel.SetActive(true);
        if (winnerIdx == game1Manager.myIdx)
        {
            gameOverText.text = "승리!";
        }
        else if(winnerIdx == 2)
        {
            gameOverText.text = "무승부";
        }
        else
        {
            gameOverText.text = "패배!";
        }
    }
}
