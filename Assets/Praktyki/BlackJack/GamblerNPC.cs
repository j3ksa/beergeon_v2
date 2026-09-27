using UnityEngine;

namespace BlackJack
{
    public class GamblerNPC : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float interactionRadius = 5.47f;
        [SerializeField] private bool allowMouseClick = true;
        [SerializeField] private bool allowKeyInteraction = true;
#pragma warning disable 0414
        [SerializeField] private KeyCode interactKey = KeyCode.E;
#pragma warning restore 0414
        [SerializeField] private float lookAngleThreshold = 0.35f;

        [Header("Prompt UI (Opcjonalny dymek nad głową)")]
        [SerializeField] private GameObject interactionPromptUI;

        [Header("Direct Reference to UI (Opcjonalne)")]
        [SerializeField] private BlackJackUI targetBlackJackUI;

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
            BlackJackUI ui = GetBlackJackUI();
            if (ui != null && ui.IsOpen)
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

        public bool CheckDistanceToPlayer()
        {
            Camera cam = Camera.main;
            if (cam == null) return false;

            float effectiveRadius = interactionRadius;
            SphereCollider sc = GetComponent<SphereCollider>();
            if (sc != null)
            {
                effectiveRadius = sc.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
            }

            float dist = Vector3.Distance(transform.position, cam.transform.position);
            return dist <= effectiveRadius;
        }

        public bool IsPlayerLookingAtGambler()
        {
            Camera cam = Camera.main;
            if (cam == null) return true;

            Vector3 dirToGambler = (transform.position - cam.transform.position).normalized;
            float dot = Vector3.Dot(cam.transform.forward, dirToGambler);
            return (dot >= lookAngleThreshold);
        }

        private BlackJackUI GetBlackJackUI()
        {
            if (targetBlackJackUI != null) return targetBlackJackUI;
            return BlackJackUI.Instance;
        }

        public void OpenBlackJack()
        {
            BlackJackUI ui = GetBlackJackUI();
            if (ui != null)
            {
                ui.OpenGame();
                HidePromptUI();
            }
            else
            {
                Debug.LogWarning("[GamblerNPC] Nie znaleziono komponentu BlackJackUI na scenie!");
            }
        }

        /// <summary>
        /// Wywoływane automatycznie przez silnik Unity, gdy gracz kliknie myszką w Collider postaci.
        /// </summary>
        private void OnMouseDown()
        {
            if (!allowMouseClick) return;

            // Sprawdzamy czy gracz jest w rozsądnej odległości
            if (CheckDistanceToPlayer())
            {
                OpenBlackJack();
            }
        }

        private void Update()
        {
            BlackJackUI ui = GetBlackJackUI();
            if (ui != null && ui.IsOpen)
            {
                HidePromptUI();
                return;
            }

            bool inRange = CheckDistanceToPlayer();
            bool isLooking = IsPlayerLookingAtGambler();
            bool canInteract = inRange && isLooking;

            if (canInteract)
            {
                ShowPromptUI();
            }
            else
            {
                HidePromptUI();
            }

            // Obsługa kliknięcia myszką poprzez raycast z kamery (dla trybów z odblokowanym kursorem lub FPS)
            if (allowMouseClick && inRange)
            {
                bool mouseClicked = false;
#if ENABLE_INPUT_SYSTEM
                if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
                {
                    mouseClicked = true;
                }
#else
                if (Input.GetMouseButtonDown(0))
                {
                    mouseClicked = true;
                }
#endif
                if (mouseClicked)
                {
                    Camera cam = Camera.main;
                    if (cam != null)
                    {
                        Ray ray = cam.ScreenPointToRay(
#if ENABLE_INPUT_SYSTEM
                            UnityEngine.InputSystem.Mouse.current != null ? UnityEngine.InputSystem.Mouse.current.position.ReadValue() : (Vector2)Input.mousePosition
#else
                            Input.mousePosition
#endif
                        );

                        if (Physics.Raycast(ray, out RaycastHit hit, interactionRadius * 2f))
                        {
                            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                            {
                                OpenBlackJack();
                                return;
                            }
                        }
                    }
                }
            }

            // Obsługa klawisza E
            if (allowKeyInteraction && canInteract)
            {
                bool keyTriggered = false;
#if ENABLE_INPUT_SYSTEM
                if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
                {
                    keyTriggered = true;
                }
#else
                if (Input.GetKeyDown(interactKey))
                {
                    keyTriggered = true;
                }
#endif
                if (keyTriggered)
                {
                    OpenBlackJack();
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}
