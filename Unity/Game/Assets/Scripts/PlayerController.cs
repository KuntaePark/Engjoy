using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using DataForm;

public class PlayerController : MonoBehaviour

{

    public string Id { get; set; }

    //변수 지정
    public int maxHP = 3; //최대 HP
    public int currentHP; //현재 HP

    public float currentMoveSpeed; //현재 이동 속도
    public float currentAttackSpeed; //현재 공격 속도

    public bool haveShield = false; //보호막 플래그
    public bool haveBuff = false; //버프 플래그

    [Header("피격 설정")]
    public float knockbackForce = 2f; //피격 판정 시 넉백 힘
    public float graceDuration = 1f; //무적 시간
    private bool isGrace = false; //무적 플래그
    private bool canMove = true; //플레이어 조작 플래그

    public bool isDown = false; //전투불능 플래그
    public bool interactInput = false; //상호작용 플래그

    public int facingDirection = 1; //스프라이트 머리방향 세팅용 변수

    public Rigidbody2D rb; //player의 rigidbody
    public Animator ani; //player에게 적용시킬 animator

    UIManager uiManager; //UIManager 스크립트 참조

    WsClient wsClient; //wsClient 참조


    // ============================== 게임 시작 전 동작 ============================== 
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        uiManager = FindObjectOfType<UIManager>();
    }


    // ============================== 게임 시작 ============================== 
    void Start()
    {
        // 게임 시작할 때 : 


        currentHP = maxHP; //플레이어 HP를 최대로 만들어주기
        //플레이어들 기본 스탯 기본에 맞춰주기


        //시작할 때 UI 업데이트 

        if (uiManager != null) 
        {
            uiManager.HPUI(currentHP);

            uiManager.EffectUI(UIManager.EffectType.BUFF, haveBuff);
            uiManager.EffectUI(UIManager.EffectType.SHIELD, haveShield);
        }

    }



    // ============================== Update ============================== 
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            interactInput = true;
        }
    }


    // ---------------------------- 몬스터와 충돌 판정 ---------------------------- 
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Monster")) //몬스터와 충돌 시
        {
            //몬스터로부터 플레이어로 향하는 넉백 방향 계산
            Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;
            //피격 판정 후 데미지 처리해주는 함수 호출
            TakeDamage(1, knockbackDirection);
        }
    }


    // ---------------------------- 피격 데미지 계산 ---------------------------- 
    public void TakeDamage(int damage, Vector2 knockbackDirection) //플레이어 피격 판정
    {

        if (isGrace) //무적 판정 중 충돌은 X
        {
            return;
        }


        if (haveShield) //보호막이 있다면 데미지 1회 무시, 보호막 제거
        {
            StartCoroutine(KnockbackAndGrace(knockbackDirection));
            RemoveShield();
            return;
        }

        currentHP -= damage;
        if (currentHP < 0)
        {
            currentHP = 0;
        }

        Debug.Log($"플레이어가 {damage}의 데미지를 입었습니다. 현재 체력 : {currentHP}");

        // UIManager에 현재 HP 상태 업데이트
        if (uiManager != null)
        {
            uiManager.HPUI(currentHP);
        }



        StartCoroutine(KnockbackAndGrace(knockbackDirection));

        if(currentHP <= 0)
        {
            Debug.Log("플레이어가 전투불능이 되었습니다.");
            //플레이어 전투불능 로직 추가
        }

    }


    // ---------------------------- 넉백 및 무적 ---------------------------- 
    private IEnumerator KnockbackAndGrace(Vector2 knockbackDirection)
    {

        //무적 상태 전환
        isGrace = true;
        canMove = false;

        //넉백 적용
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);


        //스프라이트 깜박이는 효과
        float elapsedTime = 0f;
        while (elapsedTime < graceDuration)
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = !gameObject.GetComponent<SpriteRenderer>().enabled;
            elapsedTime += 0.1f;
            yield return new WaitForSeconds(0.1f);

        }

        //무적 시간이 끝나면 복귀
        gameObject.GetComponent<SpriteRenderer>().enabled = true;
        isGrace = false;
        canMove = true;


    }



    //====================================== FixedUpdate ===============================
    void FixedUpdate()
    {
        if (!string.IsNullOrEmpty(Id) && Id == GameManager.Instance.MyPlayerId)
        {

        if (canMove)
        {

            // ---------------------------- 이동키 ---------------------------- 

            //Unity > Edit > Project Settings > Input Manager > Axes 에 'Horizontal'라는 Name으로 세팅되어 있는 키 입력을 받기.
            //A(왼쪽), D(오른쪽) = 수평 키
            float horizontal = Input.GetAxis("Horizontal");
            //W(위쪽), S(아래쪽) = 수직 키
            float vertical = Input.GetAxis("Vertical");

                ////서버에 이동값 보내기
                if (horizontal != 0 || vertical != 0)
                {
                    var inputData = new { x = horizontal, y = vertical };
                    WsClient.Instance.Send("input", JsonConvert.SerializeObject(inputData));
                }

            // ---------------------------- 상호작용키 ---------------------------- 
            if (interactInput)
            {
                Debug.Log("Sent 'interact' message to server. ");
                WsClient.Instance.Send("interact", "");

                interactInput = false;
            }

        }

        }

    }





    // ---------------------------- 아이템 적용 함수들 ---------------------------- 
    public bool ApplyPotion(int healAmount)
    {
        //플레이어의 HP가 가득찼다면 실패 반환,
        if (currentHP >= maxHP)
        {
            Debug.Log("플레이어의 HP가 가득 차 회복할 수 없습니다.");
            return false;
        }

        //HP가 3 이하라면 : 
        //아이템 적용 로직 + Debug.Log($"HP가 회복되었습니다. 현재 HP: {currentHP});
        currentHP += healAmount;
        Debug.Log($"Player가 {healAmount}만큼 체력을 회복했습니다. | 현재 HP: {currentHP}");


        //UI 업데이트(HP)
        if (uiManager != null)
        {
            uiManager.HPUI(currentHP);
        }

        return true;
    }

    public void ApplyBuff(float buffAmount)
    {
        //아이템 적용 로직
        haveBuff = true;

        //currentMoveSpeed = moveSpeed * buffAmount; //이속증가
        //currentAttackSpeed = attackSpeed * buffAmount; //공속증가

        Debug.Log("버프 적용!");

        //UI 업데이트 요청(Buff)
        if (uiManager != null)
        {
            uiManager.EffectUI(UIManager.EffectType.BUFF, haveBuff);    
        }

    }


    public void RemoveBuff()
    {
        //다음 스테이지로 넘어갈 때 버프 삭제
        haveBuff = false;

       

        Debug.Log("버프 삭제 성공.");

        //UI 업데이트 요청(Buff)
        if (uiManager != null)
        {
            uiManager.EffectUI(UIManager.EffectType.BUFF, haveBuff);
        }
        
    }


    public void ApplyShield()
    {
        //아이템 적용 로직
        haveShield = true;
        Debug.Log("보호막 적용 성공!");

        //UI 업데이트 요청(Shield)
        if (uiManager != null)
        {
            uiManager.EffectUI(UIManager.EffectType.SHIELD, haveShield);
        }

    }

    public void RemoveShield()
    {
        //아이템 적용 로직
        haveShield = false;
        Debug.Log("보호막이 공격을 막았습니다!");

        //UI 업데이트 요청(Shield)
        if (uiManager != null)
        {
            uiManager.EffectUI(UIManager.EffectType.SHIELD, haveShield);
        }

    }


    // ---------------------------- 좌우반전 ---------------------------- 
    void Flip() //스프라이트 좌우반전
    {
        facingDirection *= -1;
        transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y);
    }



}
