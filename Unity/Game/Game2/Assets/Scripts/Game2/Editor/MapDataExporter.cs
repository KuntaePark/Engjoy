using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.IO;
using System.Linq;


[System.Serializable]
public class MapData
{
    public List<Vector2Int> colliders;

    public MapData()
    {
        colliders = new List<Vector2Int>();
    }
}

public class MapDataExporter
{
    [MenuItem("Tools/Export Map Colliders")]
    public static void ExportMapColliders()
    {
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject == null)
        {
            EditorUtility.DisplayDialog("����", "���� Hierarchy â���� ������ �� ������Ʈ�� �������ּ���.", "Ȯ��");
            return;
        }

        //��� �浹 Ÿ�� ��ǥ�� ���� ����Ʈ
        List<Vector2Int> allColliderTiles = new List<Vector2Int>();
        //JSON���� ������ Ÿ�ϸʵ� �̸�
        string[] tilemapNamesToExport = { "Collider-Field", "Collider-Objects", "Collider-Wall" };

        //������ �̸��� Ÿ�ϸʵ��� ��ȸ�ϸ� �浹 ��ǥ ����
        foreach (string mapName in tilemapNamesToExport)
        {
            //Grid ������Ʈ �Ʒ����� �̸����� Ÿ�ϸ� ã��
            Transform tilemapTransform = selectedObject.transform.Find(mapName);
            if(tilemapTransform == null)
            {
                Debug.LogWarning($"'{mapName}'Ÿ�ϸ��� ã�� �� �����ϴ�.");
                continue; //���� Ÿ�ϸ����� �̵�
            }
            Tilemap tilemap = tilemapTransform.GetComponent<Tilemap>();
            if(tilemap == null)
            {
                Debug.LogWarning($"'{mapName}' ������Ʈ�� Tilemap ������Ʈ�� ���� �ǳʶݴϴ�.");
                continue; //���� Ÿ�ϸ�����
            }

            //Ÿ�ϸ��� ��ȿ ������ ��ȸ
            BoundsInt bounds = tilemap.cellBounds;
            for(int y = bounds.yMin; y < bounds.yMax; y++)
            {
                for(int x = bounds.xMin; x < bounds.xMax; x++)
                {
                    //�ش� ��ǥ�� Ÿ���� �ִٸ� ����Ʈ�� �߰�
                    if(tilemap.HasTile(new Vector3Int(x, y, 0)))
                    {
                        allColliderTiles.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        //����� �����͸� JSON ���Ϸ� ����
        MapData mapData = new MapData();
        //�� ���̾ ��ġ�� Ÿ���� �ִٸ� �ߺ� ��ǥ ����
        mapData.colliders = allColliderTiles.Distinct().ToList();

        //�ϼ��� �ʵ����� JSON ���ڿ��� ��ȯ
        string defaultFileName = selectedObject.name + ".json";
        //���� ���� â ����
        string path = EditorUtility.SaveFilePanel("�浹 �� ������ ����", "", defaultFileName, "json");

        //��ΰ� ���õǾ��ٸ� ������ �����ϰ� �α� �����
        if (path.Length != 0)
        {
            string jsonContents = JsonUtility.ToJson(mapData, true);
            File.WriteAllText(path, jsonContents);

            Debug.Log($"����: �� �����Ͱ� ���� ��ο� ����Ǿ����ϴ�: {path}. �� {mapData.colliders.Count}���� �浹 Ÿ��.");
            EditorUtility.RevealInFinder(path); //����� ���� ����
        }

    }
}
