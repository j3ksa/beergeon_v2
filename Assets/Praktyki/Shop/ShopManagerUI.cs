using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CurrencySystem;

namespace ShopSystem
{
    public class ShopManagerUI : MonoBehaviour
    {
        private static ShopManagerUI _instance;
        public static ShopManagerUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindFirstObjectByType<ShopManagerUI>(FindObjectsInactive.Include);
                }
                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }

        [Header("UI Panels")]
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private Button backButton;

        [Header("Shop Slots (np. 6 slotów)")]
        [SerializeField] private List<ShopItemSlotUI> slotUIs = new List<ShopItemSlotUI>();

        [Header("Pula Przedmiotów (Item Pool)")]
        [SerializeField] private List<ShopItemSO> itemPool = new List<ShopItemSO>();

        [Header("System Reroll (Przyslosowanie oferty)")]
        [SerializeField] private Button rerollButton;
        [SerializeField] private TextMeshProUGUI rerollCostText;
        [SerializeField] private int rerollCost = 10;
        [SerializeField] private int rerollCostIncrease = 5; // Wzrost ceny rerolla o +5 kapsli przy każdym losowaniu

        [Header("Skalowanie Cen z Poziomem Gracza")]
        [SerializeField] private float priceMultiplierPerLevel = 0.15f; // +15% drożej za każdy poziom gracza (LVL 1 = 100%, LVL 2 = 115%...)

#pragma warning disable 0414
        [Header("Ustawienia")]
        [SerializeField] private bool allowDuplicateItemsInShop = true;
#pragma warning restore 0414

        [Header("HUD Management")]
        [Tooltip("Elementy HUD, które mają zostać ukryte po otwarciu sklepu (pasek zdrowia, staminy, XP, ekwipunek itp.)")]
        [SerializeField] private List<GameObject> hudElementsToHide = new List<GameObject>();

        private bool isOpen = false;
        private bool isInitialized = false;
        private List<GameObject> previouslyActiveHUDElements = new List<GameObject>();

        public bool IsOpen => isOpen;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Start()
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(CloseShop);
            }

            if (rerollButton != null)
            {
                rerollButton.onClick.AddListener(OnRerollClicked);
            }

            UpdateRerollUI();
            AutoFindHUDElements();

            // Wczytaj ewentualne dodatkowe przedmioty z Resources/ShopItems (np. Kusza, Strzały)
            ShopItemSO[] resourceShopItems = Resources.LoadAll<ShopItemSO>("ShopItems");
            if (resourceShopItems != null && resourceShopItems.Length > 0)
            {
                if (itemPool == null) itemPool = new List<ShopItemSO>();
                foreach (var rItem in resourceShopItems)
                {
                    if (rItem != null && !itemPool.Contains(rItem))
                    {
                        itemPool.Add(rItem);
                    }
                }
            }

            // Losujemy ofertę TYLKO RAZ przy rozpoczęciu gry
            if (!isInitialized)
            {
                RandomizeShopItems();
                isInitialized = true;
            }
        }

        private void AutoFindHUDElements()
        {
            if (hudElementsToHide == null) hudElementsToHide = new List<GameObject>();

            // Wyszukujemy wyłącznie znane paski HUD (Zostawiamy CurrencyUI w spokoju!)
            string[] targetHUDNames = new string[] { "HealthBar", "StaminaBar", "XPBar", "Slots", "UIDesign", "PotionEffects", "MonkeyEffect" };
            foreach (string hName in targetHUDNames)
            {
                GameObject found = GameObject.Find(hName);
                if (found != null && !hudElementsToHide.Contains(found))
                {
                    hudElementsToHide.Add(found);
                }
            }
        }

        private void Update()
        {
            if (!isOpen) return;

            bool closePressed = false;

#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame ||
                    UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
                {
                    closePressed = true;
                }
            }
#else
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E))
            {
                closePressed = true;
            }
