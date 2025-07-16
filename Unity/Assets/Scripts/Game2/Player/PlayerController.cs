using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using DataForm;
using System.Runtime.CompilerServices;
using WebSocketSharp;



public class PlayerController : MonoBehaviour
{

    //[Header("Sprites")]
    //public Material hitFlashMaterial; //피격 시 머터리얼
    //private Material originalMaterial;

    //private bool isFacingRight = true;

    public long Id { get; private set; } = -1;
    public bool IsHoldingKeyword { get; private set; }
    private bool IsMine => (Id >= 0) && Id == GameManager.Instance.MyPlayerId;

    private bool isEscaped = false;
    private bool isDown = false;

    //카메라 & 오디오 리스너
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioListener audioListener;

    [SerializeField]
    private CharacterRenderer characterRenderer;

    //변수 지정
    public int maxHP = 3; //최대 HP
    public int currentHP; //현재 HP

    private bool isBuffed; //버프 플래그
    private bool hasShield; //쉴드 플래그
    private InventoryData inventory; //인벤토리 데이터

    private Vector3 targetPosition; //타겟의 위치
    private float positionLerpFactor; //lerp값



    //sprite 컴포넌트
    public SpriteRenderer spriteRenderer;
    public Animator ani; //player에게 적용시킬 animator



    //상호작용 관련 상태
    private bool canInteractWithKeyword; //키워드
    private bool canInteractWithExit; //출구

    //UIManager uiManager; //UIManager 스크립트 참조

    //WsClient wsClient; //wsClient 참조

    [Header("Sound Effects")]
    [SerializeField] private AudioClip[] attackSound;
    [SerializeField] private AudioClip takeDamageSound;
    [SerializeField] private AudioClip deadSound;
    [SerializeField] private AudioClip useItemSound;

    private AudioSource audioSource;

    private void Awake()
    {

    }

    // ============================== 초기화 (PlayerManager에서 호출) ============================== 

    public void Initialize(long id, PlayerData initialData, float lerpFactor)

    {
        Debug.Log($"id : {id}");

        Id = id;
        positionLerpFactor = lerpFactor;

        if (playerCamera == null) playerCamera = gameObject.GetComponentInChildren<Camera>();
        if (audioListener == null) audioListener = GetComponentInChildren<AudioListener>();

        //플레이어 본인의 카메라만 활성화. 다른 카메라는 비활성화.
        if (playerCamera != null) playerCamera.enabled = IsMine;
        if (audioListener != null) audioListener.enabled = IsMine;

        transform.position = new Vector3(initialData.x, initialData.y, 0);
        targetPosition = transform.position;

        characterRenderer.SetBody(initialData.bodyTypeIndex);
        characterRenderer.SetWeapon(initialData.weaponTypeIndex);

        ani = characterRenderer.bodyAnimator;

        spriteRenderer = characterRenderer.bodyInstance.GetComponent<SpriteRenderer>();

        if (IsMine)
        {
            Debug.Log($"<color=lime>This is my character! ID: {this.Id}</color>");
        }



        //받은 데이터로 최초 상태 업데이트
        UpdateVisuals(initialData);


        audioSource = GetComponent<AudioSource>();
    }





    // ============================== 게임 시작 ============================== 

