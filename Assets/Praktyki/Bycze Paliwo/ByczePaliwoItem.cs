using UnityEngine;

namespace Praktyki.ByczePaliwo
{
    [CreateAssetMenu(fileName = "ByczePaliwo_Item", menuName = "Inventory/Bycze Paliwo")]
    public class ByczePaliwoItem : EquippableItem
    {
        [Header("Bycze Paliwo - Efekty")]
        [Tooltip("Czy ma natychmiast odnowić całą staminę na 100%")]
        [SerializeField] private bool instantFullStamina = true;

        [Tooltip("Ilość odnawianej staminy (używana tylko gdy Instant Full Stamina jest wyłączone)")]
        [SerializeField] private float customStaminaAmount = 30f;

        [Header("Efekty Dźwiękowe i Wizualne")]
        [SerializeField] private AudioClip drinkSound;
        [SerializeField] private GameObject drinkEffectPrefab;

        public override void Use(PlayerController player, PlayerHand hand)
        {
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) player = playerObj.GetComponent<PlayerController>();
            }

            // Znajdź komponent PlayerStamina
            PlayerStamina stamina = null;
            if (player != null)
            {
                stamina = player.GetComponent<PlayerStamina>();
            }

            if (stamina == null)
            {
                stamina = Object.FindFirstObjectByType<PlayerStamina>();
            }

            // Odnawiamy staminę
            if (stamina != null)
            {
                if (instantFullStamina)
                {
                    stamina.RestoreStaminaToFull();
                    Debug.Log("<color=#FFD700><b>[Bycze Paliwo]</b></color> Gracz wypił Bycze Paliwo! Stamina natychmiastowo odnowiona na MAX (100%)!");
                }
                else
                {
                    stamina.RestoreStamina(customStaminaAmount);
                    Debug.Log($"<color=#FFD700><b>[Bycze Paliwo]</b></color> Gracz wypił Bycze Paliwo! Odnowiono {customStaminaAmount} staminy.");
                }
            }
            else
            {
                Debug.LogWarning("[Bycze Paliwo] Nie znaleziono komponentu PlayerStamina!");
            }

            // Wywołujemy efekt błysku energii na ekranie UI
            ByczePaliwoUI.TriggerFlash();

            // Odtworzenie dźwięku picia
            if (drinkSound != null)
            {
                Vector3 soundPos = player != null ? player.transform.position : Camera.main.transform.position;
                AudioSource.PlayClipAtPoint(drinkSound, soundPos);
            }

            // Efekt cząsteczkowy
            if (drinkEffectPrefab != null && player != null)
            {
                Instantiate(drinkEffectPrefab, player.transform.position, Quaternion.identity);
            }

            // Usunięcie potki z ekwipunku po wypiciu
            if (Inventory.instance != null)
            {
                Inventory.instance.Remove(this);
            }
        }
    }
}
