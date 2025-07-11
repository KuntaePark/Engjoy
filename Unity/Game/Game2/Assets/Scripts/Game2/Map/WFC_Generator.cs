using System.Linq; //Linq 기능을 사용하면 코드가 간결해진다.
using System.Collections.Generic;
using UnityEngine;

//맵의 각 칸(셀)의 상태를 저장할 클래스
public class Cell
{
    public bool isCollapsed = false; //타일이 최종 결정되었는지 확인하는 플래그
    public List<TileRuleData> possibleTiles; //이 칸에 놓일 수 있는 타일 후보 목록
}

public class WFC_Generator : MonoBehaviour
{
    [Header("맵 설정")]
    public Vector2Int mapSize = new Vector2Int(10, 10); //맵의 가로, 세로 크기

    [Header("타일 규칙")]
    public List<TileRuleData> allTileRules; //만들어놓은 TileRuleData 에셋들 담아놓기

    private Cell[,] grid; //맵의 모든 칸(Cell) 정보 저장_2차원 배열 (x,y)



    // ========================================== Start ==========================================
    //  그리드 초기화 함수 & WFC 작동시키며 게임을 시작
    private void Start()
    {
        //1. 그리드 초기화
        InitializeGrid();

        //2. WFC 알고리즘 실행
        StartCoroutine(Generate());
    }

    //public void GenerateNewMap()
    //{
    //    StopAllCoroutines(); //이전에 실행되고 있던 코루틴이 있다면 안전하게 중지
    //    ClearMap(); //이전 맵 타일 싹 삭제

    //    //새로운 맵 생성 코루틴 시작
    //    StartCoroutine(Generate());
    //}


    void ClearMap()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
    


    // ========================================== 그리드 초기화 함수 ==========================================
    //                                       --- 텅 빈 그리드 판 준비 ---
    void InitializeGrid()
    {
        grid = new Cell[mapSize.x, mapSize.y];

        //맵의 모든 칸을 순회하면서
        for (int x = 0; x < mapSize.x; x++)
        {

            for (int y = 0; y < mapSize.y; y++)
            {

                //각 칸(Cell)을 생성하고
                grid[x, y] = new Cell();
                //모든 타일 규칙을 '가능한 타일 후보'로 전부 넣어주기
                grid[x, y].possibleTiles = new List<TileRuleData>(allTileRules);
            }
        }
        Debug.Log("그리드 초기화 완료. 각 셀은 " + allTileRules.Count + "개의 가능성을 가졌습니다.");
    }

    // ========================================== WFC 작동 ==========================================
    //                      --- 맵이 완성될 때까지 무한 반복_while(true) ---
    IEnumerator<WaitForSeconds> Generate()
    {
        int maxRetries = 10; //무한루프 방지 천장
        int retryCount = 0;

        //맵의 모든 칸이 붕괴(결정)될 때까지 계속 반복
        while (retryCount < maxRetries)
        {

            InitializeGrid();
            bool success = true;


            while (true)
            {
                //a. 붕괴되지 않은 셀들만 모아 엔트로피가 가장 낮은 셀 찾기
                //  (다음 구현 목표)
                Cell cellToCollapse = FindLowestEntropyCell();

                // ●● 2 ●● 모든 셀이 붕괴되었는지 판별! ●● 2 ●●
                if (cellToCollapse == null)
                {
                    //더 이상 붕괴할 셀이 없으면 루프 종료
                    Debug.Log("맵 생성 완료!");
                    break;
                }

                //b. 해당 셀을 '붕괴'시키기.
                CollapseCell(cellToCollapse);

                //c. 붕괴된 셀의 정보를 주변 셀에 '전파'합니다.
                Vector2Int collapsedCellCoords = FindCellCoordinates(cellToCollapse);
                if (collapsedCellCoords.x != -1) //유효한 좌표를 찾았을 경우에만 전파!
                {
                    if (!Propagate(collapsedCellCoords))
                    {
                        success = false;
                        break;
                    }
                }

                yield return new WaitForSeconds(0.01f); //과정을 시각적으로 보기 위함
            }

            if (success)
            {

                Debug.Log("맵 생성 성공!");
                InstantiateTiles();
                yield break; //코루틴 완전 종료
            }

            //실패했다면
            retryCount++;
            Debug.Log("막다른 길 도달! 재시도... (" + retryCount + "/" + maxRetries);
            yield return null; //한 프레임 쉬고 재시도

        }

        Debug.LogError("맵 생성 실패 - 최대 재시도 횟수를 초과했습니다.");


    }


