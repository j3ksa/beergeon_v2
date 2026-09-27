using UnityEngine;

namespace Praktyki.OgnistaWoda
{
    [CreateAssetMenu(fileName = "OgnistaWoda_Item", menuName = "Inventory/Ognista Woda")]
    public class OgnistaWodaItem : EquippableItem
    {
        [Header("Ognista Woda - Redukcja Obrażeń")]
        [Tooltip("Procent redukcji otrzymywanych obrażeń (0.25 = -25% obrażeń)")]
        [SerializeField] [Range(0.05f, 0.95f)] private float damageReductionPercent = 0.25f;

        [Tooltip("Czas trwania buffa w sekundach")]
        [SerializeField] private float duration = 10.0f;

        [Header("Efekty Dźwiękowe i Cząsteczkowe")]
        [SerializeField] private AudioClip drinkSound;
        [SerializeField] private GameObject drinkEffectPrefab;

        public float DamageReductionPercent => damageReductionPercent;
        public float Duration => duration;

        public override void Use(PlayerController player, PlayerHand hand)
        {
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) player = playerObj.GetComponent<PlayerController>();
            }

            // Aktywacja buffa redukcji obrażeń na graczu
            OgnistaWodaBuff.ApplyBuff(damageReductionPercent, duration);

            // Odtworzenie dźwięku
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

            // Usunięcie z ekwipunku po wypiciu
            if (Inventory.instance != null)
            {
                Inventory.instance.Remove(this);
            }
        }
    }
}
