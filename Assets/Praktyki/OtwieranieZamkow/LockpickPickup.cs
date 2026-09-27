using UnityEngine;

namespace Praktyki.OtwieranieZamkow
{
    [RequireComponent(typeof(Collider))]
    public class LockpickPickup : MonoBehaviour, IInteractable
    {
        [Header("Item Config")]
        [SerializeField] private Item lockpickItem;
        [SerializeField] private int amount = 1;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private bool billboardToCamera = true;
        [SerializeField] private bool bobbingAnimation = true;
        [SerializeField] private float bobbingSpeed = 2.5f;
        [SerializeField] private float bobbingHeight = 0.05f;

        private Vector3 startLocalPos;
        private bool isCollected = false;

        public string InteractionPrompt => amount > 1 ? $"Podnieś Wytrychy x{amount} [E]" : "Podnieś Wytrych [E]";

        private void Awake()
        {
            startLocalPos = transform.position;
            EnsureReferences();
        }

        private void Start()
        {
            startLocalPos = transform.position;
            EnsureReferences();
        }

        private void Update()
        {
            if (billboardToCamera && Camera.main != null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
            }

            if (bobbingAnimation)
            {
                float newY = startLocalPos.y + Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
        }

        private void OnValidate()
        {
            EnsureReferences();
        }

        private void EnsureReferences()
        {
            if (lockpickItem == null)
            {
                lockpickItem = Resources.Load<Item>("Items/Wytrych");
                if (lockpickItem == null)
                {
                    lockpickItem = Resources.Load<Item>("Wytrych");
                }
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer != null && lockpickItem != null && lockpickItem.icon != null)
            {
                spriteRenderer.sprite = lockpickItem.icon;
            }
        }

        public bool Interact(GameObject interactor)
        {
            if (isCollected) return false;

            EnsureReferences();

            if (lockpickItem == null)
            {
                Debug.LogError("[LockpickPickup] Brak przypisanego przedmiotu Wytrych!");
                return false;
            }

            if (Inventory.instance == null)
            {
                Debug.LogError("[LockpickPickup] Brak instancji Inventory w scenie!");
                return false;
            }

            bool added = Inventory.instance.Add(lockpickItem, amount);
            if (added)
            {
                isCollected = true;
                Debug.Log($"<color=#32CD32>[LockpickPickup] Podniesiono wytrych x{amount}!</color>");
                Destroy(gameObject);
                return true;
            }
            else
            {
                Debug.LogWarning("[LockpickPickup] Ekwipunek jest pełny!");
                return false;
            }
        }
    }
}
