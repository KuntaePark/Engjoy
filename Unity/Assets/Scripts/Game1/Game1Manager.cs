using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using DataForm;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class Game1Manager : MonoBehaviour
{

    [System.Serializable]
    public class GameState
    {
        public long startTime { get; set; } //게임 시작 시간
        public string state = "ready";
        public Game1PlayerData[] players { get; set; } = new Game1PlayerData[2]; //플레이어 데이터 배열
    }

    public GameClient1 gameClient; //게임 클라이언트 스크립트 참조
    public GameState gameState; //게임 상태 데이터
    public int myIdx = -1;
    
    public CameraAnimator camAnimator;


    public const int timeLimit = 99; //게임 시간 제한(초)
    public const int countdownTime = 4; //카운트다운 시간(초)

    public UIController uiController; //UI 컨트롤러 스크립트 참조

    public CharacterRenderer[] characterRenderers = new CharacterRenderer[2]; //캐릭터 렌더러 배열

    // Start is called before the first frame update
    void Start()
    {
        for(int i=0; i < 2; i++)
        {
            gameState.players[i] = new Game1PlayerData(); // 플레이어 데이터 초기화
            characterRenderers[i].SetBody(0); //기본 바디 타입 설정
            characterRenderers[i].SetWeapon(0); //기본 무기 타입 설정
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateGameState(string payload)
    {
        //게임 상태 업데이트 처리
        gameState = JsonConvert.DeserializeObject<GameState>(payload);
        //캐릭터 업데이트
        for(int i = 0; i < 2; i++)
        {
            if (characterRenderers[i].bodyTypeIndex != gameState.players[i].bodyTypeIndex)
            {
                Debug.Log("update body type for player " + i + ": " + gameState.players[i].bodyTypeIndex);
                characterRenderers[i].SetBody(gameState.players[i].bodyTypeIndex);
            }
            if(characterRenderers[i].weaponTypeIndex != gameState.players[i].weaponTypeIndex)
            {
                Debug.Log("update weapon type for player " + i + ": " + gameState.players[i].weaponTypeIndex);
                characterRenderers[i].SetWeapon(gameState.players[i].weaponTypeIndex);
            }
        }
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
        long currentTime = (long)(DateTime.UtcNow - epoch).TotalMilliseconds;
        return timeLimit * 1000 - (currentTime - gameState.startTime - countdownTime * 1000);
    }
        
    public long getCountdownTimeLeft()
    {
        if (gameState.startTime == 0)
        {
            //게임이 시작되지 않았으면 0 반환
            return 0;
        }
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return countdownTime * 1000 + gameState.startTime - (long)(DateTime.UtcNow - epoch).TotalMilliseconds;
    }

    public void endGame(JObject gameEndData)
    {
        //게임 종료 처리
        int winnerIdx = (int)gameEndData["winner"];
        int score = (int)gameEndData["score"];
        int diff = (int)gameEndData["diff"];
        Debug.Log("Game ended. Winner index: " + winnerIdx);
        gameState.startTime = 0;
        gameState.state = "end";

        //게임 끝 연출 재생
        if(winnerIdx != 2)
        {
         
            StartCoroutine(camAnimator.finalBlowCameraMovement(characterRenderers[1 - winnerIdx].bodyAnimator, 1 - winnerIdx, () => { uiController.showGameOver(winnerIdx,score,diff); }));
        }
        else
        {
            uiController.showGameOver(winnerIdx, score, diff);
        }
        
        

    }
}
