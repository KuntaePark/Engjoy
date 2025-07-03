using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DataForm;

public class MonsterManager : MonoBehaviour
{
    [SerializeField] 
    private GameObject runnerPrefab; //����-���� ������ ����
    [SerializeField]
    private GameObject chaserPrefab; //����-ü�̼� ������ ���� 

    private Dictionary<string, MonsterController> monsters = new Dictionary<string, MonsterController>();

    public void UpdateMonsters(Dictionary<string, MonsterData> serverMonsters)
    {
        //������ �ִµ� Ŭ���̾�Ʈ�� ���� ���ʹ� ���� ����)
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


                //���ο� ���� ����
                if(prefabToSpawn != null)
                {
                GameObject newMonsterObj = Instantiate(prefabToSpawn, new Vector3(monsterData.Value.x, monsterData.Value.y, 0), Quaternion.identity);
                MonsterController newMonsterController = newMonsterObj.GetComponent<MonsterController>();
                newMonsterController.Initialize(monsterData.Key, monsterData.Value);
                monsters.Add(monsterData.Key, newMonsterController);
                }
            }
        }

        //���� ������ �������� ���� ������Ʈ
        foreach (var monster in monsters)
        {
            if(serverMonsters.ContainsKey(monster.Key))
            {
                monster.Value.UpdateState(serverMonsters[monster.Key]);
            }
        }

        //Ŭ���̾�Ʈ�� �ִµ� ������ ���� ���� ����
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
