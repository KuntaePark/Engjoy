using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class DataManager : MonoBehaviour
{
    public class ItemData
    {
        public int itemCount;
        public Item item;
    }

    public long id = -1;

    public static DataManager Instance { get; private set; }

    //inventory data of user
    public List<ItemData> inventory = new List<ItemData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
