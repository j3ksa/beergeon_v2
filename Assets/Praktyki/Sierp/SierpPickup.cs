using UnityEngine;

namespace Praktyki.Sierp
{
    /// <summary>
    /// Obsługuje interakcję podnoszenia Sierpa w świecie gry klawiszem [E].
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SierpPickup : MonoBehaviour, IInteractable
    {
        [Header("Przedmiot")]
        [SerializeField] private Item sierpItem;

        [Header("Wizualia")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        private bool isCollected = false;

        public string InteractionPrompt => "Podnieś Sierp [E]";

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
            if (sierpItem == null)
            {
                sierpItem = Resources.Load<Item>("Items/Sierp");
            }
        }

        private void EnsureVisuals()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null && sierpItem != null && sierpItem.icon != null)
            {
                spriteRenderer.sprite = sierpItem.icon;
            }
        }

        private void EnsureVisualScale()
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true; // Zapobiega kolizjom fizycznym blokującym gracza
            }
        }

        public bool CanInteract(GameObject interactor)
        {
            if (isCollected) return false;
            return true;
        }

        public bool Interact(GameObject interactor)
        {
            if (isCollected) return false;

            EnsureItemReference();
            if (sierpItem == null)
            {
                Debug.LogError("[SierpPickup] Brak przypisanego przedmiotu Item!");
                return false;
            }

            if (Inventory.instance == null)
            {
                Debug.LogError("[SierpPickup] Brak instancji Inventory w scenie!");
                return false;
            }

            bool added = Inventory.instance.Add(sierpItem, 1);
            if (added)
            {
                isCollected = true;
                Debug.Log($"<color=#32CD32>[SierpPickup]</color> Gracz podniósł broń: {sierpItem.itemName}!");
                Destroy(gameObject);
                return true;
            }
            else
            {
                Debug.LogWarning("[SierpPickup] Ekwipunek jest pełny! Nie można podnieść sierpa.");
                return false;
            }
        }
    }
}
