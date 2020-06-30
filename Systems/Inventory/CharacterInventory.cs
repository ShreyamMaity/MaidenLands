using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CharacterInventory : MonoBehaviour
{
    public static CharacterInventory instance;

    public CharacterStats charStats;
    public GameObject inventoryUI;

    public Image[] hotBarDisplayHolders = new Image[4];

    public Dictionary<int, InventoryEntry> inventory = new Dictionary<int, InventoryEntry>();
    public InventoryEntry invEntry;

    ItemSpriteInfo[] itemsSpritesInfo = new ItemSpriteInfo[30];
    int inventoryItemCap = 20;
    [SerializeField]
    int nextItemIndex = 1;
    bool addedItem = true;

    GameObject foundStats;

    void Start()
    {
        instance = this;
        invEntry = new InventoryEntry(0, null, null);
        foundStats = GameObject.FindGameObjectWithTag("Player");
        charStats = foundStats.GetComponentInParent<CharacterStats>();

        inventory.Clear();

        itemsSpritesInfo = inventoryUI.GetComponentsInChildren<ItemSpriteInfo>();
        foreach (var item in itemsSpritesInfo)
        {
            item.inv = this;
        }
    }

    void Update()
    {
        //Checking for a hotbar key to be pressed
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TriggerItemUse(101);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TriggerItemUse(102);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TriggerItemUse(103);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            TriggerItemUse(104);
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            DisplayInventory();
        }

        //Check to see if the item has already been added - Prevent duplicate adds for 1 item
        if (!addedItem)
        {
            TryPickUp();
        }
    }

    public void StoreItem(ItemPickUp itemToStore)
    {
        addedItem = false;

        if ((charStats.characterDefinition.currentEncumbrance + itemToStore.itemDefinition.itemWeight) <= charStats.characterDefinition.maxEncumbrance)
        {
            invEntry.item = itemToStore;
            invEntry.stackSize = 1;
            invEntry.hbSprite = itemToStore.itemDefinition.itemIcon;

            // addedItem = false;
            itemToStore.gameObject.SetActive(false);
        }
    }

    void TryPickUp()
    {
        bool itemInInventory = false;

        //Check to see if the item to be stored was properly submitted to the inventory - Continue if Yes otherwise do nothing
        if(invEntry.item == null) return;

        // loop through all the items in the inventory
        foreach (KeyValuePair<int, InventoryEntry> ie in inventory)
        {
            // if item to add (invEntry) is already preasent in inventory
            if (invEntry.item.itemDefinition == ie.Value.item.itemDefinition)
            {
                // if it is stackable
                // than just increase its stack size
                if (ie.Value.item.itemDefinition.isStackable)
                {
                    Debug.Log("Stacked");
                    ie.Value.stackSize += 1;
                    itemInInventory = true;
                    Destroy(invEntry.item.gameObject);
                    return;
                }
                else
                {
                    itemInInventory = false;
                }
            }
        }

        // if inventory is full
        if (inventory.Count == inventoryItemCap)
        {
            invEntry.item.gameObject.SetActive(true);
            Debug.Log("Inventory is Full");
            return;
        }

        if (!itemInInventory)
        {
            addedItem = AddItemToInv(addedItem);
            itemInInventory = true;
            return;
        }
    }

    bool AddItemToInv(bool finishedAdding)
    {
        // add to dictionary
        InventoryEntry newEntry = new InventoryEntry(invEntry.stackSize, Instantiate(invEntry.item), invEntry.hbSprite);
        inventory.Add(nextItemIndex, newEntry);
        Destroy(invEntry.item.gameObject);

        newEntry.inventorySlotIndex = nextItemIndex;
        itemsSpritesInfo[nextItemIndex + 7].image.sprite = newEntry.hbSprite;
        itemsSpritesInfo[nextItemIndex + 7].invEntry = newEntry;

        nextItemIndex = IncreaseID(nextItemIndex);

        // AddItemToHotBar(itemsInInventory[idCount]);

        invEntry.item = null;
        invEntry.stackSize = 0;
        invEntry.hbSprite = null;

        finishedAdding = true;
        return finishedAdding;
    }

    void RemoveItem(int itemIndex)
    {
        if (!inventory.ContainsKey(itemIndex))
        {
            Debug.Log("item not in inventory");
            return;
        }

        // else get the item
        var item = inventory[itemIndex];
        // if item is stackable
        if (item.item.itemDefinition.isStackable)
        {
            Debug.LogFormat("stackable " + item.stackSize);
            if (item.stackSize > 1) { item.stackSize--; return; }
        }
        // clear the sprite
        itemsSpritesInfo[itemIndex+7].image.sprite = null;
        itemsSpritesInfo[itemIndex+7] = null;

        // finally remove from inventory
        inventory.Remove(itemIndex);

        // decrement next item index
        nextItemIndex--;

        // and update the UI display for inventory
        UpdateUIDisplay();
    }

    int IncreaseID(int currentID)
    {
        int newID = currentID;

        for (int itemCount = 1; itemCount <= inventory.Count; itemCount++)
        {
            if (inventory.ContainsKey(newID))
            {
                newID += 1;
            }
            else return newID;
        }

        return newID;
    }

    int DecreaseID() { return 0; }

    private void AddItemToHotBar(InventoryEntry itemForHotBar)
    {
        int hotBarCounter = 0;
        bool increaseCount = false;

        //Check for open hotbar slot
        foreach (Image images in hotBarDisplayHolders)
        {
            hotBarCounter += 1;

            if (itemForHotBar.hotBarSlot == 0)
            {
                if (images.sprite == null)
                {
                    //Add item to open hotbar slot
                    itemForHotBar.hotBarSlot = hotBarCounter;
                    //Change hotbar sprite to show item
                    images.sprite = itemForHotBar.hbSprite;
                    increaseCount = true;
                    break;
                }
            }
            else if (itemForHotBar.item.itemDefinition.isStackable)
            {
                increaseCount = true;
            }
        }

        if (increaseCount)
        {
            hotBarDisplayHolders[itemForHotBar.hotBarSlot - 1].GetComponentInChildren<Text>().text = itemForHotBar.stackSize.ToString();
        }

        increaseCount = false;
    }

    void DisplayInventory()
    {
        if (inventoryUI.activeSelf == true)
        {
            inventoryUI.SetActive(false);
        }
        else
        {
            inventoryUI.SetActive(true);
        }
    }

    void UpdateUIDisplay()
    {
        // clear out the itemsSpritesInfo
        itemsSpritesInfo = new ItemSpriteInfo[30];
        itemsSpritesInfo = inventoryUI.GetComponentsInChildren<ItemSpriteInfo>();

        foreach (var item in itemsSpritesInfo)
        {
            item.inv = this;
            item.image.sprite = null;
            item.invEntry = null;
        }

        int i = 1;
        foreach (var item in inventory.Keys)
        {
            itemsSpritesInfo[i + 7].inv = this;
            itemsSpritesInfo[i + 7].image.sprite = inventory[item].hbSprite;
            itemsSpritesInfo[i + 7].invEntry = inventory[item];
            i++;
        }
    }

    public void UseItem()
    {
    }

    public void TriggerItemUse(int itemIndex)
    {
        //bool triggerItem = false;
        //first hot bar item usage
        //foreach (KeyValuePair<int, InventoryEntry> ie in inventory)
        //{

        //}


        //foreach (KeyValuePair<int, InventoryEntry> ie in inventory)
        //{
        //    if (itemID > 100)
        //    {
        //        itemID -= 100;

        //        if (ie.Value.hotBarSlot == itemID)
        //        {
        //            triggerItem = true;
        //        }
        //    }
        //    else if (ie.Value.inventorySlotIndex == itemID)
        //    {
        //        triggerItem = true;
        //    }

        //    if (triggerItem)
        //    {
        //        if (ie.Value.stackSize == 1)
        //        {
        //            if (ie.Value.item.itemDefinition.isStackable)
        //            {
        //                if (ie.Value.hotBarSlot != 0)
        //                {
        //                    hotBarDisplayHolders[ie.Value.hotBarSlot - 1].sprite = null;
        //                    hotBarDisplayHolders[ie.Value.hotBarSlot - 1].GetComponentInChildren<Text>().text = "0";
        //                }

        //                ie.Value.item.UseItem();
        //                inventory.Remove(ie.Key);
        //                break;
        //            }
        //            else
        //            {
        //                ie.Value.item.UseItem();

        //                meaning is_industructable = true
        //                if (!ie.Value.item.itemDefinition.isIndestructable)
        //                {
        //                    inventory.Remove(ie.Key);
        //                    break;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            ie.Value.item.UseItem();
        //            ie.Value.stackSize -= 1;
        //            hotBarDisplayHolders[ie.Value.hotBarSlot - 1].GetComponentInChildren<Text>().text = ie.Value.stackSize.ToString();
        //            break;
        //        }
        //    }
        //}

    }

    public void OnItemClick(int itemIndex)
    {
        if (inventory.ContainsKey(itemIndex)){ /*Debug.Log(inventory[itemIndex].item.itemDefinition.itemName);*/ }
        else { Debug.Log("Item not found"); return; }

        RemoveItem(itemIndex);
    }
}