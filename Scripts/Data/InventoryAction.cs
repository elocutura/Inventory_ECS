using Unity.Entities;

public class InventoryAction : IComponentData
{

    public delegate void InventoryActionRequest();
    public static event InventoryActionRequest OnInventoryActionRequest;

    public static void AskForInventoryActionRequest()
    {
        OnInventoryActionRequest();
    }

    public enum action
    {
        Deposit,
        UnStack,
        Move,
        MoveBySlot,
        Delete,
        DeleteBySlot,
        MoveFromInventory
    }

    public action _action;
    public Item _item;
    public uint moveTo;
    public InventoryData oldInventory;
}