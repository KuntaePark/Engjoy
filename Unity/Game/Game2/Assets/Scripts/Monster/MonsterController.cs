using System.Collections;
using System.Collections.Generic;
using DataForm;
using TMPro;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
   //���� 2025�� 07�� 02�� ���� 10�� 46��
   //���� Monster���� ���ٰ� �ִ�.
   //ī������ ����� ���´�.

    public string Id { get; private set; }
    public int Hp {  get; private set; }
    public string Type {  get; private set; }
    public bool isActive { get; private set; }

    [SerializeField] private TextMeshPro hpText; // ������ HP�� ǥ���� �ؽ�Ʈ (�׽�Ʈ��_�Ŀ� HP�ٷ� ����)
    private SpriteRenderer spriteRenderer;

    private Vector3 targetPosition;
    private float positionLerpFactor = 15f;


    public void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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

        //HP���� ������Ʈ
        if(data.hp < this.Hp)
        {
            //�ǰ� ������ �ð��� ȿ��
            StartCoroutine(TakeDamageEffect());
        }

        
        this.Hp = data.hp;
        if(hpText != null)
        {
            hpText.text = ""+this.Hp;
        }


        
        Type = data.type;
        isActive = data.isActive;

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
        if(spriteRenderer == null) yield break;

        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.color = Color.white;
    }

    private void Update()
    {
        //��ǥ ��ġ�� �ε巴�� �̵������ֱ�
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionLerpFactor);
    }
}
