using UnityEngine;

namespace Praktyki.PitnaWiedza
{
    /// <summary>
    /// Skrypt mikstury "Pitna Wiedza" w świecie gry (World Pickup).
    /// Automatycznie obraca się przodem do kamery gracza (Billboard), bez efektu magnesu.
    /// Nie podnosi się automatycznie przy wejściu w kolizję – podnosi się wyłącznie po kliknięciu 'E' (interakcja) do ekwipunku gracza (EQ).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PitnaWiedzaPotion : MonoBehaviour, IInteractable
    {
        [Header("Przedmiot do Ekwipunku")]
        [Tooltip("Referencja do przedmiotu PitnaWiedza, który trafia do EQ")]
        [SerializeField] private Item itemData;

        [Header("Efekty Wizualne i Dźwiękowe")]
        [SerializeField] private GameObject pickupEffectPrefab;
        [SerializeField] private AudioClip pickupSound;

        private bool isCollected = false;

        public string InteractionPrompt => itemData != null ? $"Podnieś {itemData.itemName}" : "Podnieś miksturę (E)";

        private void Start()
        {
            // Automatycznie dodajemy komponent Billboard
            if (GetComponent<Billboard>() == null)
            {
                gameObject.AddComponent<Billboard>();
            }
        }

        /// <summary>
        /// Podniesienie mikstury wyłącznie po wciśnięciu klawisza interakcji 'E'.
        /// </summary>
        public bool Interact(GameObject interactor)
        {
            if (isCollected) return false;
            return Collect(interactor);
        }

        private bool Collect(GameObject player)
        {
            if (Inventory.instance == null)
            {
                Debug.LogWarning("[Pitna Wiedza] Brak instancji Inventory w scenie!");
                return false;
            }

            // Fallback na ItemPickup jeśli nie przypisano itemData
            if (itemData == null)
            {
                ItemPickup pickup = GetComponent<ItemPickup>();
                if (pickup != null)
                {
                    return pickup.Interact(player);
                }
            }

            if (itemData != null)
            {
                bool added = Inventory.instance.Add(itemData);
                if (!added)
                {
                    Debug.Log("[Pitna Wiedza] Ekwipunek jest pełny! Nie można podnieść przedmiotu.");
                    return false;
                }
            }

            isCollected = true;
            Debug.Log("<color=#FFD700><b>[Pitna Wiedza]</b></color> Podniesiono Pitną Wiedzę do ekwipunku [E]!");

            // Efekty cząsteczkowe i dźwięk podniesienia
            if (pickupEffectPrefab != null)
            {
                Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            }

            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            Destroy(gameObject);
            return true;
        }
    }
}
