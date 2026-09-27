using UnityEngine;

namespace Praktyki.SpeedPotion
{
    [CreateAssetMenu(fileName = "New Speed Potion", menuName = "Inventory/Speed Potion")]
    public class SpeedPotionItem : EquippableItem
    {
        [Header("Speed Potion Settings")]
        [Tooltip("Mnożnik prędkości gracza (np. 1.5 = +50% prędkości)")]
        [SerializeField] private float speedMultiplier = 1.5f;

        [Tooltip("Czas trwania efektu przyspieszenia w sekundach")]
        [SerializeField] private float duration = 6f;

        public float SpeedMultiplier => speedMultiplier;
        public float Duration => duration;

        public override void Use(PlayerController player, PlayerHand hand)
        {
            if (player == null) return;

            // Pobieramy lub dodajemy komponent zarządzający buffem prędkości na graczu
            PlayerSpeedBuff speedBuff = player.GetComponent<PlayerSpeedBuff>();
            if (speedBuff == null)
            {
                speedBuff = player.gameObject.AddComponent<PlayerSpeedBuff>();
            }

            // Aktywujemy buff przyspieszenia
            speedBuff.ApplySpeedBuff(speedMultiplier, duration);

            Debug.Log($"[SpeedPotion] Wypito potkę przyspieszenia! Prędkość x{speedMultiplier} na {duration}s.");

            // Usunięcie potki z ekwipunku po użyciu
            if (Inventory.instance != null)
            {
                Inventory.instance.Remove(this);
            }
        }
    }
}
