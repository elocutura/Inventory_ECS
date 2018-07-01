using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu (fileName = "New Item", menuName = "Items/Base Item")]
public class Item : ScriptableObject, IComponentData
{
    public uint itemID;
    public string itemName;
    [HideInInspector]
    public uint itemInventorySlot = 0;
    public uint quantity;
    public uint maxQuantity;

    public Sprite itemIcon;

    public virtual Item CreateCopy()
    {
        Item i = Item.CreateInstance<Item>();
        i.itemID = itemID;
        i.itemName = itemName;
        i.itemInventorySlot = itemInventorySlot;
        i.quantity = quantity;
        i.maxQuantity = maxQuantity;

        i.itemIcon = itemIcon;

        return i;
    }
}
