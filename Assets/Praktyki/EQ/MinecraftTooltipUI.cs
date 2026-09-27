using UnityEngine;
using TMPro;

namespace Praktyki.EQ
{
    public class MinecraftTooltipUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private RectTransform tooltipRect;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemTypeText;
        [SerializeField] private TextMeshProUGUI itemStatsText;
        [SerializeField] private TextMeshProUGUI itemDescText;

        [Header("Settings")]
        [SerializeField] private Vector2 cursorOffset = new Vector2(18f, -18f);

        private Canvas parentCanvas;

        public void InitializeReferences(RectTransform rect, CanvasGroup cg, TextMeshProUGUI nameTxt, TextMeshProUGUI typeTxt, TextMeshProUGUI statsTxt, TextMeshProUGUI descTxt)
        {
            tooltipRect = rect;
            canvasGroup = cg;
            itemNameText = nameTxt;
            itemTypeText = typeTxt;
            itemStatsText = statsTxt;
            itemDescText = descTxt;
            parentCanvas = GetComponentInParent<Canvas>();
        }

        private void Awake()
        {
            if (tooltipRect == null) tooltipRect = GetComponent<RectTransform>();
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            parentCanvas = GetComponentInParent<Canvas>();
            Hide();
        }

        private void Update()
        {
            if (canvasGroup != null && canvasGroup.alpha > 0.01f)
            {
                FollowCursor();
            }
        }

        public void Show(Item item, int count)
        {
            if (item == null)
            {
                Hide();
                return;
            }

            // 1. Nazwa i styl przedmiotu
            string nameColor = "#FFFFFF";
            string typeString = "Przedmiot";

            if (item is WeaponItem weapon)
            {
                nameColor = "#FFAA00"; // Złoty Minecraft
                typeString = "Broń";
            }
            else if (item is PotionItem potion)
            {
                nameColor = "#FF55FF"; // Różowy / Magiczny Minecraft
                typeString = "Mikstura";
            }
            else if (item is Praktyki.Zbroja.ArmorItem armorItem)
            {
                nameColor = "#64B5F6"; // Jasnoniebieski pancerz
                string slotName = armorItem.slotType switch
                {
                    Praktyki.Zbroja.ArmorSlotType.Helmet => "Hełm",
                    Praktyki.Zbroja.ArmorSlotType.Chestplate => "Napierśnik",
                    Praktyki.Zbroja.ArmorSlotType.Leggings => "Nagolenniki",
                    Praktyki.Zbroja.ArmorSlotType.Boots => "Buty",
                    _ => "Zbroja"
                };
                typeString = $"Zbroja ({slotName})";
            }
            else if (item is EquippableItem)
            {
                nameColor = "#55FFFF"; // Diamentowy / Aqua
                typeString = "Wyposażenie";
            }

            if (itemNameText != null)
            {
                int upgLvl = 0;
                if (Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance != null)
                {
                    upgLvl = Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance.GetUpgradeLevel(item);
                }
                string lvlSuffix = upgLvl > 0 ? $" <color=#FFD700>+{upgLvl}</color>" : "";
                itemNameText.text = $"<color={nameColor}><b>{item.itemName}{lvlSuffix}</b></color>";
            }

            if (itemTypeText != null)
            {
                itemTypeText.text = $"<color=#AAAAAA><i>[{typeString}]</i></color>";
            }

            // 2. Statystyki
            string stats = "";
            if (item is WeaponItem wp)
            {
                int wBonus = Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance != null ? Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance.GetBonusDamage(wp) : 0;
                stats += $"<color=#55FF55>+ Obrażenia: {wp.damage + wBonus}</color>\n";
                stats += $"<color=#FFFF55>- Zasięg: {wp.range:0.0}m</color>\n";
                stats += $"<color=#55FFFF>- Odnowienie: {wp.cooldown:0.00}s</color>\n";
            }
            else if (item is PotionItem pot)
            {
                stats += $"<color=#FF55FF>- Efekt: {pot.potionType} (+{pot.amount})</color>\n";
                if (pot.duration > 0)
                {
                    stats += $"<color=#55FFFF>- Czas trwania: {pot.duration:0.0}s</color>\n";
                }
            }
            else if (item is Praktyki.Zbroja.ArmorItem armor)
            {
                var mgr = Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance;
                int lvl = mgr != null ? mgr.GetUpgradeLevel(armor) : 0;
                int def = mgr != null ? mgr.GetArmorEffectiveDefense(armor) : armor.defense;
                int hp = mgr != null ? mgr.GetArmorEffectiveHealth(armor) : armor.bonusHealth;
                float spd = mgr != null ? mgr.GetArmorEffectiveSpeedModifier(armor) : armor.speedBonus;
                float stam = mgr != null ? mgr.GetArmorEffectiveStaminaDrainPenalty(armor) : armor.staminaDrainPenalty;

                string bonusDefStr = lvl > 0 ? $" <color=#FFD700>(+{lvl} Kowadło)</color>" : "";
                stats += $"<color=#64B5F6>+ Pancerz: +{def} DEF{bonusDefStr}</color>\n";
                if (hp > 0)
                {
                    stats += $"<color=#FF5555>+ Zdrowie: +{hp} HP</color>\n";
                }
                if (Mathf.Abs(spd) > 0.001f)
                {
                    string spdColor = spd > 0 ? "#55FF55" : "#FFAA00";
                    string sign = spd > 0 ? "+" : "";
                    stats += $"<color={spdColor}>{sign}{spd * 100:0}% Prędkości ruchu</color>\n";
                }
                else
                {
                    stats += $"<color=#55FF55>+ 0% Kary do prędkości (lekki)</color>\n";
                }
                if (Mathf.Abs(stam) > 0.001f)
                {
                    string stColor = stam > 0 ? "#FF7777" : "#55FF55";
                    string sign = stam > 0 ? "+" : "";
                    stats += $"<color={stColor}>{sign}{stam * 100:0}% Zużycia staminy (Sprint)</color>\n";
                }
                else
                {
                    stats += $"<color=#55FF55>+ 0% Kary do staminy</color>\n";
                }

                if (lvl >= 6)
                {
                    stats += $"<color=#FFD700>★ Mistrzowskie Wykucie (Hartowana Stal)</color>\n";
                }
            }
            else if (item is EquippableItem eq)
            {
                stats += $"<color=#55FFFF>- Czas użycia: {eq.cooldown:0.00}s</color>\n";
            }

            if (item.isStackable)
            {
                stats += $"<color=#888888>- Max stos: {item.maxStack}</color>";
            }

            if (itemStatsText != null)
            {
                itemStatsText.text = stats.TrimEnd();
                itemStatsText.gameObject.SetActive(!string.IsNullOrEmpty(stats));
            }

            // 3. Opis
            if (itemDescText != null)
            {
                string desc = item.description;
                if (!string.IsNullOrEmpty(desc))
                {
                    itemDescText.text = $"<color=#CCCCCC>{desc}</color>";
                    itemDescText.gameObject.SetActive(true);
                }
                else
                {
                    itemDescText.gameObject.SetActive(false);
                }
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = false;
            }

            // Natychmiastowe przeliczenie wymiarów tooltipa po zmianie tekstów
            if (tooltipRect != null)
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
            }

