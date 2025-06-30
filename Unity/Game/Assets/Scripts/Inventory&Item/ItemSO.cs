using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "아이템 규칙", menuName = "Item/Item Data")]
public class ItemSO : ScriptableObject
{

    public string itemName; //아이템 이름
    [TextArea] public string itemDesc; //아이템 설명
    public Sprite itemIcon; //아이템 아이콘

    /* 
        아이템 종류 :
            포션
            민첩
            보호막
     */

    public bool isShield; //보호막

    [Header("Movement Stats")]
    public float dex; //버프 아이템

    [Header("Health Stats")]
    public int hp; //피

}