    // ========================================== 셀 붕괴 ==========================================
    void CollapseCell(Cell cell)
    {
        //모든 후보 타일들의 가중치 총합 계산
        float totalWeight = 0f;
        foreach (var tile in cell.possibleTiles)
        {
            totalWeight += tile.weight;
        }

        float randomValue = Random.Range(0, totalWeight);

        //뽑힌 숫자가 어느 타일의 구간에 속하는지 찾기
        TileRuleData selectedTile = null;
        float cumulativeWeight = 0f;
        foreach (var tile in cell.possibleTiles)
        {
            cumulativeWeight += tile.weight;
            if (randomValue < cumulativeWeight)
            {
                selectedTile = tile;
                break;
            }
        }

        //오류처리 - 오류날 시 마지막 타일 선택
        if (selectedTile == null)
        {
            selectedTile = cell.possibleTiles[cell.possibleTiles.Count - 1];
        }

        //선택된 타일 하나만 남기고 나머지 후보 전부 제거
        cell.possibleTiles.Clear();
        cell.possibleTiles.Add(selectedTile);

        //이 셀은 붕괴됨을 표시
        cell.isCollapsed = true;
    }

    // ========================================== 셀 전파 ==========================================
    bool Propagate(Vector2Int startCoords)
    {
        //Stack 사용하여 전파할 셀 위치 관리 - 공부한대로임
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(startCoords);

        while (stack.Count > 0)
        {
            Vector2Int currentCoords = stack.Pop();
            Cell currentCell = grid[currentCoords.x, currentCoords.y];

            //현재 셀의 유효한 규칙들 (보통은 붕괴 후 하나만 남음)
            List<TileRuleData> validRulesForCurrent = currentCell.possibleTiles;

            //4방향 (상하좌우 이웃 확인)
            Vector2Int[] directions = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            for (int i = 0; i < directions.Length; i++)
            {
                Vector2Int neighborCoords = currentCoords + directions[i];

                //맵 경계를 벗어나는지 확인
                if (neighborCoords.x < 0 || neighborCoords.x >= mapSize.x || neighborCoords.y < 0 || neighborCoords.y >= mapSize.y)
                {
                    continue;
                }

                Cell neightborCell = grid[neighborCoords.x, neighborCoords.y];

                //이미 붕괴된 셀은 건너뜀

                if (neightborCell.isCollapsed) { continue; }

                List<TileRuleData> neighborPossibilities = neightborCell.possibleTiles;
                int originalNeighborCount = neighborPossibilities.Count;

                //이웃의 후보 목록을 뒤에서부터 순회 & 제거 (리스트 순서 꼬임 방지)
                for (int j = neighborPossibilities.Count - 1; j >= 0; j--)
                {
                    TileRuleData neighborRule = neighborPossibilities[j];
                    bool isPossible = false;

                    //현재 셀의 모든 유효한 규칙과 이웃의 규칙을 비교
                    foreach (var currentRule in validRulesForCurrent)
                    {

                        switch(i)
                        {

                            //위쪽 방향: leftPin, rightPin 비교
                            case 0: //UP
                                if (currentRule.sockets.up.leftPin == neighborRule.sockets.down.leftPin &&
                                    currentRule.sockets.up.rightPin == neighborRule.sockets.down.rightPin)
                                {
                                    isPossible = true;
                                }
                                break;

                            //아래 방향: leftPin, rightPin 비교
                            case 1: //Down
                                if (currentRule.sockets.down.leftPin == neighborRule.sockets.up.leftPin &&
                                    currentRule.sockets.down.rightPin == neighborRule.sockets.up.rightPin)
                                {
                                    isPossible = true;
                                }
                                break;
                        
                            //왼쪽 방향: upPin, downPin 비교
                            case 2: //Left
                                if (currentRule.sockets.left.upPin ==  neighborRule.sockets.right.upPin &&
                                    currentRule.sockets.left.downPin == neighborRule.sockets.right.downPin)
                                {
                                    isPossible = true;
                                }
                                break;


                            //오른쪽 방향: upPin, downPin 비교
                            case 3: //Right
                                if (currentRule.sockets.right.upPin == neighborRule.sockets.left.upPin &&
                                    currentRule.sockets.right.downPin == neighborRule.sockets.left.downPin)
                                {
                                    isPossible = true;
                                }
                                break;

                        }

                        if (isPossible) break;
                     }

                    // 이웃의 이 타일이 현재 셀과 연결될 수 있는 경우가 하나도 없다면,
                    if (!isPossible)
                    {
                        // 이웃의 가능성 목록에서 해당 타일을 제거
                        neighborPossibilities.RemoveAt(j);
                    }

                }

                // 만약 이웃의 후보 목록에 변화가 생겼다면 (후보가 줄었다면),
                // 그 이웃도 다른 이웃들에게 영향을 줄 수 있으므로 스택에 추가하여 연쇄 반응을 일으킴
                if (originalNeighborCount > neighborPossibilities.Count)
                {

                    //이웃 후보 목록 0개 = 막다른 길. 
                    if (neighborPossibilities.Count == 0)
                    {
                        return false;
                    }


                    stack.Push(neighborCoords);
                }

            }

        }

        return true; //모든 전파가 성공적으로 끝나면 성공 반환

    }


