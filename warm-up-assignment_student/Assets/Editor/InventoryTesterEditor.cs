using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InventoryTester))]
public class InventoryTesterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Get the reference to the target script
        InventoryTester tester = (InventoryTester)target;

        // Draw the default inspector fields (Item Name, Special Toggle, etc.)
        DrawDefaultInspector();

        // Add a button to Add Item to the inventory
        if (GUILayout.Button("Add Item to Inventory"))
        {
            tester.AddItemToInventory();  // Calls the method to add item
        }

        // Add a button to Remove Item from the inventory
        if (GUILayout.Button("Remove Item from Inventory"))
        {
            tester.RemoveItemFromInventory();  // Calls the method to remove item
        }

        // Add a button to check if the item exists in inventory
        if (GUILayout.Button("Check if Item Exists"))
        {
            bool exists = tester.HasItemInInventory();
            if (exists)
            {
                Debug.Log($"Item {tester.itemName} exists in inventory.");
            }
            else
            {
                Debug.Log($"Item {tester.itemName} does not exist in inventory.");
            }
        }

        // Make sure the inspector is updated
        serializedObject.ApplyModifiedProperties();
    }
}

