using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
{
    [Header("Item Settings")]
    [SerializeField] private Item item;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    public string InteractionPrompt => item != null ? $"Pick up {item.itemName}" : "Pick up";

    private void Awake()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (var col in colliders)
        {
            col.isTrigger = true;
        }
    }

    private void Start()
    {
        EnsureItemReference();
        UpdateVisuals();
    }
    
    private void OnValidate()
    {
        EnsureItemReference();
        UpdateVisuals();
    }

    private void EnsureItemReference()
    {
        if (item == null)
        {
            string clean = gameObject.name.Replace("Pickup", "").Replace("(Clone)", "").Trim();
            item = Resources.Load<Item>($"Items/{clean}");
            if (item == null) item = Resources.Load<Item>($"Armor/{clean}");
            if (item == null) item = Resources.Load<Item>(clean);
            if (item == null && clean.ToLower().Contains("kusz")) item = Resources.Load<Item>("Items/Kusza");
            if (item == null && clean.ToLower().Contains("strzal")) item = Resources.Load<Item>("Items/Strzala");
            if (item == null && clean.ToLower().Contains("wytrych")) item = Resources.Load<Item>("Items/Wytrych");
            if (item == null && clean.ToLower().Contains("bat")) item = Resources.Load<Item>("Items/Bat");
            if (item == null && clean.ToLower().Contains("health")) item = Resources.Load<Item>("Items/Health_Potion");
        }
    }

    private void UpdateVisuals()
    {
        if (item != null)
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = item.icon;
            
                // Auto resize if too big
                if (item.icon != null)
                {
                    float maxDimension = Mathf.Max(item.icon.bounds.size.x, item.icon.bounds.size.y);
                    if (maxDimension > 0)
                    {
                        float scaleFactor = 1f / maxDimension; // Target size 1 unit
                        // Apply scale to the visual only
                        transform.localScale = Vector3.one * scaleFactor;
                    }
                }
            }
        }
    }

    public bool Interact(GameObject interactor)
    {
        if (Inventory.instance == null) return false;
        if (item == null) EnsureItemReference();
        if (item == null) return false;

        bool added = Inventory.instance.Add(item);
        
        if (added)
        {
            Destroy(gameObject);
            return true;
        }
        else
        {
            return false;
        }
    }
}
