using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace ShopSystem
{
    public class ShopItemSlotUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Button buyButton;
        [SerializeField] private TextMeshProUGUI buyButtonText;

        [Header("Sold Visual Settings")]
        [SerializeField] private Color normalIconColor = Color.white;
        [SerializeField] private Color soldIconColor = new Color(0.3f, 0.3f, 0.3f, 0.7f);
        [SerializeField] private string buyText = "BUY";
        [SerializeField] private string soldText = "SOLD";

        private ShopItemSO currentShopItem;
        private int currentCost;
        private Action<ShopItemSlotUI, ShopItemSO, int> onBuyCallback;
        private bool isSold = false;

        public bool IsSold => isSold;
        public ShopItemSO CurrentShopItem => currentShopItem;
        public int CurrentCost => currentCost;

        public void Setup(ShopItemSO shopItem, int finalCost, Action<ShopItemSlotUI, ShopItemSO, int> buyCallback)
        {
            currentShopItem = shopItem;
            currentCost = finalCost;
            onBuyCallback = buyCallback;
            isSold = false;

            if (currentShopItem != null)
            {
                if (iconImage != null)
                {
                    iconImage.sprite = currentShopItem.GetIcon();
                    iconImage.enabled = (iconImage.sprite != null);
                    iconImage.preserveAspect = true;
                    iconImage.color = normalIconColor;
                }

                if (nameText != null)
                {
                    nameText.text = currentShopItem.GetName();
                    nameText.enableAutoSizing = true;
                    nameText.fontSizeMin = 14f;
                    nameText.fontSizeMax = 22f;
                    nameText.textWrappingMode = TextWrappingModes.Normal;
                    nameText.overflowMode = TextOverflowModes.Overflow;
                }

                if (descriptionText != null)
                {
                    descriptionText.text = currentShopItem.GetDescription();
                    descriptionText.enableAutoSizing = true;
                    descriptionText.fontSizeMin = 13f;
                    descriptionText.fontSizeMax = 18f;
                    descriptionText.textWrappingMode = TextWrappingModes.Normal;
                    descriptionText.overflowMode = TextOverflowModes.Overflow;
                    descriptionText.color = new Color(0.92f, 0.88f, 0.82f, 1f); // Wyraźny, jasny kontrast na tle ciemnej cegły
                }

                if (costText != null) costText.text = currentCost.ToString();
            }

            if (buyButton != null)
            {
                buyButton.interactable = true;
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(OnBuyButtonClicked);
            }

            if (buyButtonText != null)
            {
                buyButtonText.text = buyText;
            }
        }

        private void OnBuyButtonClicked()
        {
            if (isSold) return;
            onBuyCallback?.Invoke(this, currentShopItem, currentCost);
        }

        public void MarkAsSold()
        {
            isSold = true;

            if (buyButton != null)
            {
                buyButton.interactable = false;
            }

            if (buyButtonText != null)
            {
                buyButtonText.text = soldText;
            }

            if (iconImage != null)
            {
                iconImage.color = soldIconColor;
            }
        }
    }
}
