using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;
using Praktyki.Zbroja;

namespace Praktyki.EQ
{
    public class MinecraftInventoryUI : MonoBehaviour
    {
        public static MinecraftInventoryUI Instance { get; private set; }
        public static bool IsOpen { get; private set; }

        [Header("Settings & Keybinds")]
        [Tooltip("Klawisz otwierania i zamykania ekwipunku (domyślnie 'I')")]
        public Key toggleKey = Key.I;
        [Tooltip("Czy wstrzymać czas w grze podczas otwartego ekwipunku (Time.timeScale = 0)")]
        public bool pauseGameWhenOpen = true;

        [Header("Window References")]
        public GameObject windowPanel;
        public CanvasGroup canvasGroup;
        public MinecraftTooltipUI tooltipUI;
        public MinecraftEQAudio eqAudio;

        [Header("Slot References (36 Slotów Minecrafta)")]
        [Tooltip("Pasek podręczny: 9 slotów (indeksy 0 do 8)")]
        public MinecraftSlotUI[] hotbarSlots = new MinecraftSlotUI[9];
        [Tooltip("Główny plecak: 27 slotów (indeksy 9 do 35)")]
        public MinecraftSlotUI[] storageSlots = new MinecraftSlotUI[27];

        [Header("Armor Slots (Zbroja: Hełm, Klata, Spodnie, Buty)")]
        [Tooltip("4 sloty zbroi: 0-Hełm, 1-Klata, 2-Spodnie, 3-Buty")]
        public MinecraftSlotUI[] armorSlots = new MinecraftSlotUI[4];
        public Item[] equippedArmor = new Item[4];
        public int TotalDefense { get; private set; } = 0;
        public float TotalArmorSpeedModifier { get; private set; } = 1f;
        public float TotalArmorStaminaDrainMultiplier { get; private set; } = 1f;
        public int TotalBonusHealth { get; private set; } = 0;

        [Tooltip("Specjalny slot do wyrzucania (Kosz / Drop)")]
        public MinecraftSlotUI trashDropSlot;

        [Header("Held Item on Cursor")]
        public RectTransform heldItemCursorRect;
        public Image heldItemIcon;
        public TextMeshProUGUI heldItemCountText;

        [Header("Player Statistics & Info Panel")]
        public TextMeshProUGUI playerHealthText;
        public TextMeshProUGUI playerCapsText;
        public TextMeshProUGUI statsDetailText; // Szczegółowe statystyki gracza
        public Image playerPreviewImage;

        // Stan przeciąganego przedmiotu
        private Item heldItem = null;
        private int heldCount = 0;
        private MinecraftSlotUI hoveredSlot = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoEnsureBootstrap()
        {
            if (FindFirstObjectByType<MinecraftEQAutoBootstrap>() == null)
            {
                GameObject bootObj = new GameObject("MinecraftEQ_AutoBootstrap", typeof(MinecraftEQAutoBootstrap));
                DontDestroyOnLoad(bootObj);
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (eqAudio == null) eqAudio = GetComponentInChildren<MinecraftEQAudio>();
            if (tooltipUI == null) tooltipUI = GetComponentInChildren<MinecraftTooltipUI>();

            // Upewnij się, że okno startowo jest ukryte
            if (windowPanel != null) windowPanel.SetActive(false);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
            IsOpen = false;

            // Zastosuj font Ibarra Real Nova do wszystkich tekstów UI
            ApplyCustomFont();
        }

        private void ApplyCustomFont()
        {
            TMP_FontAsset font = MinecraftUIBuilder.GetGameFont();
            if (font != null)
            {
                TextMeshProUGUI[] tmps = GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in tmps)
                {
                    if (t != null) t.font = font;
                }
            }
        }

        private void Start()
        {
            if (Inventory.instance != null)
            {
                Inventory.instance.OnInventoryChanged.AddListener(RefreshUI);
                Inventory.instance.OnActiveSlotChanged.AddListener(RefreshActiveHotbarSlot);
            }

            SetupSlots();
            InitializeStarterArmor();
            RecalculateArmorStats();
            ApplyCustomFont();
            RefreshUI();
        }

        public void InitializeStarterArmor()
        {
            // Jeśli żaden slot pancerza nie jest założony, załóż startowy zestaw żelaznego pancerza
            bool hasAnyArmor = false;
            if (equippedArmor != null)
            {
                for (int i = 0; i < equippedArmor.Length; i++)
                {
                    if (equippedArmor[i] != null)
                    {
                        hasAnyArmor = true;
                        break;
                    }
                }
            }
            else
            {
                equippedArmor = new Item[4];
            }

            if (!hasAnyArmor)
            {
                string[] armorPaths = new string[]
                {
                    "Armor/ZelaznyHelm",
                    "Armor/ZelaznaKlata",
                    "Armor/ZelazneSpodnie",
                    "Armor/ZelazneButy"
                };

                for (int i = 0; i < 4; i++)
                {
                    Item armor = Resources.Load<Item>(armorPaths[i]);
#if UNITY_EDITOR
                    if (armor == null)
                    {
                        string[] editorPaths = new string[]
                        {
                            "Assets/Praktyki/Zbroja/ZelaznyHelm.asset",
                            "Assets/Praktyki/Zbroja/ZelaznaKlata.asset",
                            "Assets/Praktyki/Zbroja/ZelazneSpodnie.asset",
                            "Assets/Praktyki/Zbroja/ZelazneButy.asset"
                        };
                        armor = UnityEditor.AssetDatabase.LoadAssetAtPath<Item>(editorPaths[i]);
                    }
#endif
                    if (armor != null)
                    {
                        equippedArmor[i] = armor;
                    }
                }
            }

            EnsureTestArmorsProvided();
        }

