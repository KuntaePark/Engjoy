using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EntranceController : MonoBehaviour
{
    public GameStartUI gameStartUI;
    private GameObject entranceUI; // Reference to the UI GameObject

    // Start is called before the first frame update
    void Start()
    {
        entranceUI = gameStartUI.gameObject;
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
        long id = other.GetComponent<LobbyPlayerController>().Id;
        if (other.CompareTag("Player") && id == DataManager.Instance.id)
        {
            //open entrance UI
            Debug.Log("Entrance Triggered by Player");
            entranceUI.SetActive(true);
            gameStartUI.loadGameStartUI();
        }
    }
}