    Cell FindLowestEntropyCell()
    {

        // ●● 3 ●● 남은 셀 중 최소 엔트로피 값을 가진 셀 조회! ●● 3 ●●
        List<Cell> lowestEntropyCells = new List<Cell>();
        int minEntropy = int.MaxValue; //최소 엔트로피 값을 일단 가장 큰 숫자로 초기화 (왜?)

        //그리드의 모든 셀을 순회
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Cell currentCell = grid[x, y];

                //아직 붕괴되지 않은 셀만 대상
                if (!currentCell.isCollapsed)
                {
                    int entropy = currentCell.possibleTiles.Count;

                    //현재 셀의 엔트로피가 지금까지 찾은 최소 엔트로피보다 작다면
                    if (entropy < minEntropy)
                    {
                        minEntropy = entropy; //최소 엔트로피 값을 갱신
                        lowestEntropyCells.Clear(); //이전 후보들 전부 제거
                        lowestEntropyCells.Add(currentCell); //현재 셀을 유일한 후보로 추가
                    }
                    //현재 셀의 엔트로피가 최소 엔트로피와 같다면
                    else if (entropy == minEntropy)
                    {
                        //동점 후보이므로 리스트에 추가
                        lowestEntropyCells.Add(currentCell);
                    }
                }

            }
        }

        //가장 낮은 엔트로피를 가진 후보 셀이없다면 (모든 셀이 붕괴되었다면) null 반환 >> 맵 생성 종료.
        if (lowestEntropyCells.Count == 0)
        {
            return null;
        }

        //후보가 여러개라면 그 중에서 무작위로 하나를 선택해서 반환
        int randomIndex = Random.Range(0, lowestEntropyCells.Count);
        return lowestEntropyCells[randomIndex];


    }


    // 3. 최종 타일 생성 함수
    void InstantiateTiles()
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Cell cell = grid[x, y];
                if (cell.possibleTiles.Count > 0)
                {
                    TileRuleData finalTile = cell.possibleTiles[0]; // 최종적으로 결정된 타일 규칙
                    Vector3 position = new Vector3(x, y, 0); // 2D 기준 위치
                    Instantiate(finalTile.tilePrefab, position, Quaternion.identity, this.transform);
                }
            }
        }
    }

    // 특정 셀의 좌표를 찾는 보조 함수
    Vector2Int FindCellCoordinates(Cell cell)
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                if (grid[x, y] == cell)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return new Vector2Int(-1, -1); // 못찾았을 경우
    }
}
