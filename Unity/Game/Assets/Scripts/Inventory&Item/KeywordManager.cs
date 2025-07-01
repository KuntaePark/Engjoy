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
    public static KeywordManager Instance { get; private set; }


    [SerializeField]
    private GameObject keywordPrefab; //스폰할 키워드 프리팹
    private readonly Dictionary<string, KeywordController> keywordControllers = new Dictionary<string, KeywordController>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }



    public void UpdateKeywords(Dictionary<string, KeywordData> keywordsData)
    {
        HashSet<string> serverKeywordIds = new HashSet<string>(keywordsData.Keys);

      //키워드 상태 업데이트 또는 신규 생성
        foreach (var pair in keywordsData)
        {
            string keywordId = pair.Key;
            KeywordData keywordData = pair.Value;

            if (keywordControllers.TryGetValue(keywordId, out KeywordController controller))
            {
                //이미 존재하면 상태 업데이트
                controller.UpdataState(keywordData);
            }

            else
            {

                //없다면 새로 생성
                Vector3 startPosition = new Vector3(keywordData.x, keywordData.y, 0);
                GameObject newKeywordObj = Instantiate(keywordPrefab, startPosition, Quaternion.identity);
                newKeywordObj.name = $"Keyword_{keywordId}";



                KeywordController newController = newKeywordObj.GetComponent<KeywordController>();
                newController.Initialize(keywordData); //초기화
                keywordControllers.Add(keywordId, newController);

            }
        }



        //서버에서 삭제된 키워드를 클라이언트에서도 삭제
        //키워드 줍거나 출구에 제출했을 때
        List<string> removedIds = new List<string>();
        foreach (string clientId in keywordControllers.Keys)
        {
            if (!serverKeywordIds.Contains(clientId))
            {
                removedIds.Add(clientId);
            }
        }



        foreach (string id in removedIds)
        {
            if (keywordControllers.TryGetValue(id, out KeywordController controllerToDestroy))
            {
               Destroy(controllerToDestroy.gameObject);
                keywordControllers.Remove(id);
            }
        }
    }
}