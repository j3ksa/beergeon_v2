using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Transform itemsParent;
    public GameObject inventoryUI;

    Inventory inventory;

    InventorySlot[] slots;

    void Start()
    {
        inventory = Inventory.instance;
        if (inventory != null)
        {
             inventory.OnInventoryChanged.AddListener(UpdateUI);
             inventory.OnActiveSlotChanged.AddListener(UpdateUI);
        }
        else
        {
             Debug.LogError("InventoryUI: Inventory instance not found!");
        }

        slots = itemsParent.GetComponentsInChildren<InventorySlot>();
        Debug.Log($"InventoryUI: Found {slots.Length} slots.");
        
        UpdateUI();
    }

    void UpdateUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inventory.items.Count)
            {
                slots[i].AddItem(inventory.items[i]);
            }
            else
            {
                slots[i].ClearSlot();
            }
            
            slots[i].SetActive(i == inventory.activeSlotIndex);
        }
    }
}
