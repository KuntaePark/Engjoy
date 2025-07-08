using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using DataForm;

//plugin for websocket support in webgl builds
using HybridWebSocket;

public class LobbyClient : WebSocketClient
{
    private string lobbyServerUrl = "ws://localhost:7777"; // URL of the lobby server

    public PlayerManager playerManager;

    private void Awake()
    {
        startConnection(lobbyServerUrl);
    }

    public override void handleOpen()
    {
        //send authentication information
        string message = JsonConvert.SerializeObject(DataManager.Instance.id);
        Send("auth", message);
    }

    public override void handlePacket(string type, string payload)
    {
        switch (type)
        {
            case "lobby_enter_success":
                //로비 씬 로드
                Debug.Log("Lobby enter success, loading lobby scene.");
                if(playerManager == null)
                {
                    Debug.LogError("PlayerManager not found in the scene.");
                    return;
                }
                break;
            case "player_update":
                //handle player update
                if(playerManager == null)
                {
                    Debug.LogError("PlayerManager is not initialized.");
                    return;
                }
                playerManager.updatePlayers(payload);
                break;
            case "player_exit":
                if(playerManager == null)
                {
                    Debug.LogError("PlayerManager is not initialized.");
                    return;
                }
                playerManager.exitPlayer(long.Parse(payload));
                break;
            default:
                Debug.LogWarning("Unknown packet type: " + type);
                break;
        }
    }
}
