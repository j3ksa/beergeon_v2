using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Praktyki.KowadloUlepszen
{
    /// <summary>
    /// Obsługuje wyświetlanie dynamicznego promptu ulepszania broni na środku ekranu (Screen-Space Canvas).
    /// Stylizowany przy użyciu czcionki IbarraRealNova i kapitalików (ALL CAPS).
    /// </summary>
    public class KowadloPromptUI : MonoBehaviour
    {
        public static KowadloPromptUI Instance { get; private set; }

        [Header("Konfiguracja UI")]
        [SerializeField] private TMP_FontAsset customFont;
        [SerializeField] private Canvas promptCanvas;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform promptPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI infoText;
        [SerializeField] private TextMeshProUGUI actionText;
        [SerializeField] private Image panelBackground;

        [Header("Animacja")]
        [SerializeField] private float fadeSpeed = 8f;

        private bool isVisible = false;
        private Coroutine pulseCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureUIHierarchy();
            SetAlphaInstant(0f);
        }

        private void Start()
        {
            EnsureFontLoaded();
        }

        private void EnsureFontLoaded()
        {
            if (customFont != null) return;

#if UNITY_EDITOR
            string[] possiblePaths = new string[]
            {
                "Assets/Praktyki/IbarraRealNova-VariableFont_wght SDF.asset",
                "Assets/Praktyki/SaveSystem/Graphics/IbarraRealNova-VariableFont_wght SDF.asset"
            };

            foreach (string path in possiblePaths)
            {
                TMP_FontAsset font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                if (font != null)
                {
                    customFont = font;
                    break;
                }
            }
#endif
            if (customFont == null)
            {
                customFont = Resources.Load<TMP_FontAsset>("IbarraRealNova-VariableFont_wght SDF");
            }

            if (customFont != null)
            {
                if (titleText != null) titleText.font = customFont;
                if (infoText != null) infoText.font = customFont;
                if (actionText != null) actionText.font = customFont;
            }
        }

        /// <summary>
        /// Automatycznie tworzy hierarchię Canvas, jeśli nie została przypisana w prefabie/scenie.
        /// </summary>
        public void EnsureUIHierarchy()
        {
            if (promptCanvas != null && titleText != null && infoText != null && actionText != null)
            {
                if (promptPanel != null) promptPanel.sizeDelta = new Vector2(900f, 215f);
                if (infoText != null) infoText.fontSize = 16f;
                if (actionText != null) actionText.fontSize = 18f;
                return;
            }

            // Sprawdź czy już istnieje jako dziecko
            promptCanvas = GetComponentInChildren<Canvas>(true);
            if (promptCanvas == null)
            {
                GameObject canvasObj = new GameObject("Kowadlo_ScreenHUD_Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvasObj.transform.SetParent(transform, false);
                promptCanvas = canvasObj.GetComponent<Canvas>();
                promptCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                promptCanvas.sortingOrder = 95;

                CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
            }

            canvasGroup = promptCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = promptCanvas.gameObject.AddComponent<CanvasGroup>();

            // Panel główny na środku ekranu
            Transform existingPanel = promptCanvas.transform.Find("PromptPanel");
            if (existingPanel != null && existingPanel.GetComponent<RectTransform>() == null)
            {
                if (Application.isPlaying) Destroy(existingPanel.gameObject);
                else DestroyImmediate(existingPanel.gameObject);
                existingPanel = null;
            }

            GameObject panelObj;
            if (existingPanel == null)
            {
                panelObj = new GameObject("PromptPanel", typeof(RectTransform));
                panelObj.transform.SetParent(promptCanvas.transform, false);
            }
            else
            {
                panelObj = existingPanel.gameObject;
            }

            promptPanel = panelObj.GetComponent<RectTransform>();
            if (promptPanel != null)
            {
                promptPanel.anchorMin = new Vector2(0.5f, 0.5f);
                promptPanel.anchorMax = new Vector2(0.5f, 0.5f);
                promptPanel.pivot = new Vector2(0.5f, 0.5f);
                promptPanel.anchoredPosition = new Vector2(0f, -70f); // Tuż pod celownikiem / na środku
                promptPanel.sizeDelta = new Vector2(900f, 215f); // Szeroki panel z zapasem na długie teksty
            }

            panelBackground = panelObj.GetComponent<Image>();
            if (panelBackground == null) panelBackground = panelObj.AddComponent<Image>();
            panelBackground.color = new Color(0.06f, 0.06f, 0.08f, 0.92f); // Ciemny, elegancki medieval podkład

            // Obwódka / Outline panelu
            Outline outline = panelObj.GetComponent<Outline>();
            if (outline == null) outline = panelObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.85f, 0.7f, 0.3f, 0.75f); // Złotawo-mosiężna obwódka
            outline.effectDistance = new Vector2(2, -2);

            // Vertical Layout Group
            VerticalLayoutGroup vLayout = panelObj.GetComponent<VerticalLayoutGroup>();
            if (vLayout == null) vLayout = panelObj.AddComponent<VerticalLayoutGroup>();
            vLayout.childAlignment = TextAnchor.MiddleCenter;
            vLayout.padding = new RectOffset(20, 20, 14, 14);
            vLayout.spacing = 6;
            vLayout.childControlHeight = false;
            vLayout.childForceExpandHeight = false;
            vLayout.childControlWidth = true;
            vLayout.childForceExpandWidth = true;

            EnsureFontLoaded();

            // Tytuł
            titleText = CreateTextMesh("TitleText", panelObj.transform, 22f, FontStyles.Bold, new Color(1f, 0.84f, 0.25f)); // Złoty
            // Info (aktualny poziom i damage)
            infoText = CreateTextMesh("InfoText", panelObj.transform, 18f, FontStyles.Normal, Color.white);
            // Akcja / Koszt
            actionText = CreateTextMesh("ActionText", panelObj.transform, 18f, FontStyles.Bold, new Color(0.3f, 1f, 0.4f)); // Zielonkawy / biały
        }

        private TextMeshProUGUI CreateTextMesh(string name, Transform parent, float fontSize, FontStyles style, Color color)
        {
            Transform existing = parent.Find(name);
            if (existing != null && existing.GetComponent<RectTransform>() == null)
            {
                if (Application.isPlaying) Destroy(existing.gameObject);
                else DestroyImmediate(existing.gameObject);
                existing = null;
            }

            GameObject textObj = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
            textObj.transform.SetParent(parent, false);

            TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = textObj.AddComponent<TextMeshProUGUI>();

            if (customFont != null) tmp.font = customFont;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.NoWrap; // Zapobiega rozjeżdżaniu pojedynczych słów takich jak DMG
            tmp.overflowMode = TextOverflowModes.Overflow;
            return tmp;
        }

        private void Update()
        {
            if (canvasGroup == null) return;

            float targetAlpha = isVisible ? 1f : 0f;
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
        }

        public void Show()
        {
            isVisible = true;
        }

        public void Hide()
        {
            isVisible = false;
        }

        private void SetAlphaInstant(float alpha)
        {
            if (canvasGroup != null) canvasGroup.alpha = alpha;
        }

        /// <summary>
        /// Aktualizuje treść promptu na środku ekranu.
        /// Zgodnie z wytycznymi wszystkie teksty są pisane z wielkich liter (ALL CAPS).
        /// </summary>
        public void UpdatePromptContent(Item heldItem, int playerCaps, WeaponUpgradeManager upgradeManager)
        {
            EnsureUIHierarchy();
            EnsureFontLoaded();

            if (heldItem == null)
            {
                titleText.text = "KOWADŁO ULEPSZEŃ".ToUpper();
                infoText.text = "BRAK PRZEDMIOTU W DŁONI".ToUpper();
                actionText.text = "<color=#AAAAAA>WEŹ BROŃ DO RĘKI, ABY JĄ ULEPSZYĆ NA KOWADLE</color>".ToUpper();
                return;
            }

            bool isUpgradeable = false;
            int baseDamage = 0;
            string weaponName = heldItem.itemName;

            if (upgradeManager != null)
            {
                isUpgradeable = upgradeManager.IsUpgradeableWeapon(heldItem, out baseDamage, out weaponName);
            }
            else if (heldItem is WeaponItem weapon)
            {
                isUpgradeable = true;
                baseDamage = weapon.damage;
                weaponName = weapon.itemName;
            }
            else if (heldItem is Praktyki.Kusza.CrossbowItem crossbow)
            {
                isUpgradeable = true;
                baseDamage = crossbow.weaponDamage;
                weaponName = crossbow.itemName;
            }

            if (heldItem is Praktyki.Zbroja.ArmorItem armor)
            {
                int armorLvl = upgradeManager != null ? upgradeManager.GetUpgradeLevel(armor) : 0;
                int currentDef = upgradeManager != null ? upgradeManager.GetArmorEffectiveDefense(armor) : armor.defense;
                int currentHp = upgradeManager != null ? upgradeManager.GetArmorEffectiveHealth(armor) : armor.bonusHealth;
                float currentSpd = upgradeManager != null ? upgradeManager.GetArmorEffectiveSpeedModifier(armor) : armor.speedBonus;
                float currentStam = upgradeManager != null ? upgradeManager.GetArmorEffectiveStaminaDrainPenalty(armor) : armor.staminaDrainPenalty;
                bool armorIsMax = armorLvl >= 6;

                titleText.text = $"KOWADŁO: {armor.itemName.ToUpper()}".ToUpper();

                string spdCurrent = currentSpd < 0 ? $"{currentSpd * 100:0}% SPD" : "0% SPD";
                string stamCurrent = currentStam > 0 ? $"+{currentStam * 100:0}% DRAIN" : "0% DRAIN";

                if (armorIsMax)
                {
                    infoText.text = $"POZIOM: +{armorLvl} (MAX)  |  OBRONA: +{currentDef} DEF  |  HP: +{currentHp}\nWAGA: {spdCurrent}, {stamCurrent}  |  MISTRZOWSKIE WYKUCIE (HARTOWANA STAL)".ToUpper();
                    actionText.text = "<color=#FFD700>POZIOM MAKSYMALNY (+6) - PANCERZ PERFEKCYJNIE PRZEKUTY</color>".ToUpper();
                    return;
                }

                int nextArmorLvl = armorLvl + 1;
                int nextDef = currentDef + 1;
                int nextHp = currentHp + 5;
                float nextSpd = Mathf.Min(0f, currentSpd + 0.008f);
                float nextStam = Mathf.Max(0f, currentStam - 0.02f);
                int armorCost = upgradeManager != null ? upgradeManager.GetArmorUpgradeCost(armor, armorLvl) : 25;
                bool canAffordArmor = playerCaps >= armorCost;

                string spdNext = nextSpd < 0 ? $"{nextSpd * 100:0}% SPD" : "0% SPD";
                string stamNext = nextStam > 0 ? $"+{nextStam * 100:0}% DRAIN" : "0% DRAIN";

                infoText.text = $"POZIOM: +{armorLvl}  |  +{currentDef} DEF (+{currentHp} HP)  |  WAGA: {spdCurrent}, {stamCurrent}\nPO ULEPSZENIU (+{nextArmorLvl}): +{nextDef} DEF (+{nextHp} HP)  |  LŻEJSZA: {spdNext}, {stamNext}".ToUpper();

                if (canAffordArmor)
                {
                    actionText.text = $"<color=#55FF55>[E] PRZEKUJ ZBROJĘ NA POZIOM +{nextArmorLvl} (KOSZT: {armorCost} KAPSLI)</color>".ToUpper();
                }
                else
                {
                    actionText.text = $"<color=#FF4444>[E] ZA MAŁO KAPSLI! POTRZEBA: {armorCost} (MASZ: {playerCaps})</color>".ToUpper();
                }
                return;
            }

            if (!isUpgradeable)
            {
                titleText.text = "KOWADŁO ULEPSZEŃ".ToUpper();
                infoText.text = $"TRZYMASZ: {heldItem.itemName.ToUpper()}".ToUpper();
                actionText.text = "<color=#FFAA00>WYBIERZ BROŃ LUB PANCERZ DO ULEPSZENIA.</color>".ToUpper();
                return;
            }

            int currentLvl = upgradeManager != null ? upgradeManager.GetUpgradeLevel(heldItem) : 0;
            int currentBonus = upgradeManager != null ? upgradeManager.GetBonusDamage(heldItem) : 0;
            int currentTotalDamage = baseDamage + currentBonus;
            bool isMax = upgradeManager != null && upgradeManager.IsMaxLevel(heldItem);

            titleText.text = $"KOWADŁO: {weaponName.ToUpper()}".ToUpper();

            if (isMax)
            {
                string specialMax = upgradeManager != null ? upgradeManager.GetWeaponSpecialCurrentText(heldItem, currentLvl) : string.Empty;
                string specialMaxLine = !string.IsNullOrEmpty(specialMax) ? $"\n<color=#00E5FF>BONUS: {specialMax}</color>" : string.Empty;
                infoText.text = $"POZIOM: +{currentLvl} (MAX)  |  OBRAŻENIA: {currentTotalDamage} DMG{specialMaxLine}".ToUpper();
                actionText.text = "<color=#FFD700>POZIOM MAKSYMALNY (+6) - MISTRZOWSKIE WYKUCIE (+15% KRYT)</color>".ToUpper();
                return;
            }

            int nextLvl = currentLvl + 1;
            int nextBonus = currentBonus + 5;
            int nextTotalDamage = baseDamage + nextBonus;
            int cost = upgradeManager != null ? upgradeManager.GetUpgradeCost(heldItem, currentLvl) : 20;
            bool canAfford = playerCaps >= cost;

            string specialUpgrade = upgradeManager != null ? upgradeManager.GetWeaponSpecialUpgradeText(heldItem, currentLvl, nextLvl) : string.Empty;
            string specialUpgradeLine = !string.IsNullOrEmpty(specialUpgrade) ? $"\n<color=#00E5FF>BONUS: {specialUpgrade}</color>" : string.Empty;

            infoText.text = $"AKTUALNY POZIOM: +{currentLvl}  |  OBRAŻENIA: {currentTotalDamage} DMG\nPO ULEPSZENIU (+{nextLvl}): {nextTotalDamage} DMG (+5 DMG){specialUpgradeLine}".ToUpper();

            if (canAfford)
            {
                actionText.text = $"<color=#55FF55>[E] ULEPSZ NA POZIOM +{nextLvl} (KOSZT: {cost} KAPSLI)</color>".ToUpper();
            }
            else
            {
                actionText.text = $"<color=#FF4444>[E] ZA MAŁO KAPSLI! POTRZEBA: {cost} (MASZ: {playerCaps})</color>".ToUpper();
            }
        }

        /// <summary>
        /// Efekt impulsu / powiększenia panelu po udanym ulepszeniu.
        /// </summary>
        public void TriggerUpgradePulse()
        {
            if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
            pulseCoroutine = StartCoroutine(AnimatePulse());
        }

        private IEnumerator AnimatePulse()
        {
            if (promptPanel == null) yield break;

            Vector3 baseScale = Vector3.one;
            Vector3 peakScale = new Vector3(1.12f, 1.12f, 1f);

            float t = 0f;
            while (t < 0.12f)
            {
                t += Time.deltaTime;
                promptPanel.localScale = Vector3.Lerp(baseScale, peakScale, t / 0.12f);
                yield return null;
            }

            t = 0f;
            while (t < 0.18f)
            {
                t += Time.deltaTime;
                promptPanel.localScale = Vector3.Lerp(peakScale, baseScale, t / 0.18f);
                yield return null;
            }

            promptPanel.localScale = baseScale;
        }
    }
}