        public void EnsureTestArmorsProvided()
        {
            if (Inventory.instance == null) return;

            string[] extraArmors = new string[]
            {
                "SkorzanaCzapka",
                "SkorzanaKurtka",
                "SkorzaneSpodnie",
                "SkorzaneButy",
                "KapslowyHelm",
                "KapslowyPancerz",
                "KapsloweSpodnie",
                "KapsloweButy"
            };

            for (int i = 0; i < extraArmors.Length; i++)
            {
                string armorName = extraArmors[i];
                bool alreadyOwned = false;

                // Sprawdź czy założone na ciele
                if (equippedArmor != null)
                {
                    for (int a = 0; a < equippedArmor.Length; a++)
                    {
                        if (equippedArmor[a] != null && (equippedArmor[a].name == armorName || equippedArmor[a].name.Contains(armorName)))
                        {
                            alreadyOwned = true;
                            break;
                        }
                    }
                }

                // Sprawdź czy w plecaku
                if (!alreadyOwned && Inventory.instance.items != null)
                {
                    for (int inv = 0; inv < Inventory.instance.items.Count; inv++)
                    {
                        var it = Inventory.instance.items[inv];
                        if (it != null && (it.name == armorName || it.name.Contains(armorName)))
                        {
                            alreadyOwned = true;
                            break;
                        }
                    }
                }

                // Jeśli gracz jeszcze nie ma tego elementu, dodaj go do ekwipunku
                if (!alreadyOwned)
                {
                    Item itemObj = GetOrCreateArmorItem(armorName);
                    if (itemObj != null)
                    {
                        Inventory.instance.Add(itemObj, 1);
                        Debug.Log($"<color=#55FF55>[MinecraftInventoryUI] Dodano zbroję do ekwipunku: {itemObj.itemName} ({armorName})</color>");
                    }
                    else
                    {
                        Debug.LogWarning($"[MinecraftInventoryUI] Could not load armor asset for: {armorName}");
                    }
                }
            }
        }

