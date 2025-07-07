using System;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using DataForm;

//manages player objects in the game
public class PlayerManager : MonoBehaviour
{
    private Dictionary<string, GameObject> players = new Dictionary<string, GameObject>(); // Dictionary to hold player objects by their ID
    private Dictionary<string, PlayerStateData> serverPositions = new Dictionary<string, PlayerStateData>(); // Dictionary to hold player positions from the server
    private Queue<string> playerExitQueue = new Queue<string>(); // Queue to handle player exits

    public GameObject playerPrefab; // Prefab for player objects

    public LobbyClient lobbyClient;

    public float smoothFactor = 10.0f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //remove players that are queued for exit
        while (playerExitQueue.Count > 0)
        {
            string playerId = playerExitQueue.Dequeue();
            if (players.ContainsKey(playerId))
            {
                Debug.Log("Removing player: " + playerId);
                Destroy(players[playerId]);
                players.Remove(playerId);
            }
        }

        //compare player positions and update them if necessary. if a player is not in the dictionary, create a new player object
        foreach (var kv in serverPositions)
        {
            string playerId = kv.Key;
            
            Vector3 position = new Vector3(kv.Value.x, kv.Value.y, 0);
            if (!players.ContainsKey(playerId))
            {
                // Create a new player object if it doesn't exist
                Debug.Log("Creating new player: " + playerId + " at position: " + position);
                GameObject newPlayer = Instantiate(playerPrefab, position, Quaternion.identity);
                PlayerController playerController = newPlayer.GetComponent<PlayerController>();
                playerController.Id = playerId; // Set the player ID
                players.Add(playerId, newPlayer);
            }
            else
            {
                
                // update existing player position
                Vector2 oldPos = players[playerId].transform.position;
                Vector3 targetPos = position;

                float distanceSqr = (oldPos - (Vector2)targetPos).sqrMagnitude;

                
                Animator animator = players[playerId].GetComponentInChildren<Animator>();
                bool isRunning = false;

                if (distanceSqr > 0.001f)
                {
                    Vector2 newPos = Vector2.Lerp(
                        oldPos,
                        targetPos,
                        Time.fixedDeltaTime * smoothFactor // 또는 Time.fixedDeltaTime * smoothFactor
                    );

                    players[playerId].transform.position = newPos;
                    isRunning = true;

                    if(playerId != lobbyClient.PlayerId)
                    {
                        // Only flip the player if it's not the local player
                        if (newPos.x < oldPos.x)
                            players[playerId].transform.localScale = new Vector3(-1, 1, 1); // Flip left
                        else if (newPos.x > oldPos.x)
                            players[playerId].transform.localScale = new Vector3(1, 1, 1); // Flip right
                    }
                }

               
               if(animator != null)
                {
                    animator.SetBool("isRunning", isRunning);
                }
                

            }
        }
    }

    public void updatePlayers(string JSONData)
    {
        serverPositions = JsonConvert.DeserializeObject<Dictionary<string, PlayerStateData>>(JSONData);

    }

    public void exitPlayer(string playerId)
    {
        playerExitQueue.Enqueue(playerId);
    }
}
