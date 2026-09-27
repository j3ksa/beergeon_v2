using System.Collections;
using UnityEngine;
using FMODUnity;

public class DoorController : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float animationSpeed = 2f;
    [SerializeField] private bool isOpen = false;

    [Header("FMOD Audio")] 
    [SerializeField] private EventReference doorCreakEvent;
    [SerializeField] private EventReference doorShutEvent;

    [Header("Lock Settings")]
    [SerializeField] private bool isLocked = false;
    [SerializeField] private string lockedPrompt = "Drzwi są zamknięte na kłódkę!";
    [SerializeField] private Praktyki.OtwieranieZamkow.PadlockController attachedPadlock;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine currentAnimationRoutine;

    public bool IsLocked { get => isLocked; set => isLocked = value; }
    public void SetLocked(bool locked) => isLocked = locked;
    public void SetPadlock(Praktyki.OtwieranieZamkow.PadlockController padlock) => attachedPadlock = padlock;

    public string InteractionPrompt
    {
        get
        {
            if (isLocked)
            {
                int count = GetLockpicksCount();
                return count > 0 ? $"Otwórz zamek (Wytrychy: {count}) [E]" : lockedPrompt;
            }
            return isOpen ? "Close Door" : "Open Door";
        }
    }

    private void Start()
    {
        closedRotation = transform.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);

        // Automatyczne znalezienie kłódki jeśli nie przypisana
        FindNearbyPadlock();
    }

    private void Update()
    {
        if (!isLocked) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        float dist = Vector3.Distance(transform.position, cam.transform.position);
        if (dist > 2.0f) return;

        Vector3 dir = (transform.position - cam.transform.position).normalized;
        float dot = Vector3.Dot(cam.transform.forward, dir);
        if (dot < 0.5f) return;

        int count = GetLockpicksCount();
        Praktyki.OtwieranieZamkow.DoorLockPromptUI.EnsureInstanceExists();
        if (Praktyki.OtwieranieZamkow.DoorLockPromptUI.Instance != null)
        {
            Praktyki.OtwieranieZamkow.DoorLockPromptUI.Instance.RequestShowPrompt(count > 0, count);
        }
    }

    private void FindNearbyPadlock()
    {
        if (attachedPadlock != null) return;

        attachedPadlock = GetComponentInChildren<Praktyki.OtwieranieZamkow.PadlockController>();
        if (attachedPadlock == null)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, 3.5f);
            foreach (var hit in hits)
            {
                var pad = hit.GetComponent<Praktyki.OtwieranieZamkow.PadlockController>() ?? hit.GetComponentInParent<Praktyki.OtwieranieZamkow.PadlockController>();
                if (pad != null)
                {
                    attachedPadlock = pad;
                    break;
                }
            }
        }
    }

    private int GetLockpicksCount()
    {
        if (Inventory.instance == null) return 0;

        Item wytrych = Resources.Load<Item>("Items/Wytrych");
        if (wytrych == null) wytrych = Resources.Load<Item>("Wytrych");

        int count = wytrych != null ? Inventory.instance.GetTotalItemCount(wytrych) : 0;
        if (count <= 0)
        {
            // Fallback po nazwie
            for (int i = 0; i < Inventory.instance.items.Count; i++)
            {
                if (Inventory.instance.items[i] != null && Inventory.instance.items[i].itemName.ToLower().Contains("wytrych"))
                {
                    count += Inventory.instance.GetItemCount(i);
                }
            }
        }
        return count;
    }

    public bool Interact(GameObject interactor)
    {
        if (Praktyki.OtwieranieZamkow.LockpickingMinigameUI.Instance != null && Praktyki.OtwieranieZamkow.LockpickingMinigameUI.Instance.IsActive)
        {
            return false;
        }

        if (isLocked)
        {
            Debug.Log("[DoorController] Drzwi są zablokowane na kłódkę! Próba otwarcia zamka...");

            // 1. Jeśli kłódka posiada PadlockController, deleguj do niego
            FindNearbyPadlock();
            if (attachedPadlock != null && !attachedPadlock.IsUnlocked)
            {
                return attachedPadlock.Interact(interactor);
            }

            // 2. Jeśli nie ma PadlockController, uruchom minigrę bezpośrednio
            Item wytrych = Resources.Load<Item>("Items/Wytrych");
            if (wytrych == null) wytrych = Resources.Load<Item>("Wytrych");

            int count = GetLockpicksCount();
            if (count <= 0)
            {
                Debug.LogWarning("[DoorController] Brak wytrychów w ekwipunku! Drzwi pozostają zablokowane.");
                return false;
            }

            Praktyki.OtwieranieZamkow.LockpickingMinigameUI.EnsureInstanceExists();
            if (Praktyki.OtwieranieZamkow.LockpickingMinigameUI.Instance != null)
            {
                Praktyki.OtwieranieZamkow.LockpickingMinigameUI.Instance.StartMinigame(wytrych, () =>
                {
                    Debug.Log("<color=#00FF7F>[DoorController] ZAMEK OTWARTY! Odryglowano drzwi!</color>");
                    isLocked = false;
                    UnlockNearbyPadlockVisual();
                    // Automatycznie uchyl drzwi
                    Interact(interactor);
                }, () =>
                {
                    Debug.Log("<color=#E74C3C>[DoorController] Otwieranie zamka przerwane lub nieudane.</color>");
                });
                return true;
            }

            return false;
        }

        isOpen = !isOpen;

        RuntimeManager.PlayOneShot(doorCreakEvent, transform.position);

        if (currentAnimationRoutine != null)
        {
            StopCoroutine(currentAnimationRoutine);
        }

        currentAnimationRoutine = StartCoroutine(AnimateDoor(isOpen ? openRotation : closedRotation));
        return true;
    }

    private void UnlockNearbyPadlockVisual()
    {
        // Znajdź obiekt kłódki w scenie i zrzuć go z fizyką
        GameObject klodka = GameObject.Find("klodka");
        if (klodka == null)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, 3.5f);
            foreach (var h in hits)
            {
                if (h.name.ToLower().Contains("klodka"))
                {
                    klodka = h.gameObject;
                    break;
                }
            }
        }

        if (klodka != null)
        {
            klodka.transform.SetParent(null);
            Rigidbody rb = klodka.GetComponent<Rigidbody>();
            if (rb == null) rb = klodka.AddComponent<Rigidbody>();
            rb.mass = 0.8f;
            Vector3 popDir = (transform.forward * 0.5f + Vector3.down * 0.3f + Random.insideUnitSphere * 0.2f).normalized;
            rb.AddForce(popDir * 1.5f, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 3f, ForceMode.Impulse);
            Destroy(klodka, 20f);
        }
    }

    private IEnumerator AnimateDoor(Quaternion targetRotation)
    {
        while (Quaternion.Angle(transform.localRotation, targetRotation) > 5.0f)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * animationSpeed);
            yield return null;
        }

        transform.localRotation = targetRotation;

        if (!isOpen)
        {
            RuntimeManager.PlayOneShot(doorShutEvent, transform.position);
        }

        currentAnimationRoutine = null;
    }
}