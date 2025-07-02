using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using DataForm;
using System.Runtime.CompilerServices;



public class PlayerController : MonoBehaviour
{



    public string Id { get; private set; }
    public bool IsHoldingKeyword { get; private set; }
    private bool IsMine => !string.IsNullOrEmpty(Id) && Id == GameManager.Instance.MyPlayerId;

    //카메라 & 오디오 리스너
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioListener audioListener;


    //변수 지정
    public int maxHP = 3; //최대 HP
    public int currentHP; //현재 HP



    private Vector3 targetPosition; //타겟의 위치
    private float positionLerpFactor; //lerp값



    //sprite 컴포넌트
    private SpriteRenderer spriteRenderer;
    //public Animator ani; //player에게 적용시킬 animator



    //상호작용 관련 상태
    private bool canInteractWithKeyword; //키워드
    private bool canInteractWithExit; //출구

    UIManager uiManager; //UIManager 스크립트 참조

    WsClient wsClient; //wsClient 참조





    // ============================== 초기화 (PlayerManager에서 호출) ============================== 

    public void Initialize(string id, PlayerData initialData, float lerpFactor)

    {

        Id = id;
        positionLerpFactor = lerpFactor;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        // animator = GetComponent<Animator>();
        if (playerCamera == null) playerCamera = gameObject.GetComponentInChildren<Camera>();
        if (audioListener == null) audioListener = GetComponentInChildren<AudioListener>();

        //플레이어 본인의 카메라만 활성화. 다른 카메라는 비활성화.
        if (playerCamera != null) playerCamera.enabled = IsMine;
        if (audioListener != null) audioListener.enabled = IsMine;

        transform.position = new Vector3(initialData.x, initialData.y, 0);
        targetPosition = transform.position;

        if (IsMine)
        {
            Debug.Log($"<color=lime>This is my character! ID: {this.Id}</color>");
        }

        //받은 데이터로 최초 상태 업데이트
        UpdateVisuals(initialData);
    }





    // ============================== 게임 시작 ============================== 

    public void UpdateState(PlayerData data)
    {
        //목표 위치 갱신
        targetPosition = new Vector3(data.x, data.y, 0);

        //서버가 결정한 상호작용 가능 상태 갱신
        canInteractWithKeyword = data.interactableKeywordId != null;
        canInteractWithExit = data.canInteractWithExit;
        IsHoldingKeyword = !string.IsNullOrEmpty(data.holdingKeywordId);

        //서버 데이터에 따른 시각적 요소 업데이트
        UpdateVisuals(data);
    }





    public void UpdateVisuals(PlayerData data)
    {
        //상호작용 가능 상태일 때 느낌표 표시 등의 UI 관련 기능
    }





    // ============================== Update ============================== 

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionLerpFactor);

        //이 클라이언트가 조종하는 '내' 캐릭터일 경우에만 입력 처리
        if (IsMine)
        {
            HandleInput();
        }

    }


    private void HandleInput()
    {

        //이동 입력
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (horizontal != 0 || vertical != 0)
        {
            var inputData = new { x = horizontal, y = vertical };
            WsClient.Instance.Send("input", JsonConvert.SerializeObject(inputData));
        }


        //상호작용 입력(Space)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            WsClient.Instance.Send("interact", "");
        }


        //공격 입력(Z) 
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if(!IsHoldingKeyword)
            {

            //서버에 플레이어 공격 요청 전송
            WsClient.Instance.Send("playerAttack", "");
            //즉각적인 시각적 효과를 위한 코루틴
            StartCoroutine(AttackEffectCoroutine());

            Debug.Log("Attack input sent to server.");
            }
            else
            {
                Debug.Log("Cannot attack while holding a keyword.");
            }
        }

    }

    private IEnumerator AttackEffectCoroutine()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;

            //시각적인 효과
            spriteRenderer.color = Color.red;

            yield return new WaitForSeconds(0.15f);

            //원래 색상으로 복원
            spriteRenderer.color = originalColor;
        }







    }
}