using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//DataForm : 서버에서 필요한 판정 데이터를 담아두는 상자 (Entity나 Dto같은 느낌)
namespace DataForm
{
    [System.Serializable]
    public class GameState
    {
        public Dictionary<long, PlayerData> players; //플레이어 딕셔너리
        public Dictionary<string, KeywordData> keywords; //키워드 딕셔너리
        public Dictionary<string, MonsterData> monsters; //몬스터 딕셔너리
        public ExitData exit; //출구 딕셔너리

        public string status; //"MATCHINGROOM" / "PLAY"
        public bool isGameOver; //게임오버

        public int gameLevel; //게임 레벨
        public string mapName; //스폰맵
        public int score; //게임 점수
        public int gold; //게임 내 획득한 골드
        public List<SentenceData> completedSentences; //문장 정보

        public float timeLimit; //시간제한 
        public float countdown; //대기방 카운트다운

    }

    [System.Serializable]
    public class SentenceData
    {
        public int id;
        public string text;
        public string meaning;
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
        public float inputH;
        public float inputV;

        public string holdingKeywordId; //player's interacting keyword
        public string interactableKeywordId; //player's closest interactable keyword
        public bool canInteractWithExit; //interactable exit flag

        public bool isReady; //matchingRoom - ready flag
        public bool isEscaped; //excaped flag
        public bool isDown;
        public long revivablePlayerId = -1;
        public float reviveProgress;

        public string nickname = "";
        public int bodyTypeIndex = 0;
        public int weaponTypeIndex = 0;

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
        public long carrierId = -1; //interacting player's id (if !carrierId, null)
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