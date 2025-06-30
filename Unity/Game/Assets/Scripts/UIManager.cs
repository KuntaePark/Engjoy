using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro; //TextMeshPro�� �ʿ�
using UnityEngine;

public class UIManager : MonoBehaviour
{

    public enum ItemType { POTION, BUFF, SHIELD };
    public enum EffectType { BUFF, SHIELD };



    [Header("HP UI")]
    public Image[] heartIcons; //HP �������� ��� �迭
    public Sprite fullHeart; //�� �� HP��Ʈ ��������Ʈ
    public Sprite emptyHeart; //�� �� HP��Ʈ ��������Ʈ

    [Header("Item UI")]
    //����
    public Image potionIcons; //���� ������ ������
    public TextMeshProUGUI potionCount; //���� ������ ����

    //����
    public Image buffIcons; //���� ������ ������
    public TextMeshProUGUI buffCount; //���� ������ ����

    //��ȣ�� ������
    public Image shieldIcons; //��ȣ�� ������ ������
    public TextMeshProUGUI shieldCount; //��ȣ�� ������ ����

    //Ȱ�� & ��Ȱ�� �� ����
    private Color iconDisabledColor = new Color(0.5f, 0.5f, 0.5f, 1f); //ȸ��
    private Color iconEnabledColor = Color.white; //���� ���� 

    [Header("Effect UI")]
    public Image buffEffect; //���� ���� ������
    public Image shieldEffect; //��ȣ�� ���� ������


    // ========================================== HP UI ==========================================
    public void HPUI(int currentHP)
    {
        for (int i = 0; i < heartIcons.Length; i++)
        {
            //i�� ���� ü��(currentHP)���� ���� ��� = �� �� ��Ʈ �̹��� ������
            if(i < currentHP)
            {
                heartIcons[i].sprite = fullHeart;
            }
            //i�� ���� ü�º��� ũ�ų� ���� ��� �� ��Ʈ �̹��� ������
            else
            {
                heartIcons[i].sprite = emptyHeart;
            }
        }
    }


    // ========================================== Effect UI ==========================================
    public void EffectUI(EffectType type, bool isActivated)
    {
        GameObject targetObject = null;

        switch (type)
        {
            case EffectType.BUFF:
                targetObject = buffEffect.gameObject;
                break;

            case EffectType.SHIELD:
                targetObject = shieldEffect.gameObject;
                break;
        }

        if (targetObject != null)
        {
            targetObject.SetActive(isActivated);
        }

    }




    // ========================================== Item UI ==========================================
    public void ItemUI(ItemType type, int count)
    {
        Image targetIcon = null;
        TextMeshProUGUI targetCount = null;

        //� �������� ������Ʈ�ϴ��� ����
        switch(type)
        {
            case ItemType.POTION:
               targetIcon = potionIcons;
               targetCount = potionCount;
                break;

            case ItemType.BUFF: 
                targetIcon = buffIcons;
                targetCount = buffCount;
                break;

            case ItemType.SHIELD:
                targetIcon = shieldIcons;
                targetCount = shieldCount;
                break;
        }

        if (targetIcon == null || targetCount == null) return;

        //���� �ؽ�Ʈ ������Ʈ
        targetCount.text = count.ToString();

        //������ 0�� ��� ������ ��Ӱ�
        if (count <= 0)
        {
            targetIcon.color = iconDisabledColor;
        }
        else
        {
             targetIcon.color = iconEnabledColor;
        }

    }

}
