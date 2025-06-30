using System.Collections;
using System.Collections.Generic;
using DataForm;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //다른 스크립트에서 GameManager를 참조할 수 있도록 싱글턴 패턴 사용
    public static GameManager Instance { get; private set; }

    public PlayerManager playerManager;
    public KeywordManager keywordManager;

    public string MyPlayerId {  get; private set; }

    private void Awake()
    {
        //인스턴스 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); //씬이 바뀌어도 파괴되지 않게 설정!
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        //player, keyword를 GameManager와 연결!
        playerManager = FindObjectOfType<PlayerManager>();
        keywordManager = FindObjectOfType<KeywordManager>();
    }

    //WSClient에 호출될 함수 : 플레이어 ID 설정
    public void SetMyPlayerId(string id)
    {
        MyPlayerId = id;
        Debug.Log($"<color=green>GameManager: My ID is set to {MyPlayerId}</color>");
    }

    //Game 내에 있는 플레이어와 키워드에게 작업 분배
    public void UpdateGameState(GameState newState)
    {
        //KeywordManager에는 키워드 데이터를 넘겨주기
        if(keywordManager != null)
        {
            keywordManager.UpdateKeywords(newState.keywords);
        }
        //PlayerManager에는 플레이어 데이터 넘겨주기
        if(playerManager != null)
        {
            playerManager.UpdatePlayers(newState.players);
        }

    }

}
