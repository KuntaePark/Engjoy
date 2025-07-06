using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    public Image IconImage;
    public Text ItemCount;

    public int ItemIndex { get; set; } = -1; // Index of the item in the inventory
    public InventoryUI Owner { get; set; }

    private void Start()
    {
        IconImage.sprite = null; // Initialize the icon image to null
        IconImage.enabled = false; // Hide the icon image initially
        ItemCount.text = ""; // Initialize the item count text to empty
    }

    public void loadItem(DataManager.ItemData itemData, int itemIndex)
    {
        ItemIndex = itemIndex; // Set the item index
        IconImage.sprite = itemData.item.itemImage;
        ItemCount.text = itemData.itemCount.ToString();
        IconImage.enabled = true; // Show the icon image
    }

    public void clearSlot()
    {
        ItemIndex = -1; // Reset the item index
        IconImage.sprite = null; // Clear the icon image
        ItemCount.text = ""; // Clear the item count text
        IconImage.enabled = false; // Hide the icon image
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(ItemIndex == -1)
        {
            // No item in this slot, do nothing
            return;
        }
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Handle left click on the item slot
            Debug.Log("Left click on item slot: " + gameObject.name);
            //왼 클릭 시 아이템 설명 표시
            Owner.ShowItemDescription(ItemIndex);

        }
       
    }
}
