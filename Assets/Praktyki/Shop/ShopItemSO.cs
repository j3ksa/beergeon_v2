using UnityEngine;

namespace ShopSystem
{
    [CreateAssetMenu(fileName = "New Shop Item", menuName = "Shop/Shop Item")]
    public class ShopItemSO : ScriptableObject
    {
        [Header("Item Reference")]
        [Tooltip("Referencja do bazowego przedmiotu z ekwipunku (np. WeaponItem, PotionItem, Item)")]
        public Item itemData;

        [Header("Shop Settings")]
        [Tooltip("Koszt przedmiotu w kapslach")]
        public int cost = 10;

        [Tooltip("Ilość sztuk otrzymywana przy zakupie (np. 5 dla strzał)")]
        public int amountToGive = 1;

        [Tooltip("Opcjonalny własny opis, jeśli chcemy nadpisać opis z przedmiotu")]
        [TextArea] public string customDescription = "";

        public Item GetItemData()
        {
            if (itemData != null) return itemData;

            // Spróbuj znaleźć po nazwie assetu sklepowego
            string cleanName = name.Replace("_ShopItem", "").Trim();
            itemData = Resources.Load<Item>($"Items/{cleanName}");
            if (itemData == null)
            {
                itemData = Resources.Load<Item>(cleanName);
            }
            return itemData;
        }

        public string GetName()
        {
            Item item = GetItemData();
            if (item != null && !string.IsNullOrEmpty(item.itemName))
            {
                return amountToGive > 1 ? $"{item.itemName} x{amountToGive}" : item.itemName;
            }

            // Fallback: sformatowana nazwa assetu
            string fallbackName = name.Replace("_ShopItem", "").Replace("Item", "").Trim();
            return amountToGive > 1 ? $"{fallbackName} x{amountToGive}" : fallbackName;
        }

        public Sprite GetIcon()
        {
            Item item = GetItemData();
            if (item != null && item.icon != null) return item.icon;

            // Fallback: szukanie sprite'a po nazwie
            string cleanName = name.Replace("_ShopItem", "").Trim();
            Sprite spr = Resources.Load<Sprite>($"Items/{cleanName}");
            if (spr == null) spr = Resources.Load<Sprite>(cleanName);
            return spr;
        }

        public string GetDescription()
        {
            if (!string.IsNullOrEmpty(customDescription)) return customDescription;
            Item item = GetItemData();
            return item != null ? item.description : "";
        }
    }
}
