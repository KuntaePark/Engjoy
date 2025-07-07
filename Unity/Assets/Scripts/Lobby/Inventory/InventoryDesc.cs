using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryDesc : MonoBehaviour
{
    public Image ItemImage; // 아이템 이미지
    public Text ItemName; // 아이템 이름 텍스트
    public Text ItemDesc; // 아이템 설명 텍스트


    // Start is called before the first frame update
    void Start()
    {
        ItemImage.sprite = null; // Initialize the item image to null
        ItemImage.enabled = false; // Hide the item image initially
        ItemName.text = ""; // Initialize the item name text to empty
        ItemDesc.text = ""; // Initialize the item description text to empty
    }

    public void showItemDesc(DataManager.ItemData itemData)
    {
        ItemImage.sprite = itemData.item.itemImage; // Set the item image
        ItemImage.enabled = true; // Show the item image
        ItemName.text = itemData.item.itemName; // Set the item name text
        ItemDesc.text = itemData.item.itemDescription; // Set the item description text
    }

    public void clearItemDesc()
    {
        ItemImage.sprite = null; // Clear the item image
        ItemImage.enabled = false; // Hide the item image
        ItemDesc.text = ""; // Clear the item description text
        ItemName.text = ""; // Clear the item name text
    }
}
