using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using DataForm;
public class PlayerController : MonoBehaviour
{
    public LobbyClient lobbyClient;

    public CharacterRenderer characterRenderer;

    private long id; 
    public long Id { get { return id; } set { id = value; } }

    private float axisH;
    private float axisV;

    public float speed = 5.0f; //make sure it is synchronized with the server
    public Boolean isTestDummy = false;

    //testing value
    private float[] Hinputs = new float[2] { -1.0f, 1.0f };
    private float[] Vinputs = new float[2] { -1.0f, 1.0f };
    public float maxMoveTime = 0.5f;
    public float moveTime = 0;
    public float breakTime = 1.0f;
    public float currentTime = 0;

    public string curState = "move";

    // Start is called before the first frame update
    void Start()
    {
        lobbyClient = GameObject.Find("LobbyClient").GetComponent<LobbyClient>();
        moveTime = UnityEngine.Random.Range(0.1f, maxMoveTime);
        if (Id == DataManager.Instance.id)
        {
            //해당 플레이어의 정보 요청
            StartCoroutine(DataManager.Instance.getUserData((UserGameData data) =>
            {
                if (data != null)
                {
                    Debug.Log("loading mine.");
                    //유저 데이터에서 바디 타입과 무기 타입을 가져옴
                    characterRenderer.SetBody(data.bodyTypeIndex);
                    characterRenderer.SetWeapon(data.weaponTypeIndex);
                }
                else
                {
                    Debug.LogError("Failed to load user data for customization.");
                }
            }));
        }

    }

    // Update is called once per frame
    void Update()
    {
        //send input to server, only if this player object is controlled by the local player
        if (Id == DataManager.Instance.id)
        {
            if(Input.GetKeyDown("space"))
            {
                isTestDummy = !isTestDummy;
            }
            
            if(Input.GetKeyDown("z"))
            {
                //interact request
                Debug.Log("interact");
                characterRenderer.weaponAnimator.SetTrigger("Swing");
                lobbyClient.Send("input_interact", "");
            }

            if(!isTestDummy)
            {
                axisH = Input.GetAxisRaw("Horizontal");
                axisV = Input.GetAxisRaw("Vertical");
            }
            else
            {
                dummyMovement();
            }

            if (axisH != 0 || axisV != 0)
            {
                lobbyClient.Send("input_move", JsonConvert.SerializeObject(new PlayerStateData(axisH, axisV)));
            }

            if (axisH != 0)
            {
                transform.localScale = new Vector3(Mathf.Sign(axisH), 1, 1);
            }


            if (axisH != 0.0f || axisV != 0.0f)
            {
                characterRenderer.bodyAnimator.SetBool("isRunning", true);
            }
            else
            {
                characterRenderer.bodyAnimator.SetBool("isRunning", false);
            }
        }

        
        
    }

    private void FixedUpdate()
    {
        transform.Translate(new Vector3(axisH, axisV, 0) * speed * Time.deltaTime);
    }

    private void dummyMovement()
    {
        switch (curState)
        {
            case "break":
                if (currentTime < breakTime)
                {
                    currentTime += Time.deltaTime;
                    axisH = 0;
                    axisV = 0;
                }
                else
                {
                    int hIndex = UnityEngine.Random.Range(0, Hinputs.Length);
                    int vIndex = UnityEngine.Random.Range(0, Vinputs.Length);
                    axisH = Hinputs[hIndex];
                    axisV = Vinputs[vIndex];
                    curState = "move";
                    currentTime = 0;
                    moveTime = UnityEngine.Random.Range(0.1f, maxMoveTime);
                }
                break;
            case "move":
                if (currentTime < moveTime)
                {
                    currentTime += Time.deltaTime;
                }
                else
                {
                    currentTime = 0;
                    curState = "break";
                }
                break;

        }
    }

}
