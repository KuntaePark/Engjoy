using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapInfo
{
    public string mapName;
    public GameObject mapPrefab;
}

public class MapManager : MonoBehaviour
{
    public List<MapInfo> mapList; //설정할 맵 리스트
    private GameObject currentMapInstance; //현재 활성화된 맵

    public void SwitchMap(string mapName)
    {
        //이미 해당 맵이 활성화되어 있는 상태라면 아무것도 하지 않기
        if(currentMapInstance != null && currentMapInstance.name == mapName + "(Clone)")
        {
            return;
        }

        //기존에 있던 맵이 있다면 파괴
        if(currentMapInstance != null)
        {
            Destroy(currentMapInstance);
        }

        //리스트에서 이름이 일치하는 맵 정보 찾기
        MapInfo mapToLoad = mapList.Find(m => m.mapName == mapName);

        if (mapToLoad != null && mapToLoad.mapPrefab != null)
        {
            //새 맵 프리팹을 씬에 생성
            currentMapInstance = Instantiate(mapToLoad.mapPrefab, Vector3.zero, Quaternion.identity);
            Debug.Log($"<color=cyan>[MapManager] Switched to map: {mapName}</color>");

        }
        else
        {
            Debug.LogError($"[MapManager] Map prefab for '{mapName}' not found!");
        }
    }
}
