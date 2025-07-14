using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DataForm;



public class ExitController : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite openSprite; //열린 출구 스프라이트
    public Sprite closedSprite; //닫힌 출구 스프라이트

    private SpriteRenderer spriteRenderer;
    private bool currentIsOpenState = false;


    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }



    public void UpdateState(ExitData data)
    {
        // Exit 오브젝트 관리(열린 상태냐 아니냐)
        transform.position = new Vector3(data.x, data.y, 0);

        if (currentIsOpenState != data.isOpen)
        {
            currentIsOpenState = data.isOpen;
            spriteRenderer.sprite = currentIsOpenState ? openSprite : closedSprite;

            if (currentIsOpenState)
            {
                Debug.Log("The Exit is now open!");
            }
        }

        // 화면의 UI 업데이트 요청
        ExitUIManager.Instance.UpdateExitUI(data.sentence, data.translation);
    }

}





