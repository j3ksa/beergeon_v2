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
            icon.enabled = true;
        }
        else
        {
            
        }
    }

    public void ClearSlot()
    {
        item = null;

        icon.sprite = null;
        icon.enabled = false;
    }

    public void SetActive(bool isActive)
    {
        if (activeBorder != null)
        {
            activeBorder.gameObject.SetActive(isActive);
        }
    }
}

