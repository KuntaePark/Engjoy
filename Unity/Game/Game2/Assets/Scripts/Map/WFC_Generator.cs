using System.Linq; //Linq ����� ����ϸ� �ڵ尡 ����������.
using System.Collections.Generic;
using UnityEngine;

//���� �� ĭ(��)�� ���¸� ������ Ŭ����
public class Cell
{
    public bool isCollapsed = false; //Ÿ���� ���� �����Ǿ����� Ȯ���ϴ� �÷���
    public List<TileRuleData> possibleTiles; //�� ĭ�� ���� �� �ִ� Ÿ�� �ĺ� ���
}

public class WFC_Generator : MonoBehaviour
{
    [Header("�� ����")]
    public Vector2Int mapSize = new Vector2Int(10, 10); //���� ����, ���� ũ��

    [Header("Ÿ�� ��Ģ")]
    public List<TileRuleData> allTileRules; //�������� TileRuleData ���µ� ��Ƴ���

    private Cell[,] grid; //���� ��� ĭ(Cell) ���� ����_2���� �迭 (x,y)



    // ========================================== Start ==========================================
    //  �׸��� �ʱ�ȭ �Լ� & WFC �۵���Ű�� ������ ����
    private void Start()
    {
        //1. �׸��� �ʱ�ȭ
        InitializeGrid();

        //2. WFC �˰��� ����
        StartCoroutine(Generate());
    }

    //public void GenerateNewMap()
    //{
    //    StopAllCoroutines(); //������ ����ǰ� �ִ� �ڷ�ƾ�� �ִٸ� �����ϰ� ����
    //    ClearMap(); //���� �� Ÿ�� �� ����

    //    //���ο� �� ���� �ڷ�ƾ ����
    //    StartCoroutine(Generate());
    //}


    void ClearMap()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
    


    // ========================================== �׸��� �ʱ�ȭ �Լ� ==========================================
    //                                       --- �� �� �׸��� �� �غ� ---
    void InitializeGrid()
    {
        grid = new Cell[mapSize.x, mapSize.y];

        //���� ��� ĭ�� ��ȸ�ϸ鼭
        for (int x = 0; x < mapSize.x; x++)
        {

            for (int y = 0; y < mapSize.y; y++)
            {

                //�� ĭ(Cell)�� �����ϰ�
                grid[x, y] = new Cell();
                //��� Ÿ�� ��Ģ�� '������ Ÿ�� �ĺ�'�� ���� �־��ֱ�
                grid[x, y].possibleTiles = new List<TileRuleData>(allTileRules);
            }
        }
        Debug.Log("�׸��� �ʱ�ȭ �Ϸ�. �� ���� " + allTileRules.Count + "���� ���ɼ��� �������ϴ�.");
    }

    // ========================================== WFC �۵� ==========================================
    //                      --- ���� �ϼ��� ������ ���� �ݺ�_while(true) ---
    IEnumerator<WaitForSeconds> Generate()
    {
        int maxRetries = 10; //���ѷ��� ���� õ��
        int retryCount = 0;

        //���� ��� ĭ�� �ر�(����)�� ������ ��� �ݺ�
        while (retryCount < maxRetries)
        {

            InitializeGrid();
            bool success = true;


            while (true)
            {
                //a. �ر����� ���� ���鸸 ��� ��Ʈ���ǰ� ���� ���� �� ã��
                //  (���� ���� ��ǥ)
                Cell cellToCollapse = FindLowestEntropyCell();

                // �ܡ� 2 �ܡ� ��� ���� �ر��Ǿ����� �Ǻ�! �ܡ� 2 �ܡ�
                if (cellToCollapse == null)
                {
                    //�� �̻� �ر��� ���� ������ ���� ����
                    Debug.Log("�� ���� �Ϸ�!");
                    break;
                }

                //b. �ش� ���� '�ر�'��Ű��.
                CollapseCell(cellToCollapse);

                //c. �ر��� ���� ������ �ֺ� ���� '����'�մϴ�.
                Vector2Int collapsedCellCoords = FindCellCoordinates(cellToCollapse);
                if (collapsedCellCoords.x != -1) //��ȿ�� ��ǥ�� ã���� ��쿡�� ����!
                {
                    if (!Propagate(collapsedCellCoords))
                    {
                        success = false;
                        break;
                    }
                }

                yield return new WaitForSeconds(0.01f); //������ �ð������� ���� ����
            }

            if (success)
            {

                Debug.Log("�� ���� ����!");
                InstantiateTiles();
                yield break; //�ڷ�ƾ ���� ����
            }

            //�����ߴٸ�
            retryCount++;
            Debug.Log("���ٸ� �� ����! ��õ�... (" + retryCount + "/" + maxRetries);
            yield return null; //�� ������ ���� ��õ�

        }

        Debug.LogError("�� ���� ���� - �ִ� ��õ� Ƚ���� �ʰ��߽��ϴ�.");


    }


