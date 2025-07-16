using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using DataForm;



//plugin for websocket support in webgl builds
using HybridWebSocket;
using System.Collections.Concurrent;
using System;



public class WsClient : WebSocketClient
{
    public static WsClient Instance { get; private set; }

    private string game2ServerUrl = "ws://localhost:7780";

    //메인 스레드에서 실행할 액션을 담을 큐
    private readonly ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }


    void Start()
    {
        startConnection(game2ServerUrl);
    }

    public override void handleOpen()
    {
        //인증 정보 전송
        string message = JsonConvert.SerializeObject(new { id = DataManager.Instance.id });
        Send("auth", message);
    }


    void Update()
    {
        // 큐에 실행할 액션이 있는지 확인
        while (mainThreadActions.TryDequeue(out var action))
        {
            // 큐에서 액션을 꺼내와 메인 스레드에서 실행
            action?.Invoke();
        }
    }

    public override void handlePacket(string type, string payload)
    {
        try
        {
            switch (type)
            {
                //payload를 DataForm의 GameState로 파싱
                case "gameStateUpdate":
                    GameState newState = JsonConvert.DeserializeObject<GameState>(payload);
                    //파싱된 GameState를 GameManager에 싹 보내줌
                    mainThreadActions.Enqueue(() =>
                    {
                        GameManager.Instance.UpdateGameState(newState);
                    });
                    break;
                default:
                    Debug.LogWarning("Unknown packet type received: " + type);
                    break;

            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("!!!!!!!!!! REAL ERROR FOUND !!!!!!!!!!");
            Debug.LogError("[Error Message] " + e.Message);
            Debug.LogError("[Error Source] " + e.Source);
            Debug.LogError("[Stack Trace] " + e.StackTrace);
        }
    }
}