using UnityEngine;
using System.Collections.Generic;

public class InventorySystem : MonoBehaviour
{
    private Dictionary<string, int> inventory = new Dictionary<string, int>();

    void Start()
    {
        AddItem("Banana");
        AddItem("Fortnite");
        AddItem("Banana");
        RemoveItem("Big ben");
        RemoveItem("Banana");
        DisplayInventory();
    }

    public void AddItem(string itemName)
    {
        if (inventory.ContainsKey(itemName))
        {
            inventory[itemName]++;
        }
        else
        {
            inventory[itemName] = 1;
        }
    }

    public void RemoveItem(string itemName)
    {
        if (inventory.ContainsKey(itemName))
        {
            inventory[itemName]--;

            if (inventory[itemName] <= 0)
            {
                inventory.Remove(itemName);
            }
        }
    }

    public void DisplayInventory()
    {
        Debug.Log("Inventory:");

        if (inventory.Count == 0)
        {
            Debug.Log("Inventory is empty.");
            return;
        }

        foreach (var item in inventory)
        {
            Debug.Log(item.Key + ": " + item.Value);
        }
    }

    public bool HasItem(string itemName)
    {
        return inventory.ContainsKey(itemName);
    }



}
