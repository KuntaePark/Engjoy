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
    public MonsterManager monsterManager;

    public GameObject exitPrefab; //GameManager가 직접 Exit를 관리
    private ExitController exitController; //ExitController를 직접 관리

    public string MyPlayerId { get; private set; }


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
        monsterManager = FindObjectOfType<MonsterManager>();
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
        if (keywordManager != null)
        {
           keywordManager.UpdateKeywords(newState.keywords);
        }

        //PlayerManager에는 플레이어 데이터 넘겨주기
        if (playerManager != null)
        {
            playerManager.UpdatePlayers(newState.players);
        }
        if (newState.exit != null)
        {
            if (exitController == null)
            {
                //출구가 씬에 없다면 새로 생성
                GameObject exitObject = Instantiate(exitPrefab);
                exitController = exitObject.GetComponent<ExitController>();

                //출구 UI키기
                ExitUIManager.Instance.ShowExitUI();

            }
            //출구 상태 업데이트
            exitController.UpdateState(newState.exit);
        }

       if(monsterManager != null && newState.monsters != null)
        {
            monsterManager.UpdateMonsters(newState.monsters);
        }
        else if (exitController != null)
        {
           //서버에 출구가 없는데 클라이언트에 있다면 파괴
            Destroy(exitController.gameObject);
            exitController = null;

            //출구가 없다면 UI 파괴
            ExitUIManager.Instance.HideExitUI();

        }



    }



}