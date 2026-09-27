using UnityEngine;

namespace Praktyki.Wlocznia
{
    /// <summary>
    /// Skrypt obsługujący podnoszenie Włóczni w świecie gry klawiszem [E].
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class WloczniaPickup : MonoBehaviour, IInteractable
    {
        [Header("Przedmiot")]
        [SerializeField] private Item spearItem;

        [Header("Wizualia")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        private bool isCollected = false;

        public string InteractionPrompt => "Podnieś Włócznię [E]";

        private void Awake()
        {
            EnsureItemReference();
            EnsureVisuals();
            EnsureVisualScale();
        }

        private void Start()
        {
            EnsureItemReference();
            EnsureVisuals();
            EnsureVisualScale();
        }

        private void OnValidate()
        {
            EnsureItemReference();
            EnsureVisuals();
            EnsureVisualScale();
        }

        private void EnsureItemReference()
        {
            if (spearItem == null)
            {
                spearItem = Resources.Load<Item>("Items/Wlocznia");
            }
        }

        private void EnsureVisuals()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null && spearItem != null && spearItem.icon != null)
            {
                spriteRenderer.sprite = spearItem.icon;
            }
        }

        private void EnsureVisualScale()
        {
            // Włócznia jest długa, więc upewniamy się, że w świecie gry ma odpowiedni rozmiar
            if (transform.localScale == Vector3.one)
            {
                transform.localScale = new Vector3(0.045f, 0.045f, 0.045f);
            }
        }

        public bool Interact(GameObject interactor)
        {
            if (isCollected) return false;

            EnsureItemReference();

            if (spearItem == null)
            {
                Debug.LogError("[WloczniaPickup] Brak referencji do spearItem (Wlocznia)!");
                return false;
            }

            if (Inventory.instance == null)
            {
                Debug.LogError("[WloczniaPickup] Brak instancji Inventory w scenie!");
                return false;
            }

            bool added = Inventory.instance.Add(spearItem, 1);

            if (added)
            {
                isCollected = true;
                Debug.Log($"<color=#32CD32>[WloczniaPickup] Podniesiono: {spearItem.itemName}</color>");
                Destroy(gameObject);
                return true;
            }
            else
            {
                Debug.LogWarning("[WloczniaPickup] Ekwipunek jest pełny! Nie można podnieść włóczni.");
                return false;
            }
        }
    }
}
