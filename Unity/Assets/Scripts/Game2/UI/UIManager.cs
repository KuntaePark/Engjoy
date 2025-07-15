using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro; //TextMeshPro에 필요
using UnityEngine;
using DataForm;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{

    public static UIManager Instance {  get; private set; }

    [Header("UI Panels")]
    public GameObject matchingRoomPanel; //대기방 UI 패널
    public GameObject playingPanel; //인게임 UI 패널

    [Header("Matching Room UI(MATCHINGROOM)")]
    public Button readyButton; //준비완료 버튼
    public Sprite notReadyBtnImage; //준비완료 버튼 이미지
    public Sprite readyBtnImage; //준비완료 눌린 버튼
    public TextMeshProUGUI buttonText; //준비완료 버튼 내의 텍스트
    public TextMeshProUGUI countdownText; //카운트다운 텍스트

    private bool isPlayerReady = false;

    [Header("Game Info UI(PLAY)")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI timerText;
    public Image timeBar; //타이머 바

    [Header("HP UI")]
    public List<Image> heartIcons;
    public Sprite fullHeart; // 🖤
    public Sprite emptyHeart; // 🤍

    [Header("Inventory UI")]
    public TextMeshProUGUI potionCountText;
    public TextMeshProUGUI buffCOuntText;
    public TextMeshProUGUI shieldCountText;

    [Header("Status Effect UI")]
    public GameObject buffStatusIcon;
    public GameObject shieldStatusIcon;

    [Header("Other Players UI")]
    public GameObject playerStatusUIPrefab; //UI 프리팹
    public Transform playerStatusLayout; //UI들이 정렬될 부모 오브젝트

    [Header("Revive UI")]
    public GameObject reviveUIPrefab; //부활UI 프리팹

    private GameObject reviveUIInstance;
    private Image reviveProgressBarInstance;


    private Dictionary<long, PlayerStatusUI> otherPlayerUIs = new Dictionary<long, PlayerStatusUI>();

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(transform.root.gameObject);
        }
    }


    public void SetUIForGameState(string status)
    {
        if (matchingRoomPanel == null || playingPanel == null) return;

        bool isMatchingRoom = (status == "MATCHINGROOM");
        matchingRoomPanel.SetActive(isMatchingRoom);
        playingPanel.SetActive(!isMatchingRoom);
    }

    public void UpdateCountdown(float countdownValue)
    {
        if (countdownText == null || readyButton == null) return;

        //countdownValue > 0 : 서버에서 카운트다운 시작
        if(countdownValue > 0)
        {
            countdownText.text = Mathf.CeilToInt(countdownValue).ToString();
        }
        else
        {
            countdownText.text = "Waiting...";
        }

        //버튼 활성화, 비활성화 로직
        if(countdownValue > 0 && countdownValue <= 5)
        {
            readyButton.interactable = false;
        }
        else
        {
            readyButton.interactable = true;
        }
    }





    //준비 완료 버튼
    public void OnReadyButtonClicked()
    {
        //현재 준비 상태 반전시키기
        isPlayerReady = !isPlayerReady;

        //새로운 상태에 따라 payload 생성
        string payload = $"{{\"isReady\":{isPlayerReady.ToString().ToLower()}}}";

        //서버에 메시지 전송
        WsClient.Instance.Send("ready", payload);
        Debug.Log($"Ready state changed to: {isPlayerReady}");

        if(isPlayerReady)
        {
            readyButton.image.sprite = readyBtnImage;
            if (buttonText != null) buttonText.text = "CANCEL";
        }
        else
        {
            readyButton.image.sprite = notReadyBtnImage;
            if (buttonText != null) buttonText.text = "READY";
        }
    }


    //부활 업데이트
    public void UpdateReviveUI(long targetId, float progress)
    {
       if(targetId >= 0)
        {
            if(reviveUIInstance == null)
            {
                //프리팹 생성, UIManager의 자식으로 (왜만들지?)
                reviveUIInstance = Instantiate(reviveUIPrefab, this.transform);

                //생성된 인스턴스에서 채워지는 Image컴포넌트 찾아서 저장하기 
                reviveProgressBarInstance = reviveUIInstance.transform.Find("progressBar").GetComponent<Image>();
            }

            if(reviveProgressBarInstance != null)
            {
                reviveProgressBarInstance.fillAmount = progress / 3f;
            }

            //UI를 대상 플레이어의 머리 위로 이동
            PlayerController targetController = PlayerManager.Instance.GetPlayerObjectById(targetId);
            if(targetController != null )
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(targetController.transform.position);
                reviveUIInstance.transform.position = screenPos + new Vector3(0, 60, 0);
            }
            else
            {
                if (reviveUIInstance != null)
                {
                    Destroy(reviveUIInstance);
                    reviveUIInstance = null; // 참조를 깨끗하게 비워줍니다.
                    reviveProgressBarInstance = null;
                }
            }
        }
    }

    //HP 업데이트
    public void UpdateHP(int currentHp)
    {
        for (int i=0; i <heartIcons.Count; i++)
        {
           if(i<currentHp)
            {
                heartIcons[i].sprite = fullHeart;
            }
            else
            {
                heartIcons[i].sprite = emptyHeart;
            }
        }
    }
    //인벤토리 업데이트
    public void UpdateInventory(InventoryData inventory)
    {
        if(inventory != null)
        {
            potionCountText.text = inventory.potion.ToString();
            buffCOuntText.text = inventory.buff.ToString();
            shieldCountText.text = inventory.shield.ToString();
        }
    }

    //버프상태 업데이트
    public void UpdateStatusEffects(bool isBuffed, bool hasShield)
    {
        buffStatusIcon.SetActive(isBuffed);
        shieldStatusIcon.SetActive(hasShield);
    }


    public void UpdateOtherPlayersUI(Dictionary<long, PlayerData> allPlayers)
    {
        //서버에 있는데 클라이언트에는 없는 UI 생성
        foreach (var pair in allPlayers)
        {
            long playerId = pair.Key;
            PlayerData playerData = pair.Value;

            //내 정보는 여기에서 안그림
            if (playerId == GameManager.Instance.MyPlayerId) continue;

            //아직 UI가 생성되지 않은 다른 플레이어라면 새로 생성
            if (!otherPlayerUIs.ContainsKey(playerId))
            {
                GameObject uiObject = Instantiate(playerStatusUIPrefab, playerStatusLayout);
                PlayerStatusUI newStatusUI = uiObject.GetComponent<PlayerStatusUI>();
                otherPlayerUIs.Add(playerId, newStatusUI);
            }

            //해당 플레이어의 UI 상태 업데이트
            otherPlayerUIs[playerId].UpdateUI(playerData);
        }

        //서버에서는 나갔는데 클라이언트에 UI가 남아있는 경우 삭제
        List<long> disconnectedIds = new List<long>();
        foreach(long existingId in otherPlayerUIs.Keys)
        {
            if(!allPlayers.ContainsKey(existingId))
            {
                disconnectedIds.Add(existingId);
            }
        }

        foreach(long id in disconnectedIds)
        {
            Destroy(otherPlayerUIs[id].gameObject);
            otherPlayerUIs.Remove(id);
        }
    }

    public void UpdateGameInfo(int level, float currentTime, float maxTime)
    {
        if(levelText != null)
        {
            levelText.text = $"Level: {level}";
        }
        if(timerText != null)
        {
            timerText.text = Mathf.CeilToInt(currentTime).ToString();
        }
        if(timeBar != null && maxTime > 0)
        {
            float fillRatio = currentTime / maxTime;
            timeBar.fillAmount = Mathf.Clamp01(fillRatio);
        }
    }

}
