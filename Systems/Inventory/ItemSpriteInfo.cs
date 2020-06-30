using UnityEngine;
using UnityEngine.UI;


public class ItemSpriteInfo : MonoBehaviour
{
    [HideInInspector]
    public CharacterInventory inv = null;
    public InventoryEntry invEntry;
    public Image image;

    void OnValidate()
    {
        image = GetComponent<Image>(); 
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        inv.OnItemClick(invEntry.inventorySlotIndex);
    }
}
