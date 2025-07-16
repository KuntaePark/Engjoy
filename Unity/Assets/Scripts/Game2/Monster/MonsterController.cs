using System.Collections;
using System.Collections.Generic;
using DataForm;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MonsterController : MonoBehaviour
{
   //현재 2025년 07월 02일 오전 10시 46분
   //속이 Monster같이 날뛰고 있다.
   //카페인은 사람을 찢는다.

    public string Id { get; private set; }
    public int Hp {  get; private set; }
    public string Type {  get; private set; }
    public bool isActive { get; private set; }


    [SerializeField] private TextMeshPro hpText;
    private SpriteRenderer spriteRenderer;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip takeDamageSound;
    private AudioSource audioSource;

    private Vector3 targetPosition;
    private float positionLerpFactor = 15f;

    private Coroutine takeDamageCoroutine;
    [SerializeField] private Material flashMaterial;
    private Material originalMaterial; 


    public void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if(spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material; 
        }

        audioSource = GetComponent<AudioSource>();
    }


    public void Initialize(string id, MonsterData initialData)
    {
        Id = id;
        transform.position = new Vector3(initialData.x, initialData.y, 0);
        targetPosition = transform.position;

        //초기 데이터로 상태 업데이트
        UpdateState(initialData);
    }

    public void UpdateState(MonsterData data)
    {
        //서버로부터 받은 새 위치를 목표 위치로 설정
        targetPosition = new Vector3(data.x, data.y, 0);
        bool isChaserBecomingActive = (this.Type == "chaser" && !this.isActive && data.isActive);


        //HP상태 업데이트
        if (data.hp < this.Hp)
        {
            if(takeDamageCoroutine != null)
                StopCoroutine(takeDamageCoroutine);
            //피격 판정의 시각적 효과
            takeDamageCoroutine = StartCoroutine(TakeDamageEffect());
        }

        
        this.Hp = data.hp;
        if(hpText != null)
        {
            hpText.text = this.Hp.ToString();
        }

        Type = data.type;
        isActive = data.isActive;

        Vector3 newPosition = new Vector3(data.x, data.y, 0);
        targetPosition = newPosition;
        if(isChaserBecomingActive)
        {
            transform.position = newPosition;
        }

        UpdateVisibility();
    }

    void UpdateVisibility()
    {
        bool visible = true; //기본값은 보임

        if (Type == "chaser")
        {
            visible = isActive; //Chaser는 활성화 상태일 때만 보임
        }

        if (spriteRenderer != null) spriteRenderer.enabled = visible;
        if(hpText != null) hpText.enabled = visible;
    }

    private IEnumerator TakeDamageEffect()
    {

        if (takeDamageSound != null) audioSource.PlayOneShot(takeDamageSound);


        float duration = 0.14f;
        float bounceAmount = 1.2f;
        Vector3 originalScale = transform.localScale;

        if(flashMaterial != null)
        {
            spriteRenderer.material = flashMaterial;
        }

        //이펙트
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            float bounce = Mathf.Sin(progress * Mathf.PI);
            transform.localScale = Vector3.Lerp(originalScale, originalScale * bounceAmount, bounce);

            yield return null;
        }

        //이펙트 종료: 원래 상태로 복구
        transform.localScale = originalScale;
        spriteRenderer.material = originalMaterial; 
    }

    private void Update()
    {
        //목표 위치로 부드럽게 이동시켜주기
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionLerpFactor);
    }
}
