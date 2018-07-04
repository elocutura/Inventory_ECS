using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu (fileName = "New Item", menuName = "Items/Base Item")]
public class Item : ScriptableObject, IComponentData
{

    public enum ItemType
    {
        Generic,
        Consumable,
        Weapon,
        OffHand,
        Chestpiece,
        Legs,
        Helmet,
        Gloves,
        Amulet,
        Ring,
        Trinket,
        Cloak
    }

    public uint itemID;
    public string itemName;
    public ItemType itemType;
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
        i.itemType = itemType;
        i.itemInventorySlot = itemInventorySlot;
        i.quantity = quantity;
        i.maxQuantity = maxQuantity;

        i.itemIcon = itemIcon;

        return i;
    }
}
