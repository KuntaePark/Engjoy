//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class PlayerStats : MonoBehaviour
//{
//    // Start is called before the first frame update

//    // ======== �⺻ ���� =========

//    public int maxHP = 3; //�ִ� HP
//    public int currentHP; //���� HP

//    public float moveSpeed = 5.0f; //�̵� �ӵ�
//    public float attackSpeed = 1.0f; //���� �ӵ�

//    public float currentMoveSpeed; //���� �̵� �ӵ�
//    public float currentAttackSpeed; //���� ���� �ӵ�

//    public bool haveShield = false; //��ȣ�� �÷���
//    public bool haveBuff = false; //���� �÷���

//    [Header("�ǰ� ����")]
//    public float knockbackForce = 5f; //�ǰ� ���� �� �˹� ��
//    public float graceDuration = 1f; //���� �ð�
//    private bool isGrace = false; //���� �÷���

//    private Rigidbody2D rb; //�÷��̾� rb ��ƿ���

//    // ===== ���߿� UIManager ���� =====
//    //  >> HP UI, Buff�� Shield ���� ����

//    private void Awake()
//    {
//        rb = GetComponent<Rigidbody2D>();
//    }


//    private void Start()
//    {
//        // ���� ������ �� : 

//        currentHP = maxHP; //�÷��̾� HP�� �ִ�� ������ֱ�
//        //�÷��̾�� �⺻ ���� �⺻�� �����ֱ�
//        currentMoveSpeed = moveSpeed;
//        currentAttackSpeed = attackSpeed;

//        //������ �� UI ������Ʈ 
//        // UIManager�� �� �۵��ϴ��� Ȯ�� > ���� HP UI ����

//    }

//    private void OnCollisionEnter2D(Collision2D collision)
//    {
//        if (collision.gameObject.CompareTag("Monster")) //���Ϳ� �浹 ��
//        {
//            //���ͷκ��� �÷��̾�� ���ϴ� �˹� ���� ���
//            Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;
//            //�ǰ� ���� �� ������ ó�����ִ� �Լ� ȣ��
//            TakeDamage(1, knockbackDirection);
//        }
//    }


//    public void TakeDamage(int damage, Vector2 knockbackDirection) //�÷��̾� �ǰ� ����
//    {

//        if(isGrace) //���� ���� �� �浹�� X
//        {
//            return; 
//        }


//        if (haveShield) //��ȣ���� �ִٸ� ������ 1ȸ ����, ��ȣ�� ����
//        {
//            haveShield = true; //��ȣ�� ����
//            Debug.Log("��ȣ���� ������ ���ҽ��ϴ�!");

//            return;
//        }

//    }
//}
