using System.Collections;
using System.Collections.Generic;
using DataForm;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MonsterController : MonoBehaviour
{
   //���� 2025�� 07�� 02�� ���� 10�� 46��
   //���� Monster���� ���ٰ� �ִ�.
   //ī������ ����� ���´�.

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

        //�ʱ� �����ͷ� ���� ������Ʈ
        UpdateState(initialData);
    }

    public void UpdateState(MonsterData data)
    {
        //�����κ��� ���� �� ��ġ�� ��ǥ ��ġ�� ����
        targetPosition = new Vector3(data.x, data.y, 0);
        bool isChaserBecomingActive = (this.Type == "chaser" && !this.isActive && data.isActive);


        //HP���� ������Ʈ
        if (data.hp < this.Hp)
        {
            if(takeDamageCoroutine != null)
                StopCoroutine(takeDamageCoroutine);
            //�ǰ� ������ �ð��� ȿ��
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
        bool visible = true; //�⺻���� ����

        if (Type == "chaser")
        {
            visible = isActive; //Chaser�� Ȱ��ȭ ������ ���� ����
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

        //����Ʈ
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            float bounce = Mathf.Sin(progress * Mathf.PI);
            transform.localScale = Vector3.Lerp(originalScale, originalScale * bounceAmount, bounce);

            yield return null;
        }

        //����Ʈ ����: ���� ���·� ����
        transform.localScale = originalScale;
        spriteRenderer.material = originalMaterial; 
    }

    private void Update()
    {
        //��ǥ ��ġ�� �ε巴�� �̵������ֱ�
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionLerpFactor);
    }
}
