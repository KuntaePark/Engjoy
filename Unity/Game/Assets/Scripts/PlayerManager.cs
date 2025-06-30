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

    //WSClient -> GameManager�� ���� ȣ��� ���� �Լ�
    public void UpdatePlayers(Dictionary<string, PlayerData> players)
    {
        //�÷��̾� ���� �� ������Ʈ
        HashSet<string> serverPlayerIds = new HashSet<string>(players.Keys);

        foreach (var playerPair in players)
        {
            string playerId = playerPair.Key;
            PlayerData playerData = playerPair.Value;
            Vector3 serverPosition = new Vector3(playerData.x, playerData.y, 0);

            //���ӿ� �÷��̾ �����ϴ� ���-> ��ġ ������Ʈ
            if (playerObjects.TryGetValue(playerId, out GameObject playerObj))
            {
                playerObj.transform.position = Vector3.Lerp(playerObj.transform.position, serverPosition, Time.deltaTime * smoothFactor);
            }

            //�÷��̾ ���� ��� -> ����
            else
            {
                GameObject newPlayer = Instantiate(playerPrefab, serverPosition, Quaternion.identity);
                newPlayer.name = $"Player_{playerId}";

                //PlayerController�� ID ����
                PlayerController controller = newPlayer.GetComponent<PlayerController>();
                if(controller != null)
                {
                    controller.Id = playerId;
                }

                playerObjects.Add(playerId, newPlayer);
                Debug.Log($"<color=cyan>Player Created: {playerId}</color>");

            }
        }
        
        //������ ���µ� ���ÿ� �����ִ� �÷��̾� ����

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

    //�ٸ� ��ũ��Ʈ���� �÷��̾� ������Ʈ ������ �޼���
    public GameObject GetPlayerObjectById(string playerId)
    {
        playerObjects.TryGetValue(playerId, out GameObject player);

        return player;
    }
}
