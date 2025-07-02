using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DataForm;

public class MonsterManager : MonoBehaviour
{
    [SerializeField] 
    private GameObject runnerPrefab; //몬스터-러너 프리팹 세팅
    [SerializeField]
    private GameObject chaserPrefab; //몬스터-체이서 프리팹 세팅 

    private Dictionary<string, MonsterController> monsters = new Dictionary<string, MonsterController>();

    public void UpdateMonsters(Dictionary<string, MonsterData> serverMonsters)
    {
        //서버에 있는데 클라이언트에 없는 몬스터는 새로 스폰)
        foreach (var monsterData in serverMonsters)
        {
            if (!monsters.ContainsKey(monsterData.Key))
            {
                string monsterType = monsterData.Value.type;
                GameObject prefabToSpawn = null;

                switch (monsterType)
                {
                    case "runner":
                        prefabToSpawn = runnerPrefab;
                        break;
                    case "chaser":
                        prefabToSpawn = chaserPrefab;
                        break;
                    default:
                        Debug.LogWarning($"Prefab for monster type '{monsterType} doesn't exist.");
                        break;
                }


                //새로운 몬스터 생성
                if(prefabToSpawn != null)
                {
                GameObject newMonsterObj = Instantiate(prefabToSpawn, new Vector3(monsterData.Value.x, monsterData.Value.y, 0), Quaternion.identity);
                MonsterController newMonsterController = newMonsterObj.GetComponent<MonsterController>();
                newMonsterController.Initialize(monsterData.Key, monsterData.Value);
                monsters.Add(monsterData.Key, newMonsterController);
                }
            }
        }

        //서버 데이터 기준으로 상태 업데이트
        foreach (var monster in monsters)
        {
            if(serverMonsters.ContainsKey(monster.Key))
            {
                monster.Value.UpdateState(serverMonsters[monster.Key]);
            }
        }

        //클라이언트에 있는데 서버에 없는 몬스터 삭제
        List<string> idsToRemove = monsters.Keys.Where (id => !serverMonsters.ContainsKey (id)).ToList();
        foreach (string id in idsToRemove)
        {
            if(monsters.TryGetValue(id, out MonsterController monsterToDestroy))
            {
                Destroy(monsterToDestroy.gameObject);
                monsters.Remove(id);
            }
        }

    }
}