    public void UpdateState(PlayerData data)
    {

        //디버깅용
        //if (data.inputH != 0)
        //{
        //    Debug.Log($"<color=cyan>[PlayerController] {this.Id} processing inputH: {data.inputH}</color>");
        //}

        if (data.isDown)
        {
            audioSource.PlayOneShot(deadSound);
            ani.SetTrigger("dead");
        }

        float speed = new Vector2(data.inputH, data.inputV).magnitude;
        if (speed > 0.01f)
        {
            ani.SetBool("isRunning", true);
        }
        else
        {
            ani.SetBool("isRunning", false);
        }

        if (data.inputH < 0)
        {
            gameObject.transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (data.inputH > 0)
        {
            gameObject.transform.localScale = new Vector3(1, 1, 1);
        }

        if(data.hp < this.currentHP)
        {
            if(ani != null)
            {
                ani.SetTrigger("hit");
                audioSource.PlayOneShot(takeDamageSound);
            }
        }

        //목표 위치 갱신
        targetPosition = new Vector3(data.x, data.y, 0);

        //서버가 결정한 상호작용 가능 상태 갱신
        canInteractWithKeyword = data.interactableKeywordId != null;
        canInteractWithExit = data.canInteractWithExit;
        IsHoldingKeyword = !string.IsNullOrEmpty(data.holdingKeywordId);

        this.isEscaped = data.isEscaped;
        this.isDown = data.isDown;


        //플레이어의 상태 갱신
        this.currentHP = data.hp;
        this.maxHP = data.maxHp;
        this.isBuffed = data.isBuffed;
        this.hasShield = data.hasShield;
        this.inventory = data.inventory;


        //서버 데이터에 따른 시각적 요소 업데이트
        UpdateVisuals(data);

        //UI 업데이트 호출
        //이 캐릭터가 내 캐릭터인 경우에만
        if (IsMine)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateHP(this.currentHP);
                UIManager.Instance.UpdateInventory(this.inventory);
                UIManager.Instance.UpdateStatusEffects(this.isBuffed, this.hasShield);

                if (playerCamera != null)
                {
                    CameraController camConttroller = playerCamera.GetComponentInChildren<CameraController>();
                    if (camConttroller != null)
                    {
                        bool shouldZoom = data.revivablePlayerId >= 0;
                        camConttroller.SetReviveZoom(shouldZoom);
                    }
                }
            }
        }
    }





    public void UpdateVisuals(PlayerData data)
    {
        if (spriteRenderer != null)
        {

            if (this.hasShield)
            {
                spriteRenderer.color = Color.cyan; //쉴드 적용 상태 확인
            }
            else if (this.isBuffed)
            {
                spriteRenderer.color = Color.yellow; //버프 적용 상태 확인 (황달?)
            }
            else
            {
                spriteRenderer.color = Color.white; //기본 상태일 때 흰색
            }

        }
    }





    // ============================== Update ============================== 

    private void Update()
    {


        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionLerpFactor);

        //이 클라이언트가 조종하는 '내' 캐릭터일 경우에만 입력 처리
        if (IsMine && !GameManager.Instance.IsGameOver)
        {
            HandleInput();
        }

    }


    private void HandleInput()
    {

        //게임오버 상태거나 탈출 상태이면 입력 처리 X
        if (GameManager.Instance.IsGameOver || this.isEscaped || this.isDown)
        {
            return;
        }

        //이동 입력
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        bool isHoldingInteract = Input.GetKey(KeyCode.Space);

        if (isHoldingInteract)
        {
            Debug.Log($"[클라이언트] 스페이스 누름 상태 전송: {isHoldingInteract}");
        }

        var inputData = new { x = horizontal, y = vertical, interactHold = isHoldingInteract };
        WsClient.Instance.Send("input", JsonConvert.SerializeObject(inputData));

        //상호작용 입력(Space)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            WsClient.Instance.Send("interact", "");
        }


        //공격 입력(Z) 
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (!IsHoldingKeyword)
            {

                //서버에 플레이어 공격 요청 전송
                WsClient.Instance.Send("playerAttack", "");
                //즉각적인 시각적 효과를 위한 코루틴
                characterRenderer.weaponAnimator.SetTrigger("Swing");

                int randomIndex = UnityEngine.Random.Range(0, attackSound.Length);

                audioSource.PlayOneShot(attackSound[randomIndex]);

                Debug.Log("Attack input sent to server.");
            }
            else
            {
                Debug.Log("Cannot attack while holding a keyword.");
            }
        }


        //아이템 사용(A) - potion
        if (Input.GetKeyDown(KeyCode.A))
        {
            var payload = new { itemType = "potion" };
            WsClient.Instance.Send("useItem", JsonConvert.SerializeObject(payload));
            Debug.Log("Sent request to use HP Potion");

            if(useItemSound != null && this.currentHP < 3)
            {
                audioSource.PlayOneShot(useItemSound);
            }
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            var payload = new { itemType = "buff" };
            WsClient.Instance.Send("useItem", JsonConvert.SerializeObject(payload));
            Debug.Log("Sent request to use Buff");

            if (useItemSound != null && !isBuffed)
            {
                audioSource.PlayOneShot(useItemSound);
            }
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            var payload = new { itemType = "shield" };
            WsClient.Instance.Send("useItem", JsonConvert.SerializeObject(payload));
            Debug.Log("Sent request to use Shield");

            if (useItemSound != null && !hasShield)
            {
                audioSource.PlayOneShot(useItemSound);
            }
        }
    }
}