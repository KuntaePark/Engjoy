using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EntranceController : MonoBehaviour
{
    public GameObject entranceUI; // Reference to the UI GameObject
    public LobbyClient lobbyClient; // Reference to the WebSocket client
    public GameStartUI gameStartUI;

    // Start is called before the first frame update
    void Start()
    {
        entranceUI.SetActive(false); // Ensure the UI is hidden at the start   
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("escape") && entranceUI.activeSelf)
        {
            //close entrance UI
            Debug.Log("Closing Entrance UI");
            entranceUI.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        string id = other.GetComponent<PlayerController>().Id;
        if (other.CompareTag("Player") && id == lobbyClient.PlayerId)
        {
            //open entrance UI
            Debug.Log("Entrance Triggered by Player");
            entranceUI.SetActive(true);
            gameStartUI.loadGameStartUI();
        }
    }
}
