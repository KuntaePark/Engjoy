using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataForm;
using UnityEngine.UI;

public class PlayerPanel : MonoBehaviour
{
    //각 플레이어 패널
    public Text username;
    
    //hp
    public Slider HPbar;
    public Text HpText;

    //mp
    public Slider MPbar;
    public Text MpText;

    //strengthLevel
    public Slider StrBar;

    public RectTransform[] actionPanels = new RectTransform[3]; // 액션 패널 이미지 배열 (공격, 방어, 스페셜)
    public int selected = 0;

    public Text[] buttonTexts = new Text[2]; //키 버튼 텍스트

    [SerializeField]
    private RectTransform selector;

    // Start is called before the first frame update
    void Start()
    {
        HPbar.maxValue = 100; // 최대 HP 값 설정
        HPbar.value = 100;

        MPbar.maxValue = 10; // 최대 MP 값 설정
        MPbar.value = 0;

        StrBar.maxValue = 5;
        StrBar.value = 0;

    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < actionPanels.Length; i++)
        {
            if (i == selected)
            {
                placeSelector(i); // 선택된 액션 패널에 셀렉터 배치
            }
        }
    }

    private void placeSelector(int index)
    {         
        // 선택된 액션 패널의 위치에 셀렉터를 배치
            
        selector.position = actionPanels[index].position;
    }

    public void showPlayerInfo(Game1PlayerData player)
    {
        username.text = player.nickname; // 플레이어 이름 표시
        HPbar.value = player.hp; // 플레이어 HP 슬라이더 값 업데이트
        HpText.text = player.hp.ToString();
        MPbar.value = player.mp; // 플레이어 MP 슬라이더 값 업데이트
        MpText.text = player.mp.ToString();
        StrBar.value = player.strengthLevel;
    }

    public void setButtonText(bool isActionSelected)
    {
        if (isActionSelected)
        {
            buttonTexts[0].text = "시전\n(Ctrl)"; // 선택 완료 버튼 텍스트
            buttonTexts[1].text = "취소\n(Alt)"; // 취소 버튼 텍스트
        }
        else
        {
            buttonTexts[0].text = "결정\n(Ctrl)"; // 선택 버튼 텍스트
            buttonTexts[1].text = "마나 충전\n(Alt)"; // 마나 충전 버튼 텍스트
        }
    }
}
