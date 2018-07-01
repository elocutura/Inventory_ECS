using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryInit : MonoBehaviour {

    [SerializeField]
    public Item[] startingItemsList = new Item[0];
    public InventoryData inventory;

	// Use this for initialization
	void Start ()
    {
        for (int i = 0; i < startingItemsList.Length; i++)
        {
            if (startingItemsList[i] != null)
            {
                Item toSetup = startingItemsList[i].CreateCopy();
                toSetup.itemInventorySlot = (uint)i;

                InventoryAction action = new InventoryAction();
                action._action = InventoryAction.action.Deposit;
                action._item = toSetup;

                inventory.pendingActions.Add(action);
            }
        }
	}

}
