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



    public void UpdataState(KeywordData data)
    {
        //data.carrierId를 보고 스스로 목표 위치를 결정
        if (!string.IsNullOrEmpty(data.carrierId))
        {

            //나를 어부바한 플레이어를 찾는다.
            PlayerController carrier = PlayerManager.Instance.GetPlayerObjectById(data.carrierId);

            if (carrier != null)
            {
                //그 플레이어의 위치를 나의 목표 위치로 삼는다.
                targetPosition = carrier.transform.position + followOffset;
            }
        }

        else
        {
            //들고 있는 플레이어가 없으면 서버가 지정한 월드 좌표를 목표 위치로 삼는다.
            targetPosition = new Vector3(data.x, data.y, 0);
        }
    }



    private void Update()
    {
        //어떤 상태든 항상 목표 위치로 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionLerpFactor);
    }

}