using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using DataForm;

public class PlayerStatusUI : MonoBehaviour
{
    public Image portraitImage;
    public List<Image> heartIcons;
    public GameObject buffIcon;
    public GameObject shieldIcon;

    public Sprite fullHeart; // 🖤
    public Sprite emptyHeart; // 🤍


    public void UpdateUI(PlayerData data)
    {
        //타 플레이어의 HP 업데이트
        for (int i = 0; i < heartIcons.Count; i++)
        {
            heartIcons[i].sprite = (i < data.hp) ? fullHeart : emptyHeart;
        }

        buffIcon.SetActive(data.isBuffed); 
        shieldIcon.SetActive(data.hasShield);
    }


}
