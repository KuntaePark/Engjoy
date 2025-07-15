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
    public static PlayerManager Instance { get; private set; }

    public GameObject playerPrefab;
    public float positionLerpFactor = 15.0f;

    //씬에 활성화된 플레이어 오브젝트들 관리
    private readonly Dictionary<long, PlayerController> playerControllers = new Dictionary<long, PlayerController>();



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
    public void UpdatePlayers(Dictionary<long, PlayerData> playersData)
    {
        //플레이어 생성 및 업데이트
        HashSet<long> serverPlayerIds = new HashSet<long>(playersData.Keys);



        foreach (var playerPair in playersData)
        {
           long playerId = playerPair.Key;
           PlayerData playerData = playerPair.Value;



            //게임에 플레이어가 존재하는 경우-> 위치 업데이트
            if (playerControllers.TryGetValue(playerId, out PlayerController controller))
            {
                controller.UpdateState(playerData);
            }



            //플레이어가 없는 경우 -> 생성
            else
            {
                Vector3 startPosition = new Vector3(playerData.x, playerData.y, 0);
                GameObject newPlayer = Instantiate(playerPrefab, startPosition, Quaternion.identity);
                newPlayer.name = $"Player_{playerId}";



                //PlayerController에 ID 설정
                PlayerController newController = newPlayer.GetComponent<PlayerController>();
                newController.Initialize(playerId, playerData, positionLerpFactor);



                playerControllers.Add(playerId, newController);
                Debug.Log($"<color=cyan>Player Created: {playerId}</color>");

            }

            //UI 업데이트
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateOtherPlayersUI(playersData);
            }
        }



        //서버에 없는데 로컬에 남아있는 플레이어 삭제
        List<long> disconnectedIds = new List<long>();

        foreach (long clientId in playerControllers.Keys)
        {
            if (!serverPlayerIds.Contains(clientId))
            {
                disconnectedIds.Add(clientId);
            }
        }



        foreach (long id in disconnectedIds)
        {
            if (playerControllers.TryGetValue(id, out PlayerController controllerToDestroy))
            {

                Destroy(controllerToDestroy.gameObject);
                playerControllers.Remove(id);

                Debug.Log($"<color=red>Player removed: {id}</color>");
            }
        }

    }



    //다른 스크립트에서 플레이어 오브젝트 참조용 메서드

    public PlayerController GetPlayerObjectById(long playerId)
    {
        playerControllers.TryGetValue(playerId, out PlayerController controller);

        return controller;
    }
}