using UnityEngine;
using UnityEngine.InputSystem;

namespace Praktyki.MlotekDoKotletow
{
    /// <summary>
    /// Skrypt obsługujący podnoszenie Młotka Do Kotletów w świecie gry.
    /// Zapewnia podnoszenie klawiszem [E] (zarówno przez Raycast IInteractable,
    /// jak i przez naciśnięcie [E] w pobliżu młotka).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class MlotekDoKotletowPickup : MonoBehaviour, IInteractable
    {
        [Header("Przedmiot")]
        [SerializeField] private Item hammerItem;

        [Header("Wizualia")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        private bool isCollected = false;

        public string InteractionPrompt => "Podnieś Młotek Do Kotletów [E]";

        private void Awake()
        {
            EnsureItemReference();
            EnsureVisuals();
        }

        private void Start()
        {
            EnsureItemReference();
            EnsureVisuals();
        }

        private void OnValidate()
        {
            EnsureItemReference();
            EnsureVisuals();
        }

        private void EnsureItemReference()
        {
            if (hammerItem == null)
            {
                hammerItem = Resources.Load<Item>("Items/MlotekDoKotletow");
            }
        }

        private void EnsureVisuals()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null && hammerItem != null && hammerItem.icon != null)
            {
                spriteRenderer.sprite = hammerItem.icon;
            }
        }

        public bool Interact(GameObject interactor)
        {
            if (isCollected) return false;
            return Collect(interactor);
        }

        private bool Collect(GameObject player)
        {
            if (isCollected) return false;

            EnsureItemReference();

            if (Inventory.instance == null)
            {
                Debug.LogWarning("[Młotek Do Kotletów] Brak instancji Inventory w scenie!");
                return false;
            }

            if (hammerItem == null)
            {
                Debug.LogError("[Młotek Do Kotletów] Nie znaleziono assetu MlotekDoKotletow w Resources/Items!");
                return false;
            }

            bool added = Inventory.instance.Add(hammerItem, 1);
            if (added)
            {
                isCollected = true;
                Debug.Log("<color=#39FF14>[Młotek Do Kotletów] Pomyślnie podniesiono Młotek Do Kotletów do ekwipunku!</color>");
                Destroy(gameObject);
                return true;
            }
            else
            {
                Debug.LogWarning("[Młotek Do Kotletów] Ekwipunek jest pełny!");
                return false;
            }
        }
    }
}