            FollowCursor();
        }

        public void Hide()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
            }
        }

        private void FollowCursor()
        {
            if (tooltipRect == null) return;
            if (parentCanvas == null) parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null) return;

            RectTransform canvasRect = parentCanvas.transform as RectTransform;
            if (canvasRect == null) return;

            Vector2 mousePos = UnityEngine.InputSystem.Mouse.current != null 
                ? UnityEngine.InputSystem.Mouse.current.position.ReadValue() 
                : Vector2.zero;

            Camera cam = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, mousePos, cam, out Vector2 localPoint);

            float tooltipW = tooltipRect.rect.width > 50f ? tooltipRect.rect.width : 310f;
            float tooltipH = tooltipRect.rect.height;

            float halfW = canvasRect.rect.width * 0.5f;
            float halfH = canvasRect.rect.height * 0.5f;

            // Pozycja początkowa: tuż na prawo i lekko w dół od kursora
            Vector2 targetPos = localPoint + new Vector2(16f, -16f);

            // Jeśli wychodzi poza prawą krawędź ekranu, przerzuć na lewo od kursora
            if (targetPos.x + tooltipW > halfW - 10f)
            {
                targetPos.x = localPoint.x - tooltipW - 16f;
            }

            // Jeśli wychodzi poza dolną krawędź ekranu (np. nad dolnym paskiem), przerzuć nad kursor
            if (targetPos.y - tooltipH < -halfH + 10f)
            {
                targetPos.y = localPoint.y + tooltipH + 16f;
            }

            // Ogranicz do granic ekranu
            targetPos.x = Mathf.Clamp(targetPos.x, -halfW + 10f, halfW - tooltipW - 10f);
            targetPos.y = Mathf.Clamp(targetPos.y, -halfH + tooltipH + 10f, halfH - 10f);

            tooltipRect.pivot = new Vector2(0f, 1f);
            tooltipRect.anchoredPosition = targetPos;
        }
    }
}
