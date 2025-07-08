using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyEnterRequester : MonoBehaviour
{
    // Start is called before the first frame update

    private BrowserRequest browserRequest = new BrowserRequest();

    [SerializeField]
    private Button EnterButton;

    void Start()
    {
        EnterButton.onClick.AddListener(requestLobbyEnter);
    }

    private void requestLobbyEnter()
    {
        int requestId = browserRequest.StartRequest("POST", "/game/lobby/join", "");

        Debug.Log("Lobby enter request sent.");
        StartCoroutine(browserRequest.waitForResponse(requestId, 10.0f, (response) =>
        {
            if(response != null && response.status == 200)
            {
                Debug.Log("Lobby enter successful: " + response.body);
                //로비 요청 승인 완료, 로비 접속 시도
                DataManager.Instance.id = long.Parse(response.body);
                SceneController.Instance.loadScene("LobbyScene");
            }
            else
            {
                Debug.LogError("Lobby enter failed: " + (response != null ? response.body : "No response received."));
            }
        }));
    }
}