#endif

            if (closePressed)
            {
                CloseShop();
            }
        }

        public void OpenShop()
        {
            isOpen = true;

            // Włączamy obiekt shopPanel oraz wszystkich jego rodziców (np. PauseMenu),
            // aby upewnić się, że panel zostanie wyrenderowany na ekranie
            Transform current = shopPanel != null ? shopPanel.transform : transform;
            while (current != null)
            {
                current.gameObject.SetActive(true);
                if (current.GetComponent<Canvas>() != null) break;
                current = current.parent;
            }

            if (shopPanel != null)
            {
                shopPanel.SetActive(true);
            }
            else
            {
                gameObject.SetActive(true);
            }

            Debug.Log("[ShopManagerUI] Sklep został OTWARTY!");

            // Ukrywamy niepotrzebny HUD (paski życia, staminy itd.)
            HideActiveHUD();

            // Odblokowanie kursoru myszki
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Wstrzymanie czasu podczas otwartego sklepu
            Time.timeScale = 0f;
        }

        public void CloseShop()
        {
            isOpen = false;
            UIOverlayManager.NotifyOverlayClosed();

            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
            }

            Debug.Log("[ShopManagerUI] Sklep został ZAMKNIĘTY.");

            // Przywracamy ukryty HUD
            RestoreHUD();

            // Zablokowanie kursoru myszki z powrotem do gry
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            Time.timeScale = 1f;
        }

        private void HideActiveHUD()
        {
            previouslyActiveHUDElements.Clear();
            if (hudElementsToHide == null) return;

            foreach (var hudItem in hudElementsToHide)
            {
                // Upewniamy się, że nie wyłączamy samego siebie
                if (hudItem != null && hudItem != shopPanel && hudItem != gameObject && hudItem.activeSelf)
                {
                    previouslyActiveHUDElements.Add(hudItem);
                    hudItem.SetActive(false);
                }
            }
        }

        private void RestoreHUD()
        {
            foreach (var hudItem in previouslyActiveHUDElements)
            {
                if (hudItem != null)
                {
                    hudItem.SetActive(true);
                }
            }
            previouslyActiveHUDElements.Clear();
        }

        public void ToggleShop()
        {
            if (isOpen) CloseShop();
            else OpenShop();
        }

        public void RandomizeShopItems()
        {
            if (itemPool == null || itemPool.Count == 0)
            {
                Debug.LogWarning("[ShopManagerUI] Pula przedmiotów (itemPool) jest pusta!");
                return;
            }

            // Oczyszczamy z ewentualnych pustych referencji
            itemPool.RemoveAll(item => item == null);
            if (itemPool.Count == 0)
            {
                Debug.LogWarning("[ShopManagerUI] Pula przedmiotów po oczyszczeniu jest pusta!");
                return;
            }

            // Pobieramy poziom gracza (domyślnie 1)
            int playerLevel = 1;
            PlayerStats stats = Object.FindFirstObjectByType<PlayerStats>();
            if (stats != null)
            {
                playerLevel = stats.Level;
            }

            // Mnożnik skalowania ceny z poziomem (np. LVL 1 = 1.0, LVL 2 = 1.15, LVL 3 = 1.30...)
            float levelMultiplier = 1f + (playerLevel - 1) * priceMultiplierPerLevel;

            for (int i = 0; i < slotUIs.Count; i++)
            {
                if (slotUIs[i] == null) continue;

                // Losowanie przedmiotu z puli
                ShopItemSO randomItem = itemPool[Random.Range(0, itemPool.Count)];

                // Przeliczanie skalowanej ceny
                int finalCost = Mathf.RoundToInt(randomItem.cost * levelMultiplier);
                if (finalCost < 1) finalCost = 1;

                slotUIs[i].Setup(randomItem, finalCost, OnSlotBuyClicked);
            }
        }

        private void OnSlotBuyClicked(ShopItemSlotUI slot, ShopItemSO shopItem, int finalCost)
        {
            if (shopItem == null || slot == null || slot.IsSold) return;

            // Sprawdzamy czy gracz posiada odpowiednią ilość kapsli
            if (PlayerCurrency.Instance != null)
            {
                if (PlayerCurrency.Instance.TrySpendCaps(finalCost))
                {
                    // Sukces zakupu!
                    GiveItemToPlayer(shopItem.GetItemData(), shopItem.amountToGive);
                    slot.MarkAsSold();
                    Debug.Log($"[ShopManagerUI] Zakupiono: {shopItem.GetName()} za {finalCost} kapsli.");
                }
                else
                {
                    Debug.Log("[ShopManagerUI] Za mało kapsli, aby dokonać zakupu!");
                }
            }
            else
            {
                // Jeśli brak menedżera waluty w scenie (testowo)
                GiveItemToPlayer(shopItem.GetItemData(), shopItem.amountToGive);
                slot.MarkAsSold();
                Debug.LogWarning("[ShopManagerUI] PlayerCurrency.Instance nie znaleziono, przyznano przedmiot darmowo (test).");
            }
        }

        private void GiveItemToPlayer(Item item, int amount = 1)
        {
            if (item == null) return;

            // Jeśli istnieje ekwipunek w grze, dodajmy go
            if (Inventory.instance != null)
            {
                Inventory.instance.Add(item, amount);
                Debug.Log($"[ShopManagerUI] Dodano {item.itemName} x{amount} do ekwipunku.");
            }
            else
            {
                Debug.LogWarning($"[ShopManagerUI] Inventory.instance jest null! Nie można dodać przedmiotu {item.itemName}.");
            }
        }

        private void OnRerollClicked()
        {
            if (PlayerCurrency.Instance != null)
            {
                if (PlayerCurrency.Instance.TrySpendCaps(rerollCost))
                {
                    rerollCost += rerollCostIncrease;
                    UpdateRerollUI();
                    RandomizeShopItems();
                    Debug.Log($"[ShopManagerUI] Przelosowano ofertę sklepu. Nowy koszt rerolla wynosi {rerollCost} kapsli.");
                }
                else
                {
                    Debug.Log("[ShopManagerUI] Za mało kapsli na przelosowanie oferty!");
                }
            }
            else
            {
                rerollCost += rerollCostIncrease;
                UpdateRerollUI();
                RandomizeShopItems();
            }
        }

        private void UpdateRerollUI()
        {
            if (rerollCostText != null)
            {
                rerollCostText.text = rerollCost.ToString();
            }
        }
    }
}
