using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryTester : MonoBehaviour
{
    public Item[] itemsToTest;

    void Update()
    {
        // Check for new Input System
        if (Keyboard.current == null) return;

        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            if (itemsToTest != null && itemsToTest.Length > 0)
            {
                // Pick a random item
                Item itemToAdd = itemsToTest[Random.Range(0, itemsToTest.Length)];
                bool added = Inventory.instance.Add(itemToAdd);
                
            }
            else
            {
                
            }
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            // Remove the first item found in inventory that matches one of our test items
            if (itemsToTest != null && itemsToTest.Length > 0)
            {
                foreach (var item in itemsToTest)
                {
                    if (Inventory.instance.items.Contains(item))
                    {
                        Inventory.instance.Remove(item);
                        
                        break;
                    }
                }
            }
        }
    }
}
