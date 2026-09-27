using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Praktyki.EQ
{
    public class MinecraftSlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Slot Identification")]
        public int slotIndex = -1; // 0-8: Hotbar, 9-35: Plecak, -1: Kosz / Zrzut
        public bool isTrashSlot = false;
        public bool isArmorSlot = false;
        public Praktyki.Zbroja.ArmorSlotType requiredArmorSlot;

        [Header("Visual References")]
        public Image slotBackground;
        public Image placeholderSilhouette;
        public Image hoverOverlay;
        public Image itemIcon;
        public TextMeshProUGUI countText;
        public GameObject activeBorder;
        public TextMeshProUGUI slotNumberText;

        [Header("Runtime State")]
        private Item currentItem;
        private int currentCount;

        public Item CurrentItem => currentItem;
        public int CurrentCount => currentCount;

        private void Awake()
        {
            if (hoverOverlay != null) hoverOverlay.gameObject.SetActive(false);
            if (activeBorder != null) activeBorder.SetActive(false);
        }

        public void SetItem(Item item, int count)
        {
            currentItem = item;
            currentCount = count;

            if (item != null && count > 0)
            {
                if (itemIcon != null)
                {
                    itemIcon.sprite = item.icon;
                    itemIcon.preserveAspect = true;
                    itemIcon.gameObject.SetActive(true);
                    itemIcon.enabled = true;
                }

                if (placeholderSilhouette != null)
                {
                    placeholderSilhouette.gameObject.SetActive(false);
                }

                if (countText != null)
                {
                    if (count > 1)
                    {
                        countText.text = count.ToString();
                        countText.gameObject.SetActive(true);
                    }
                    else
                    {
                        countText.text = "";
                        countText.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                ClearSlot();
            }
        }

        public void ClearSlot()
        {
            currentItem = null;
            currentCount = 0;

            if (itemIcon != null)
            {
                itemIcon.sprite = null;
                itemIcon.gameObject.SetActive(false);
            }

            if (placeholderSilhouette != null)
            {
                placeholderSilhouette.gameObject.SetActive(true);
            }

            if (countText != null)
            {
                countText.text = "";
                countText.gameObject.SetActive(false);
            }
        }

        public void SetActiveBorder(bool isActive)
        {
            if (activeBorder != null)
            {
                activeBorder.SetActive(isActive);
            }
        }

        public void SetHover(bool isHovered)
        {
            if (hoverOverlay != null)
            {
                hoverOverlay.gameObject.SetActive(isHovered);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            bool isShift = false;
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                isShift = UnityEngine.InputSystem.Keyboard.current.leftShiftKey.isPressed ||
                          UnityEngine.InputSystem.Keyboard.current.rightShiftKey.isPressed;
            }

            if (MinecraftInventoryUI.Instance != null)
            {
                MinecraftInventoryUI.Instance.OnSlotClicked(this, eventData.button, isShift);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetHover(true);
            if (MinecraftInventoryUI.Instance != null)
            {
                MinecraftInventoryUI.Instance.OnSlotHoverEnter(this);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetHover(false);
            if (MinecraftInventoryUI.Instance != null)
            {
                MinecraftInventoryUI.Instance.OnSlotHoverExit(this);
            }
        }
    }
}
