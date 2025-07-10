using System;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using DataForm;

//manages player objects in the game
public class PlayerManager : MonoBehaviour
{
    private Dictionary<long, GameObject> players = new Dictionary<long, GameObject>(); // Dictionary to hold player objects by their ID
    private Dictionary<long, PlayerStateData> serverPositions = new Dictionary<long, PlayerStateData>(); // Dictionary to hold player positions from the server
    private Queue<long> playerExitQueue = new Queue<long>(); // Queue to handle player exits

    public GameObject playerPrefab; // Prefab for player objects

    public float smoothFactor = 10.0f;
    // Start is called before the first frame update
    
    private void Awake()
    {
        var lobbyClient = GameObject.Find("LobbyClient").GetComponent<LobbyClient>();
        if (lobbyClient == null)
        {
            Debug.LogError("LobbyClient not found in the scene.");
            return;
        }
        else
        {
            lobbyClient.playerManager = this; // Set the PlayerManager reference in LobbyClient
        }
    }
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //remove players that are queued for exit
        while (playerExitQueue.Count > 0)
        {
            long playerId = playerExitQueue.Dequeue();
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
            long playerId = kv.Key;
            var data = kv.Value;
            Vector3 position = new Vector3(data.x, data.y, 0);
            if (!players.ContainsKey(playerId))
            {
                // Create a new player object if it doesn't exist
                Debug.Log("Creating new player: " + playerId + " at position: " + position);
                GameObject newPlayer = Instantiate(playerPrefab, position, Quaternion.identity);
                PlayerController playerController = newPlayer.GetComponent<PlayerController>();
                CharacterRenderer characterRenderer = newPlayer.GetComponentInChildren<CharacterRenderer>();
                playerController.Id = playerId; // Set the player ID
                characterRenderer.SetBody(data.bodyTypeIndex);
                characterRenderer.SetWeapon(data.weaponTypeIndex);
                players.Add(playerId, newPlayer);
            }
            else
            {
                
                // update existing player position
                Vector2 oldPos = players[playerId].transform.position;
                Vector3 targetPos = position;

                float distanceSqr = (oldPos - (Vector2)targetPos).sqrMagnitude;

                PlayerController playerController = players[playerId].GetComponent<PlayerController>();
                CharacterRenderer characterRenderer = playerController.characterRenderer;
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

                    if(playerId != DataManager.Instance.id)
                    {
                        // Only flip the player if it's not the local player
                        if (newPos.x < oldPos.x)
                            players[playerId].transform.localScale = new Vector3(-1, 1, 1); // Flip left
                        else if (newPos.x > oldPos.x)
                            players[playerId].transform.localScale = new Vector3(1, 1, 1); // Flip right
                    }
                }

               
                if (characterRenderer != null)
                {
                    if (playerId != DataManager.Instance.id)
                    {
                        characterRenderer.bodyAnimator.SetBool("isRunning", isRunning);
                        if(data.isAttacking)
                        {
                            characterRenderer.weaponAnimator.SetTrigger("Swing");
                        }
                    }
                    //커스터마이징 정보에 변경이 있을 경우 업데이트
                    if (characterRenderer.bodyTypeIndex != data.bodyTypeIndex)
                    {
                        characterRenderer.SetBody(data.bodyTypeIndex);
                    }
                    if (characterRenderer.weaponTypeIndex != data.weaponTypeIndex)
                    {
                        characterRenderer.SetWeapon(data.weaponTypeIndex);
                    }
                }
                else
                {
                    Debug.LogError("CharacterRenderer not found for player: " + playerId);
                }
                
            }
        }
    }

    public void updatePlayers(string JSONData)
    {
        serverPositions = JsonConvert.DeserializeObject<Dictionary<long, PlayerStateData>>(JSONData);
    }

    public void exitPlayer(long playerId)
    {
        playerExitQueue.Enqueue(playerId);
    }
}
