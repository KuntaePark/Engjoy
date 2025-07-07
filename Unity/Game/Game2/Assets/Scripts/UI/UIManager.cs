using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro; //TextMeshPro에 필요
using UnityEngine;
using DataForm;

public class UIManager : MonoBehaviour
{

    public static UIManager Instance {  get; private set; }

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

    private Dictionary<string, PlayerStatusUI> otherPlayerUIs = new Dictionary<string, PlayerStatusUI>();

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
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


    public void UpdateOtherPlayersUI(Dictionary<string, PlayerData> allPlayers)
    {
        //서버에 있는데 클라이언트에는 없는 UI 생성
        foreach (var pair in allPlayers)
        {
            string playerId = pair.Key;
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
        List<string> disconnectedIds = new List<string>();
        foreach(string existingId in otherPlayerUIs.Keys)
        {
            if(!allPlayers.ContainsKey(existingId))
            {
                disconnectedIds.Add(existingId);
            }
        }

        foreach(string id in disconnectedIds)
        {
            Destroy(otherPlayerUIs[id].gameObject);
            otherPlayerUIs.Remove(id);
        }
    }

}
