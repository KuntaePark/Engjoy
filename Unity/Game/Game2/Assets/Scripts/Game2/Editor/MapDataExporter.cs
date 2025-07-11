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
            EditorUtility.DisplayDialog("에러", "먼저 Hierarchy 창에서 내보낼 맵 오브젝트를 선택해주세요.", "확인");
            return;
        }

        //모든 충돌 타일 좌표를 담을 리스트
        List<Vector2Int> allColliderTiles = new List<Vector2Int>();
        //JSON으로 추출할 타일맵들 이름
        string[] tilemapNamesToExport = { "Collider-Field", "Collider-Objects", "Collider-Wall" };

        //지정된 이름의 타일맵들을 순회하며 충돌 좌표 추출
        foreach (string mapName in tilemapNamesToExport)
        {
            //Grid 오브젝트 아래에서 이름으로 타일맵 찾기
            Transform tilemapTransform = selectedObject.transform.Find(mapName);
            if(tilemapTransform == null)
            {
                Debug.LogWarning($"'{mapName}'타일맵을 찾을 수 없습니다.");
                continue; //다음 타일맵으로 이동
            }
            Tilemap tilemap = tilemapTransform.GetComponent<Tilemap>();
            if(tilemap == null)
            {
                Debug.LogWarning($"'{mapName}' 오브젝트에 Tilemap 컴포넌트가 없어 건너뜁니다.");
                continue; //다음 타일맵으로
            }

            //타일맵의 유효 영역을 순회
            BoundsInt bounds = tilemap.cellBounds;
            for(int y = bounds.yMin; y < bounds.yMax; y++)
            {
                for(int x = bounds.xMin; x < bounds.xMax; x++)
                {
                    //해당 좌표에 타일이 있다면 리스트에 추가
                    if(tilemap.HasTile(new Vector3Int(x, y, 0)))
                    {
                        allColliderTiles.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        //추출된 데이터를 JSON 파일로 저장
        MapData mapData = new MapData();
        //두 레이어에 겹치는 타일이 있다면 중복 좌표 제거
        mapData.colliders = allColliderTiles.Distinct().ToList();

        //완성된 맵데이터 JSON 문자열로 변환
        string defaultFileName = selectedObject.name + ".json";
        //파일 저장 창 열기
        string path = EditorUtility.SaveFilePanel("충돌 맵 데이터 저장", "", defaultFileName, "json");

        //경로가 선택되었다면 파일을 저장하고 로그 남기기
        if (path.Length != 0)
        {
            string jsonContents = JsonUtility.ToJson(mapData, true);
            File.WriteAllText(path, jsonContents);

            Debug.Log($"성공: 맵 데이터가 다음 경로에 저장되었습니다: {path}. 총 {mapData.colliders.Count}개의 충돌 타일.");
            EditorUtility.RevealInFinder(path); //저장된 폴더 열기
        }

    }
}
