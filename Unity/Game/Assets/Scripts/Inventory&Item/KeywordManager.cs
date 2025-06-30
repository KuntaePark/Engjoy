using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System; // Action을 사용하기 위해 추가
using DataForm;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public class KeywordManager : MonoBehaviour
{
    public GameObject keywordPrefab;
    private Dictionary<string, GameObject> keywordObjects = new Dictionary<string, GameObject>();

    public static KeywordManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    //WSClient -> GameManager를 통해 호출될 메인 함수
    public void UpdateKeywords(Dictionary<string, KeywordData> keywords)
    {
        HashSet<string> serverKeywordIds = new HashSet<string>(keywords.Keys);

        foreach (var keywordPair in keywords)
        {
            string keywordId = keywordPair.Key;
            KeywordData keywordData = keywordPair.Value;

            GameObject keywordObj;
            keywordObjects.TryGetValue(keywordId, out keywordObj);

                //게임(씬)에 키워드가 없는 경우 -> 새로 생성
                if (!keywordObjects.TryGetValue(keywordId, out keywordObj))
                {
                    Vector3 keywordPosition = new Vector3(keywordData.x, keywordData.y, 0);
                    keywordObj = Instantiate(keywordPrefab, keywordPosition, Quaternion.identity);
                    keywordObj.name = $"Keyword_{keywordId}";
                    keywordObjects.Add(keywordId, keywordObj);

                //KeywordController 초기화
                KeywordController controller = keywordObj.GetComponent<KeywordController>();
                if (controller != null)
                {
                    controller.Initialize(keywordId, keywordData.text);
                }

                }

            //키워드의 소유자 상태 및 위치 업데이트
            KeywordController keywordController = keywordObj.GetComponent<KeywordController>();

            if (keywordController == null) continue;

            //어떤 플레이어가 키워드를 홀딩하고 있는 경우
            if (!string.IsNullOrEmpty(keywordData.carrierId))
            {
                //PlayerManager에서 해당 keyword의 carrierId와 동일한 플레이어 찾기
                GameObject playerObj = PlayerManager.Instance.GetPlayerObjectById(keywordData.carrierId);
                if (playerObj != null) //찾았다면
                {
                    keywordController.SetCarrier(playerObj.transform);
                }
                
            }

            else //못찾았다면 (땅에 떨어진 상태라면)
            {
                //따라다닐 대상 null 설정, 서버가 지정해준 위치로 이동
                keywordController.SetCarrier(null);
                keywordObj.transform.position = new Vector3(keywordData.x, keywordData.y, 0);
            }


        }


    }
}