    // ========================================== �� �ر� ==========================================
    void CollapseCell(Cell cell)
    {
        //��� �ĺ� Ÿ�ϵ��� ����ġ ���� ���
        float totalWeight = 0f;
        foreach (var tile in cell.possibleTiles)
        {
            totalWeight += tile.weight;
        }

        float randomValue = Random.Range(0, totalWeight);

        //���� ���ڰ� ��� Ÿ���� ������ ���ϴ��� ã��
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

        //����ó�� - ������ �� ������ Ÿ�� ����
        if (selectedTile == null)
        {
            selectedTile = cell.possibleTiles[cell.possibleTiles.Count - 1];
        }

        //���õ� Ÿ�� �ϳ��� ����� ������ �ĺ� ���� ����
        cell.possibleTiles.Clear();
        cell.possibleTiles.Add(selectedTile);

        //�� ���� �ر����� ǥ��
        cell.isCollapsed = true;
    }

    // ========================================== �� ���� ==========================================
    bool Propagate(Vector2Int startCoords)
    {
        //Stack ����Ͽ� ������ �� ��ġ ���� - �����Ѵ����
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(startCoords);

        while (stack.Count > 0)
        {
            Vector2Int currentCoords = stack.Pop();
            Cell currentCell = grid[currentCoords.x, currentCoords.y];

            //���� ���� ��ȿ�� ��Ģ�� (������ �ر� �� �ϳ��� ����)
            List<TileRuleData> validRulesForCurrent = currentCell.possibleTiles;

            //4���� (�����¿� �̿� Ȯ��)
            Vector2Int[] directions = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            for (int i = 0; i < directions.Length; i++)
            {
                Vector2Int neighborCoords = currentCoords + directions[i];

                //�� ��踦 ������� Ȯ��
                if (neighborCoords.x < 0 || neighborCoords.x >= mapSize.x || neighborCoords.y < 0 || neighborCoords.y >= mapSize.y)
                {
                    continue;
                }

                Cell neightborCell = grid[neighborCoords.x, neighborCoords.y];

                //�̹� �ر��� ���� �ǳʶ�

                if (neightborCell.isCollapsed) { continue; }

                List<TileRuleData> neighborPossibilities = neightborCell.possibleTiles;
                int originalNeighborCount = neighborPossibilities.Count;

                //�̿��� �ĺ� ����� �ڿ������� ��ȸ & ���� (����Ʈ ���� ���� ����)
                for (int j = neighborPossibilities.Count - 1; j >= 0; j--)
                {
                    TileRuleData neighborRule = neighborPossibilities[j];
                    bool isPossible = false;

                    //���� ���� ��� ��ȿ�� ��Ģ�� �̿��� ��Ģ�� ��
                    foreach (var currentRule in validRulesForCurrent)
                    {

                        switch(i)
                        {

                            //���� ����: leftPin, rightPin ��
                            case 0: //UP
                                if (currentRule.sockets.up.leftPin == neighborRule.sockets.down.leftPin &&
                                    currentRule.sockets.up.rightPin == neighborRule.sockets.down.rightPin)
                                {
                                    isPossible = true;
                                }
                                break;

                            //�Ʒ� ����: leftPin, rightPin ��
                            case 1: //Down
                                if (currentRule.sockets.down.leftPin == neighborRule.sockets.up.leftPin &&
                                    currentRule.sockets.down.rightPin == neighborRule.sockets.up.rightPin)
                                {
                                    isPossible = true;
                                }
                                break;
                        
                            //���� ����: upPin, downPin ��
                            case 2: //Left
                                if (currentRule.sockets.left.upPin ==  neighborRule.sockets.right.upPin &&
                                    currentRule.sockets.left.downPin == neighborRule.sockets.right.downPin)
                                {
                                    isPossible = true;
                                }
                                break;


                            //������ ����: upPin, downPin ��
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

                    // �̿��� �� Ÿ���� ���� ���� ����� �� �ִ� ��찡 �ϳ��� ���ٸ�,
                    if (!isPossible)
                    {
                        // �̿��� ���ɼ� ��Ͽ��� �ش� Ÿ���� ����
                        neighborPossibilities.RemoveAt(j);
                    }

                }

                // ���� �̿��� �ĺ� ��Ͽ� ��ȭ�� ����ٸ� (�ĺ��� �پ��ٸ�),
                // �� �̿��� �ٸ� �̿��鿡�� ������ �� �� �����Ƿ� ���ÿ� �߰��Ͽ� ���� ������ ����Ŵ
                if (originalNeighborCount > neighborPossibilities.Count)
                {

                    //�̿� �ĺ� ��� 0�� = ���ٸ� ��. 
                    if (neighborPossibilities.Count == 0)
                    {
                        return false;
                    }


                    stack.Push(neighborCoords);
                }

            }

        }

        return true; //��� ���İ� ���������� ������ ���� ��ȯ

    }


    Cell FindLowestEntropyCell()
    {

        // �ܡ� 3 �ܡ� ���� �� �� �ּ� ��Ʈ���� ���� ���� �� ��ȸ! �ܡ� 3 �ܡ�
        List<Cell> lowestEntropyCells = new List<Cell>();
        int minEntropy = int.MaxValue; //�ּ� ��Ʈ���� ���� �ϴ� ���� ū ���ڷ� �ʱ�ȭ (��?)

        //�׸����� ��� ���� ��ȸ
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Cell currentCell = grid[x, y];

                //���� �ر����� ���� ���� ���
                if (!currentCell.isCollapsed)
                {
                    int entropy = currentCell.possibleTiles.Count;

                    //���� ���� ��Ʈ���ǰ� ���ݱ��� ã�� �ּ� ��Ʈ���Ǻ��� �۴ٸ�
                    if (entropy < minEntropy)
                    {
                        minEntropy = entropy; //�ּ� ��Ʈ���� ���� ����
                        lowestEntropyCells.Clear(); //���� �ĺ��� ���� ����
                        lowestEntropyCells.Add(currentCell); //���� ���� ������ �ĺ��� �߰�
                    }
                    //���� ���� ��Ʈ���ǰ� �ּ� ��Ʈ���ǿ� ���ٸ�
                    else if (entropy == minEntropy)
                    {
                        //���� �ĺ��̹Ƿ� ����Ʈ�� �߰�
                        lowestEntropyCells.Add(currentCell);
                    }
                }

            }
        }

        //���� ���� ��Ʈ���Ǹ� ���� �ĺ� ���̾��ٸ� (��� ���� �ر��Ǿ��ٸ�) null ��ȯ >> �� ���� ����.
        if (lowestEntropyCells.Count == 0)
        {
            return null;
        }

        //�ĺ��� ��������� �� �߿��� �������� �ϳ��� �����ؼ� ��ȯ
        int randomIndex = Random.Range(0, lowestEntropyCells.Count);
        return lowestEntropyCells[randomIndex];


    }


    // 3. ���� Ÿ�� ���� �Լ�
    void InstantiateTiles()
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Cell cell = grid[x, y];
                if (cell.possibleTiles.Count > 0)
                {
                    TileRuleData finalTile = cell.possibleTiles[0]; // ���������� ������ Ÿ�� ��Ģ
                    Vector3 position = new Vector3(x, y, 0); // 2D ���� ��ġ
                    Instantiate(finalTile.tilePrefab, position, Quaternion.identity, this.transform);
                }
            }
        }
    }

    // Ư�� ���� ��ǥ�� ã�� ���� �Լ�
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
        return new Vector2Int(-1, -1); // ��ã���� ���
    }
}
