using System.Collections;
using UnityEngine;

namespace Praktyki.OtwieranieZamkow
{
    [RequireComponent(typeof(Collider))]
    public class PadlockController : MonoBehaviour, IInteractable
    {
        [Header("Door Target")]
        [Tooltip("Drzwi, które ta kłódka blokuje. Jeśli puste, skrypt spróbuje znaleźć DoorController w otoczeniu.")]
        [SerializeField] private DoorController targetDoor;

        [Header("Lockpick Item")]
        [SerializeField] private Item lockpickItem;

        [Header("Audio")]
        [SerializeField] private AudioClip padlockRattleSound;

        [Header("Physics on Unlock")]
        [SerializeField] private bool dropPhysicsOnUnlock = true;
        [SerializeField] private float despawnDelay = 20f;

        private bool isUnlocked = false;
        private Collider col;

        public bool IsUnlocked => isUnlocked;

        public string InteractionPrompt
        {
            get
            {
                if (isUnlocked) return string.Empty;

                EnsureItemReference();
                int count = Inventory.instance != null && lockpickItem != null 
                    ? Inventory.instance.GetTotalItemCount(lockpickItem) 
                    : 0;

                if (count > 0)
                {
                    return $"Otwórz kłódkę (Wytrychy: {count}) [E]";
                }
                else
                {
                    return "Kłódka - Wymaga wytrychu!";
                }
            }
        }

        private void Awake()
        {
            EnsureCollider();
            EnsureItemReference();
            FindTargetDoor();
        }

        private void Start()
        {
            EnsureCollider();
            EnsureItemReference();
            FindTargetDoor();

            // Zablokuj drzwi na start
            if (targetDoor != null && !isUnlocked)
            {
                targetDoor.SetLocked(true);
                targetDoor.SetPadlock(this);
            }
        }

        private void Update()
        {
            if (isUnlocked) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            float dist = Vector3.Distance(transform.position, cam.transform.position);
            if (dist > 2.0f) return;

            Vector3 dir = (transform.position - cam.transform.position).normalized;
            float dot = Vector3.Dot(cam.transform.forward, dir);
            if (dot < 0.5f) return;

            int count = Inventory.instance != null && lockpickItem != null ? Inventory.instance.GetTotalItemCount(lockpickItem) : 0;
            DoorLockPromptUI.EnsureInstanceExists();
            if (DoorLockPromptUI.Instance != null)
            {
                DoorLockPromptUI.Instance.RequestShowPrompt(count > 0, count);
            }
        }

        private void EnsureCollider()
        {
            col = GetComponent<Collider>();
            if (col == null)
            {
                BoxCollider bc = gameObject.AddComponent<BoxCollider>();
                bc.size = new Vector3(2.5f, 3.5f, 1.5f);
                col = bc;
            }
        }

        private void EnsureItemReference()
        {
            if (lockpickItem == null)
            {
                lockpickItem = Resources.Load<Item>("Items/Wytrych");
                if (lockpickItem == null) lockpickItem = Resources.Load<Item>("Wytrych");
            }
        }

        private void FindTargetDoor()
        {
            if (targetDoor != null) return;

            // Szukanie w rodzicu lub dzieciach
            targetDoor = GetComponentInParent<DoorController>();
            if (targetDoor == null)
            {
                targetDoor = GetComponentInChildren<DoorController>();
            }

            // Szukanie w promieniu 3 metrów
            if (targetDoor == null)
            {
                Collider[] hits = Physics.OverlapSphere(transform.position, 3.5f);
                foreach (var hit in hits)
                {
                    DoorController door = hit.GetComponent<DoorController>() ?? hit.GetComponentInParent<DoorController>();
                    if (door != null)
                    {
                        targetDoor = door;
                        break;
                    }
                }
            }
        }

        public bool Interact(GameObject interactor)
        {
            if (isUnlocked) return false;
            if (LockpickingMinigameUI.Instance != null && LockpickingMinigameUI.Instance.IsActive) return false;

            EnsureItemReference();

            int lockpicks = Inventory.instance != null && lockpickItem != null 
                ? Inventory.instance.GetTotalItemCount(lockpickItem) 
                : 0;

            if (lockpicks <= 0)
            {
                Debug.LogWarning("<color=#FFA500>[PadlockController]</color> Nie masz żadnych wytrychów, aby otworzyć tę kłódkę!");
                if (padlockRattleSound != null)
                {
                    AudioSource.PlayClipAtPoint(padlockRattleSound, transform.position);
                }
                return false;
            }

            // Uruchomienie minigry
            LockpickingMinigameUI.EnsureInstanceExists();
            if (LockpickingMinigameUI.Instance != null)
            {
                LockpickingMinigameUI.Instance.StartMinigame(lockpickItem, OnUnlockSuccess, OnUnlockFailed);
                return true;
            }
            else
            {
                Debug.LogError("[PadlockController] Nie udało się zainicjalizować LockpickingMinigameUI!");
                return false;
            }
        }

        private void OnUnlockSuccess()
        {
            if (isUnlocked) return;
            isUnlocked = true;

            Debug.Log("<color=#00FF7F><b>[PadlockController] KŁÓDKA ZOSTAŁA OTWARTE! Drzwi odblokowane!</b></color>");

            // Odblokowanie drzwi
            if (targetDoor != null)
            {
                targetDoor.SetLocked(false);
            }

            DoorLockPromptUI.Instance?.HidePromptInstant();

            // Efekt fizyki: kłódka odpada z drzwi i spada na ziemię
            if (dropPhysicsOnUnlock)
            {
                transform.SetParent(null); // Odpinamy od drzwi, by nie obracała się razem z nimi

                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

                rb.mass = 0.8f;
                rb.interpolation = RigidbodyInterpolation.Interpolate;

                // Lekki impuls do przodu i w dół z rotacją
                Vector3 popDirection = (transform.forward * 0.4f + Vector3.down * 0.2f + Random.insideUnitSphere * 0.2f).normalized;
                rb.AddForce(popDirection * 1.5f, ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * 3f, ForceMode.Impulse);

                if (col != null)
                {
                    col.isTrigger = false;
                }

                // Usunięcie po pewnym czasie
                if (despawnDelay > 0)
                {
                    Destroy(gameObject, despawnDelay);
                }
            }

            // Wyłączamy możliwość ponownej interakcji
            enabled = false;
        }

        private void OnUnlockFailed()
        {
            Debug.Log("<color=#E74C3C>[PadlockController] Próba otwarcia kłódki nie powiodła się!</color>");
        }
    }
}
