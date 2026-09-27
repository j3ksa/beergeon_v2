using UnityEngine;
using UnityEngine.InputSystem;

namespace Praktyki.ZatrutyMiecz
{
    /// <summary>
    /// Skrypt obsługujący podnoszenie Zatrutego Miecza w świecie gry.
    /// Zapewnia podnoszenie klawiszem [E] (zarówno przez Raycast IInteractable,
    /// jak i przez naciśnięcie [E] w pobliżu miecza), z automatycznym pobieraniem danych przedmiotu.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ZatrutyMieczPickup : MonoBehaviour, IInteractable
    {
        [Header("Przedmiot")]
        [SerializeField] private Item swordItem;

        [Header("Wizualia")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        private bool isCollected = false;

        public string InteractionPrompt => "Podnieś Zatruty Miecz [E]";

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
            if (swordItem == null)
            {
                swordItem = Resources.Load<Item>("Items/ZatrutyMiecz");
            }
        }

        private void EnsureVisuals()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null && swordItem != null && swordItem.icon != null)
            {
                spriteRenderer.sprite = swordItem.icon;
            }
        }

        private void EnsureVisualScale()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                float maxDim = Mathf.Max(spriteRenderer.sprite.bounds.size.x, spriteRenderer.sprite.bounds.size.y);
                if (maxDim > 0.1f)
                {
                    // Ustalona wielkość miecza w świecie gry: 1.0 jednostka (dokładnie jak w pierwotnej wersji)
                    float targetSize = 1.0f;
                    float scale = targetSize / maxDim;
                    transform.localScale = Vector3.one * scale;
                }
            }
            else
            {
                // Bezpieczny fallback dla skali
                if (transform.localScale.x > 0.2f)
                {
                    transform.localScale = new Vector3(0.046f, 0.046f, 0.046f);
                }
            }
        }

        /// <summary>
        /// Wywoływane przez system PlayerInteraction podczas wycelowania i naciśnięcia [E].
        /// </summary>
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
                Debug.LogWarning("[Zatruty Miecz] Brak instancji Inventory w scenie!");
                return false;
            }

            if (swordItem == null)
            {
                Debug.LogError("[Zatruty Miecz] Nie znaleziono assetu ZatrutyMiecz w Resources/Items!");
                return false;
            }

            bool added = Inventory.instance.Add(swordItem, 1);
            if (added)
            {
                isCollected = true;
                Debug.Log("<color=#39FF14>[Zatruty Miecz] Pomyślnie podniesiono Zatruty Miecz do ekwipunku!</color>");
                Destroy(gameObject);
                return true;
            }
            else
            {
                Debug.LogWarning("[Zatruty Miecz] Ekwipunek jest pełny!");
                return false;
            }
        }
    }
}
