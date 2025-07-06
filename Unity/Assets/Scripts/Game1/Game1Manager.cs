using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using DataForm;
using Newtonsoft.Json;

public class Game1Manager : MonoBehaviour
{

    [System.Serializable]
    public class GameState
    {
        public long startTime { get; set; } //게임 시작 시간
        public string state = "ready";
        public PlayerData[] players { get; set; } = new PlayerData[2]; //플레이어 데이터 배열
    }

    public GameClient1 gameClient; //게임 클라이언트 스크립트 참조
    public GameState gameState; //게임 상태 데이터
    public int myIdx = -1;

    public const int timeLimit = 99; //게임 시간 제한(초)

    // Start is called before the first frame update
    void Start()
    {
        gameState.players[0] = new PlayerData(); // 플레이어 1 초기화
        gameState.players[1] = new PlayerData(); // 플레이어 2 초기화
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateGameState(string payload)
    {
        //게임 상태 업데이트 처리

        gameState = JsonConvert.DeserializeObject<GameState>(payload);
    }

    public bool checkActionSelected() { return gameState.players[myIdx].isActionSelected; }

    public long getTimesLeft()
    {
        if (gameState.startTime == 0)
        {
            //게임이 시작되지 않았으면 0 반환
            return 0;
        }
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return timeLimit * 1000 + gameState.startTime - (long)(DateTime.UtcNow - epoch).TotalMilliseconds;
    }

    public void endGame(int winnerIdx)
    {
        //게임 종료 처리
        Debug.Log("Game ended. Winner index: " + winnerIdx);
        gameState.startTime = 0;
        gameState.state = "end";
        if (winnerIdx == myIdx)
        {
            Debug.Log("You win!");
        }
        else if(winnerIdx == 1 - myIdx)
        {
            Debug.Log("You lose!");
        }
        else
        {
            Debug.Log("It's a draw!");
        }

    }
}
