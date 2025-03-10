using UnityEngine;

public class InventoryTester : MonoBehaviour
{
    public InventorySystem inventory;   // The InventorySystem to interact with
    public string itemName;             // Item name entered in Inspector
    public bool isSpecial;              // Whether the item is special (non-stacking)

    // Adds the selected item when the button is clicked
    public void AddItemToInventory()
    {
        if (!string.IsNullOrEmpty(itemName))
        {
            inventory.AddItem(itemName, isSpecial);
            Debug.Log($"Added {itemName} (Special: {isSpecial})");
            inventory.DisplayInventory();
        }
        else
        {
            Debug.LogWarning("Item name is empty! Please enter a valid item name.");
        }
    }

    // Removes the selected item when the button is clicked
    public void RemoveItemFromInventory()
    {
        if (!string.IsNullOrEmpty(itemName))
        {
            inventory.RemoveItem(itemName);
            Debug.Log($"Removed {itemName}");
            inventory.DisplayInventory();
        }
        else
        {
            Debug.LogWarning("Item name is empty! Please enter a valid item name.");
        }
    }

    // Checks if the selected item exists
    public bool HasItemInInventory()
    {
        return inventory.HasItem(itemName);
    }
}



