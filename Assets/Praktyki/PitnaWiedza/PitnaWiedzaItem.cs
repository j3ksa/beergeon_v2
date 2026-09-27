using UnityEngine;

namespace Praktyki.PitnaWiedza
{
    [CreateAssetMenu(fileName = "PitnaWiedza_Item", menuName = "Inventory/Pitna Wiedza")]
    public class PitnaWiedzaItem : EquippableItem
    {
        [Header("Pitna Wiedza - Poziomy")]
        [Tooltip("Liczba poziomów przyznawana po wypiciu mikstury")]
        [SerializeField] private int levelsToAdd = 1;

        [Header("Efekty Dźwiękowe i Wizualne")]
        [SerializeField] private AudioClip drinkSound;
        [SerializeField] private GameObject drinkEffectPrefab;

        public int LevelsToAdd => levelsToAdd;

        public override void Use(PlayerController player, PlayerHand hand)
        {
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) player = playerObj.GetComponent<PlayerController>();
            }

            // Znajdź komponent PlayerStats
            PlayerStats stats = null;
            if (player != null)
            {
                stats = player.GetComponent<PlayerStats>();
            }

            if (stats == null)
            {
                stats = Object.FindFirstObjectByType<PlayerStats>();
            }

            // Bezpośredni awans o +1 poziom
            if (stats != null)
            {
                stats.LevelUpDirectly(levelsToAdd);
                Debug.Log($"<color=#FFD700><b>[Pitna Wiedza]</b></color> Gracz wypił Pitną Wiedzę! Poziom wzrósł o +{levelsToAdd} (Aktualny poziom: {stats.Level})!");
            }
            else
            {
                Debug.LogWarning("[Pitna Wiedza] Nie znaleziono komponentu PlayerStats na obiekcie gracza!");
            }

            // Złoty rozbłysk wiedzy na ekranie UI
            PitnaWiedzaUI.TriggerFlash();

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

            // Usunięcie potki z ekwipunku po użyciu
            if (Inventory.instance != null)
            {
                Inventory.instance.Remove(this);
            }
        }
    }
}
