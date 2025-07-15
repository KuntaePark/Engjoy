using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using DataForm;



//plugin for websocket support in webgl builds
using HybridWebSocket;
using System.Collections.Concurrent;
using System;



public class WsClient : MonoBehaviour
{
    public static WsClient Instance { get; private set; }
    private WebSocket ws;

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
        ws = WebSocketFactory.CreateInstance("ws://localhost:7777");
        ws.Connect();

        ws.OnOpen += () => Debug.Log("Connected");
        ws.OnMessage += Call;

        ws.OnError += (string errorMsg) =>
        {
            Debug.LogError("==== WebSocket Error: " + errorMsg + " ====");
        };

        ws.OnClose += (WebSocketCloseCode code) =>
        {
            Debug.LogWarning("==== WebSocket Closed with code: " + code.ToString() + " ====");
        };
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







    private void OnDestroy()
    {
        if (ws != null)
        {
            ws.Close();
            Debug.Log("WebSocket connection closed.");
        }
    }

    void Call(byte[] message)
    {
        string JsonData = System.Text.Encoding.UTF8.GetString(message);

        try
        {

            Packet packet = JsonConvert.DeserializeObject<Packet>(JsonData);

            switch (packet.type)
            {
                //payload를 DataForm의 GameState로 파싱
                case "gameStateUpdate":
                    GameState newState = JsonConvert.DeserializeObject<GameState>(packet.payload);
                    //파싱된 GameState를 GameManager에 싹 보내줌
                    mainThreadActions.Enqueue(() =>
                    {
                        GameManager.Instance.UpdateGameState(newState);
                    });
                    break;

                //플레이어 정보 서버에서 받아서 GameManager에 넘겨주기
                case "playerId":

                    // 큐에 추가.
                    mainThreadActions.Enqueue(() =>
                    {
                        GameManager.Instance.SetMyPlayerId(packet.payload);
                    });
                    break;

                default:
                    Debug.LogWarning("Unknown packet type received: " + packet.type);
                    break;

            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("!!!!!!!!!! REAL ERROR FOUND !!!!!!!!!!");
            Debug.LogError("[Error Message] " + e.Message);
            Debug.LogError("[Error Source] " + e.Source);
            Debug.LogError("[Stack Trace] " + e.StackTrace);
            Debug.LogError("[Original JSON Data] " + JsonData); // 에러를 일으킨 원본 데이터
        }
    }



    public void Send(string type, string JSONMessage)
    {
        if (ws != null && ws.GetState() == WebSocketState.Open)
        {
            Packet sendPacket = new Packet
            {
                type = type,
                payload = JSONMessage
            };
            byte[] data = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sendPacket));
            ws.Send(data);
            //Debug.Log("Sent packet type: " + type + ", payload: " + JSONMessage);
        }
        else
        {
            Debug.LogWarning("WebSocket is not connected.");
        }
    }
}