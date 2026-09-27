using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    // Static persistence data
    private static List<Item> savedItems = new List<Item>();
    private static List<int> savedItemCounts = new List<int>();
    private static int savedActiveSlot = 0;
    private static bool hasSavedData = false;

    public const int HOTBAR_SIZE = 9;
    public const int TOTAL_SLOTS = 36;

    [Header("Configuration")]
    [SerializeField] private int space = 36;
    public List<Item> items = new List<Item>();
    public List<int> itemCounts = new List<int>();

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
        EnsureItemCountsValid();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SaveToGlobal();
        }
    }

    public void EnsureItemCountsValid()
    {
        if (space < TOTAL_SLOTS) space = TOTAL_SLOTS;

        while (items.Count < space)
        {
            items.Add(null);
        }
        while (itemCounts.Count < items.Count)
        {
            itemCounts.Add(0);
        }
        if (itemCounts.Count > items.Count)
        {
            itemCounts.RemoveRange(items.Count, itemCounts.Count - items.Count);
        }
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null)
            {
                itemCounts[i] = 0;
            }
            else if (itemCounts[i] <= 0)
            {
                itemCounts[i] = 1;
            }
        }
    }

    // Creating a copy of the list to avoid reference issues if objects are destroyed
    private void SaveToGlobal()
    {
        EnsureItemCountsValid();
        savedItems = new List<Item>(items);
        savedItemCounts = new List<int>(itemCounts);
        savedActiveSlot = activeSlotIndex;
        hasSavedData = true;
    }

    private void LoadFromGlobal()
    {
        if (hasSavedData)
        {
            items = new List<Item>(savedItems);
            itemCounts = new List<int>(savedItemCounts);
            EnsureItemCountsValid();
            activeSlotIndex = savedActiveSlot;
        }
    }

    public static void ResetGlobal()
    {
        savedItems.Clear();
        savedItemCounts.Clear();
        savedActiveSlot = 0;
        hasSavedData = false;
    }

    [Header("Active Slot")]
    public int activeSlotIndex = 0;
    public UnityEvent OnActiveSlotChanged;

    public bool Add(Item item)
    {
        return Add(item, 1);
    }

    public bool Add(Item item, int count)
    {
        EnsureItemCountsValid();
        if (item == null || count <= 0) return false;

        // Try to stack on existing matching items if stackable
        if (item.isStackable)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null && items[i] == item && itemCounts[i] < item.maxStack)
                {
                    int spaceLeft = item.maxStack - itemCounts[i];
                    int toAdd = Mathf.Min(spaceLeft, count);
                    itemCounts[i] += toAdd;
                    count -= toAdd;

                    OnInventoryChanged?.Invoke();
                    if (i == activeSlotIndex) OnActiveSlotChanged?.Invoke();

                    if (count <= 0) return true;
                }
            }
        }

        // Add remaining count to first empty slot
        while (count > 0)
        {
            int emptyIndex = -1;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == null)
                {
                    emptyIndex = i;
                    break;
                }
            }

            if (emptyIndex == -1)
            {
                // Full inventory
                return false;
            }

            int toAdd = item.isStackable ? Mathf.Min(item.maxStack, count) : 1;
            items[emptyIndex] = item;
            itemCounts[emptyIndex] = toAdd;
            count -= toAdd;

            OnInventoryChanged?.Invoke();
            if (emptyIndex == activeSlotIndex) OnActiveSlotChanged?.Invoke();

            if (!item.isStackable)
            {
                break;
            }
        }

        return true;
    }

    public void Remove(Item item)
    {
        Remove(item, 1);
    }

    public bool Remove(Item item, int count)
    {
        EnsureItemCountsValid();
        if (item == null || count <= 0) return false;

        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (items[i] != null && items[i] == item)
            {
                if (itemCounts[i] > count)
                {
                    itemCounts[i] -= count;
                    count = 0;
                    OnInventoryChanged?.Invoke();
                    if (i == activeSlotIndex) OnActiveSlotChanged?.Invoke();
                    return true;
                }
                else
                {
                    count -= itemCounts[i];
                    items[i] = null;
                    itemCounts[i] = 0;
                    OnInventoryChanged?.Invoke();
                    if (i == activeSlotIndex) OnActiveSlotChanged?.Invoke();
                    if (count <= 0) return true;
                }
            }
        }

        return count <= 0;
    }

    public bool HasItem(Item item, int count = 1)
    {
        return GetTotalItemCount(item) >= count;
    }

    public int GetTotalItemCount(Item item)
    {
        EnsureItemCountsValid();
        if (item == null) return 0;
        int total = 0;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null && (items[i] == item || items[i].itemName == item.itemName))
            {
                total += itemCounts[i];
            }
        }
        return total;
    }

    public bool ConsumeItem(Item item, int count = 1)
    {
        if (!HasItem(item, count)) return false;

        EnsureItemCountsValid();
        int remaining = count;
        for (int i = items.Count - 1; i >= 0 && remaining > 0; i--)
        {
            if (items[i] != null && (items[i] == item || items[i].itemName == item.itemName))
            {
                if (itemCounts[i] > remaining)
                {
                    itemCounts[i] -= remaining;
                    remaining = 0;
                }
                else
                {
                    remaining -= itemCounts[i];
                    items[i] = null;
                    itemCounts[i] = 0;
                }
            }
        }

        OnInventoryChanged?.Invoke();
        OnActiveSlotChanged?.Invoke();
        return true;
    }

    public int GetItemCount(int slotIndex)
    {
        EnsureItemCountsValid();
        if (slotIndex >= 0 && slotIndex < itemCounts.Count && items[slotIndex] != null)
        {
            return itemCounts[slotIndex];
        }
        return 0;
    }
    
    public void ScrollActiveSlot(float direction)
    {
        int limit = Mathf.Min(HOTBAR_SIZE, items.Count);
        if (limit <= 0) limit = 9;

        if (direction > 0)
        {
            activeSlotIndex--;
            if (activeSlotIndex < 0) activeSlotIndex = limit - 1;
        }
        else if (direction < 0)
        {
            activeSlotIndex++;
            if (activeSlotIndex >= limit) activeSlotIndex = 0;
        }
        
        if (direction != 0)
        {
            OnActiveSlotChanged?.Invoke();
        }
    }

    public void SetActiveSlot(int index)
    {
        int limit = Mathf.Min(HOTBAR_SIZE, items.Count);
        if (limit <= 0) limit = 9;

        if (index >= 0 && index < limit && index != activeSlotIndex)
        {
            activeSlotIndex = index;
            OnActiveSlotChanged?.Invoke();
        }
    }

    public void SetSlot(int slotIndex, Item item, int count)
    {
        EnsureItemCountsValid();
        if (slotIndex < 0 || slotIndex >= items.Count) return;

        items[slotIndex] = count > 0 ? item : null;
        itemCounts[slotIndex] = count > 0 ? count : 0;

        OnInventoryChanged?.Invoke();
        if (slotIndex == activeSlotIndex) OnActiveSlotChanged?.Invoke();
    }

    public void SwapSlots(int slotA, int slotB)
    {
        EnsureItemCountsValid();
        if (slotA < 0 || slotA >= items.Count || slotB < 0 || slotB >= items.Count) return;

        Item tempItem = items[slotA];
        int tempCount = itemCounts[slotA];

        items[slotA] = items[slotB];
        itemCounts[slotA] = itemCounts[slotB];

        items[slotB] = tempItem;
        itemCounts[slotB] = tempCount;

        OnInventoryChanged?.Invoke();
        if (slotA == activeSlotIndex || slotB == activeSlotIndex) OnActiveSlotChanged?.Invoke();
    }

    public void QuickMoveSlot(int fromSlot)
    {
        EnsureItemCountsValid();
        if (fromSlot < 0 || fromSlot >= items.Count) return;
        Item item = items[fromSlot];
        int count = itemCounts[fromSlot];
        if (item == null || count <= 0) return;

        int targetStart = (fromSlot < HOTBAR_SIZE) ? HOTBAR_SIZE : 0;
        int targetEnd = (fromSlot < HOTBAR_SIZE) ? items.Count : HOTBAR_SIZE;

        // Try stacking first
        if (item.isStackable)
        {
            for (int i = targetStart; i < targetEnd; i++)
            {
                if (items[i] != null && items[i] == item && itemCounts[i] < item.maxStack)
                {
                    int spaceLeft = item.maxStack - itemCounts[i];
                    int toMove = Mathf.Min(spaceLeft, count);
                    itemCounts[i] += toMove;
                    count -= toMove;
                    if (count <= 0)
                    {
                        items[fromSlot] = null;
                        itemCounts[fromSlot] = 0;
                        OnInventoryChanged?.Invoke();
                        OnActiveSlotChanged?.Invoke();
                        return;
                    }
                }
            }
        }

        // Move to first empty slot
        for (int i = targetStart; i < targetEnd; i++)
        {
            if (items[i] == null)
            {
                items[i] = item;
                itemCounts[i] = count;
                items[fromSlot] = null;
                itemCounts[fromSlot] = 0;
                OnInventoryChanged?.Invoke();
                OnActiveSlotChanged?.Invoke();
                return;
            }
        }

        // If partial remaining
        itemCounts[fromSlot] = count;
        OnInventoryChanged?.Invoke();
        OnActiveSlotChanged?.Invoke();
    }
    
    public void DropActiveItem(Vector3 dropPos, Quaternion dropRot)
    {
        DropItemAt(activeSlotIndex, 1, dropPos, dropRot);
    }

    public void DropItemAt(int slotIndex, int count, Vector3 dropPos, Quaternion dropRot)
    {
        EnsureItemCountsValid();
        if (slotIndex < 0 || slotIndex >= items.Count) return;
        Item item = items[slotIndex];
        if (item == null || count <= 0) return;

        int toDrop = Mathf.Min(count, itemCounts[slotIndex]);
        itemCounts[slotIndex] -= toDrop;
        if (itemCounts[slotIndex] <= 0)
        {
            items[slotIndex] = null;
            itemCounts[slotIndex] = 0;
        }

        SpawnItemPickup(item, dropPos, dropRot);
        OnInventoryChanged?.Invoke();
        if (slotIndex == activeSlotIndex) OnActiveSlotChanged?.Invoke();
    }

    public GameObject SpawnItemPickup(Item item, Vector3 dropPos, Quaternion dropRot)
    {
        if (item == null) return null;
        GameObject prefabToSpawn = item.pickupPrefab;
        if (prefabToSpawn == null)
        {
            string cleanName = item.name.Replace(" ", "");
            prefabToSpawn = Resources.Load<GameObject>($"Items/{cleanName}Pickup");
            if (prefabToSpawn == null)
                prefabToSpawn = Resources.Load<GameObject>($"{cleanName}Pickup");
            if (prefabToSpawn == null && !string.IsNullOrEmpty(item.itemName))
            {
                string cleanItemName = item.itemName.Replace(" ", "");
                prefabToSpawn = Resources.Load<GameObject>($"Items/{cleanItemName}Pickup");
                if (prefabToSpawn == null)
                    prefabToSpawn = Resources.Load<GameObject>($"{cleanItemName}Pickup");
            }

            // Rozpoznawanie wariantów nazw zbroi
            if (prefabToSpawn == null)
            {
                string n = (item.name + " " + item.itemName).ToLower();
                if (n.Contains("kapsl") && (n.Contains("helm") || n.Contains("hełm")))
                    prefabToSpawn = Resources.Load<GameObject>("Items/HelmKapslowyPickup");
                else if (n.Contains("kapsl") && (n.Contains("pancerz") || n.Contains("klat") || n.Contains("napier")))
                    prefabToSpawn = Resources.Load<GameObject>("Items/PancerzKapslowyPickup");
                else if (n.Contains("kapsl") && (n.Contains("spodni") || n.Contains("nogawic")))
                    prefabToSpawn = Resources.Load<GameObject>("Items/SpodnieKapslowePickup");
                else if (n.Contains("kapsl") && n.Contains("but"))
                    prefabToSpawn = Resources.Load<GameObject>("Items/ButyKapslowePickup");
                else if (n.Contains("czapk") || (n.Contains("skorzan") && n.Contains("helm")))
                    prefabToSpawn = Resources.Load<GameObject>("Items/CzapkaSkorzanaPickup");
                else if (n.Contains("kurtk") || (n.Contains("skorzan") && (n.Contains("klat") || n.Contains("napier"))))
                    prefabToSpawn = Resources.Load<GameObject>("Items/KurtkaSkorzanaPickup");
                else if (n.Contains("skorzan") && (n.Contains("spodni") || n.Contains("nogawic")))
                    prefabToSpawn = Resources.Load<GameObject>("Items/SpodnieSkorzanePickup");
                else if (n.Contains("skorzan") && n.Contains("but"))
                    prefabToSpawn = Resources.Load<GameObject>("Items/ButySkorzanePickup");
                else if (n.Contains("helm") || n.Contains("hełm"))
                    prefabToSpawn = Resources.Load<GameObject>("Items/HelmZelaznyPickup");
                else if (n.Contains("klat") || n.Contains("napierśnik") || n.Contains("napiersnik"))
                    prefabToSpawn = Resources.Load<GameObject>("Items/KlataZelaznaPickup");
                else if (n.Contains("spodni") || n.Contains("nogawic"))
                    prefabToSpawn = Resources.Load<GameObject>("Items/SpodnieZelaznePickup");
                else if (n.Contains("but"))
                    prefabToSpawn = Resources.Load<GameObject>("Items/ButyZelaznePickup");
            }
        }

        if (prefabToSpawn != null)
        {
            GameObject obj = Instantiate(prefabToSpawn, dropPos, dropRot);
            ItemPickup p = obj.GetComponent<ItemPickup>();
            if (p != null)
            {
                var f = typeof(ItemPickup).GetField("item", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                if (f != null && f.GetValue(p) == null) f.SetValue(p, item);
            }
            Debug.Log($"[Inventory] Dropped {item.itemName}");
            return obj;
        }
        else
        {
            // Dynamiczny uniwersalny pickup w świecie gry (gwarancja braku utraty przedmiotu)
            GameObject fallbackObj = new GameObject($"{item.name}Pickup");
            fallbackObj.transform.position = dropPos;
            fallbackObj.transform.rotation = dropRot;

            BoxCollider col = fallbackObj.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(1.2f, 1.2f, 1f);

            SpriteRenderer sr = fallbackObj.AddComponent<SpriteRenderer>();
            sr.sprite = item.icon;
            Material spriteMat = Resources.Load<Material>("SpriteUnlitDefault");
            if (spriteMat != null) sr.material = spriteMat;

            fallbackObj.AddComponent<Billboard>();

            ItemPickup pickup = fallbackObj.AddComponent<ItemPickup>();
            var itemField = typeof(ItemPickup).GetField("item", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (itemField != null) itemField.SetValue(pickup, item);

            Debug.Log($"[Inventory] Created dynamic world pickup for {item.itemName}");
            return fallbackObj;
        }
    }

    public Item GetActiveItem()
    {
        if (activeSlotIndex >= 0 && activeSlotIndex < items.Count)
        {
            return items[activeSlotIndex];
        }
        return null;
    }
}
