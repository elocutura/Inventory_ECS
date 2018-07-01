using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {


    public InventoryData inventory;

    [Header("Item to Add")]
    public Item itemToAdd = null;

    [Header("Remove Values")]
    public uint removeItemFromSlot = 0;
    public uint removeItemQuantity = 1;

    [Header("Move Values")]
    public uint moveItemSlot = 0;
    public uint moveItemQuantity = 1;
    public uint moveToSpot = 0;


    public void AddItem()
    { 
        if (itemToAdd != null)
        {
            Item i = Item.CreateInstance<Item>();

            i.itemID = itemToAdd.itemID;
            i.quantity = itemToAdd.quantity;
            i.maxQuantity = itemToAdd.maxQuantity;
            i.itemIcon = itemToAdd.itemIcon;
            i.itemName = itemToAdd.itemName;


            InventoryAction act = new InventoryAction();
            act._action = InventoryAction.action.Deposit;
            act._item = i;

            inventory.pendingActions.Add(act);
            InventoryAction.AskForInventoryActionRequest();
        }
    }

    public void DeleteItem()
    {
        Item i = Item.CreateInstance<Item>();
        i.itemInventorySlot = removeItemFromSlot;
        i.quantity = removeItemQuantity;

        InventoryAction act = new InventoryAction();
        act._action = InventoryAction.action.DeleteBySlot;
        act._item = i;

        inventory.pendingActions.Add(act);
        InventoryAction.AskForInventoryActionRequest();
    }

    public void MoveItem()
    {
        Item i = Item.CreateInstance<Item>();
        i.itemInventorySlot = moveItemSlot;
        i.quantity = moveItemQuantity;

        InventoryAction act = new InventoryAction();
        act._action = InventoryAction.action.MoveBySlot;
        act._item = i;
        act.moveTo = moveToSpot;

        inventory.pendingActions.Add(act);
        InventoryAction.AskForInventoryActionRequest();
    }

}
