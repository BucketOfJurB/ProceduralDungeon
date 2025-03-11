using UnityEngine;
using System.Collections.Generic;

public class InventorySystem : MonoBehaviour
{
    private Dictionary<string, List<string>> inventory = new Dictionary<string, List<string>>();

    void Start()
    {
        AddItem("Banana");
        AddItem("Banana");
        AddItem("Banana");
        DisplayInventory();
    }

    public void AddItem(string itemName, bool isSpecial = false)
    {
        if (!inventory.ContainsKey(itemName))
        {
            inventory[itemName] = new List<string>();
        }

        if (isSpecial)
        {
            // Generate a unique ID for each special item
            string uniqueID = itemName + "_" + System.Guid.NewGuid().ToString();
            inventory[itemName].Add(uniqueID); // Add separate instance
        }
        else
        {
            if (inventory[itemName].Count == 0)
            {
                inventory[itemName].Add(itemName + " x1"); // First item added
            }
            else
            {
                // Update stack count by modifying the last entry
                string lastEntry = inventory[itemName][0];
                int currentCount = int.Parse(lastEntry.Split('x')[1]);
                inventory[itemName][0] = itemName + " x" + (currentCount + 1);
            }
        }
    }


    public void RemoveItem(string itemName)
    {
        if (!inventory.ContainsKey(itemName) || inventory[itemName].Count == 0) return;

        if (inventory[itemName].Count == 1 && inventory[itemName][0].Contains("x"))
        {
            // If it's a stackable item, decrease its count
            int currentCount = int.Parse(inventory[itemName][0].Split('x')[1]);
            if (currentCount > 1)
            {
                inventory[itemName][0] = itemName + " x" + (currentCount - 1);
            }
            else
            {
                inventory.Remove(itemName);
            }
        }
        else
        {
            // Remove a special item (removes one instance)
            inventory[itemName].RemoveAt(0);

            if (inventory[itemName].Count == 0)
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
            foreach (string instance in item.Value)
            {
                Debug.Log(instance); // Display each instance separately
            }
        }
    }


    public bool HasItem(string itemName)
    {
        return inventory.ContainsKey(itemName);
    }



}
