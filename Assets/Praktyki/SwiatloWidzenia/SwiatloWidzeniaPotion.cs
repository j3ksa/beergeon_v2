using UnityEngine;

namespace Praktyki.SwiatloWidzenia
{
    /// <summary>
    /// Mikstura "Światło Widzenia" leżąca w świecie gry.
    /// Posiada Billboard (obraca się do kamery), nie posiada magnesu.
    /// Zgodnie z wytycznymi podnosi się wyłącznie po wciśnięciu klawisza 'E' (IInteractable) do ekwipunku gracza.
    /// Wejście ciałem w butelkę nic nie robi.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SwiatloWidzeniaPotion : MonoBehaviour, IInteractable
    {
        [Header("Przedmiot do Ekwipunku")]
        [Tooltip("Referencja do przedmiotu SwiatloWidzenia, który trafi do ekwipunku")]
        [SerializeField] private Item itemData;

        [Header("Efekty Wizualne i Dźwiękowe")]
        [SerializeField] private GameObject pickupEffectPrefab;
        [SerializeField] private AudioClip pickupSound;

        private bool isCollected = false;

        public string InteractionPrompt => itemData != null ? $"Podnieś {itemData.itemName} (E)" : "Podnieś Światło Widzenia (E)";

        private void Start()
        {
            if (GetComponent<Billboard>() == null)
            {
                gameObject.AddComponent<Billboard>();
            }
        }

        public bool Interact(GameObject interactor)
        {
            if (isCollected) return false;
            return Collect(interactor);
        }

        private bool Collect(GameObject player)
        {
            if (Inventory.instance == null)
            {
                Debug.LogWarning("[Światło Widzenia] Brak instancji Inventory w scenie!");
                return false;
            }

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
                    Debug.Log("[Światło Widzenia] Ekwipunek pełny! Nie można podnieść przedmiotu.");
                    return false;
                }
            }

            isCollected = true;
            Debug.Log("<color=#00BFFF><b>[Światło Widzenia]</b></color> Podniesiono Światło Widzenia do ekwipunku [E]!");

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
