using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//DataForm : �������� �ʿ��� ���� �����͸� ��Ƶδ� ���� (Entity�� Dto���� ����)
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
        public Dictionary<string, PlayerData> players;
        public Dictionary<string, KeywordData> keywords;
    }


    [System.Serializable]
    public class PlayerData
    {
        public string id;
        public float x;
        public float y;
        public string holdingKeywordId; //player's interacting keyword
        public string interactableKeywordId; //player's closest interactable keyword
    }

    [System.Serializable]
    public class  KeywordData
    {
        public string id;
        public string text;
        public float x;
        public float y;
        public string carrierId; //interacting player's id (if !carrierId, null)
        
    }

}
