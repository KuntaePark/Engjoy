using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataForm;
using Newtonsoft.Json;

public class MatchClient : WebSocketClient
{
    private const string matchServerUrl = "ws://localhost:7779";

    public long id { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void startConnection()
    {
        startConnection(matchServerUrl);
    }

    public override void handleOpen()
    {
        //인증 정보 전송
        string message = JsonConvert.SerializeObject(DataManager.Instance.id);
        Send("auth", message);
    }

    public override void handlePacket(string type, string payload)
    {
        switch(type)
        {
            case "match_success":
                //매칭 성공, 게임 화면 로드
                SceneController.Instance.loadScene("game1Scene");
                break;
            default:
                Debug.Log("unknown packet type.");
                break;
        }
    }

}
