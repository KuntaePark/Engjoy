using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using DataForm;

public class PlayerController : MonoBehaviour

{

    public string Id { get; set; }

    //���� ����
    public int maxHP = 3; //�ִ� HP
    public int currentHP; //���� HP

    public float currentMoveSpeed; //���� �̵� �ӵ�
    public float currentAttackSpeed; //���� ���� �ӵ�

    public bool haveShield = false; //��ȣ�� �÷���
    public bool haveBuff = false; //���� �÷���

    [Header("�ǰ� ����")]
    public float knockbackForce = 2f; //�ǰ� ���� �� �˹� ��
    public float graceDuration = 1f; //���� �ð�
    private bool isGrace = false; //���� �÷���
    private bool canMove = true; //�÷��̾� ���� �÷���

    public bool isDown = false; //�����Ҵ� �÷���
    public bool interactInput = false; //��ȣ�ۿ� �÷���

    public int facingDirection = 1; //��������Ʈ �Ӹ����� ���ÿ� ����

    public Rigidbody2D rb; //player�� rigidbody
    public Animator ani; //player���� �����ų animator

    UIManager uiManager; //UIManager ��ũ��Ʈ ����

    WsClient wsClient; //wsClient ����


    // ============================== ���� ���� �� ���� ============================== 
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        uiManager = FindObjectOfType<UIManager>();
    }


    // ============================== ���� ���� ============================== 
    void Start()
    {
        // ���� ������ �� : 


        currentHP = maxHP; //�÷��̾� HP�� �ִ�� ������ֱ�
        //�÷��̾�� �⺻ ���� �⺻�� �����ֱ�


        //������ �� UI ������Ʈ 

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


    // ---------------------------- ���Ϳ� �浹 ���� ---------------------------- 
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Monster")) //���Ϳ� �浹 ��
        {
            //���ͷκ��� �÷��̾�� ���ϴ� �˹� ���� ���
            Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;
            //�ǰ� ���� �� ������ ó�����ִ� �Լ� ȣ��
            TakeDamage(1, knockbackDirection);
        }
    }


    // ---------------------------- �ǰ� ������ ��� ---------------------------- 
    public void TakeDamage(int damage, Vector2 knockbackDirection) //�÷��̾� �ǰ� ����
    {

        if (isGrace) //���� ���� �� �浹�� X
        {
            return;
        }


        if (haveShield) //��ȣ���� �ִٸ� ������ 1ȸ ����, ��ȣ�� ����
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

        Debug.Log($"�÷��̾ {damage}�� �������� �Ծ����ϴ�. ���� ü�� : {currentHP}");

        // UIManager�� ���� HP ���� ������Ʈ
        if (uiManager != null)
        {
            uiManager.HPUI(currentHP);
        }



        StartCoroutine(KnockbackAndGrace(knockbackDirection));

        if(currentHP <= 0)
        {
            Debug.Log("�÷��̾ �����Ҵ��� �Ǿ����ϴ�.");
            //�÷��̾� �����Ҵ� ���� �߰�
        }

    }


    // ---------------------------- �˹� �� ���� ---------------------------- 
    private IEnumerator KnockbackAndGrace(Vector2 knockbackDirection)
    {

        //���� ���� ��ȯ
        isGrace = true;
        canMove = false;

        //�˹� ����
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);


        //��������Ʈ �����̴� ȿ��
        float elapsedTime = 0f;
        while (elapsedTime < graceDuration)
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = !gameObject.GetComponent<SpriteRenderer>().enabled;
            elapsedTime += 0.1f;
            yield return new WaitForSeconds(0.1f);

        }

        //���� �ð��� ������ ����
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

            // ---------------------------- �̵�Ű ---------------------------- 

            //Unity > Edit > Project Settings > Input Manager > Axes �� 'Horizontal'��� Name���� ���õǾ� �ִ� Ű �Է��� �ޱ�.
            //A(����), D(������) = ���� Ű
            float horizontal = Input.GetAxis("Horizontal");
            //W(����), S(�Ʒ���) = ���� Ű
            float vertical = Input.GetAxis("Vertical");

                ////������ �̵��� ������
                if (horizontal != 0 || vertical != 0)
                {
                    var inputData = new { x = horizontal, y = vertical };
                    WsClient.Instance.Send("input", JsonConvert.SerializeObject(inputData));
                }

            // ---------------------------- ��ȣ�ۿ�Ű ---------------------------- 
            if (interactInput)
            {
                Debug.Log("Sent 'interact' message to server. ");
                WsClient.Instance.Send("interact", "");

                interactInput = false;
            }

        }

        }

    }





    // ---------------------------- ������ ���� �Լ��� ---------------------------- 
    public bool ApplyPotion(int healAmount)
    {
        //�÷��̾��� HP�� ����á�ٸ� ���� ��ȯ,
        if (currentHP >= maxHP)
        {
            Debug.Log("�÷��̾��� HP�� ���� �� ȸ���� �� �����ϴ�.");
            return false;
        }

        //HP�� 3 ���϶�� : 
        //������ ���� ���� + Debug.Log($"HP�� ȸ���Ǿ����ϴ�. ���� HP: {currentHP});
        currentHP += healAmount;
        Debug.Log($"Player�� {healAmount}��ŭ ü���� ȸ���߽��ϴ�. | ���� HP: {currentHP}");


        //UI ������Ʈ(HP)
        if (uiManager != null)
        {
            uiManager.HPUI(currentHP);
        }

        return true;
    }

    public void ApplyBuff(float buffAmount)
    {
        //������ ���� ����
        haveBuff = true;

        //currentMoveSpeed = moveSpeed * buffAmount; //�̼�����
        //currentAttackSpeed = attackSpeed * buffAmount; //��������

        Debug.Log("���� ����!");

        //UI ������Ʈ ��û(Buff)
        if (uiManager != null)
        {
            uiManager.EffectUI(UIManager.EffectType.BUFF, haveBuff);    
        }

    }


    public void RemoveBuff()
    {
        //���� ���������� �Ѿ �� ���� ����
        haveBuff = false;

       

        Debug.Log("���� ���� ����.");

        //UI ������Ʈ ��û(Buff)
        if (uiManager != null)
        {
            uiManager.EffectUI(UIManager.EffectType.BUFF, haveBuff);
        }
        
    }


    public void ApplyShield()
    {
        //������ ���� ����
        haveShield = true;
        Debug.Log("��ȣ�� ���� ����!");

        //UI ������Ʈ ��û(Shield)
        if (uiManager != null)
        {
            uiManager.EffectUI(UIManager.EffectType.SHIELD, haveShield);
        }

    }

    public void RemoveShield()
    {
        //������ ���� ����
        haveShield = false;
        Debug.Log("��ȣ���� ������ ���ҽ��ϴ�!");

        //UI ������Ʈ ��û(Shield)
        if (uiManager != null)
        {
            uiManager.EffectUI(UIManager.EffectType.SHIELD, haveShield);
        }

    }


    // ---------------------------- �¿���� ---------------------------- 
    void Flip() //��������Ʈ �¿����
    {
        facingDirection *= -1;
        transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y);
    }



}
