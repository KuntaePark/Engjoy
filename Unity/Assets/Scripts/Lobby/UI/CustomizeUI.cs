using DataForm;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomizeUI : MonoBehaviour
{
    [SerializeField]
    private RenderTexture renderTexture; //렌더링할 텍스처
    [SerializeField]
    private RawImage characterImage; //렌더링된 이미지를 표시할 UI 요소
    private Camera previewCamera; //캐릭터를 렌더링할 카메라 

    public int currentBodyTypeIndex = 0; // 현재 바디 타입 인덱스
    public int currentWeaponTypeIndex = 0; // 현재 무기 타입 인덱스

    private const int maxBodyTypes = 2; // 최대 바디 타입 수
    private const int maxWeaponTypes = 2; // 최대 무기 타입 수

    [SerializeField]
    private Button nextBodyButton; // 다음 바디 타입 버튼
    [SerializeField]
    private Button prevBodyButton; // 이전 바디 타입 버튼
    [SerializeField]
    private Text bodyTypeText;

    [SerializeField]
    private Button nextWeaponButton;
    [SerializeField]
    private Button prevWeaponButton;
    [SerializeField]
    private Text weaponTypeText;

    [SerializeField]
    private Button saveButton;


    [SerializeField]
    private GameObject characterPrefab;
    private GameObject characterInstance;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false); // 초기에는 UI 비활성화

        CreatePreviewCamera();
        //캐릭터 오브젝트 생성, 임시로 기본 바디 타입 및 기본 무기
        characterInstance = Instantiate(characterPrefab, new Vector3(0, -1000.5f, 0), Quaternion.identity);
        var characterRenderer = characterInstance.GetComponent<CharacterRenderer>();

        nextBodyButton.onClick.AddListener(() =>
        {
            currentBodyTypeIndex = (currentBodyTypeIndex + 1) % maxBodyTypes; // 다음 바디 타입으로 변경
            changeBodyType();
        });

        prevBodyButton.onClick.AddListener(() =>
        {
            currentBodyTypeIndex = (currentBodyTypeIndex - 1 + maxBodyTypes) % maxBodyTypes; // 이전 바디 타입으로 변경
            changeBodyType();
        });

        nextWeaponButton.onClick.AddListener(() =>
        {
            currentWeaponTypeIndex = (currentWeaponTypeIndex + 1) % maxWeaponTypes; // 다음 무기 타입으로 변경
            changeWeaponType();
        });

        prevWeaponButton.onClick.AddListener(() =>
        {
            currentWeaponTypeIndex = (currentWeaponTypeIndex - 1 + maxWeaponTypes) % maxWeaponTypes; // 이전 무기 타입으로 변경
            changeWeaponType();
        });

        saveButton.onClick.AddListener(() =>
        {
            //수정된 커스터마이징 저장 요청
            StartCoroutine(DataManager.Instance.saveCustomization(currentBodyTypeIndex, currentWeaponTypeIndex, (userGameData) =>
            {
                if (userGameData != null)
                {
                    Debug.Log("커스터마이징 저장 성공: " + userGameData.bodyTypeIndex + ", " + userGameData.weaponTypeIndex);
                }
                else
                {
                    Debug.LogError("커스터마이징 저장 실패");
                }
            }));
        });
    }

    public void showCustomizeUI()
    {
        gameObject.SetActive(true); // UI 활성화
        changeBodyType(); // 초기 바디 타입 설정
        changeWeaponType(); // 초기 무기 타입 설정
    }

    private void changeBodyType()
    {
        characterInstance.GetComponent<CharacterRenderer>().SetBody(currentBodyTypeIndex);
        SetLayerRecursively(characterInstance, LayerMask.NameToLayer("Preview")); // 자식 오브젝트도 레이어 설정
        SetSortingLayerRecursively(characterInstance, "Preview", 0); // "Preview" 레이어의 정렬 레이어 설정
        bodyTypeText.text = currentBodyTypeIndex + "번 프리셋";
    }

    private void changeWeaponType()
    {
        characterInstance.GetComponent<CharacterRenderer>().SetWeapon(currentWeaponTypeIndex);
        SetLayerRecursively(characterInstance, LayerMask.NameToLayer("Preview")); // 자식 오브젝트도 레이어 설정
        SetSortingLayerRecursively(characterInstance, "Preview", 0); // "Preview" 레이어의 정렬 레이어 설정
        weaponTypeText.text = currentWeaponTypeIndex + "번 프리셋";
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            gameObject.SetActive(false); // ESC 키로 UI 닫기
        }
    }

    void CreatePreviewCamera()
    {
        // 카메라 GameObject 생성
        GameObject camObj = new GameObject("CharacterPreviewCamera");
        previewCamera = camObj.AddComponent<Camera>();

        // RenderTexture에 렌더링되도록 설정
        previewCamera.targetTexture = renderTexture;
        previewCamera.clearFlags = CameraClearFlags.Color;
        previewCamera.backgroundColor = new Color(0, 0, 0, 0); // 투명 배경
        previewCamera.orthographic = true;
        previewCamera.orthographicSize = 1.5f;
        
        previewCamera.cullingMask = LayerMask.GetMask("Preview"); // "Preview" 레이어만 렌더링

        // 씬 내에서 게임 월드와 겹치지 않게 위치 지정
        previewCamera.transform.position = new Vector3(0, -1000, -10);
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    void SetSortingLayerRecursively(GameObject obj, string layerName, int order = 0)
    {
        foreach (var sr in obj.GetComponentsInChildren<SpriteRenderer>(true))
        {
            sr.sortingLayerName = layerName;
            sr.sortingOrder = order;
        }
    }

}
