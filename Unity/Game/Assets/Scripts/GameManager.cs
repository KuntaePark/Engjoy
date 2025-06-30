using System.Collections;
using System.Collections.Generic;
using DataForm;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //�ٸ� ��ũ��Ʈ���� GameManager�� ������ �� �ֵ��� �̱��� ���� ���
    public static GameManager Instance { get; private set; }

    public PlayerManager playerManager;
    public KeywordManager keywordManager;

    public string MyPlayerId {  get; private set; }

    private void Awake()
    {
        //�ν��Ͻ� ����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); //���� �ٲ� �ı����� �ʰ� ����!
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        //player, keyword�� GameManager�� ����!
        playerManager = FindObjectOfType<PlayerManager>();
        keywordManager = FindObjectOfType<KeywordManager>();
    }

    //WSClient�� ȣ��� �Լ� : �÷��̾� ID ����
    public void SetMyPlayerId(string id)
    {
        MyPlayerId = id;
        Debug.Log($"<color=green>GameManager: My ID is set to {MyPlayerId}</color>");
    }

    //Game ���� �ִ� �÷��̾�� Ű���忡�� �۾� �й�
    public void UpdateGameState(GameState newState)
    {
        //KeywordManager���� Ű���� �����͸� �Ѱ��ֱ�
        if(keywordManager != null)
        {
            keywordManager.UpdateKeywords(newState.keywords);
        }
        //PlayerManager���� �÷��̾� ������ �Ѱ��ֱ�
        if(playerManager != null)
        {
            playerManager.UpdatePlayers(newState.players);
        }

    }

}
