using UnityEngine;

namespace ShopSystem
{
    public class MerchantNPC : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
#pragma warning disable 0414
        [SerializeField] private KeyCode interactKey = KeyCode.E;
#pragma warning restore 0414
        [SerializeField] private GameObject interactionPromptUI; // Dymek "[E] SHOP" nad paskiem LVL
        [SerializeField] private ShopManagerUI targetShopUI;
        [SerializeField] private float lookAngleThreshold = 0.35f; // Wykrywanie patrzenia na handlarza

        private void Awake()
        {
            EnsurePromptUIFound();
            HidePromptUI();
        }

        private void Start()
        {
            EnsurePromptUIFound();
            HidePromptUI();
        }

        private void EnsurePromptUIFound()
        {
            if (interactionPromptUI == null)
            {
                Canvas childCanvas = GetComponentInChildren<Canvas>(true);
                if (childCanvas != null)
                {
                    interactionPromptUI = childCanvas.gameObject;
                }
            }
        }

        private void ShowPromptUI()
        {
            EnsurePromptUIFound();
            ShopManagerUI manager = GetShopManager();
            if (manager != null && manager.IsOpen)
            {
                HidePromptUI();
                return;
            }

            if (interactionPromptUI != null && !interactionPromptUI.activeSelf)
            {
                interactionPromptUI.SetActive(true);
            }
        }

        private void HidePromptUI()
        {
            EnsurePromptUIFound();
            if (interactionPromptUI != null && interactionPromptUI.activeSelf)
            {
                interactionPromptUI.SetActive(false);
            }
        }

        public string InteractionPrompt => "[E] Shop";

        public bool IsPlayerLookingAtMerchant()
        {
            Camera cam = Camera.main;
            if (cam == null) return true;

            Vector3 dirToMerchant = (transform.position - cam.transform.position).normalized;
            float dot = Vector3.Dot(cam.transform.forward, dirToMerchant);
            return (dot >= lookAngleThreshold);
        }

        public bool CheckDistanceToPlayer()
        {
            Camera cam = Camera.main;
            if (cam == null) return false;

            // Dokładne wyliczenie promienia zielonej kuli (SphereCollider) w świecie 3D:
            SphereCollider sc = GetComponent<SphereCollider>();
            float effectiveRadius = 2.5f;

            if (sc != null)
            {
                // Promień kuli pomnożony przez skalę obiektu Handlarza (0.089)
                effectiveRadius = sc.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
            }

            float dist = Vector3.Distance(transform.position, cam.transform.position);
            return dist <= effectiveRadius;
        }

        private ShopManagerUI GetShopManager()
        {
            if (targetShopUI != null) return targetShopUI;
            ShopManagerUI manager = ShopManagerUI.Instance;
            if (manager == null)
            {
                manager = Object.FindFirstObjectByType<ShopManagerUI>(FindObjectsInactive.Include);
            }
            return manager;
        }

        public bool Interact(GameObject interactor)
        {
            // BEZWZGLĘDNA ZASADA: Sklep otworzy się TYLKO i WYŁĄCZNIE wtedy,
            // gdy gracz jest w promieniu zielonej kuli ORAZ patrzy na Handlarza!
            if (!CheckDistanceToPlayer() || !IsPlayerLookingAtMerchant())
            {
                return false;
            }

            ShopManagerUI manager = GetShopManager();

            if (manager != null)
            {
                manager.ToggleShop();
                HidePromptUI();
                return true;
            }
            return false;
        }

        private void Update()
        {
            ShopManagerUI manager = GetShopManager();
            if (manager != null && manager.IsOpen)
            {
                HidePromptUI();
                return;
            }

            // Dymek i funkcja Interact() korzystają z dokładnie tego samego warunku:
            bool inRange = CheckDistanceToPlayer();
            bool isLooking = IsPlayerLookingAtMerchant();
            bool canInteract = inRange && isLooking;

            if (canInteract)
            {
                ShowPromptUI();
            }
            else
            {
                HidePromptUI();
            }

            if (!canInteract) return;

            bool interactPressed = false;

#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
            {
                interactPressed = true;
            }
#else
            if (Input.GetKeyDown(interactKey))
            {
                interactPressed = true;
            }
#endif

            if (interactPressed)
            {
                Interact(gameObject);
            }
        }
    }
}
