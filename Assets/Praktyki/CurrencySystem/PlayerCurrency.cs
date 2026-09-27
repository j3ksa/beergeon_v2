using UnityEngine;
using UnityEngine.Events;

namespace CurrencySystem
{
    public class PlayerCurrency : MonoBehaviour
    {
        public static PlayerCurrency Instance { get; private set; }

        [Header("Currency Settings")]
        [SerializeField] private int currentCaps = 0;

        [Header("Events")]
        public UnityEvent<int> OnCapsChanged;

        public int CurrentCaps => currentCaps;

        private static int savedCaps = 0;
        private static bool isInitialized = false;

        public static void ResetCaps()
        {
            savedCaps = 0;
            isInitialized = false;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            if (!isInitialized)
            {
                savedCaps = currentCaps;
                isInitialized = true;
            }
            else
            {
                currentCaps = savedCaps;
            }
        }

        private void Start()
        {
            OnCapsChanged?.Invoke(currentCaps);
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.cKey.wasPressedThisFrame)
            {
                AddCaps(50);
                Debug.Log("[PlayerCurrency] Testowy klawisz C: Dodano +50 kapsli!");
            }
#else
            if (Input.GetKeyDown(KeyCode.C))
            {
                AddCaps(50);
                Debug.Log("[PlayerCurrency] Testowy klawisz C: Dodano +50 kapsli!");
            }
#endif
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Dodaje określoną ilość kapsli graczowi.
        /// </summary>
        public void AddCaps(int amount)
        {
            if (amount <= 0) return;

            currentCaps += amount;
            savedCaps = currentCaps;
            OnCapsChanged?.Invoke(currentCaps);
            Debug.Log($"[PlayerCurrency] Dodano {amount} kapsli. Aktualny stan: {currentCaps}");
        }

        /// <summary>
        /// Próbuje wydać daną ilość kapsli. Zwraca true jeśli zakupy się powiodły.
        /// </summary>
        public bool TrySpendCaps(int amount)
        {
            if (amount <= 0) return false;

            if (currentCaps >= amount)
            {
                currentCaps -= amount;
                savedCaps = currentCaps;
                OnCapsChanged?.Invoke(currentCaps);
                Debug.Log($"[PlayerCurrency] Wydano {amount} kapsli. Pozostało: {currentCaps}");
                return true;
            }

            Debug.LogWarning($"[PlayerCurrency] Za mało kapsli! Wymagano: {amount}, Posiadane: {currentCaps}");
            return false;
        }

        /// <summary>
        /// Ustawia bezpośrednio liczbę kapsli (używane podczas odczytu zapisu gry).
        /// </summary>
        public void SetCaps(int amount)
        {
            currentCaps = Mathf.Max(0, amount);
            savedCaps = currentCaps;
            OnCapsChanged?.Invoke(currentCaps);
        }
    }
}
