using UnityEngine;


public class InventoryEntry
{
    public ItemPickUp item;
    public Sprite hbSprite;

    public int stackSize;
    public int inventorySlotIndex;
    public int hotBarSlot;

    public InventoryEntry(int stackSize, ItemPickUp item, Sprite hbSprite)
    {
        this.item = item;

        this.stackSize = stackSize;
        this.hotBarSlot = -1;
        this.inventorySlotIndex = 0;
        this.hbSprite = hbSprite;
    }
}