        private ArmorItem GetOrCreateArmorItem(string armorName)
        {
            // 1. Spróbuj załadować z Resources
            ArmorItem armor = Resources.Load<ArmorItem>($"Armor/{armorName}");
            if (armor != null) return armor;

#if UNITY_EDITOR
            // 2. Spróbuj załadować z AssetDatabase
            armor = UnityEditor.AssetDatabase.LoadAssetAtPath<ArmorItem>($"Assets/Praktyki/Zbroja/{armorName}.asset");
            if (armor != null) return armor;
            armor = UnityEditor.AssetDatabase.LoadAssetAtPath<ArmorItem>($"Assets/Resources/Armor/{armorName}.asset");
            if (armor != null) return armor;
#endif

            // 3. Dynamiczny fallback runtime (tworzony w pamięci RAM jeśli Unity jeszcze nie zaimportowało assetu)
            armor = ScriptableObject.CreateInstance<ArmorItem>();
            armor.name = armorName;

            string spriteFileName = "";
            string pickupPrefabPath = "";

            switch (armorName)
            {
                case "KapslowyHelm":
                    armor.itemName = "Kapslowy Hełm";
                    armor.slotType = ArmorSlotType.Helmet;
                    armor.defense = 3;
                    armor.bonusHealth = 20;
                    armor.speedBonus = 0.01f;
                    armor.staminaDrainPenalty = -0.03f;
                    armor.description = "Legendarny hełm z utwardzanych złotych kapsli.\n+ 3 Obrony | + 20 Max HP\n+ 1% Prędkości | - 3% Zużycia staminy";
                    spriteFileName = "helm_kapslowy.png";
                    pickupPrefabPath = "Items/HelmKapslowyPickup";
                    break;
                case "KapslowyPancerz":
                    armor.itemName = "Kapslowy Pancerz";
                    armor.slotType = ArmorSlotType.Chestplate;
                    armor.defense = 6;
                    armor.bonusHealth = 40;
                    armor.speedBonus = 0.02f;
                    armor.staminaDrainPenalty = -0.05f;
                    armor.description = "Mityczny pancerz łuskowy ze splecionych kapsli.\n+ 6 Obrony | + 40 Max HP\n+ 2% Prędkości | - 5% Zużycia staminy";
                    spriteFileName = "pancerz_kapslowy.png";
                    pickupPrefabPath = "Items/PancerzKapslowyPickup";
                    break;
                case "KapsloweSpodnie":
                    armor.itemName = "Kapslowe Nogawice";
                    armor.slotType = ArmorSlotType.Leggings;
                    armor.defense = 4;
                    armor.bonusHealth = 25;
                    armor.speedBonus = 0.01f;
                    armor.staminaDrainPenalty = -0.03f;
                    armor.description = "Elastyczne nogawice wzmacniane złotymi kapslami.\n+ 4 Obrony | + 25 Max HP\n+ 1% Prędkości | - 3% Zużycia staminy";
                    spriteFileName = "spodnie_kapslowe.png";
                    pickupPrefabPath = "Items/SpodnieKapslowePickup";
                    break;
                case "KapsloweButy":
                    armor.itemName = "Kapslowe Buty";
                    armor.slotType = ArmorSlotType.Boots;
                    armor.defense = 3;
                    armor.bonusHealth = 15;
                    armor.speedBonus = 0.02f;
                    armor.staminaDrainPenalty = -0.04f;
                    armor.description = "Piórkowo lekkie buty podbite sprężystymi kapslami.\n+ 3 Obrony | + 15 Max HP\n+ 2% Prędkości | - 4% Zużycia staminy";
                    spriteFileName = "buty_kapslowe.png";
                    pickupPrefabPath = "Items/ButyKapslowePickup";
                    break;
                case "SkorzanaCzapka":
                    armor.itemName = "Skórzana Czapka";
                    armor.slotType = ArmorSlotType.Helmet;
                    armor.defense = 1;
                    armor.bonusHealth = 5;
                    armor.speedBonus = 0.0f;
                    armor.staminaDrainPenalty = 0.0f;
                    armor.description = "Lekka czapka z miękkiej skóry.\n+ 1 Obrony | + 5 Max HP\nBrak spowolnienia";
                    spriteFileName = "czapka_skorzana.png";
                    pickupPrefabPath = "Items/CzapkaSkorzanaPickup";
                    break;
                case "SkorzanaKurtka":
                    armor.itemName = "Skórzana Kurtka";
                    armor.slotType = ArmorSlotType.Chestplate;
                    armor.defense = 2;
                    armor.bonusHealth = 10;
                    armor.speedBonus = -0.01f;
                    armor.staminaDrainPenalty = 0.02f;
                    armor.description = "Wygodna skórzana kurtka podróżna.\n+ 2 Obrony | + 10 Max HP\n- 1% Prędkości | + 2% Zużycia staminy";
                    spriteFileName = "kurtka_skorzana.png";
                    pickupPrefabPath = "Items/KurtkaSkorzanaPickup";
                    break;
                case "SkorzaneSpodnie":
                    armor.itemName = "Skórzane Spodnie";
                    armor.slotType = ArmorSlotType.Leggings;
                    armor.defense = 1;
                    armor.bonusHealth = 5;
                    armor.speedBonus = 0.0f;
                    armor.staminaDrainPenalty = 0.01f;
                    armor.description = "Proste spodnie ze skóry.\n+ 1 Obrony | + 5 Max HP\n+ 1% Zużycia staminy";
                    spriteFileName = "spodnie_skorzane.png";
                    pickupPrefabPath = "Items/SpodnieSkorzanePickup";
                    break;
                case "SkorzaneButy":
                    armor.itemName = "Skórzane Buty";
                    armor.slotType = ArmorSlotType.Boots;
                    armor.defense = 1;
                    armor.bonusHealth = 5;
                    armor.speedBonus = 0.0f;
                    armor.staminaDrainPenalty = 0.0f;
                    armor.description = "Wygodne, miękkie buty ze skóry.\n+ 1 Obrony | + 5 Max HP\nBrak spowolnienia";
                    spriteFileName = "buty_skorzane.png";
                    pickupPrefabPath = "Items/ButySkorzanePickup";
                    break;
            }

            // Załaduj ikonę Sprite
            if (!string.IsNullOrEmpty(spriteFileName))
            {
#if UNITY_EDITOR
                Sprite sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Praktyki/Zbroja/Graphics/{spriteFileName}");
                if (sp != null) armor.icon = sp;
#endif
                if (armor.icon == null)
                {
                    string diskPath = System.IO.Path.Combine(Application.dataPath, "Praktyki/Zbroja/Graphics", spriteFileName);
                    if (System.IO.File.Exists(diskPath))
                    {
                        byte[] bytes = System.IO.File.ReadAllBytes(diskPath);
                        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                        if (tex.LoadImage(bytes))
                        {
                            armor.icon = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(pickupPrefabPath))
            {
                armor.pickupPrefab = Resources.Load<GameObject>(pickupPrefabPath);
            }

            return armor;
        }

        private void SetupSlots()
        {
            // Indeksy hotbara 0..8
            if (hotbarSlots != null)
            {
                for (int i = 0; i < hotbarSlots.Length; i++)
                {
                    if (hotbarSlots[i] != null)
                    {
                        hotbarSlots[i].slotIndex = i;
                        hotbarSlots[i].isArmorSlot = false;
                        if (hotbarSlots[i].slotNumberText != null)
                        {
                            hotbarSlots[i].slotNumberText.text = (i + 1).ToString();
                        }
                    }
                }
            }

            // Indeksy plecaka 9..35
            if (storageSlots != null)
            {
                for (int i = 0; i < storageSlots.Length; i++)
                {
                    if (storageSlots[i] != null)
                    {
                        storageSlots[i].slotIndex = 9 + i;
                        storageSlots[i].isArmorSlot = false;
                    }
                }
            }

            // Sloty zbroi: 0-Hełm, 1-Klata, 2-Spodnie, 3-Buty
            if (armorSlots != null)
            {
                for (int i = 0; i < armorSlots.Length; i++)
                {
                    if (armorSlots[i] != null)
                    {
                        armorSlots[i].slotIndex = 100 + i;
                        armorSlots[i].isArmorSlot = true;
                        armorSlots[i].requiredArmorSlot = (ArmorSlotType)i;
                    }
                }
            }

            if (trashDropSlot != null)
            {
                trashDropSlot.slotIndex = -1;
                trashDropSlot.isTrashSlot = true;
            }
        }

        private void Update()
        {
            HandleToggleInput();

            if (IsOpen)
            {
                UpdateHeldItemCursorPosition();
                HandleShortcutKeys();
                RefreshPlayerInfo();
            }
        }

        private float lastToggleTime = -1f;

        private void HandleToggleInput()
        {
            if (Keyboard.current == null) return;

            // Escape zamyka otwarty ekwipunek
            if (IsOpen && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CloseInventory();
                return;
            }

            bool togglePressed = false;
            if (toggleKey == Key.I && Keyboard.current.iKey.wasPressedThisFrame) togglePressed = true;
            else if (toggleKey == Key.E && Keyboard.current.eKey.wasPressedThisFrame) togglePressed = true;
            else if (Keyboard.current[toggleKey].wasPressedThisFrame) togglePressed = true;

            if (togglePressed)
            {
                ToggleInventory();
            }
        }

        public static void EnsureInstanceExists()
        {
            if (Instance == null)
            {
                MinecraftUIBuilder.BuildMinecraftUI(null);
            }
        }

        public void ToggleInventory()
        {
            if (Time.unscaledTime - lastToggleTime < 0.25f) return;
            lastToggleTime = Time.unscaledTime;

            if (IsOpen)
            {
                CloseInventory();
            }
            else
            {
                OpenInventory();
            }
        }

        public void OpenInventory()
        {
            lastToggleTime = Time.unscaledTime;
            IsOpen = true;

            // Upewnij się, że obiekt i ewentualni rodzice są w pełni aktywni
            Transform current = transform;
            while (current != null)
            {
                current.gameObject.SetActive(true);
                current = current.parent;
            }

            if (windowPanel != null) windowPanel.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }

            // Kursor myszy odblokowany do klikania po slotach
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (pauseGameWhenOpen)
            {
                Time.timeScale = 0f;
            }

            EnsureTestArmorsProvided();
            RefreshUI();
            RefreshPlayerInfo();

            if (eqAudio != null) eqAudio.PlayOpen();
            Debug.Log("<color=#55FF55>[Minecraft EQ] Otwarto ekwipunek (Klawisz 'I')</color>");
        }

        public void CloseInventory(bool playSound = true)
        {
            lastToggleTime = Time.unscaledTime;
            IsOpen = false;
            UIOverlayManager.NotifyOverlayClosed();

            // Zwracamy trzymany przedmiot do ekwipunku gracza, aby nic nie przepadło
            ReturnHeldItemToInventory();

            if (tooltipUI != null) tooltipUI.Hide();

            if (windowPanel != null) windowPanel.SetActive(false);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            // Zablokowanie kursora z powrotem do kamery gracza
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (pauseGameWhenOpen)
            {
                Time.timeScale = 1f;
            }

            if (playSound && eqAudio != null) eqAudio.PlayClose();
            Debug.Log("<color=#FFAA00>[Minecraft EQ] Zamknięto ekwipunek</color>");
        }

        public void RefreshUI()
        {
            if (Inventory.instance == null) return;

            // 1. Hotbar (0..8)
            if (hotbarSlots != null)
            {
                for (int i = 0; i < hotbarSlots.Length; i++)
                {
                    if (hotbarSlots[i] == null) continue;
                    int index = i;
                    if (index < Inventory.instance.items.Count && Inventory.instance.items[index] != null)
                    {
                        hotbarSlots[i].SetItem(Inventory.instance.items[index], Inventory.instance.GetItemCount(index));
                    }
                    else
                    {
                        hotbarSlots[i].ClearSlot();
                    }
                }
            }

            // 2. Plecak (9..35)
            if (storageSlots != null)
            {
                for (int i = 0; i < storageSlots.Length; i++)
                {
                    if (storageSlots[i] == null) continue;
                    int index = 9 + i;
                    if (index < Inventory.instance.items.Count && Inventory.instance.items[index] != null)
                    {
                        storageSlots[i].SetItem(Inventory.instance.items[index], Inventory.instance.GetItemCount(index));
                    }
                    else
                    {
                        storageSlots[i].ClearSlot();
                    }
                }
            }

            // 3. Zbroja (0..3)
            if (armorSlots != null)
            {
                for (int i = 0; i < armorSlots.Length; i++)
                {
                    if (armorSlots[i] == null) continue;
                    if (i < equippedArmor.Length && equippedArmor[i] != null)
                    {
                        armorSlots[i].SetItem(equippedArmor[i], 1);
                    }
                    else
                    {
                        armorSlots[i].ClearSlot();
                    }
                }
            }

            RefreshActiveHotbarSlot();
            UpdateHeldItemVisuals();
            RefreshPlayerInfo();
        }

        private void RefreshActiveHotbarSlot()
        {
            if (Inventory.instance == null || hotbarSlots == null) return;

            for (int i = 0; i < hotbarSlots.Length; i++)
            {
                if (hotbarSlots[i] != null)
                {
                    hotbarSlots[i].SetActiveBorder(i == Inventory.instance.activeSlotIndex);
                }
            }
        }

        public void RecalculateArmorStats()
        {
            int def = 0;
            int hpBonus = 0;
            float speedMod = 0f;
            float stamDrainMod = 0f;

            var upgradeMgr = Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance;

            if (equippedArmor != null)
            {
                for (int i = 0; i < equippedArmor.Length; i++)
                {
                    if (equippedArmor[i] is ArmorItem armor)
                    {
                        if (upgradeMgr != null)
                        {
                            def += upgradeMgr.GetArmorEffectiveDefense(armor);
                            hpBonus += upgradeMgr.GetArmorEffectiveHealth(armor);
                            speedMod += upgradeMgr.GetArmorEffectiveSpeedModifier(armor);
                            stamDrainMod += upgradeMgr.GetArmorEffectiveStaminaDrainPenalty(armor);
                        }
                        else
                        {
                            def += armor.defense;
                            hpBonus += armor.bonusHealth;
                            speedMod += armor.speedBonus;
                            stamDrainMod += armor.staminaDrainPenalty;
                        }
                    }
                }
            }

            TotalDefense = def;
            TotalBonusHealth = hpBonus;
            TotalArmorSpeedModifier = Mathf.Clamp(1f + speedMod, 0.2f, 2f);
            TotalArmorStaminaDrainMultiplier = Mathf.Max(0.1f, 1f + stamDrainMod);

            // Aplikacja prędkości do gracza
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                player.ArmorSpeedModifier = TotalArmorSpeedModifier;
            }

            // Aplikacja zużycia staminy do gracza
            PlayerStamina stamina = FindFirstObjectByType<PlayerStamina>();
            if (stamina != null)
            {
                stamina.ArmorStaminaDrainMultiplier = TotalArmorStaminaDrainMultiplier;
            }

            // Aplikacja bonusowego życia do gracza
            PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
            if (health != null)
            {
                health.SetBonusMaxHealth(TotalBonusHealth);
            }

            RefreshPlayerInfo();
        }

        private void RefreshPlayerInfo()
        {
            // Zdrowie gracza
            PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
            int curHp = health != null ? health.CurrentHealth : 100;
            int maxHp = health != null ? health.MaxHealth : 100;

            if (playerHealthText != null)
            {
                playerHealthText.text = $"<color=#FF5555><b>HP: {curHp} / {maxHp}</b></color>";
            }

            // Kapsle / Waluta
            int caps = 0;
            CurrencySystem.PlayerCurrency currency = FindFirstObjectByType<CurrencySystem.PlayerCurrency>();
            if (currency != null) caps = currency.CurrentCaps;

            if (playerCapsText != null)
            {
                playerCapsText.text = $"<color=#F1C40F><b>Kapsle: {caps}</b></color>";
            }

            // Stamina
            PlayerStamina stamina = FindFirstObjectByType<PlayerStamina>();
            float curStam = stamina != null ? stamina.CurrentStamina : 30f;
            float maxStam = stamina != null ? stamina.MaxStamina : 30f;

            // Statystyki z PlayerStats
            PlayerStats stats = FindFirstObjectByType<PlayerStats>();
            int strength = stats != null ? stats.Strength : 10;
            int vitality = stats != null ? stats.Vitality : 10;
            int level = stats != null ? stats.Level : 1;
            int xp = stats != null ? stats.Experience : 0;

            // Przedmiot w ręce
            string itemName = "Puste ręce";
            string itemExtra = "";
            if (Inventory.instance != null)
            {
                Item activeIt = Inventory.instance.GetActiveItem();
                if (activeIt != null)
                {
                    itemName = activeIt.itemName;
                    if (activeIt is WeaponItem wp) itemExtra = $" (+{wp.damage} DMG)";
                }
            }

            // Szczegółowy Panel Statystyk Gracza (Klimat Medieval Tavern / RPG)
            if (statsDetailText != null)
            {
                float armorReductionPercent = Mathf.Clamp(TotalDefense * 4f, 0f, 80f);
                float speedDiffPercent = (TotalArmorSpeedModifier - 1f) * 100f;
                float stamDiffPercent = (TotalArmorStaminaDrainMultiplier - 1f) * 100f;

                string armorWeightStr = "";
                if (Mathf.Abs(speedDiffPercent) > 0.01f || Mathf.Abs(stamDiffPercent) > 0.01f)
                {
                    string spdStr = speedDiffPercent < 0 ? $"<color=#FFAA00>{speedDiffPercent:0}% Spd</color>" : $"<color=#55FF55>+{speedDiffPercent:0}% Spd</color>";
                    string stamStr = stamDiffPercent > 0 ? $"<color=#FF7777>+{stamDiffPercent:0}% Drain</color>" : $"<color=#55FF55>{stamDiffPercent:0}% Drain</color>";
                    armorWeightStr = $"  <color=#E0D0B0><b>CIĘŻAR ZBROI:</b></color> {spdStr}   <color=#E0D0B0><b>SPRINT DRAIN:</b></color> {stamStr}\n";
                }
                else
                {
                    armorWeightStr = $"  <color=#E0D0B0><b>CIĘŻAR ZBROI:</b></color> <color=#888888>Brak (lekki ekwipunek)</color>\n";
                }

                string statsStr =
                    $"<align=center><color=#D4AF37><b>── STATYSTYKI BOHATERA ──</b></color></align>\n\n" +
                    $"  <color=#E0D0B0><b>POZIOM:</b></color> <color=#00FF7F><b>{level}</b></color> <color=#888888>(XP: {xp})</color>           <color=#E0D0B0><b>KAPSLE:</b></color> <color=#FFD700><b>{caps}</b></color>\n" +
                    $"  <color=#E0D0B0><b>ZDROWIE:</b></color> <color=#FF5555><b>{curHp} / {maxHp} HP</b></color>         <color=#E0D0B0><b>WYTRZYMAŁOŚĆ:</b></color> <color=#00E5FF><b>{curStam:0} / {maxStam:0}</b></color>\n" +
                    $"  <color=#E0D0B0><b>PANCERZ:</b></color> <color=#64B5F6><b>+{TotalDefense} DEF</b></color> <color=#AAAAAA>(-{armorReductionPercent:0}% DMG)</color>   <color=#E0D0B0><b>WITALNOŚĆ:</b></color> <color=#BA68C8><b>{vitality}</b></color>\n" +
                    armorWeightStr +
                    $"  <color=#E0D0B0><b>SIŁA (ATAK):</b></color> <color=#FF9800><b>{strength}</b></color>\n" +
                    $"  <color=#E0D0B0><b>PRZEDMIOT W RĘCE:</b></color> <color=#FFFFFF><b>{itemName}</b></color><color=#FFD54F>{itemExtra}</color>";

                statsDetailText.text = statsStr;
            }
        }

        #region Slot Interaction (Minecraft Drag & Drop + Armor)

        public void OnSlotClicked(MinecraftSlotUI slot, PointerEventData.InputButton button, bool isShift)
        {
            if (slot == null || Inventory.instance == null) return;

            // Obsługa slotu kosza / wyrzucania
            if (slot.isTrashSlot)
            {
                HandleTrashSlotClick(button);
                return;
            }

            // Obsługa slotu zbroi
            if (slot.isArmorSlot)
            {
                HandleArmorSlotClick(slot, button, isShift);
                return;
            }

            int index = slot.slotIndex;
            if (index < 0 || index >= Inventory.instance.items.Count) return;

            Item slotItem = Inventory.instance.items[index];
            int slotCount = Inventory.instance.GetItemCount(index);

            // SHIFT + LPM: Szybkie ubieranie zbroi lub przeniesienie między Hotbarem a Plecakiem
            if (isShift && button == PointerEventData.InputButton.Left)
            {
                if (slotItem != null)
                {
                    // Jeśli to zbroja, spróbuj automatycznie założyć na pasujący slot zbroi
                    if (slotItem is ArmorItem armor)
                    {
                        int aIdx = (int)armor.slotType;
                        if (aIdx >= 0 && aIdx < equippedArmor.Length && equippedArmor[aIdx] == null)
                        {
                            equippedArmor[aIdx] = armor;
                            Inventory.instance.SetSlot(index, null, 0);
                            RecalculateArmorStats();
                            if (eqAudio != null) eqAudio.PlayPlaceDown();
                            RefreshUI();
                            return;
                        }
                    }

                    // Standardowe szybkie przenoszenie (Hotbar <-> Plecak)
                    Inventory.instance.QuickMoveSlot(index);
                    if (eqAudio != null) eqAudio.PlayPlaceDown();
                    RefreshUI();
                    if (tooltipUI != null && hoveredSlot == slot)
                    {
                        tooltipUI.Show(slot.CurrentItem, slot.CurrentCount);
                    }
                }
                return;
            }

            // LPM (LEWY PRZYCISK MYSZY): Cały stos / swap / merge
            if (button == PointerEventData.InputButton.Left)
            {
                if (heldItem == null)
                {
                    // Podniesienie całego stosu z tego slotu na kursor
                    if (slotItem != null)
                    {
                        heldItem = slotItem;
                        heldCount = slotCount;
                        Inventory.instance.SetSlot(index, null, 0);
                        if (eqAudio != null) eqAudio.PlayPickUp();
                    }
                }
                else
                {
                    if (slotItem == null)
                    {
                        // Położenie całego trzymanego stosu na pusty slot
                        Inventory.instance.SetSlot(index, heldItem, heldCount);
                        heldItem = null;
                        heldCount = 0;
                        if (eqAudio != null) eqAudio.PlayPlaceDown();
                    }
                    else if (slotItem == heldItem && heldItem.isStackable)
                    {
                        // Łączenie takiego samego przedmiotu (merge do limitu maxStack)
                        int spaceLeft = heldItem.maxStack - slotCount;
                        int toAdd = Mathf.Min(spaceLeft, heldCount);

                        Inventory.instance.SetSlot(index, heldItem, slotCount + toAdd);
                        heldCount -= toAdd;
                        if (heldCount <= 0)
                        {
                            heldItem = null;
                            heldCount = 0;
                        }
                        if (eqAudio != null) eqAudio.PlayPlaceDown();
                    }
                    else
                    {
                        // Zamiana przedmiotów (swap na kursorze)
                        Item tempItem = slotItem;
                        int tempCount = slotCount;

                        Inventory.instance.SetSlot(index, heldItem, heldCount);
                        heldItem = tempItem;
                        heldCount = tempCount;
                        if (eqAudio != null) eqAudio.PlayPickUp();
                    }
                }
            }
            // PPM (PRAWY PRZYCISK MYSZY): Podział na pół / odłożenie 1 sztuki
            else if (button == PointerEventData.InputButton.Right)
            {
                if (heldItem == null)
                {
                    // Podniesienie połowy stosu
                    if (slotItem != null)
                    {
                        int half = Mathf.CeilToInt(slotCount / 2f);
                        int remaining = slotCount - half;

                        heldItem = slotItem;
                        heldCount = half;

                        Inventory.instance.SetSlot(index, remaining > 0 ? slotItem : null, remaining);
                        if (eqAudio != null) eqAudio.PlayPickUp();
                    }
                }
                else
                {
                    if (slotItem == null)
                    {
                        // Odłożenie dokładnie 1 sztuki na pusty slot
                        Inventory.instance.SetSlot(index, heldItem, 1);
                        heldCount--;
                        if (heldCount <= 0)
                        {
                            heldItem = null;
                            heldCount = 0;
                        }
                        if (eqAudio != null) eqAudio.PlayPlaceDown();
                    }
                    else if (slotItem == heldItem && heldItem.isStackable && slotCount < heldItem.maxStack)
                    {
                        // Odłożenie 1 sztuki do istniejącego stosu
                        Inventory.instance.SetSlot(index, heldItem, slotCount + 1);
                        heldCount--;
                        if (heldCount <= 0)
                        {
                            heldItem = null;
                            heldCount = 0;
                        }
                        if (eqAudio != null) eqAudio.PlayPlaceDown();
                    }
                }
            }

            RefreshUI();
            UpdateHeldItemVisuals();

            if (tooltipUI != null)
            {
                if (heldItem == null && slot.CurrentItem != null)
                {
                    tooltipUI.Show(slot.CurrentItem, slot.CurrentCount);
                }
                else
                {
                    tooltipUI.Hide();
                }
            }
        }

        private void HandleArmorSlotClick(MinecraftSlotUI slot, PointerEventData.InputButton button, bool isShift)
        {
            int armorIndex = (int)slot.requiredArmorSlot;
            if (armorIndex < 0 || armorIndex >= equippedArmor.Length) return;

            Item currentArmor = equippedArmor[armorIndex];

            // Shift + LPM: Zdejmij zbroję do pierwszego wolnego miejsca w plecaku
            if (isShift && currentArmor != null)
            {
                bool added = Inventory.instance.Add(currentArmor, 1);
                if (added)
                {
                    equippedArmor[armorIndex] = null;
                    slot.ClearSlot();
                    RecalculateArmorStats();
                    if (eqAudio != null) eqAudio.PlayPickUp();
                    RefreshUI();
                }
                return;
            }

            if (heldItem == null)
            {
                // Zdejmij zbroję na kursor
                if (currentArmor != null)
                {
                    heldItem = currentArmor;
                    heldCount = 1;
                    equippedArmor[armorIndex] = null;
                    slot.ClearSlot();
                    RecalculateArmorStats();
                    if (eqAudio != null) eqAudio.PlayPickUp();
                }
            }
            else
            {
                // Sprawdź czy trzymany przedmiot to pasująca zbroja
                if (heldItem is ArmorItem newArmor && newArmor.slotType == slot.requiredArmorSlot)
                {
                    if (currentArmor == null)
                    {
                        // Załóż zbroję
                        equippedArmor[armorIndex] = newArmor;
                        heldCount--;
                        if (heldCount <= 0) heldItem = null;
                        if (eqAudio != null) eqAudio.PlayPlaceDown();
                    }
                    else
                    {
                        // Zamień zbroję
                        Item old = currentArmor;
                        equippedArmor[armorIndex] = newArmor;
                        heldItem = old;
                        heldCount = 1;
                        if (eqAudio != null) eqAudio.PlayPickUp();
                    }
                    RecalculateArmorStats();
                }
            }

            RefreshUI();
            UpdateHeldItemVisuals();
        }

        private void HandleTrashSlotClick(PointerEventData.InputButton button)
        {
            if (heldItem != null)
            {
                // Wyrzucenie trzymanego przedmiotu do świata gry
                DropHeldItemIntoWorld();
            }
            else if (trashDropSlot != null && trashDropSlot.CurrentItem != null)
            {
                trashDropSlot.ClearSlot();
            }
        }

        public void OnSlotHoverEnter(MinecraftSlotUI slot)
        {
            hoveredSlot = slot;

            if (tooltipUI != null && heldItem == null && slot.CurrentItem != null)
            {
                tooltipUI.Show(slot.CurrentItem, slot.CurrentCount);
            }
        }

        public void OnSlotHoverExit(MinecraftSlotUI slot)
        {
            if (hoveredSlot == slot)
            {
                hoveredSlot = null;
            }

            if (tooltipUI != null)
            {
                tooltipUI.Hide();
            }
        }

        private void HandleShortcutKeys()
        {
            if (Keyboard.current == null || Inventory.instance == null) return;

            // Klawisz Q: Wyrzucenie 1 sztuki z najechanego slotu lub trzymanego na kursorze
            if (Keyboard.current.qKey.wasPressedThisFrame)
            {
                if (heldItem != null)
                {
                    DropItemFromHeld(1);
                }
                else if (hoveredSlot != null)
                {
                    if (hoveredSlot.isArmorSlot)
                    {
                        int aIdx = (int)hoveredSlot.requiredArmorSlot;
                        if (aIdx >= 0 && aIdx < equippedArmor.Length && equippedArmor[aIdx] != null)
                        {
                            Item it = equippedArmor[aIdx];
                            equippedArmor[aIdx] = null;
                            RecalculateArmorStats();
                            Vector3 pos; Quaternion rot;
                            GetDropPositionAndRotation(out pos, out rot);
                            Inventory.instance.SpawnItemPickup(it, pos, rot);
                            RefreshUI();
                        }
                    }
                    else if (hoveredSlot.slotIndex >= 0)
                    {
                        DropItemFromSlot(hoveredSlot.slotIndex, 1);
                    }
                }
            }

            // Klawisze 1-9: Szybka zamiana najechanego slotu z odpowiednim slotem Hotbara
            if (hoveredSlot != null && hoveredSlot.slotIndex >= 0 && !hoveredSlot.isArmorSlot)
            {
                int hotbarTarget = -1;
                if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame) hotbarTarget = 0;
                else if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame) hotbarTarget = 1;
                else if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame) hotbarTarget = 2;
                else if (Keyboard.current.digit4Key.wasPressedThisFrame || Keyboard.current.numpad4Key.wasPressedThisFrame) hotbarTarget = 3;
                else if (Keyboard.current.digit5Key.wasPressedThisFrame || Keyboard.current.numpad5Key.wasPressedThisFrame) hotbarTarget = 4;
                else if (Keyboard.current.digit6Key.wasPressedThisFrame || Keyboard.current.numpad6Key.wasPressedThisFrame) hotbarTarget = 5;
                else if (Keyboard.current.digit7Key.wasPressedThisFrame || Keyboard.current.numpad7Key.wasPressedThisFrame) hotbarTarget = 6;
                else if (Keyboard.current.digit8Key.wasPressedThisFrame || Keyboard.current.numpad8Key.wasPressedThisFrame) hotbarTarget = 7;
                else if (Keyboard.current.digit9Key.wasPressedThisFrame || Keyboard.current.numpad9Key.wasPressedThisFrame) hotbarTarget = 8;

