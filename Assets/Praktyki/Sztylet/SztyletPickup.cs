using UnityEngine;

namespace Praktyki.Sztylet
{
    /// <summary>
    /// Skrypt obsługujący podnoszenie Sztyletu w świecie gry klawiszem [E].
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SztyletPickup : MonoBehaviour, IInteractable
    {
        [Header("Przedmiot")]
        [SerializeField] private Item daggerItem;

        [Header("Wizualia")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        private bool isCollected = false;

        public string InteractionPrompt => "Podnieś Sztylet [E]";

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
            if (daggerItem == null)
            {
                daggerItem = Resources.Load<Item>("Items/Sztylet");
            }
        }

        private void EnsureVisuals()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null && daggerItem != null && daggerItem.icon != null)
            {
                spriteRenderer.sprite = daggerItem.icon;
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
                    float targetSize = 0.85f;
                    float scale = targetSize / maxDim;
                    transform.localScale = Vector3.one * scale;
                }
            }
            else
            {
                if (transform.localScale.x > 0.2f)
                {
                    transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
                }
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
                Debug.LogWarning("[Sztylet] Brak instancji Inventory w scenie!");
                return false;
            }

            if (daggerItem == null)
            {
                Debug.LogError("[Sztylet] Nie znaleziono assetu Sztylet w Resources/Items!");
                return false;
            }

            bool added = Inventory.instance.Add(daggerItem, 1);
            if (added)
            {
                isCollected = true;
                Debug.Log("<color=#FFD700>[Sztylet] Pomyślnie podniesiono Sztylet do ekwipunku!</color>");
                Destroy(gameObject);
                return true;
            }
            else
            {
                Debug.LogWarning("[Sztylet] Ekwipunek jest pełny!");
                return false;
            }
        }
    }
}
