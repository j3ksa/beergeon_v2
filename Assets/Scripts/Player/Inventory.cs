using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    // Static persistence data
    private static List<Item> savedItems = new List<Item>();
    private static int savedActiveSlot = 0;
    private static bool hasSavedData = false;

    [Header("Configuration")]
    [SerializeField] private int space = 9;
    public List<Item> items = new List<Item>();

    [Header("Events")]
    public UnityEvent OnInventoryChanged;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject); // Standard singleton pattern for duplicate check
            return;
        }
        instance = this;
        
        LoadFromGlobal();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SaveToGlobal();
        }
    }

    // Creating a copy of the list to avoid reference issues if objects are destroyed
    private void SaveToGlobal()
    {
        savedItems = new List<Item>(items);
        savedActiveSlot = activeSlotIndex;
        hasSavedData = true;
    }

    private void LoadFromGlobal()
    {
        if (hasSavedData)
        {
            items = new List<Item>(savedItems);
            activeSlotIndex = savedActiveSlot;
            // Notify UI roughly a frame later or immediately? 
            // In Awake, listeners aren't subscribed yet normally (UI subscribes in Start).
            // So data is ready for them when they Start.
        }
    }

    public static void ResetGlobal()
    {
        savedItems.Clear();
        savedActiveSlot = 0;
        hasSavedData = false;
    }

    [Header("Active Slot")]
    public int activeSlotIndex = 0;
    public UnityEvent OnActiveSlotChanged;

    public bool Add(Item item)
    {
        if (items.Count >= space)
        {
            
            return false;
        }

        items.Add(item);
        OnInventoryChanged?.Invoke();
        if (items.Count - 1 == activeSlotIndex) OnActiveSlotChanged?.Invoke(); 
        return true;
    }

    public void Remove(Item item)
    {
        items.Remove(item);
        OnInventoryChanged?.Invoke();
        OnActiveSlotChanged?.Invoke(); // Content of active slot might have changed
    }
    
    public void ScrollActiveSlot(float direction)
    {
        if (direction > 0)
        {
            activeSlotIndex--;
            if (activeSlotIndex < 0) activeSlotIndex = space - 1;
        }
        else if (direction < 0)
        {
            activeSlotIndex++;
            if (activeSlotIndex >= space) activeSlotIndex = 0;
        }
        
        if (direction != 0)
        {
            OnActiveSlotChanged?.Invoke();
        }
    }
    
    public void DropActiveItem(Vector3 dropPos, Quaternion dropRot)
    {
        Item activeItem = GetActiveItem();
        if (activeItem != null)
        {
            Remove(activeItem);
            
            if (activeItem.pickupPrefab != null)
            {
                Instantiate(activeItem.pickupPrefab, dropPos, dropRot);
            }
            
            Debug.Log($"Dropped {activeItem.itemName}");
        }
    }

    public Item GetActiveItem()
    {
        if (activeSlotIndex < items.Count && activeSlotIndex >= 0)
        {
            return items[activeSlotIndex];
        }
        return null;
    }
}
