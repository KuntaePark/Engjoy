using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro; //TextMeshPro에 필요
using UnityEngine;

public class UIManager : MonoBehaviour
{

    public enum ItemType { POTION, BUFF, SHIELD };
    public enum EffectType { BUFF, SHIELD };



    [Header("HP UI")]
    public Image[] heartIcons; //HP 아이콘을 담올 배열
    public Sprite fullHeart; //꽉 찬 HP하트 스프라이트
    public Sprite emptyHeart; //텅 빈 HP하트 스프라이트

    [Header("Item UI")]
    //포션
    public Image potionIcons; //포션 아이템 아이콘
    public TextMeshProUGUI potionCount; //포션 아이템 수량

    //버프
    public Image buffIcons; //버프 아이템 아이콘
    public TextMeshProUGUI buffCount; //버프 아이템 수량

    //보호막 아이템
    public Image shieldIcons; //보호막 아이템 아이콘
    public TextMeshProUGUI shieldCount; //보호막 아이템 수량

    //활성 & 비활성 색 변수
    private Color iconDisabledColor = new Color(0.5f, 0.5f, 0.5f, 1f); //회색
    private Color iconEnabledColor = Color.white; //원래 색상 

    [Header("Effect UI")]
    public Image buffEffect; //버프 적용 아이콘
    public Image shieldEffect; //보호막 적용 아이콘


    // ========================================== HP UI ==========================================
    public void HPUI(int currentHP)
    {
        for (int i = 0; i < heartIcons.Length; i++)
        {
            //i가 현재 체력(currentHP)보다 작을 경우 = 꽉 찬 하트 이미지 보여줌
            if(i < currentHP)
            {
                heartIcons[i].sprite = fullHeart;
            }
            //i가 현재 체력보다 크거나 같을 경우 빈 하트 이미지 보여줌
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

        //어떤 아이템을 업데이트하는지 선택
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

        //개수 텍스트 업데이트
        targetCount.text = count.ToString();

        //개수가 0일 경우 아이콘 어둡게
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
