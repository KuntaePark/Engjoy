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
    public List<MapInfo> mapList; //������ �� ����Ʈ
    private GameObject currentMapInstance; //���� Ȱ��ȭ�� ��

    public void SwitchMap(string mapName)
    {
        //�̹� �ش� ���� Ȱ��ȭ�Ǿ� �ִ� ���¶�� �ƹ��͵� ���� �ʱ�
        if(currentMapInstance != null && currentMapInstance.name == mapName + "(Clone)")
        {
            return;
        }

        //������ �ִ� ���� �ִٸ� �ı�
        if(currentMapInstance != null)
        {
            Destroy(currentMapInstance);
        }

        //����Ʈ���� �̸��� ��ġ�ϴ� �� ���� ã��
        MapInfo mapToLoad = mapList.Find(m => m.mapName == mapName);

        if (mapToLoad != null && mapToLoad.mapPrefab != null)
        {
            //�� �� �������� ���� ����
            currentMapInstance = Instantiate(mapToLoad.mapPrefab, Vector3.zero, Quaternion.identity);
            Debug.Log($"<color=cyan>[MapManager] Switched to map: {mapName}</color>");

        }
        else
        {
            Debug.LogError($"[MapManager] Map prefab for '{mapName}' not found!");
        }
    }
}
