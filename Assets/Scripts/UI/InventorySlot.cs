using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public Button removeButton;

    Item item;

    public void AddItem(Item newItem)
    {
        item = newItem;

        if (icon != null)
        {
            icon.sprite = item.icon;
            icon.enabled = true;
        }
        else
        {
            Debug.LogError("InventorySlot: Icon Image is not assigned in the Inspector!", this);
        }

        if (removeButton != null)
        {
            removeButton.interactable = true;
        }
        else
        {
            Debug.LogError("InventorySlot: Remove Button is not assigned in the Inspector!", this);
        }
    }

    public void ClearSlot()
    {
        item = null;

        icon.sprite = null;
        icon.enabled = false;
        removeButton.interactable = false;
    }

    public void OnRemoveButton()
    {
        Inventory.instance.Remove(item);
    }
}
