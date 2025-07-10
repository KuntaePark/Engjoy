using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Collection of data format classes. 
 */

namespace DataForm
{
    [System.Serializable]
    public class Packet
    {
        public string type;
        public string payload;
    }


    //로비에서 플레이어의 상태를 나타내는 데이터
    [System.Serializable]
    public class PlayerStateData
    {
        public float x;
        public float y;

        public bool isAttacking = false;

        public int bodyTypeIndex = 0; //몸통 타입 인덱스
        public int weaponTypeIndex = 0; //무기 타입 인덱스


        public PlayerStateData(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }

    [System.Serializable]
    //item data received from the server
    public class ItemData : ScriptableObject
    {
        private long itemId;
        public long ItemId { get; set; }

        private string itemName;
        public string ItemName { get; set; }

        private string itemDescription;
        public string ItemDescription { get; set; }

        private string itemImgPath;
        public string ItemImgPath { get; set; }

    }

    [System.Serializable]
    public class wordData
    {
        public long expr_id = -1;
        public string word_text = "";
        public string meaning = "";
        public int difficulty = -1;
    }

    [System.Serializable]
    public class PlayerData
    {
        //기본 수치
        public string id = ""; //플레이어 ID, 서버로부터 배정받음
        public int idx = -1;
        public float hp = -1;
        public float mp = -1;
        public int strengthLevel = -1;
        public bool isActionSelected = false;
        public string currentAction = "";

        //단어 관련
        public wordData[] words = null;
        public int correctIdx = -1;

        //애니메이션 관련
        public bool isCharging = false;
        public bool isCasting = false;
        public bool castEnd = false;
    }

    [System.Serializable]
    public class UserGameData
    {
        public string nickname;
        public int game1Score;
        public int game2Score;
        public int gold;
        public long ranking;
        public float rankingPercent;
        
        //커스터마이징 정보
        public int bodyTypeIndex;
        public int weaponTypeIndex;

    }
}

