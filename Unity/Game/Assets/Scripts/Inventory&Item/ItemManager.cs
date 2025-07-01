//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class ItemManager : MonoBehaviour
//{

//    // 아이템 개수
//    // 플레이어의 아이템 보유량을 데이터로 끌어와서 - 일정 수량일 시 최대 수량만 사용할 수 있게 조절할 예정

//    public int potionCount = 3; //포션 보유량 개수
//    public int buffCount = 1; //버프 보유량 개수
//    public int shieldCount = 1; //보호막 보유량 개수

//    public PlayerController playerController; //PlayerController > Stat 참조
//    public UIManager uiManager; //uiManager 참조



//    // ============================== 게임 시작 ============================== 
//    private void Awake()
//    {
//        playerController = FindObjectOfType<PlayerController>();
//        uiManager = FindObjectOfType<UIManager>();
//    }




//    // ============================== 게임 시작 ============================== 
//    void Start()
//    {
//        //게임 시작 시 : 

//        //UI에 아이템 세팅해주기
//    }

//    void Update()
//    {
//        //키 입력으로 아이템 사용.
//        if (Input.GetKeyDown(KeyCode.A))
//        {
//            UsePotion();
//        }

//        if (Input.GetKeyDown(KeyCode.S))
//        {
//            UseBuff();
//        }

//        if (Input.GetKeyDown(KeyCode.D))
//        {
//            UseShield();
//        }


//    }
//    private void UsePotion()
//    {
//        //아이템 남아있는지부터 확인
//        if(potionCount > 0)
//        {
//            bool healed = playerController.ApplyPotion(1);

//            //플레이어의 HP 확인
//            //작동 로직 + Debug.Log("회복 완료!") or Debug.Log("회복 아이템이 없습니다.");
//            if (healed)
//            {
//                potionCount--;
//                Debug.Log($"포션 아이템을 사용했습니다. 현재 {potionCount}의 포션이 남았습니다.");

//                //UI 업데이트 요청(Item)
//                if(uiManager != null)
//                {
//                    uiManager.ItemUI(UIManager.ItemType.POTION, potionCount);
//                }
//            }

//        }
//            else
//            {
//                Debug.Log("회복 아이템이 모두 소진되었습니다. 회복할 수 없습니다.");
//            }


//    }

//    private void UseBuff()
//    {
//        //아이템이 남아있는지 확인
//        if (buffCount > 0)
//        {
//            playerController.ApplyBuff(1.25f);

//        //작동 로직 + Debug.Log("아이템 사용 성공!") or Debug.Log("아이템 사용 실패.");
//            buffCount--;
//            Debug.Log($"버프 아이템을 사용했습니다. 현재 {buffCount}의 버프 아이템이 남았습니다.");

//        //UI 업데이트 요청(Item)
//            if(uiManager!=null)
//            {
//                uiManager.ItemUI(UIManager.ItemType.BUFF, buffCount);
//            }
//        }
//        else
//        {
//            Debug.Log("버프 아이템이 모두 소진되었습니다. 아이템을 사용할 수 없습니다.");
//        }

//    }

//    private void UseShield()
//    {
//        //아이템이 남아있는지 확인
//        if (shieldCount > 0)
//        {
//            playerController.ApplyShield();
//            //작동 로직 + Debug.Log("보호막 사용 성공!") or Debug.Log("보호막 사용 실패.");
//            shieldCount--;
//            Debug.Log($"보호막 아이템을 사용했습니다. 현재 {shieldCount}의 보호막 아이템이 남았습니다.");

//            //UI 업데이트 요청(Item)
//            if (uiManager!=null)
//            {
//                uiManager.ItemUI(UIManager.ItemType.SHIELD, shieldCount);
//            }

//        }

//        else
//        {
//            Debug.Log("버프 아이템이 모두 소진되었습니다. 아이템을 사용할 수 없습니다.");
//        }

//    }
//}
