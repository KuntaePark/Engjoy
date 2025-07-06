using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using DataForm;

//plugin for websocket support in webgl builds
using HybridWebSocket;

public class LobbyClient : WebSocketClient
{
    // Start is called before the first frame update
    private WebSocket ws;
    private string playerId;
    public string PlayerId{ get; set; }
    private int lobbyId;

    public PlayerManager playerManager;

    void Start()
    {
        startConnection("ws://localhost:7777");
    }

    public override void handlePacket(string type, string payload)
    {
        switch (type)
        {
            case "lobbyEnterSuccess":
                //receive assigned player ID
                Dictionary<string, string> idInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(payload);
                Debug.Log("Assigned Player ID: " + idInfo["playerId"] + ", lobbyId: " + idInfo["lobbyId"]);
                PlayerId = idInfo["playerId"];
                lobbyId = int.Parse(idInfo["lobbyId"]);
                break;
            case "playerUpdate":
                //handle player update
                playerManager.updatePlayers(payload);
                break;
            case "playerExit":
                playerManager.exitPlayer(payload);
                break;
            default:
                Debug.LogWarning("Unknown packet type: " + type);
                break;
        }
    }
}
