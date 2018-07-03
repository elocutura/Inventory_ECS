using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class UI_DragUIItemsSys : ComponentSystem
{

    private struct Filter
    {
        public int Length;
        public ComponentArray<InventoryData> invData;
    }
    private struct TempUI
    {
        public UI_TempSlot tempSlot;
    }

    private UI_TempSlot tempSlot = null;

    private UI_Slot draggingSlot = null;
    private InventoryData draggingFromInventory = null;

    [Inject] private Filter data;

    protected override void OnStartRunning()
    {
        base.OnStartRunning();

        foreach (var tempUI in GetEntities<TempUI>())
        {
            tempSlot = tempUI.tempSlot;

        }
    }

    protected override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && draggingSlot == null) // If you click loop thought the inventories
        {

            for (int i = 0; i < data.Length; i++)
            {
                if (data.invData[i].allowInteractions)
                {
                    foreach (UI_Slot slot in data.invData[i].inventoryUIGrid) // For each slot in that inventory
                    {
                        if (MouseInsideUiItem(slot.rectTransform) && slot.itemHolding != null) // If the mouse is inside a valid slot and we are not dragging something aleady, setup to drag item in this specific slot
                        {
                            draggingSlot = slot;
                            draggingFromInventory = data.invData[i];

                            tempSlot.slotImage.sprite = draggingSlot.slotImage.sprite;
                            tempSlot.slotImage.color = new Color(1, 1, 1, 1);
                            tempSlot.stackText.text = draggingSlot.stackText.text;

                            Cursor.visible = false; // Hide mouse cursor, the tempSlot already shows the cursor position
                        }
                    }
                }
            }

        }
        else if (Input.GetKeyUp(KeyCode.Mouse0) && draggingSlot != null)
        {
            Dictionary<UI_Slot, InventoryData> destinationSlotInventory = OnTopOfSlot();
            UI_Slot destinationSlot = null;
            InventoryData destionationInventory = null;
            Item i;
            Cursor.visible = true;

            if (destinationSlotInventory != null) // Check if the mouse is on top of a slot
            {

                foreach (KeyValuePair<UI_Slot, InventoryData> dictData in destinationSlotInventory)
                {
                    destinationSlot = dictData.Key;
                    destionationInventory = dictData.Value;
                }
                if (destionationInventory != null && destinationSlot != null)
                {
                    if (destionationInventory == draggingFromInventory) // If the destination slot is in the same inventory as the items, tell the inventorySystem we want to move this item into the new pos
                    {
                        i = draggingSlot.itemHolding.CreateCopy();

                        InventoryAction action = new InventoryAction();
                        action._action = InventoryAction.action.MoveBySlot;
                        action.moveTo = (uint)destinationSlot.slotIndex;
                        action._item = i;

                        if (Input.GetKey(KeyCode.LeftShift)) // If we are pressing shift, unstack
                        {
                            action._item.quantity = action._item.quantity / 2; // UNTIL UI FOR THIS, USTACK HALf
                        }

                        destionationInventory.pendingActions.Add(action); // Add the action to the pending  and ask the inventorySystem to process the changes
                        InventoryAction.AskForInventoryActionRequest();
                    }
                    else // If we are trying to move this item into a new inventory
                    {
                        i = draggingSlot.itemHolding.CreateCopy();

                        InventoryAction depositAction = new InventoryAction(); // A deposit action for the inventory that will receive the item
                        depositAction._action = InventoryAction.action.Deposit;
                        depositAction._item = i;
                        depositAction._item.itemInventorySlot = (uint) destinationSlot.slotIndex;

                        InventoryAction deleteAction = new InventoryAction(); // A delete action from the current inventory
                        deleteAction._action = InventoryAction.action.Delete;
                        deleteAction._item = draggingSlot.itemHolding;

                        draggingFromInventory.pendingActions.Add(deleteAction); // Assign both actions to the respective inventories and ask the inventorySystem to process the changes
                        destionationInventory.pendingActions.Add(depositAction);
                        InventoryAction.AskForInventoryActionRequest();
                    }
                }
            }
            draggingSlot = null;
            draggingFromInventory = null;

            tempSlot.slotImage.color = new Color(1, 1, 1, 0);
            tempSlot.slotImage.sprite = null;
            tempSlot.stackText.text = "";
            tempSlot.transform.position = new Vector3(-200, -200, 0); // Move it out of the canvas, its not disabled and enabled every time for performance reasons
        }
        if (draggingSlot != null)
        {
            tempSlot.transform.position = Input.mousePosition;
        }
    }

    private Dictionary<UI_Slot, InventoryData> OnTopOfSlot()
    {
        Vector3 mousePos = Input.mousePosition;
        Dictionary<UI_Slot, InventoryData> toReturn = new Dictionary<UI_Slot, InventoryData>();

        Canvas canvas;
        float correctedWidth;
        float correctedHeight;

        for (int i = 0; i < data.Length; i++) // loop through all inventories
        {
            if (data.invData[i].allowInteractions) // Only check this inventory slots if the inventory allowsinteractions
            {
                foreach (UI_Slot slot in data.invData[i].inventoryUIGrid) // Look through all slots in that inventory
                {
                    canvas = slot.rectTransform.GetComponentInParent<Canvas>(); // Canvas holding this RectTransform
                                                                                // Correction of width and height to match screen for the calculations of the cursor
                    correctedWidth = slot.rectTransform.rect.width * canvas.scaleFactor;
                    correctedHeight = slot.rectTransform.rect.height * canvas.scaleFactor;

                    if (mousePos.x > slot.rectTransform.position.x - correctedWidth / 2 && mousePos.x < slot.rectTransform.position.x + correctedWidth / 2) // Its in the same vertical coordinates
                    {
                        if (mousePos.y > slot.rectTransform.position.y - correctedHeight / 2 && mousePos.y < slot.rectTransform.position.y + correctedHeight / 2)// Its in the same horizontal coordinates
                        {
                            toReturn.Add(slot, data.invData[i]);
                            return toReturn;
                        }
                    }
                }
            }
        }
        return null;
    }

    private bool MouseInsideUiItem(RectTransform itemTransform) // Returns true if the mouse is pointing at the specified Ui Item
    {
        if (!itemTransform.gameObject.activeSelf) // If the object we are trying to point to is not active, return false
            return false;

        Vector3 mousePos = Input.mousePosition;

        Canvas canvas = itemTransform.GetComponentInParent<Canvas>(); // Canvas holding this RectTransform
        // Correction of width and height to match screen for the calculations of the cursor
        float correctedWidth = itemTransform.rect.width * canvas.scaleFactor;
        float correctedHeight = itemTransform.rect.height * canvas.scaleFactor;

        if (mousePos.x > itemTransform.position.x - correctedWidth / 2 && mousePos.x < itemTransform.position.x + correctedWidth / 2) // Its in the same vertical coordinates
        {
            if (mousePos.y > itemTransform.position.y - correctedHeight / 2 && mousePos.y < itemTransform.position.y + correctedHeight / 2)// Its in the same horizontal coordinates
            {
                return true;
            }
        }
        return false;
    }
}
