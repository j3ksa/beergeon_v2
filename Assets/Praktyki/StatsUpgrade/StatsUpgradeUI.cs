using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class StatsUpgradeUI : MonoBehaviour
{
    public static StatsUpgradeUI Instance { get; private set; }
    public bool IsOpen => upgradePanel != null && upgradePanel.activeSelf;

    [Header("Skrypt Logiki Ulepszeń")]
    [SerializeField] private PlayerStatsUpgrade statsUpgrade;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerStamina playerStamina;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerStats playerStats;

    [Header("Kontener Głównego Panelu UI")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Key toggleKey = Key.U;

    [Header("Powiadomienie pod pakiem zdrowia (Notification Badge)")]
    [Tooltip("Obiekt ikonki/plakietki informującej o wolnych punktach (pojawia się tylko gdy availablePoints > 0)")]
    [SerializeField] private GameObject notificationBadge;
    [Tooltip("Tekst na ikonce/obok ikonki, np. '+1' lub '+2'")]
    [SerializeField] private TextMeshProUGUI notificationText;
    [Tooltip("Przycisk na ikonce (opcjonalny - pozwala otworzyć panel kliknięciem myszki)")]
    [SerializeField] private Button notificationButton;

    [Header("Nagłówek Panelu i Dostępne Punkty")]
    [SerializeField] private TextMeshProUGUI availablePointsText;

    [Header("--- Wiersz 1: ZDROWIE (HEALTH) ---")]
    [SerializeField] private TextMeshProUGUI healthNameText;       // Kolumna NAME
    [SerializeField] private TextMeshProUGUI healthValueText;      // Kolumna STATISTIC
    [SerializeField] private TextMeshProUGUI healthLevelText;      // Kolumna LEVEL
    [SerializeField] private Button healthAddButton;

    [Header("--- Wiersz 2: STAMINA ---")]
    [SerializeField] private TextMeshProUGUI staminaNameText;
    [SerializeField] private TextMeshProUGUI staminaValueText;
    [SerializeField] private TextMeshProUGUI staminaLevelText;
    [SerializeField] private Button staminaAddButton;

    [Header("--- Wiersz 3: REGENERACJA STAMINY ---")]
    [SerializeField] private TextMeshProUGUI staminaRegenNameText;
    [SerializeField] private TextMeshProUGUI staminaRegenValueText;
    [SerializeField] private TextMeshProUGUI staminaRegenLevelText;
    [SerializeField] private Button staminaRegenAddButton;

    [Header("--- Wiersz 4: PRĘDKOŚĆ RUCHU (SPEED) ---")]
    [SerializeField] private TextMeshProUGUI speedNameText;
    [SerializeField] private TextMeshProUGUI speedValueText;
    [SerializeField] private TextMeshProUGUI speedLevelText;
    [SerializeField] private Button speedAddButton;

    [Header("--- Wiersz 5: SIŁA (STRENGTH) ---")]
    [SerializeField] private TextMeshProUGUI strengthNameText;
    [SerializeField] private TextMeshProUGUI strengthValueText;
    [SerializeField] private TextMeshProUGUI strengthLevelText;
    [SerializeField] private Button strengthAddButton;

    [Header("--- Wiersz 6: SZANSA NA KRYTYK (CRIT CHANCE) ---")]
    [SerializeField] private TextMeshProUGUI critChanceNameText;
    [SerializeField] private TextMeshProUGUI critChanceValueText;
    [SerializeField] private TextMeshProUGUI critChanceLevelText;
    [SerializeField] private Button critChanceAddButton;

    [Header("Przycisk Zamknięcia / Powrotu")]
    [SerializeField] private Button closeButton;

    [Header("Obiekty HUD do ukrywania podczas ulepszeń (Optional)")]
    [Tooltip("Przeciągnij tutaj obiekty HUD (HealthBar, StaminaBar, CurrencyUI, Ekwipunek/XPBar), które mają znikać po otwarciu okna U")]
    [SerializeField] private GameObject[] hudElementsToHide;

    private void Awake()
    {
        Instance = this;
        FindPlayerComponents();
        SetupButtons();
    }

    private void FindPlayerComponents()
    {
        if (statsUpgrade == null) statsUpgrade = FindFirstObjectByType<PlayerStatsUpgrade>();
        if (playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerStamina == null) playerStamina = FindFirstObjectByType<PlayerStamina>();
        if (playerController == null) playerController = FindFirstObjectByType<PlayerController>();
        if (playerStats == null) playerStats = FindFirstObjectByType<PlayerStats>();

        if (hudElementsToHide == null || hudElementsToHide.Length == 0)
        {
            var foundHUDs = new System.Collections.Generic.List<GameObject>();
            string[] defaultHudNames = new string[] { "HealthBar", "StaminaBar", "XPBar", "CurrencyUI", "Slots", "UIDesign" };
            foreach (string hudName in defaultHudNames)
            {
                GameObject obj = GameObject.Find(hudName);
                if (obj != null) foundHUDs.Add(obj);
            }
            hudElementsToHide = foundHUDs.ToArray();
        }
    }

    private void Start()
    {
        if (statsUpgrade != null)
        {
            statsUpgrade.OnPointsOrStatsChanged.AddListener(UpdateUI);
        }

        UpdateUI();

        // Domyślnie panel ulepszeń jest zamknięty
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current[toggleKey].wasPressedThisFrame)
            {
                TogglePanel();
            }
            else if (IsOpen && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ClosePanel();
            }
        }
    }

    private void SetupButtons()
    {
        if (healthAddButton != null) healthAddButton.onClick.AddListener(() => { statsUpgrade.UpgradeHealth(); });
        if (staminaAddButton != null) staminaAddButton.onClick.AddListener(() => { statsUpgrade.UpgradeStamina(); });
        if (staminaRegenAddButton != null) staminaRegenAddButton.onClick.AddListener(() => { statsUpgrade.UpgradeStaminaRegen(); });
        if (speedAddButton != null) speedAddButton.onClick.AddListener(() => { statsUpgrade.UpgradeSpeed(); });
        if (strengthAddButton != null) strengthAddButton.onClick.AddListener(() => { statsUpgrade.UpgradeStrength(); });
        if (critChanceAddButton != null) critChanceAddButton.onClick.AddListener(() => { statsUpgrade.UpgradeCritChance(); });
        if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
        if (notificationButton != null) notificationButton.onClick.AddListener(OpenPanel);
    }

    public void TogglePanel()
    {
        if (upgradePanel != null)
        {
            bool isActive = !upgradePanel.activeSelf;
            if (isActive) OpenPanel();
            else ClosePanel();
        }
    }

    public void OpenPanel()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(true);
            UpdateUI();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f; // Zatrzymujemy czas gry, gdy gracz ulepsza statystyki!

            if (hudElementsToHide != null)
            {
                foreach (var hud in hudElementsToHide)
                {
                    if (hud != null) hud.SetActive(false);
                }
            }
        }
    }

    public void ClosePanel()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
            UIOverlayManager.NotifyOverlayClosed();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f; // Wzmawiamy czas gry po zamknięciu menu!

            if (hudElementsToHide != null)
            {
                foreach (var hud in hudElementsToHide)
                {
                    if (hud != null) hud.SetActive(true);
                }
            }
        }
    }

    public void UpdateUI()
    {
        if (statsUpgrade == null) FindPlayerComponents();
        if (statsUpgrade == null) return;

        int points = statsUpgrade.AvailablePoints;

        // 1. Powiadomienie pod HP (widoczne wizualnie tylko gdy są punkty)
        if (notificationBadge != null)
        {
            bool hasPoints = points > 0;

            if (notificationBadge == gameObject)
            {
                // Jeśli skrypt jest bezpośrednio na obiekcie powiadomienia, wyłączamy tylko jego grafikę, aby skrypt wciąż działał
                Image badgeImg = notificationBadge.GetComponent<Image>();
                if (badgeImg != null) badgeImg.enabled = hasPoints;

                Graphic[] childGraphics = notificationBadge.GetComponentsInChildren<Graphic>(true);
                foreach (var g in childGraphics)
                {
                    if (upgradePanel != null && g.transform.IsChildOf(upgradePanel.transform)) continue;
                    g.enabled = hasPoints;
                }
            }
            else
            {
                notificationBadge.SetActive(hasPoints);
            }

            if (hasPoints && notificationText != null)
            {
                notificationText.text = $"+{points}";
            }
        }

        // 2. Nagłówek punktów
        if (availablePointsText != null)
        {
            availablePointsText.text = $"POINTS: {points}";
        }

        // --- ZDROWIE (HEALTH) ---
        if (healthNameText != null) healthNameText.text = "HEALTH";
        if (healthValueText != null)
        {
            int currentMaxHP = playerHealth != null ? (playerStats != null ? playerStats.GetMaxHealth() + (int)statsUpgrade.GetBonusHealth() : 100 + (int)statsUpgrade.GetBonusHealth()) : 100;
            healthValueText.text = $"{currentMaxHP} HP";
        }
        if (healthLevelText != null) healthLevelText.text = $"LVL {statsUpgrade.HealthUpgrades}";
        if (healthAddButton != null) healthAddButton.interactable = statsUpgrade.CanUpgradeHealth();

        // --- STAMINA ---
        if (staminaNameText != null) staminaNameText.text = "STAMINA";
        if (staminaValueText != null)
        {
            float totalStamina = playerStamina != null ? playerStamina.EffectiveMaxStamina : 30f + statsUpgrade.GetBonusStamina();
            staminaValueText.text = $"{totalStamina:F0} STM";
        }
        if (staminaLevelText != null) staminaLevelText.text = $"LVL {statsUpgrade.StaminaUpgrades}";
        if (staminaAddButton != null) staminaAddButton.interactable = statsUpgrade.CanUpgradeStamina();

        // --- REGENERACJA STAMINY ---
        if (staminaRegenNameText != null) staminaRegenNameText.text = "STAMINA REGEN";
        if (staminaRegenValueText != null)
        {
            float totalRegen = playerStamina != null ? playerStamina.EffectiveStaminaRegen : 8f + statsUpgrade.GetBonusStaminaRegen();
            staminaRegenValueText.text = $"{totalRegen:F1} / S";
        }
        if (staminaRegenLevelText != null) staminaRegenLevelText.text = $"LVL {statsUpgrade.StaminaRegenUpgrades}";
        if (staminaRegenAddButton != null) staminaRegenAddButton.interactable = statsUpgrade.CanUpgradeStaminaRegen();

        // --- PRĘDKOŚĆ (SPEED) ---
        if (speedNameText != null) speedNameText.text = "SPEED";
        if (speedValueText != null)
        {
            float totalSpeed = playerController != null ? playerController.MoveSpeed : 5f + statsUpgrade.GetBonusSpeed();
            speedValueText.text = $"{totalSpeed:F1} SPD";
        }
        if (speedLevelText != null) speedLevelText.text = $"LVL {statsUpgrade.SpeedUpgrades}";
        if (speedAddButton != null) speedAddButton.interactable = statsUpgrade.CanUpgradeSpeed();

        // --- SIŁA (STRENGTH) ---
        if (strengthNameText != null) strengthNameText.text = "STRENGTH";
        if (strengthValueText != null)
        {
            int totalStr = playerStats != null ? playerStats.Strength : 10 + statsUpgrade.GetBonusStrength();
            strengthValueText.text = $"{totalStr} STR";
        }
        if (strengthLevelText != null) strengthLevelText.text = $"LVL {statsUpgrade.StrengthUpgrades}";
        if (strengthAddButton != null) strengthAddButton.interactable = statsUpgrade.CanUpgradeStrength();

        // --- KRYTYK (CRIT CHANCE) ---
        if (critChanceNameText != null) critChanceNameText.text = "CRIT CHANCE";
        if (critChanceValueText != null)
        {
            critChanceValueText.text = $"{statsUpgrade.GetCurrentCritChance():F0}% CRIT";
        }
        if (critChanceLevelText != null) critChanceLevelText.text = $"LVL {statsUpgrade.CritChanceUpgrades}";
        if (critChanceAddButton != null) critChanceAddButton.interactable = statsUpgrade.CanUpgradeCritChance();
    }
}
