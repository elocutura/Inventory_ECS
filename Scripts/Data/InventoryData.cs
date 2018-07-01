using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryData : MonoBehaviour {
    
    [HideInInspector]
    public List<Item> inventoryItems = new List<Item>();
    [HideInInspector]
    public List<InventoryAction> pendingActions = new List<InventoryAction>();

    public uint maxInventorySlots = 10;

    public bool allowInteractions = true;
    public UI_Slot[] inventoryUIGrid = new UI_Slot[10];
    public Text inventoryListText;
}
