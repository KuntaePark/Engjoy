//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class ItemManager : MonoBehaviour
//{

//    // ������ ����
//    // �÷��̾��� ������ �������� �����ͷ� ����ͼ� - ���� ������ �� �ִ� ������ ����� �� �ְ� ������ ����

//    public int potionCount = 3; //���� ������ ����
//    public int buffCount = 1; //���� ������ ����
//    public int shieldCount = 1; //��ȣ�� ������ ����

//    public PlayerController playerController; //PlayerController > Stat ����
//    public UIManager uiManager; //uiManager ����



//    // ============================== ���� ���� ============================== 
//    private void Awake()
//    {
//        playerController = FindObjectOfType<PlayerController>();
//        uiManager = FindObjectOfType<UIManager>();
//    }




//    // ============================== ���� ���� ============================== 
//    void Start()
//    {
//        //���� ���� �� : 

//        //UI�� ������ �������ֱ�
//    }

//    void Update()
//    {
//        //Ű �Է����� ������ ���.
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
//        //������ �����ִ������� Ȯ��
//        if(potionCount > 0)
//        {
//            bool healed = playerController.ApplyPotion(1);

//            //�÷��̾��� HP Ȯ��
//            //�۵� ���� + Debug.Log("ȸ�� �Ϸ�!") or Debug.Log("ȸ�� �������� �����ϴ�.");
//            if (healed)
//            {
//                potionCount--;
//                Debug.Log($"���� �������� ����߽��ϴ�. ���� {potionCount}�� ������ ���ҽ��ϴ�.");

//                //UI ������Ʈ ��û(Item)
//                if(uiManager != null)
//                {
//                    uiManager.ItemUI(UIManager.ItemType.POTION, potionCount);
//                }
//            }

//        }
//            else
//            {
//                Debug.Log("ȸ�� �������� ��� �����Ǿ����ϴ�. ȸ���� �� �����ϴ�.");
//            }


//    }

//    private void UseBuff()
//    {
//        //�������� �����ִ��� Ȯ��
//        if (buffCount > 0)
//        {
//            playerController.ApplyBuff(1.25f);

//        //�۵� ���� + Debug.Log("������ ��� ����!") or Debug.Log("������ ��� ����.");
//            buffCount--;
//            Debug.Log($"���� �������� ����߽��ϴ�. ���� {buffCount}�� ���� �������� ���ҽ��ϴ�.");

//        //UI ������Ʈ ��û(Item)
//            if(uiManager!=null)
//            {
//                uiManager.ItemUI(UIManager.ItemType.BUFF, buffCount);
//            }
//        }
//        else
//        {
//            Debug.Log("���� �������� ��� �����Ǿ����ϴ�. �������� ����� �� �����ϴ�.");
//        }

//    }

//    private void UseShield()
//    {
//        //�������� �����ִ��� Ȯ��
//        if (shieldCount > 0)
//        {
//            playerController.ApplyShield();
//            //�۵� ���� + Debug.Log("��ȣ�� ��� ����!") or Debug.Log("��ȣ�� ��� ����.");
//            shieldCount--;
//            Debug.Log($"��ȣ�� �������� ����߽��ϴ�. ���� {shieldCount}�� ��ȣ�� �������� ���ҽ��ϴ�.");

//            //UI ������Ʈ ��û(Item)
//            if (uiManager!=null)
//            {
//                uiManager.ItemUI(UIManager.ItemType.SHIELD, shieldCount);
//            }

//        }

//        else
//        {
//            Debug.Log("���� �������� ��� �����Ǿ����ϴ�. �������� ����� �� �����ϴ�.");
//        }

//    }
//}
