using UnityEngine;

namespace Praktyki.MlekoStarego
{
    [CreateAssetMenu(fileName = "MlekoStarego_Item", menuName = "Inventory/Mleko Starego")]
    public class MlekoStaregoItem : EquippableItem
    {
        [Header("Mleko Starego - Ochrona")]
        [Tooltip("Czas trwania odporności na truciznę w sekundach (10s)")]
        [SerializeField] private float immunityDuration = 10.0f;

        [Tooltip("Czy zdejmuje również spowolnienie ze szyszek Starego Chmielnika")]
        [SerializeField] private bool cleanseSlow = true;

        [Header("Efekty Dźwiękowe i Cząsteczkowe")]
        [SerializeField] private AudioClip drinkSound;
        [SerializeField] private GameObject drinkEffectPrefab;

        public float ImmunityDuration => immunityDuration;

        public override void Use(PlayerController player, PlayerHand hand)
        {
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) player = playerObj.GetComponent<PlayerController>();
            }

            // Aktywacja odporności oraz oczyszczenie organizmu z trucizny i spowolnienia
            MlekoStaregoBuff.ApplyBuff(immunityDuration, cleanseSlow);

            // Odtworzenie dźwięku wypicia
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
