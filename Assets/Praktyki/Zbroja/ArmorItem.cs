using UnityEngine;

namespace Praktyki.Zbroja
{
    public enum ArmorSlotType
    {
        Helmet,     // Hełm
        Chestplate, // Klata
        Leggings,   // Spodnie
        Boots       // Buty
    }

    [CreateAssetMenu(fileName = "New Armor", menuName = "Inventory/Armor Item")]
    public class ArmorItem : EquippableItem
    {
        [Header("Armor Configuration")]
        public ArmorSlotType slotType;

        [Tooltip("Punkty pancerza/obrony (np. Hełm: 2, Klata: 5, Spodnie: 3, Buty: 2)")]
        public int defense = 2;

        [Tooltip("Bonus do maksymalnego zdrowia")]
        public int bonusHealth = 0;

        [Tooltip("Bonus lub kara do prędkości poruszania się (np. -0.06 = -6% prędkości za ciężką zbroję)")]
        public float speedBonus = 0f;

        [Tooltip("Kara do zużycia staminy podczas sprintu (np. 0.15 = +15% szybsze zużywanie staminy)")]
        public float staminaDrainPenalty = 0f;

        public override void Use(PlayerController player, PlayerHand hand)
        {
            // Opcjonalne użycie
        }
    }
}
