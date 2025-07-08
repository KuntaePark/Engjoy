using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;

public class Game1InputController : MonoBehaviour
{
    public enum ActionType
    {
        ATTACK,
        DEFENSE,
        SPECIAL
    }


    public Game1Manager game1Manager; //게임 매니저 스크립트 참조
    public UIController uiController;

    //유저 입력을 받아 서버에 전달
    public GameClient1 gameClient;
    public ActionType actionType = ActionType.ATTACK;
   

    private float inputDelay = 0.2f; //입력 딜레이 시간
    private float lastInputTime = 0f; //마지막 입력 시간

    //방향 인덱스 매핑
    Dictionary<Vector2, int> directionIndexMap = new Dictionary<Vector2, int>
    {
        { new Vector2(0, 1), 0 },   // 위
        { new Vector2(1, 0), 1 },   // 오른쪽
        { new Vector2(0, -1), 2 },  // 아래
        { new Vector2(-1, 0), 3 }   // 왼쪽
    };

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(game1Manager.gameState.state != "start")
        {
            //게임이 시작되지 않았으면 입력 무시
            return;
        }

        if(Time.time - lastInputTime < inputDelay)
        {
            //입력 딜레이가 걸려있으면 무시
            return;
        }

        float axisH = Input.GetAxisRaw("Horizontal");
        float axisV = Input.GetAxisRaw("Vertical");
       
        bool isActionSelected = game1Manager.checkActionSelected(); //액션 선택 상태

        //입력 처리, input.type 종류: chargeMana, actionSelect, actionCancel, wordSelect 
        if (!isActionSelected)
        {
            //행동 미선택 상태에서의 입력 처리
            if (Input.GetKey(KeyCode.X))
            {
                //마나 충전
                gameClient.Send("input", JsonConvert.SerializeObject(new { type = "chargeMana" }));
            }
            if (axisH == -1.0f)
            {
                if (actionType != ActionType.ATTACK)
                {
                    Debug.Log("Action to left");
                    actionType--;
                    uiController.setAction((int)actionType);
                    lastInputTime = Time.time; //입력 시간 갱신
                }
            }
            if (axisH == 1.0f)
            {
                if (actionType != ActionType.SPECIAL)
                {
                    Debug.Log("Action to right");
                    actionType++;
                    uiController.setAction((int)actionType);
                    lastInputTime = Time.time; //입력 시간 갱신
                }
            }
            //액션 결정
            if (Input.GetKeyDown(KeyCode.Z))
            {
                Debug.Log("Action selected: " + actionType.ToString());
                gameClient.Send("input", JsonConvert.SerializeObject(new { type = "actionSelect", action = actionType.ToString() }));
                isActionSelected = true; //상태 변경
                lastInputTime = Time.time; //입력 시간 갱신
            }
        }
        else
        {
            //행동 선택 상태에서의 입력 처리
            if (Input.GetKeyDown(KeyCode.Z))
            {

                //행동 시전
                Debug.Log("Action confirm");
                gameClient.Send("input", JsonConvert.SerializeObject(new { type = "actionConfirm" }));
                lastInputTime = Time.time; //입력 시간 갱신
            }

            //행동 취소
            if (Input.GetKeyDown(KeyCode.X))
            {
                Debug.Log("Action cancelled");
                gameClient.Send("input", JsonConvert.SerializeObject(new { type = "actionCancel" }));
                isActionSelected = false; //상태 변경
                lastInputTime = Time.time; //입력 시간 갱신
            }

            //단어 뜻 선택
            if(directionIndexMap.TryGetValue(new Vector2(axisH, axisV), out int index))
            {
                //방향 입력에 따라 단어 선택
                Debug.Log("Selecting word at index: " + index);
                gameClient.Send("input", JsonConvert.SerializeObject(new { type = "wordSelect", idx = index }));
                lastInputTime = Time.time; //입력 시간 갱신
            }
        }
    }
}
