using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
{
    [Header("Item Settings")]
    [SerializeField] private Item item;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    public string InteractionPrompt => $"Pick up {item.itemName}";

    private void Start()
    {
        UpdateVisuals();
    }
    
    private void OnValidate()
    {
        UpdateVisuals();
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

        bool added = Inventory.instance.Add(item); // Simple add for now, amount support depends on inventory logic
        
        if (added)
        {
            
            // Optional: Play sound
            Destroy(gameObject);
            return true;
        }
        else
        {
            
            return false;
        }
    }
}
