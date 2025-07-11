using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

//타일 규칙 설계도 스크립트

//System.Serializable : 이 Attribute가 붙은 클래스는 Inspector 창에 내용을 표시하고 갑을 저장할 수 있게 됨.
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

//Unity 에디터에 넣는 명령어
//Project View -> Create -> WFC -> Tile Rule Data 메뉴 생성
//편의성을 위한 것
[CreateAssetMenu(fileName = "새 타일 규칙", menuName = "WFC/Tile Rule Data")]

//ScriptableObject : 하나의 데이터 덩어리를 나타내는 Unity 자체의 클래스.
//                  ㄴ> 비유하자면: 프로필 카드, 게임 캐릭터 등등.
//MonoBehaviour는 게임 씬에 존재하는 GameObject(캐릭터, 사물, 카메라 등)에 '부착'해서 사용하는 개념

//그래서: TileRuleData 는 각 타일의 규칙 정보를 담을 '프로필 카드'와 같은 데이터 타입으로 스크립트 구성하는 것.


// ============================== TileRuleData ==============================  
public class TileRuleData : ScriptableObject
{
    [Header("타일 기본 정보")]
    [Tooltip("맵에 실제로 보여질 타일의 게임 오브젝트(프리팹)입니다.")]
    public GameObject tilePrefab;

    [Tooltip("타일의 생성 가중치입니다. 숫자가 높을수록 더 자주 등장합니다.")]
    public float weight = 1.0f;

    [Header("타일 연결 규칙 (Sockets)")]
    [Tooltip("타일의 위, 오른쪽, 아래, 왼쪽의 소켓ID를 지정해줍니다.")]

    //위의 Socket 클래스를 변수로 사용
    public Socket sockets;


    // ============================== 타일 소켓ID 규칙 ============================== 
    /* 
        0 : 땅
        1 : 해변
        2 : 바다
    
     
     
     */
}
