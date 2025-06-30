using System;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using DataForm;
using System.Linq;

//manages player objects in the game
public class PlayerManager : MonoBehaviour
{
   private Dictionary<string, GameObject> playerObjects = new Dictionary<string, GameObject>();

    public GameObject playerPrefab;
    public float smoothFactor = 10.0f; 

    public static PlayerManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //WSClient -> GameManager를 통해 호출될 메인 함수
    public void UpdatePlayers(Dictionary<string, PlayerData> players)
    {
        //플레이어 생성 및 업데이트
        HashSet<string> serverPlayerIds = new HashSet<string>(players.Keys);

        foreach (var playerPair in players)
        {
            string playerId = playerPair.Key;
            PlayerData playerData = playerPair.Value;
            Vector3 serverPosition = new Vector3(playerData.x, playerData.y, 0);

            //게임에 플레이어가 존재하는 경우-> 위치 업데이트
            if (playerObjects.TryGetValue(playerId, out GameObject playerObj))
            {
                playerObj.transform.position = Vector3.Lerp(playerObj.transform.position, serverPosition, Time.deltaTime * smoothFactor);
            }

            //플레이어가 없는 경우 -> 생성
            else
            {
                GameObject newPlayer = Instantiate(playerPrefab, serverPosition, Quaternion.identity);
                newPlayer.name = $"Player_{playerId}";

                //PlayerController에 ID 설정
                PlayerController controller = newPlayer.GetComponent<PlayerController>();
                if(controller != null)
                {
                    controller.Id = playerId;
                }

                playerObjects.Add(playerId, newPlayer);
                Debug.Log($"<color=cyan>Player Created: {playerId}</color>");

            }
        }
        
        //서버에 없는데 로컬에 남아있는 플레이어 삭제

        List<string> disconnectedIds = playerObjects.Keys.Except(serverPlayerIds).ToList();

        foreach (string id in disconnectedIds)
        {
            if(playerObjects.TryGetValue(id, out GameObject playerToDestroy))
            { 
                Destroy(playerToDestroy);
                playerObjects.Remove(id);
                Debug.Log($"<color=red>Player removed: {id}</color>");
            }
        }

    }

    //다른 스크립트에서 플레이어 오브젝트 참조용 메서드
    public GameObject GetPlayerObjectById(string playerId)
    {
        playerObjects.TryGetValue(playerId, out GameObject player);

        return player;
    }
}
