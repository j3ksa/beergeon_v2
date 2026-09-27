using UnityEngine;

namespace Praktyki.MlekoStarego
{
    /// <summary>
    /// Skrypt mikstury "Mleko Starego" w świecie gry (World Pickup).
    /// Automatycznie obraca się przodem do kamery gracza (Billboard).
    /// Podnosi się WYŁĄCZNIE po wycelowaniu celownikiem i wciśnięciu klawisza interakcji [E] (IInteractable).
    /// Nie podnosi się automatycznie przy wejściu w butelkę (brak OnTriggerEnter).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class MlekoStaregoPotion : MonoBehaviour, IInteractable
    {
        [Header("Przedmiot do Ekwipunku")]
        [Tooltip("Referencja do przedmiotu MlekoStarego trafiającego do ekwipunku")]
        [SerializeField] private Item itemData;

        [Header("Ustawienia Ekwipunku")]
        [Tooltip("Czy przy braku miejsca w EQ gracz natychmiast wypija mleko z ziemi")]
        [SerializeField] private bool drinkDirectlyIfInventoryFull = false;

        [Header("Efekty Wizualne i Dźwiękowe")]
        [SerializeField] private GameObject pickupEffectPrefab;
        [SerializeField] private AudioClip pickupSound;

        private bool isCollected = false;

        public string InteractionPrompt => itemData != null ? $"Podnieś {itemData.itemName} [E]" : "Podnieś Mleko Starego [E]";

        private void Start()
        {
            if (GetComponent<Billboard>() == null)
            {
                gameObject.AddComponent<Billboard>();
            }

            if (itemData == null)
            {
                itemData = Resources.Load<Item>("Items/MlekoStarego");
            }
        }

        /// <summary>
        /// Podniesienie mikstury wyłącznie po wycelowaniu i wciśnięciu [E].
        /// </summary>
        public bool Interact(GameObject interactor)
        {
            if (isCollected) return false;
            return Collect(interactor);
        }

        private bool Collect(GameObject player)
        {
            if (isCollected) return false;

            // Spróbuj dodać do ekwipunku
            bool addedToInventory = false;
            if (itemData == null) itemData = Resources.Load<Item>("Items/MlekoStarego");
            if (Inventory.instance != null && itemData != null)
            {
                addedToInventory = Inventory.instance.Add(itemData);
            }

            if (!addedToInventory)
            {
                if (drinkDirectlyIfInventoryFull)
                {
                    // Wypij bezpośrednio z ziemi jeśli ekwipunek jest pełny
                    MlekoStaregoBuff.ApplyBuff(10.0f, true);
                    Debug.Log("<color=#FFFFFF><b>[Mleko Starego]</b></color> Ekwipunek pełny – Mleko Starego zostało wypite natychmiast!");
                }
                else
                {
                    Debug.Log("[Mleko Starego] Ekwipunek jest pełny! Nie można podnieść przedmiotu.");
                    return false;
                }
            }
            else
            {
                Debug.Log("<color=#FFFFFF><b>[Mleko Starego]</b></color> Podniesiono Mleko Starego do ekwipunku!");
            }

            isCollected = true;

            // Efekty cząsteczkowe i dźwięk
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
