using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

//Ÿ�� ��Ģ ���赵 ��ũ��Ʈ

//System.Serializable : �� Attribute�� ���� Ŭ������ Inspector â�� ������ ǥ���ϰ� ���� ������ �� �ְ� ��.
[System.Serializable]
public struct TopBottomSocket
{
    public int leftPin;
    public int rightPin;
}

[System.Serializable]
public struct LeftRightSocket
{
    public int upPin;
    public int downPin;
}


[System.Serializable]
public class Socket
{
    public TopBottomSocket up;
    public LeftRightSocket right;
    public TopBottomSocket down;
    public LeftRightSocket left;
}

//Unity �����Ϳ� �ִ� ��ɾ�
//Project View -> Create -> WFC -> Tile Rule Data �޴� ����
//���Ǽ��� ���� ��
[CreateAssetMenu(fileName = "�� Ÿ�� ��Ģ", menuName = "WFC/Tile Rule Data")]

//ScriptableObject : �ϳ��� ������ ����� ��Ÿ���� Unity ��ü�� Ŭ����.
//                  ��> �������ڸ�: ������ ī��, ���� ĳ���� ���.
//MonoBehaviour�� ���� ���� �����ϴ� GameObject(ĳ����, �繰, ī�޶� ��)�� '����'�ؼ� ����ϴ� ����

//�׷���: TileRuleData �� �� Ÿ���� ��Ģ ������ ���� '������ ī��'�� ���� ������ Ÿ������ ��ũ��Ʈ �����ϴ� ��.


// ============================== TileRuleData ==============================  
public class TileRuleData : ScriptableObject
{
    [Header("Ÿ�� �⺻ ����")]
    [Tooltip("�ʿ� ������ ������ Ÿ���� ���� ������Ʈ(������)�Դϴ�.")]
    public GameObject tilePrefab;

    [Tooltip("Ÿ���� ���� ����ġ�Դϴ�. ���ڰ� �������� �� ���� �����մϴ�.")]
    public float weight = 1.0f;

    [Header("Ÿ�� ���� ��Ģ (Sockets)")]
    [Tooltip("Ÿ���� ��, ������, �Ʒ�, ������ ����ID�� �������ݴϴ�.")]

    //���� Socket Ŭ������ ������ ���
    public Socket sockets;


    // ============================== Ÿ�� ����ID ��Ģ ============================== 
    /* 
        0 : ��
        1 : �غ�
        2 : �ٴ�
    
     
     
     */
}
