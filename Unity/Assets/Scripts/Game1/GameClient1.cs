using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

using DataForm;

/*
 * 게임 1 전용 웹소켓 연결
 */

public class GameClient1 : WebSocketClient
{
    public string id { get; private set; } //플레이어 ID (서버로부터 배정받음)
    public Game1Manager game1Manager; //게임 매니저 스크립트 참조

    private void Awake()
    {
        //서버 연결
        startConnection("ws://192.168.0.51:7778");
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public override void handleOpen()
    {
        Send("auth",JsonConvert.SerializeObject(DataManager.Instance.id));
    }

    public override void handlePacket(string type, string payload)
    {
        switch (type) 
        {
            case "session_connect":
                //서버로부터 세션 ID를 받았을 때
                int idx = JsonConvert.DeserializeObject<int>(payload);
                game1Manager.myIdx = idx;
                break;
            case "gameState":
                //게임 상태 업데이트를 받았을 때
                //Debug.Log("received: " + payload);
                game1Manager.UpdateGameState(payload);
                break;
            case "gameEnd":
                //게임 종료 메시지를 받았을 때
                Debug.Log("Game ended: " + payload);
                int winnerIdx = JsonConvert.DeserializeObject<int>(payload);
                game1Manager.endGame(winnerIdx);
                break;
            default:
                Debug.LogWarning("Unknown packet type: " + type);
                break;
        }

    }
}
