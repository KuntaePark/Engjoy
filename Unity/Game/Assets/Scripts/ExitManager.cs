using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitManager : MonoBehaviour
{
    public GameObject exitPrefab;
    private Dictionary<string, GameObject> exitObject = new Dictionary<string, GameObject>();

    private ExitManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    //WSClient -> GameManager �� ���� ȣ��� ���� �Լ�

        //����(��)�� �ⱸ�� ���� ��� -> ���� ����

        //ExitController �ʱ�ȭ�ϱ�

        //ExitController ���� ������Ʈ (Ű���尡 �� �� ����������, ���� �ִ���)
}
