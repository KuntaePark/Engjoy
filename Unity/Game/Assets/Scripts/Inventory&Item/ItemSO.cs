using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "������ ��Ģ", menuName = "Item/Item Data")]
public class ItemSO : ScriptableObject
{

    public string itemName; //������ �̸�
    [TextArea] public string itemDesc; //������ ����
    public Sprite itemIcon; //������ ������

    /* 
        ������ ���� :
            ����
            ��ø
            ��ȣ��
     */

    public bool isShield; //��ȣ��

    [Header("Movement Stats")]
    public float dex; //���� ������

    [Header("Health Stats")]
    public int hp; //��

}
