using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{
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
            PlayerController player = collision.GetComponent<PlayerController>();
            if(player.Id == DataManager.Instance.id)
            {
                //trigger npc ui
            }
            // You can add more logic here, such as opening a dialogue box or starting a quest.
        }
    }
}
