using UnityEngine;

namespace Praktyki.SlepySzal
{
    [CreateAssetMenu(fileName = "SlepySzal_Item", menuName = "Inventory/Ślepy Szał")]
    public class SlepySzalItem : EquippableItem
    {
        [Header("Ślepy Szał - Statystyki")]
        [Tooltip("Mnożnik obrażeń (3.0 = +200% obrażeń, czyli 3x mocniejszy atak)")]
        [SerializeField] private float damageMultiplier = 3.0f;

        [Tooltip("Mnożnik ubytku zdrowia (3.0 = 3x szybszy pasywny spadek HP)")]
        [SerializeField] private float decayMultiplier = 3.0f;

        [Header("Konfiguracja Pola Widzenia (Regulacja w Inspektorze)")]
        [Tooltip("Promień czystego widoku na środku ekranu (domyślnie 0.25 = 25% widoczności)")]
        [Range(0.01f, 1.50f)]
        [SerializeField] private float visionClearRadius = 0.25f;

        [Tooltip("Promień, przy którym ekran staje się w 100% całkowicie czarny")]
        [Range(0.05f, 2.00f)]
        [SerializeField] private float visionOuterRadius = 0.48f;

        [Header("Efekty Dźwiękowe i Wizualne")]
        [SerializeField] private AudioClip drinkSound;
        [SerializeField] private GameObject drinkEffectPrefab;

        public float DamageMultiplier => damageMultiplier;
        public float DecayMultiplier => decayMultiplier;
        public float VisionClearRadius => visionClearRadius;
        public float VisionOuterRadius => visionOuterRadius;

        private void OnValidate()
        {
            // Natychmiastowe odświeżenie winiety na ekranie w trybie gry, gdy zmieniasz suwaki w pliku SlepySzal.asset!
            if (Application.isPlaying && SlepySzalBuff.IsActive)
            {
                SlepySzalUI.SetVisionRadius(visionClearRadius, visionOuterRadius);
            }
        }

        public override void Use(PlayerController player, PlayerHand hand)
        {
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) player = playerObj.GetComponent<PlayerController>();
            }

            // Aktywacja trwałego Ślepego Szału na graczu z podanymi parametrami widzenia
            SlepySzalBuff.ApplyBuff(damageMultiplier, decayMultiplier, visionClearRadius, visionOuterRadius);

            // Dźwięk
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
