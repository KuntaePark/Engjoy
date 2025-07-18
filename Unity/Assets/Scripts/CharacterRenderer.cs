using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 캐릭터 이미지 골격. 몸통 및 무기 스위칭 가능
 */

public class CharacterRenderer : MonoBehaviour
{
    //바디 위치
    [SerializeField]
    private GameObject BodyPlacer;
    //무기 위치
    [SerializeField]
    private GameObject WeaponPlacer;

    public Animator bodyAnimator { get; private set; }
    public Animator weaponAnimator { get; private set; }

    public int bodyTypeIndex = 0; //몸통 타입 인덱스
    public int weaponTypeIndex = 0; //무기 타입 인덱스

    public GameObject bodyInstance;
    public GameObject weaponInstance;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetBody(int bodyTypeIndex)
    {
        string bodyType = "body_type_" + (bodyTypeIndex + 1).ToString("D3");
        GameObject bodyPrefab = Resources.Load<GameObject>("Character/Body/" + bodyType);
        if (bodyPrefab == null)
        {
            Debug.LogError("Body prefab not found: " + bodyType);
            return;
        }
        else
        {
            if (bodyAnimator != null)
            {
                Destroy(bodyAnimator.gameObject);
            }
            Debug.Log("Loading body prefab: " + bodyType);
            this.bodyTypeIndex = bodyTypeIndex; // Update body type index
            GameObject bodyObject = Instantiate(bodyPrefab, Vector3.zero, Quaternion.identity);
            bodyObject.transform.SetParent(BodyPlacer.transform, false);
            bodyInstance = bodyObject;
            bodyAnimator = bodyObject.GetComponent<Animator>();
        }
    }

    public void SetWeapon(int weaponTypeIndex)
    {
        string weaponType = "weapon_type_" + (weaponTypeIndex + 1).ToString("D3");
        GameObject weaponPrefab = Resources.Load<GameObject>("Character/Weapon/" + weaponType);
        if (weaponPrefab == null)
        {
            Debug.LogError("Weapon prefab not found: " + weaponType);
            return;
        }
        else
        {
            if (weaponAnimator != null)
            {
                Destroy(weaponAnimator.gameObject);
            }
            Debug.Log("Loading weapon prefab: " + weaponType);
            this.weaponTypeIndex = weaponTypeIndex; // Update weapon type index
            GameObject weaponObject = Instantiate(weaponPrefab, Vector3.zero, Quaternion.identity);
            weaponObject.transform.SetParent(WeaponPlacer.transform, false);
            weaponInstance = weaponObject;
            weaponAnimator = weaponObject.GetComponent<Animator>();
        }
    }
}
