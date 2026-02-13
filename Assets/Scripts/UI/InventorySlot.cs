using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public Image activeBorder;

    Item item;

    public void AddItem(Item newItem)
    {
        item = newItem;

        if (icon != null)
        {
            icon.sprite = item.icon;
            icon.gameObject.SetActive(true);
            icon.enabled = true; // Just in case
        }
        else
        {
            Debug.LogError("InventorySlot: Icon Image is not assigned in the Inspector!", this);
        }
    }

    public void ClearSlot()
    {
        item = null;

        if (icon != null)
        {
            icon.sprite = null;
            icon.gameObject.SetActive(false);
        }
    }

    public void SetActive(bool isActive)
    {
        if (activeBorder != null)
        {
            activeBorder.gameObject.SetActive(isActive);
        }
    }
}

