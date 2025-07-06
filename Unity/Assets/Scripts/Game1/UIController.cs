using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DataForm;

public class UIController : MonoBehaviour
{

    public Game1Manager game1Manager; //게임 매니저 스크립트 참조

    public PlayerPanel[] playerPanels = new PlayerPanel[2]; //플레이어 패널 배열
    public WordPanel wordPanel; //단어 패널

    //시간
    public Slider TimeBar;
    public Text TimeText;

    // Start is called before the first frame update
    void Start()
    {
        TimeBar.maxValue = Game1Manager.timeLimit * 1000; //슬라이더 최대값 설정
        TimeBar.value = Game1Manager.timeLimit * 1000; //슬라이더 초기값 설정
    }

    // Update is called once per frame
    void Update()
    {

        if (game1Manager.gameState.state == "start")
        {
            var players = game1Manager.gameState.players;
            for(int i = 0; i < 2; i++)
            {
                playerPanels[i].showPlayerInfo(players[i]);
                if(i != game1Manager.myIdx)
                {
                    //상대방의 액션 선택은 서버와 동기화
                    string action = players[i].currentAction;
                    switch(action)
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
                            playerPanels[i].selected = -1; //선택 안함
                            break;
                    }
                }
            }

            TimeBar.value = game1Manager.getTimesLeft();
            TimeText.text = $"{game1Manager.getTimesLeft() / 1000}";

            //단어 선택 UI
            var myInfo = players[game1Manager.myIdx];
            if(myInfo.isActionSelected)
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
    }

    public void setAction(int selected)
    {
        playerPanels[game1Manager.myIdx].selected = selected;
    }
}
