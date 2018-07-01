using Unity.Entities;
using UnityEngine;
using System.Collections.Generic;

public class UI_InventorySys : ComponentSystem
{

    private struct Filter
    {
        public InventoryData invData;
    }

    public GameObject UI_ItemPrefab;

    protected override void OnStartRunning()
    {

        base.OnStartRunning();

        foreach (var entity in GetEntities<Filter>())   // Setup all inventories
        {
            InventoryData invData = entity.invData;

            for (int i = 0; i < invData.inventoryUIGrid.Length; i++)
            {
                invData.inventoryUIGrid[i].slotIndex = i;
            }

            CreateInventoryUI(invData);
            if (invData.inventoryListText != null)
                invData.inventoryListText.text = ToText(invData);
        }

        InventorySystem.Ui_UpdateRequest += UpdateUI;
    }

    protected override void OnStopRunning()
    {
        base.OnStopRunning();

        InventorySystem.Ui_UpdateRequest -= UpdateUI;
    }

    protected override void OnUpdate()
    {

    }

    private void UpdateUI(InventoryData invData)
    {
        CreateInventoryUI(invData);
        if (invData.inventoryListText != null)
            invData.inventoryListText.text = ToText(invData);
    }

    private void CreateInventoryUI(InventoryData invData)
    {
        foreach (UI_Slot slot in invData.inventoryUIGrid)
        {
            slot.itemHolding = null;
        }

        foreach (Item i in invData.inventoryItems)
        {
            UI_Slot itemSlot = invData.inventoryUIGrid[i.itemInventorySlot];
            itemSlot.itemHolding = i;
            itemSlot.slotImage.sprite = i.itemIcon;
            itemSlot.slotImage.color = new Color(1, 1, 1, 1);

            if (i.maxQuantity <= 1) // If the item is not a stack, it doesnt need a stack number
            {
                itemSlot.stackText.text = "";
            }
            else
            {
                itemSlot.stackText.text = i.quantity.ToString();
            }
        }

        foreach (UI_Slot slot in invData.inventoryUIGrid)
        {
            if (slot.itemHolding == null)
            {
                slot.slotImage.sprite = null;
                slot.slotImage.color = new Color(1,1,1,0);
                slot.stackText.text = "";
            }
        }
    }


    private string ToText(InventoryData invData) // Simple toString() from for our inventory
    {

        string toReturn = "";

        toReturn += "Number of items in the list: " + invData.inventoryItems.Count + "\n";

        foreach (Item ii in invData.inventoryItems)
        {
            toReturn += "ItemID: " + ii.itemID + " - Quantity: " + ii.quantity + "/" + ii.maxQuantity + " - On Slot: " + ii.itemInventorySlot + "\n";
        }

        return toReturn;
    }
}
