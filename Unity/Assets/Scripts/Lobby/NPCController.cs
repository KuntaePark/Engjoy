using DataForm;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    [SerializeField]
    private CustomizeUI customizeUI; // Reference to the CustomizeUI script


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Player has entered the NPC interaction area.");
        if(collision.CompareTag("Player"))
        {
            LobbyPlayerController player = collision.GetComponent<LobbyPlayerController>();
            if(player.Id == DataManager.Instance.id)
            {
                //trigger npc ui
                StartCoroutine(DataManager.Instance.getUserData((UserGameData data) =>
                {
                    if (data != null)
                    {
                        //유저 데이터에서 바디 타입과 무기 타입을 가져옴
                        customizeUI.currentBodyTypeIndex = data.bodyTypeIndex;
                        customizeUI.currentWeaponTypeIndex = data.weaponTypeIndex;
                        customizeUI.showCustomizeUI();
                    }
                    else
                    {
                        Debug.LogError("Failed to load user data for customization.");
                    }
                }));
            }
            // You can add more logic here, such as opening a dialogue box or starting a quest.
        }
    }
}
