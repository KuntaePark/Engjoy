using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataForm;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class MatchClient : WebSocketClient
{
    private const string matchServerUrl = "ws://localhost:7779";

    public long id { get; set; }
    // Start is called before the first frame update

    public void startConnection()
    {
        startConnection(matchServerUrl);
    }

    public override void handleOpen()
    {
        //인증 정보 전송
        string message = JsonConvert.SerializeObject(new { id = DataManager.Instance.id });
        Send("auth", message);
    }

    public override void handlePacket(string type, string payload)
    {
        switch (type)
        {
            case "match_success":
                //매칭 성공, 게임 화면 로드
                int gameId = JsonConvert.DeserializeObject<int>(payload);
                if (gameId == 0)
                {
                    SceneController.Instance.loadScene("game1Scene");
                }
                else if (gameId == 1)
                {
                    SceneController.Instance.loadScene("Game2-MatchingRoom");
                }
                break;
            default:
                Debug.Log("unknown packet type.");
                break;
        }
    }

}