                if (hotbarTarget >= 0 && hotbarTarget != hoveredSlot.slotIndex)
                {
                    Inventory.instance.SwapSlots(hoveredSlot.slotIndex, hotbarTarget);
                    if (eqAudio != null) eqAudio.PlayPlaceDown();
                    RefreshUI();
                    if (tooltipUI != null && hoveredSlot.CurrentItem != null)
                    {
                        tooltipUI.Show(hoveredSlot.CurrentItem, hoveredSlot.CurrentCount);
                    }
                }
            }
        }

        #endregion

        #region Held Item Cursor Visuals & Dropping

        private void UpdateHeldItemCursorPosition()
        {
            if (heldItemCursorRect == null) return;

            Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;

            heldItemCursorRect.position = mousePos;
        }

        private void UpdateHeldItemVisuals()
        {
            if (heldItemCursorRect == null) return;

            if (heldItem != null && heldCount > 0)
            {
                heldItemCursorRect.gameObject.SetActive(true);
                if (heldItemIcon != null)
                {
                    heldItemIcon.sprite = heldItem.icon;
                    heldItemIcon.preserveAspect = true;
                    heldItemIcon.gameObject.SetActive(true);
                }

                if (heldItemCountText != null)
                {
                    if (heldCount > 1)
                    {
                        heldItemCountText.text = heldCount.ToString();
                        heldItemCountText.gameObject.SetActive(true);
                    }
                    else
                    {
                        heldItemCountText.text = "";
                        heldItemCountText.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                heldItemCursorRect.gameObject.SetActive(false);
            }
        }

        private void ReturnHeldItemToInventory()
        {
            if (heldItem == null || heldCount <= 0 || Inventory.instance == null) return;

            bool added = Inventory.instance.Add(heldItem, heldCount);
            if (!added)
            {
                // Jeśli ekwipunek jest pełny, wyrzuć przedmioty pod stopy gracza
                DropHeldItemIntoWorld();
            }

            heldItem = null;
            heldCount = 0;
            UpdateHeldItemVisuals();
        }

        public void DropHeldItemIntoWorld()
        {
            if (heldItem == null || heldCount <= 0) return;

            Vector3 dropPos;
            Quaternion dropRot;
            GetDropPositionAndRotation(out dropPos, out dropRot);

            for (int i = 0; i < heldCount; i++)
            {
                Inventory.instance.SpawnItemPickup(heldItem, dropPos + Random.insideUnitSphere * 0.1f, dropRot);
            }

            if (eqAudio != null) eqAudio.PlayDropWorld();
            heldItem = null;
            heldCount = 0;
            UpdateHeldItemVisuals();
            RefreshUI();
        }

        private void DropItemFromHeld(int count)
        {
            if (heldItem == null || heldCount <= 0) return;

            int toDrop = Mathf.Min(count, heldCount);
            Vector3 dropPos;
            Quaternion dropRot;
            GetDropPositionAndRotation(out dropPos, out dropRot);

            for (int i = 0; i < toDrop; i++)
            {
                Inventory.instance.SpawnItemPickup(heldItem, dropPos, dropRot);
            }

            heldCount -= toDrop;
            if (heldCount <= 0)
            {
                heldItem = null;
                heldCount = 0;
            }

            if (eqAudio != null) eqAudio.PlayDropWorld();
            UpdateHeldItemVisuals();
            RefreshUI();
        }

        private void DropItemFromSlot(int slotIndex, int count)
        {
            if (slotIndex < 0 || slotIndex >= Inventory.instance.items.Count) return;
            Item item = Inventory.instance.items[slotIndex];
            if (item == null) return;

            Vector3 dropPos;
            Quaternion dropRot;
            GetDropPositionAndRotation(out dropPos, out dropRot);

            Inventory.instance.DropItemAt(slotIndex, count, dropPos, dropRot);
            if (eqAudio != null) eqAudio.PlayDropWorld();

            RefreshUI();
            if (tooltipUI != null)
            {
                if (hoveredSlot != null && hoveredSlot.CurrentItem != null)
                {
                    tooltipUI.Show(hoveredSlot.CurrentItem, hoveredSlot.CurrentCount);
                }
                else
                {
                    tooltipUI.Hide();
                }
            }
        }

        private void GetDropPositionAndRotation(out Vector3 dropPos, out Quaternion dropRot)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                Vector3 forward = player.transform.forward;
                forward.y = 0f;
                if (forward.sqrMagnitude > 0.001f) forward.Normalize();
                else forward = player.transform.forward;

                dropPos = player.transform.position + forward * 1.0f + Vector3.up * 0.3f;
                dropRot = Quaternion.Euler(0f, player.transform.eulerAngles.y, 0f);
            }
            else
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    dropPos = cam.transform.position + cam.transform.forward * 1.5f;
                    dropRot = Quaternion.identity;
                }
                else
                {
                    dropPos = Vector3.zero;
                    dropRot = Quaternion.identity;
                }
            }
        }

        #endregion
    }
}
