using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//DataForm : 서버에서 필요한 판정 데이터를 담아두는 상자 (Entity나 Dto같은 느낌)
namespace DataForm
{
    [System.Serializable]
    public class Packet
    {
        public string type;
        public string payload;
    }

    [System.Serializable]
    public class GameState
    {
        public Dictionary<string, PlayerData> players; //플레이어 딕셔너리
        public Dictionary<string, KeywordData> keywords; //키워드 딕셔너리
        public Dictionary<string, MonsterData> monsters; //몬스터 딕셔너리
        public ExitData exit; //출구 딕셔너리
    }

    [System.Serializable]
    public class InventoryData
    {
        public int potion;
        public int buff;
        public int shield;
    }


    [System.Serializable]
    public class PlayerData
    {
        public string id;
        public float x;
        public float y;
        public string holdingKeywordId; //player's interacting keyword
        public string interactableKeywordId; //player's closest interactable keyword
        public bool canInteractWithExit; //interactable exit flag
        public bool isEscaped; //excaped flag

        public int hp;
        public int maxHp;
        public bool isBuffed;
        public bool hasShield;
        public InventoryData inventory; //inventory Data
    }


    [System.Serializable]
    public class KeywordData
    {
        public string id;
        public string text;
        public float x;
        public float y;
        public string carrierId; //interacting player's id (if !carrierId, null)
    }


    [System.Serializable]
    public class ExitData
    {

        public float x;
        public float y;
        public bool isOpen;
        public string sentence;
        public string translation;
        public int answerCount;
        public int correctedCount;
    }

    [System.Serializable]
    public class MonsterData
    {
        public string id;
        public string type; //"RUNNER", "CHASER"    
        public float x;
        public float y;
        public int hp;
        public bool isActive; //몬스터 활성화 플래그
    }

}