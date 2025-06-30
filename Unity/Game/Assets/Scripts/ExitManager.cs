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

    //WSClient -> GameManager 를 통해 호출될 메인 함수

        //게임(씬)에 출구가 없는 경우 -> 새로 생성

        //ExitController 초기화하기

        //ExitController 상태 업데이트 (키워드가 몇 개 맞춰졌는지, 열려 있는지)
}
