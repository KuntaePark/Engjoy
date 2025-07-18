using System.Collections;
using System.Collections.Generic;
using DataForm;
using TMPro;
using UnityEngine;



public class KeywordController : MonoBehaviour
{

    [SerializeField] private TextMeshPro keywordText;
    [SerializeField] private Vector3 followOffset = new Vector3(0, 0.7f, 0); // 플레이어 머리 위 오프셋
    [SerializeField] private float positionLerpFactor = 15.0f;



    public string keywordId;
    private Vector3 targetPosition; //서버가 지정한, 혹은 따라다녀야할 목표 위치


    private void Awake()
    {
        if (keywordText == null)
        {
            keywordText = GetComponentInChildren<TextMeshPro>();
        }

    }



    public void Initialize(KeywordData initialData)
    {

        this.keywordId = initialData.id;
        if (keywordText != null)
        {
            keywordText.text = initialData.text;
        }

        //최초 위치 설정
        transform.position = new Vector3(initialData.x, initialData.y, 0);
        targetPosition = transform.position;

    }



    public void UpdateState(KeywordData data)
    {
        //플레이어가 키워드 들고 있는지 확인
        if (data.carrierId >= 0)
        {
            PlayerController carrier = PlayerManager.Instance.GetPlayerObjectById(data.carrierId);
            if (carrier != null)
            {
                targetPosition = carrier.transform.position + followOffset;
            }
            else
            {
                targetPosition = new Vector3(data.x, data.y, 0);
            }
        }
    }



    private void Update()
    {
        //어떤 상태든 항상 목표 위치로 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionLerpFactor);
    }

}