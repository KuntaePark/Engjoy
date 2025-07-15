using System.Collections;
using System.Collections.Generic;
using DataForm;
using UnityEngine;
using UnityEngine.SceneManagement;



public class GameManager : MonoBehaviour
{
    //다른 스크립트에서 GameManager를 참조할 수 있도록 싱글턴 패턴 사용
    public static GameManager Instance { get; private set; }

    [Header("Scene Names")]
    public string matchingRoomSceneName = "Game2-MatchingRoom";
    public string gameSceneName = "Game2-Play";

    public PlayerManager playerManager;
    public KeywordManager keywordManager;
    public MonsterManager monsterManager;
    public MapManager mapManager;

    public GameObject exitPrefab; //GameManager가 직접 Exit를 관리
    private ExitController exitController; //ExitController를 직접 관리

    public string MyPlayerId { get; private set; }
    public bool IsGameOver {  get; private set; }   //게임오버 상태
    public bool IsResultVisible { get; private set; } = false; // 결과창 활성화 신호_플레이어 빙글빙글 돌릴거임..

    private string currentMapName = ""; //현재 맵 이름

    private const float INITIAL_MAX_TIME = 60f;

    private bool isGameOverSequenceStarted = false;



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
        //씬이 로드될 때마다 매니저들 찾게 하기
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    //GameManager 오브젝트 파괴될 때 등록 해제
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        FindManagers();
    }



    //씬 로딩이 완료되면 호출될 함수
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log(scene.name + "씬 로드 완료. 매니저를 다시 찾습니다.");
        FindManagers();
    }

    void FindManagers()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        keywordManager = FindObjectOfType<KeywordManager>();
        monsterManager = FindObjectOfType<MonsterManager>();
        mapManager = FindObjectOfType<MapManager>();
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
        // MyPlayerId가 설정되기 전까지는 아무 처리도 하지 않음
        if (string.IsNullOrEmpty(MyPlayerId))
        {
            return; // 즉시 함수를 종료하여 아래 코드가 실행되지 않게 함
        }

        string currentSceneName = SceneManager.GetActiveScene().name;

        //서버는 인게임 상태인데 현재 씬이 인게임이 아닐 경우
        if(newState.status == "PLAY" && currentSceneName != gameSceneName)
        {
            SceneManager.LoadScene(gameSceneName);
            return;
        }

        //맵 전환
        if(mapManager != null && !string.IsNullOrEmpty(newState.mapName) && newState.mapName != currentMapName)
        {
            mapManager.SwitchMap(newState.mapName);
            currentMapName = newState.mapName; //현재 맵 이름 업데이트
        }

        IsGameOver = newState.isGameOver;

        //게임 오버 연출 시작
        if (IsGameOver && !isGameOverSequenceStarted)
        {
            isGameOverSequenceStarted = true;
            if (ResultUIManager.Instance != null)
            {
                ResultUIManager.Instance.StartGameOverSequence(newState);
            }
        }



        if (UIManager.Instance != null )
            {
                UIManager.Instance.SetUIForGameState(newState.status);

                if(newState.status == "MATCHINGROOM")
                {
                    UIManager.Instance.UpdateCountdown(newState.countdown);
                }
                
                if(newState.status == "PLAY")
                {
                    UIManager.Instance.UpdateGameInfo(newState.gameLevel, newState.timeLimit, INITIAL_MAX_TIME);
                }
             }

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

         if(newState.players.TryGetValue(MyPlayerId, out PlayerData myPlayerData))
         {
           if(UIManager.Instance != null)
           {
               UIManager.Instance.UpdateReviveUI(myPlayerData.revivablePlayerId, myPlayerData.reviveProgress);
           }
         }

    }

    public void NotifyResultVisible()
    {
        IsResultVisible = true;
    }


    }



