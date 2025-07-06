using DataForm;
using HybridWebSocket;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//웹 소켓 클라이언트 클래스.
public abstract class WebSocketClient : MonoBehaviour
{
    private WebSocket ws;

    //연결 시작 시 호출하는 메소드. 서버 URL을 인자로 받아 웹소켓 연결을 시작
    public void startConnection(string serverUrl)
    {
        ws = WebSocketFactory.CreateInstance(serverUrl);
        ws.Connect();
        ws.OnOpen += () =>
        {
            handleOpen();
            Debug.Log("Connected to WebSocket server at " + serverUrl);
        };

        ws.OnMessage += Call;
    }

    private void OnDestroy()
    {
        if (ws != null && ws.GetState() == WebSocketState.Open)
        {
            handleClose();
            ws.Close();
            Debug.Log("WebSocket connection closed.");
        }
    }

    //메시지 수신 처리 메소드. 오버라이딩하여 사용
    public void Call(byte[] message)
    {
        string JsonData = System.Text.Encoding.UTF8.GetString(message);
        Packet packet = JsonConvert.DeserializeObject<Packet>(JsonData);
        handlePacket(packet.type, packet.payload);
    }

    //메시지 전송 메소드.
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

    //수신 패킷 처리 메소드. 오버라이딩하여 사용
    public abstract void handlePacket(string type, string payload);

    //웹 소켓 연결 성공 시 호출되는 메소드. 기본 구현은 아무 작업도 하지 않음.
    public virtual void handleOpen()
    {
        return;
    }
    
    //웹 소켓 연결 종료 시 호출되는 메소드. 기본 구현은 아무 작업도 하지 않음.
    public virtual void handleClose()
    {
        return;
    }

}
