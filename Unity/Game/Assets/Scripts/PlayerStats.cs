//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class PlayerStats : MonoBehaviour
//{
//    // Start is called before the first frame update

//    // ======== 기본 스탯 =========

//    public int maxHP = 3; //최대 HP
//    public int currentHP; //현재 HP

//    public float moveSpeed = 5.0f; //이동 속도
//    public float attackSpeed = 1.0f; //공격 속도

//    public float currentMoveSpeed; //현재 이동 속도
//    public float currentAttackSpeed; //현재 공격 속도

//    public bool haveShield = false; //보호막 플래그
//    public bool haveBuff = false; //버프 플래그

//    [Header("피격 설정")]
//    public float knockbackForce = 5f; //피격 판정 시 넉백 힘
//    public float graceDuration = 1f; //무적 시간
//    private bool isGrace = false; //무적 플래그

//    private Rigidbody2D rb; //플레이어 rb 잡아오기

//    // ===== 나중에 UIManager 참고 =====
//    //  >> HP UI, Buff나 Shield 상태 관리

//    private void Awake()
//    {
//        rb = GetComponent<Rigidbody2D>();
//    }


//    private void Start()
//    {
//        // 게임 시작할 때 : 

//        currentHP = maxHP; //플레이어 HP를 최대로 만들어주기
//        //플레이어들 기본 스탯 기본에 맞춰주기
//        currentMoveSpeed = moveSpeed;
//        currentAttackSpeed = attackSpeed;

//        //시작할 때 UI 업데이트 
//        // UIManager가 잘 작동하는지 확인 > 이후 HP UI 띄우기

//    }

//    private void OnCollisionEnter2D(Collision2D collision)
//    {
//        if (collision.gameObject.CompareTag("Monster")) //몬스터와 충돌 시
//        {
//            //몬스터로부터 플레이어로 향하는 넉백 방향 계산
//            Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;
//            //피격 판정 후 데미지 처리해주는 함수 호출
//            TakeDamage(1, knockbackDirection);
//        }
//    }


//    public void TakeDamage(int damage, Vector2 knockbackDirection) //플레이어 피격 판정
//    {

//        if(isGrace) //무적 판정 중 충돌은 X
//        {
//            return; 
//        }


//        if (haveShield) //보호막이 있다면 데미지 1회 무시, 보호막 제거
//        {
//            haveShield = true; //보호막 해제
//            Debug.Log("보호막이 공격을 막았습니다!");

//            return;
//        }

//    }
//}
