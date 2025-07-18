using DataForm;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    //아이템 메뉴
    public GameObject InventoryMenu;
    private bool menuActivated;
    
    //아이템 설명란
    public InventoryDesc ItemDesc;

    public ItemSlot[] itemSlots; // Array to hold item slots
    public int selected = -1;
    

    // Start is called before the first frame update
    void Start()
    {
        foreach(var slot in itemSlots)
        {
            slot.Owner = this; // Set the owner of each item slot to this InventoryUI instance
        }

        InventoryMenu.SetActive(false); // Ensure the inventory menu is hidden at the start   
        menuActivated = false; // Initialize the menu state
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Inventory") && !menuActivated)
        {
            // Toggle the inventory menu
            Debug.Log("Inventory open");
            loadInventory();
            InventoryMenu.SetActive(true);
            menuActivated = true;
        }
        else if(Input.GetButtonDown("Inventory") && menuActivated)
        {
            // Close the inventory menu
            InventoryMenu.SetActive(false);
            menuActivated = false;
            clearInventory();
        }
    }

    public void loadInventory()
    {
        List<DataManager.ItemData> inventory = DataManager.Instance.inventory; // Get the inventory data from DataManager
        for (int i = 0; i < inventory.Count; i++)
        {
            DataManager.ItemData itemData = inventory[i]; // Get the item data
            // Find an empty slot to load the item
            foreach (var slot in itemSlots)
            {
                if(slot.ItemIndex == -1) // Check if the slot is empty
                {
                    slot.loadItem(itemData, i);
                    break; // Exit the loop once the item is loaded
                }
            }
        }
    }

    public void clearInventory()
    {
        foreach(var slot in itemSlots)
        {
            slot.clearSlot(); // Clear each item slot
        }
        ItemDesc.clearItemDesc(); // Clear the item description
    }

    public void ShowItemDescription(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= DataManager.Instance.inventory.Count)
        {
            Debug.LogError("Invalid item index: " + itemIndex);
            return;
        }
        DataManager.ItemData itemData = DataManager.Instance.inventory[itemIndex];
        // Display the item description in the UI (you can implement this as needed)
        ItemDesc.showItemDesc(itemData);
        
    }
}

