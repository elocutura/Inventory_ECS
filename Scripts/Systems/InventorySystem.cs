using Unity.Entities;
using UnityEngine;
using System.Collections.Generic;

public class InventorySystem : ComponentSystem {

    private struct Filter
    {
        public InventoryData invData;
    }

    public delegate void UI_UpdateRequest(InventoryData invData);
    public static event UI_UpdateRequest Ui_UpdateRequest;

    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        InventoryAction.OnInventoryActionRequest += CheckInventories;

        CheckInventories();
    }

    protected override void OnStopRunning()
    {
        base.OnStopRunning();
        InventoryAction.OnInventoryActionRequest -= CheckInventories;

    }

    protected override void OnUpdate()
    {
    }

    //For the event trigger
    protected void CheckInventories()
    {
        foreach (var entity in GetEntities<Filter>())
        {
            InventoryData invData = entity.invData;
            if (invData.pendingActions.Count >= 1)
            {
                foreach (InventoryAction action in invData.pendingActions.ToArray())
                {
                    if (action._action == InventoryAction.action.Deposit)
                    {
                        Deposit(invData, action);
                    }
                    else if (action._action == InventoryAction.action.Delete)
                    {
                        Delete(invData, action);
                    }
                    else if (action._action == InventoryAction.action.DeleteBySlot)
                    {
                        DeleteBySlot(invData, action);
                    }
                    else if (action._action == InventoryAction.action.Move)
                    {
                        Move(invData, action);
                    }
                    else if (action._action == InventoryAction.action.MoveBySlot)
                    {
                        MoveBySlot(invData, action);
                    }
                    else if (action._action == InventoryAction.action.MoveFromInventory)
                    {
                        MoveFromInventory(invData, action);
                    }
                    invData.pendingActions.Remove(action);
                }
            }
            OnInventoryChanged(invData);
        }
    }

    protected bool MoveFromInventory(InventoryData invData, InventoryAction action)
    {
        uint oldItemInventorySlot = action._item.itemInventorySlot; // Get the item slot for the inventory we are moving the item from
        action._item.itemInventorySlot = action.moveTo; // Assign the slot we want to put in the new item for the Deposit function

        if (Deposit(invData, action)) // If the deposit on this inventory was successful, remove item from the old inventory
        {
            action.oldInventory.inventoryItems.Remove(FindItemInSlot(action.oldInventory, oldItemInventorySlot)); // If the deposit on the new inventory was successful, remove item from the old inventory
            OnInventoryChanged(action.oldInventory); // Update the old inventory to display the removal of that item
            return true;
        }
        else
        {
            return false;
        }
    }

    protected bool Deposit(InventoryData invData, InventoryAction action)
    {
        if (action._item.quantity <= 0) // If we are trying to deposit (0) items, dont do anything
            return false;

        if (!IsStack(action._item) || invData.inventoryItems.Count <= 0) // if its not a stack or the inventory is empty, add this item/stack to the inventory
        {
            if (invData.inventoryItems.Count >= invData.maxInventorySlots) // If the inventory is full, dont add anything to it
                return false;

            if (IsSlotTaken(invData, action._item.itemInventorySlot) || !IsRightType (invData, action._item.itemInventorySlot, action._item.itemType)) // If the slot we are trying to add this item to is occupied or its not the right type, assign a new one
                action._item.itemInventorySlot = AssignSlot(invData, action._item.itemType);

            if (action._item.itemInventorySlot >= invData.maxInventorySlots) // If the previous call of AssignSlot gives us >= than the max inventory slots in the current inventory, there is no available slot for this item
                return false;

            invData.inventoryItems.Add(action._item);
            return true;
        }
        else // Else its a stack and we have at least one item in the inventory
        {
            if (action._item.itemInventorySlot != 0 && !IsSlotTaken(invData, action._item.itemInventorySlot) && IsRightType(invData, action._item.itemInventorySlot, action._item.itemType)) // If we want to place the stack in an empty slot that's not the default one (0) force it there
            {
                invData.inventoryItems.Add(action._item);
                return true;
            }
            else if (IsRightType (invData, action._item.itemInventorySlot, action._item.itemType))
            {
                invData.inventoryItems = stackDeposit(0, invData, action._item);
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    private List<Item> stackDeposit(int index, InventoryData invData, Item toDeposit)
        {
        List<Item> itemList = invData.inventoryItems;
        if (index == itemList.Count) // If its the last position on the list and haven't found suitable stack to add, add a new stack to inventory
        {
            if (invData.inventoryItems.Count >= invData.maxInventorySlots) // If the inventory is full, dont add anything to it
                return itemList;

            if (IsSlotTaken(invData, toDeposit.itemInventorySlot)) // If the slot we are trying to add this item to is occupied, assign a new one
                toDeposit.itemInventorySlot = AssignSlot(invData, toDeposit.itemType);

            itemList.Add(toDeposit);
            return itemList;
        }

        if (itemList[index].itemID == toDeposit.itemID) // If its the same type of item
        {
            if (itemList[index].maxQuantity - itemList[index].quantity >= toDeposit.quantity) // If this stack of the same item has enough room to absorb the new stack, add it and return
            {
                itemList[index].quantity += toDeposit.quantity;
                return itemList;
            }
            else // if this stack doesnt have enough room to absorb the new stack, cap out current stack and keep looking for further items of the same type
            {
                toDeposit.quantity -= (itemList[index].maxQuantity - itemList[index].quantity); // Deduce the amount that will be filled until the current stack is capped
                itemList[index].quantity = itemList[index].maxQuantity; // Cap this stack

                return stackDeposit(index + 1, invData, toDeposit); // Continue with the new toDeposit quantity that there is left to stack
            }
        }
        else // If its not an item of the same type, check for the next item
        {
            return stackDeposit(index + 1, invData, toDeposit);
        }
    }

    protected Item Delete(InventoryData invData, InventoryAction action) // Delete the specific item stack by reference provided by the action 
    {
        if (action._item.quantity <= 0) // If we are trying to delete (0) items, return null
            return null;

        if (invData.inventoryItems.Contains(action._item))
        {
            invData.inventoryItems.Remove(action._item);
            return action._item;
        }
        else
        {
            return action._item;
        }
    }
    protected Item DeleteBySlot(InventoryData invData, InventoryAction action)
    {
        if (action._item.quantity <= 0) // If we are trying to delete (0) items, return null
            return null;

        Item itemInSlot = FindItemInSlot(invData, action._item.itemInventorySlot); // Find the item in selected slot to delete

        if (itemInSlot != null) // If there is an item in such slot, delete it
        {
            if (itemInSlot.quantity > action._item.quantity) // If the stack can support only destroying the amount of items provided, do so
            {
                itemInSlot.quantity -= action._item.quantity;
            }
            else // If you want to remove the same or more items from that stack pile, destroy that stack pile
            {
                invData.inventoryItems.Remove(itemInSlot);
            }
            return itemInSlot;
        }
        else
        {
            return null; // Nothing was on that slot and nothing could be deleted
        }
    }

    protected bool Move(InventoryData invData, InventoryAction action) // Move the specific stack by reference provided by the action
    {
        if (action._item.quantity <= 0) // If we are trying to move (0) items, dont do anything
            return false;

        if (action.moveTo >= invData.maxInventorySlots || !IsRightType(invData, action.moveTo, action._item.itemType)) // Its trying to move an item to an unavailable slot in the inventory
            return false;

        if (!IsSlotTaken(invData, action.moveTo)) // If the inventory slot we are trying to move to is empty, move instantly
        {
            action._item.itemInventorySlot = action.moveTo;

            return true;
        }
        else
        {
            Item itemInMoveToSlot = FindItemInSlot(invData, action.moveTo); // Get item that is in the slot we want to move the current item

            itemInMoveToSlot.itemInventorySlot = action._item.itemInventorySlot; // Assign to this item the slot we have with the item we are moving
            action._item.itemInventorySlot = action.moveTo; // Assign the item we are moving to the now free slot, swapping spots with the above item

            return true;
        }
    }
    protected bool MoveBySlot(InventoryData invData, InventoryAction action)
    {
        if (action._item.quantity <= 0) // If we are trying to move (0) items, dont do anything
            return false;

        Item itemInSlot = FindItemInSlot(invData, action._item.itemInventorySlot); // Find the item in selected slot that we want to move
        Item itemToMoveTo = FindItemInSlot(invData, action.moveTo);

        if (action.moveTo >= invData.maxInventorySlots || !IsRightType(invData, action.moveTo, itemInSlot.itemType) || itemInSlot == null) // Its trying to move an item to an unavailable slot in the inventory or the slot selected is empty do nothing
            return false;

        if (itemToMoveTo == null) // If the inventory slot we are trying to move to is empty, move instantly the amount provided by the action
        {
            if (itemInSlot.quantity > action._item.quantity) // If we want to move a portion of the stack to an empty spot, just move that portion to allow, unStacking of items
            {
                Item newStack = itemInSlot.CreateCopy(); // Create a new Item regarding the new stack with the same parameters

                itemInSlot.quantity -= action._item.quantity; // Deduce the amount that will be transfered from the current stack
                newStack.quantity = action._item.quantity; // Assign new parameter of quantity to match en unstacked quantity from the original stack
                newStack.itemInventorySlot = action.moveTo; // Assign the new inventory slot for the new stack

                invData.inventoryItems.Add(newStack); // Deposit the newly created stack with the new quantity to the empty slot
            }
            else
            {
                itemInSlot.itemInventorySlot = action.moveTo;
            }
            return true;
        }
        else
        {
            if (itemToMoveTo == itemInSlot) // If we are trying to move the item into the same slot it is, do nothing
                return false;

            if (itemToMoveTo.itemID == itemInSlot.itemID) // If the slot already has a stack of the same type of this one, move the maximum amount possible to that stack
            {
                if (itemToMoveTo.maxQuantity - itemToMoveTo.quantity >= action._item.quantity && itemInSlot.quantity > action._item.quantity) // If the stack has enough room for the full quantity provided, move all the provided quantity on top of this one
                {
                    itemToMoveTo.quantity += action._item.quantity;
                    itemInSlot.quantity -= action._item.quantity;
                }
                else if (itemToMoveTo.maxQuantity - itemToMoveTo.quantity >= action._item.quantity && itemInSlot.quantity <= action._item.quantity) // If the stack has enough room and we are trying to move the full stack, move it and delete the empty stack
                {
                    itemToMoveTo.quantity += itemInSlot.quantity;
                    invData.inventoryItems.Remove(itemInSlot);
                }
                else // If the stack doesnt have enough room to receive the quantity provided, move the maximum quantity allowed
                {
                    itemInSlot.quantity -= (itemToMoveTo.maxQuantity - itemToMoveTo.quantity);
                    itemToMoveTo.quantity = itemToMoveTo.maxQuantity;
                }

                return true;
            }
            else
            {
                Item itemInMoveToSlot = FindItemInSlot(invData, action.moveTo); // Get item that is in the slot we want to move the current item

                itemInMoveToSlot.itemInventorySlot = itemInSlot.itemInventorySlot; // Assign to this item the slot we have with the item we are moving
                itemInSlot.itemInventorySlot = action.moveTo; // Assign the item we are moving to the now free slot, swapping spots with the above item

                return true;
            }
        }
    }

    private uint AssignSlot(InventoryData invData, Item.ItemType itemType)
    {
        uint slotToCheck = 0;
        while (slotToCheck < invData.maxInventorySlots) // Check all slot numbers until the inventory max slots
        {
            if (!IsSlotTaken(invData, slotToCheck) && IsRightType(invData, slotToCheck, itemType)) // If it hasnt found an item with that slot and the slot accepts this item type, it means the slot is free and can be allocated to the current item
            {
                return slotToCheck;
            }
            slotToCheck++;
        }
        // Inventory is full
        return invData.maxInventorySlots;
    }
    private bool IsSlotTaken( InventoryData invData, uint slotToCheck)
    {
        bool found = false;
        int count = 0;

        while (!found && count < invData.inventoryItems.Count)  // Iterate through all the inventory items to check if the inventorySlot we are trying is free
        {
            if (invData.inventoryItems[count].itemInventorySlot == slotToCheck)
            {
                found = true;
            }
            count++;
        }
        return found;

    }
    private bool IsRightType(InventoryData invData, uint slotToCheck, Item.ItemType itemType)
    {
        if (slotToCheck >= invData.maxInventorySlots) // If we are trying to check an unexisting slot, return false
        {
            return false;
        }
        else if (invData.inventoryUIGrid[slotToCheck].slotType == Item.ItemType.Generic) // If this slot accepts any kind of item return true
        {
            return true;
        }
        else if (itemType == invData.inventoryUIGrid[slotToCheck].slotType) // If the slot type matches the type of item return true
        {
            return true;
        }
        else // All other cases are not allowed
        {
            return false;
        }
    }
    private Item FindItemInSlot(InventoryData invData, uint slotToCheck)
    {
        Item toReturn = null;
        bool found = false;
        int count = 0;
        while (!found && count < invData.inventoryItems.Count)  // Iterate through all the inventory items to check if the inventorySlot we are trying is free
        {
            if (invData.inventoryItems[count].itemInventorySlot == slotToCheck)
            {
                found = true;
                toReturn = invData.inventoryItems[count];
            }
            count++;
        }
        return toReturn;
    }


    private bool IsStack(Item i) // Returns if the item is a stack or its a single slot item
    {
        if (i.maxQuantity <= 1)
            return false;
        else
            return true;
    }

    private List<Item> FindItemsByID(InventoryData invData, uint ID)
    {
        List<Item> itemsFound = new List<Item>();
        foreach (Item i in invData.inventoryItems) // Find all items of the same type
        {
            if (i.itemID == ID)
            {
                itemsFound.Add(i);
            }
        }
        return itemsFound;
    }

    public virtual void OnInventoryChanged(InventoryData invData) // Function called when the inventory was changed to update the UI/show prompts/etc...
    {
        if (Ui_UpdateRequest != null)
            Ui_UpdateRequest(invData);
    }

}
