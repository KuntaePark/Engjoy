using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System; // Action�� ����ϱ� ���� �߰�
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

    //WSClient -> GameManager�� ���� ȣ��� ���� �Լ�
    public void UpdateKeywords(Dictionary<string, KeywordData> keywords)
    {
        HashSet<string> serverKeywordIds = new HashSet<string>(keywords.Keys);

        foreach (var keywordPair in keywords)
        {
            string keywordId = keywordPair.Key;
            KeywordData keywordData = keywordPair.Value;

            GameObject keywordObj;
            keywordObjects.TryGetValue(keywordId, out keywordObj);

                //����(��)�� Ű���尡 ���� ��� -> ���� ����
                if (!keywordObjects.TryGetValue(keywordId, out keywordObj))
                {
                    Vector3 keywordPosition = new Vector3(keywordData.x, keywordData.y, 0);
                    keywordObj = Instantiate(keywordPrefab, keywordPosition, Quaternion.identity);
                    keywordObj.name = $"Keyword_{keywordId}";
                    keywordObjects.Add(keywordId, keywordObj);

                //KeywordController �ʱ�ȭ
                KeywordController controller = keywordObj.GetComponent<KeywordController>();
                if (controller != null)
                {
                    controller.Initialize(keywordId, keywordData.text);
                }

                }

            //Ű������ ������ ���� �� ��ġ ������Ʈ
            KeywordController keywordController = keywordObj.GetComponent<KeywordController>();

            if (keywordController == null) continue;

            //� �÷��̾ Ű���带 Ȧ���ϰ� �ִ� ���
            if (!string.IsNullOrEmpty(keywordData.carrierId))
            {
                //PlayerManager���� �ش� keyword�� carrierId�� ������ �÷��̾� ã��
                GameObject playerObj = PlayerManager.Instance.GetPlayerObjectById(keywordData.carrierId);
                if (playerObj != null) //ã�Ҵٸ�
                {
                    keywordController.SetCarrier(playerObj.transform);
                }
                
            }

            else //��ã�Ҵٸ� (���� ������ ���¶��)
            {
                //����ٴ� ��� null ����, ������ �������� ��ġ�� �̵�
                keywordController.SetCarrier(null);
                keywordObj.transform.position = new Vector3(keywordData.x, keywordData.y, 0);
            }


        }


    }
}