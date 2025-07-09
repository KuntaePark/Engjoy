using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using DataForm;

public class DataManager : MonoBehaviour
{
    public class ItemData
    {
        public int itemCount;
        public Item item;
    }

    public long id = -1;

    private BrowserRequest browserRequest = new BrowserRequest();

    private UserGameData userGameData = null;
    //마지막 로드로부터 지난 시간
    private float lastLoadTime = 0.0f;
    private float loadInterval = 300.0f; // 5 minutes in seconds

    public static DataManager Instance { get; private set; }

    //inventory data of user
    public List<ItemData> inventory = new List<ItemData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
#if UNITY_EDITOR
            id = 0; // Set a default ID for testing in the editor
#endif
            DontDestroyOnLoad(gameObject); // Keep this instance across scenes
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //should be loaded after server connection, but for testing purposes, we load it here
        loadInventory();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public IEnumerator getUserData(Action<UserGameData> OnResult)
    {
        if(Instance.userGameData == null || (Time.time - lastLoadTime) > loadInterval)
        {
            int requestId = Instance.browserRequest.StartRequest("GET", "/game/user/data");
            yield return StartCoroutine(Instance.browserRequest.waitForResponse(requestId, 5.0f, (response) => {
                if(response != null)
                {
                    Instance.userGameData = JsonConvert.DeserializeObject<UserGameData>(response.body);
                    Debug.Log($"User data loaded: {Instance.userGameData.nickname}");
                    lastLoadTime = Time.time; // Update last load time
                    OnResult?.Invoke(Instance.userGameData);
                }
                else
                {
                    Debug.LogError("Failed to load user data.");
                    OnResult?.Invoke(null); // Return null if failed
                }
            }));

        }
        else
        {
            OnResult?.Invoke(Instance.userGameData); // Return cached data if available
        }
    }


    //load inventory data of user
    public void loadInventory()
    {
        //load inventory data from server
        //temorarily load from resources for testing
        /*
         * JSON format:
         * {
         *   "item_potion_001": 5,
         *   "item_sword_001": 2
         * }
         */
        TextAsset jsonData = Resources.Load<TextAsset>("inventory"); //testing
        if(jsonData == null)
        {
            Debug.LogError("Inventory data not found in Resources folder.");
            return;
        }
        Dictionary<string, int> inventoryData = JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonData.text);
        foreach(var kv in inventoryData)
        { 
            Debug.Log($"Item ID: {kv.Key}, Count: {kv.Value}");
            Item item = LoadItemData(kv.Key);
            if (item != null)
            {
                ItemData itemData = new ItemData
                {
                    itemCount = kv.Value,
                    item = item
                };
                this.inventory.Add(itemData);
            }
            else
            {
                Debug.LogWarning($"Item with ID {kv.Key} not found in resources.");
            }
        }

    }

    public Item LoadItemData(string itemId)
    {
        return Resources.Load<Item>($"ItemData/{itemId}");
    }
}
