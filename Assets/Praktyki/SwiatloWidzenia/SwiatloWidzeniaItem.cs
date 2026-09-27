using UnityEngine;

namespace Praktyki.SwiatloWidzenia
{
    [CreateAssetMenu(fileName = "SwiatloWidzenia_Item", menuName = "Inventory/Światło Widzenia")]
    public class SwiatloWidzeniaItem : EquippableItem
    {
        [Header("Leczenie i Przywracanie Wzroku")]
        [Tooltip("Opcjonalne leczenie punktów życia (0 = czyste zdjęcie szału)")]
        [SerializeField] private int healthToHeal = 15;

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

            // 1. Natychmiastowe zdjęcie efektu Ślepego Szału (przywrócenie pełnego widzenia, reset ubytku HP i ataku)
            Praktyki.SlepySzal.SlepySzalBuff.CureBuff();

            // 2. Opcjonalne leczenie HP
            if (healthToHeal > 0)
            {
                PlayerHealth ph = player != null ? player.GetComponent<PlayerHealth>() : Object.FindFirstObjectByType<PlayerHealth>();
                if (ph != null)
                {
                    ph.Heal(healthToHeal);
                }
            }

            // 3. Dźwięk picia
            if (drinkSound != null)
            {
                Vector3 soundPos = player != null ? player.transform.position : Camera.main.transform.position;
                AudioSource.PlayClipAtPoint(drinkSound, soundPos);
            }

            // 4. Efekt cząsteczkowy
            if (drinkEffectPrefab != null && player != null)
            {
                Instantiate(drinkEffectPrefab, player.transform.position, Quaternion.identity);
            }

            Debug.Log("<color=#00BFFF><b>[Światło Widzenia]</b></color> Wypito Światło Widzenia! Ślepy Szał został wyleczony, wzrok przywrócony.");

            // 5. Usunięcie z ekwipunku po wypiciu
            if (Inventory.instance != null)
            {
                Inventory.instance.Remove(this);
            }
        }
    }
}
