using System;
using System.Collections.Generic;
using UnityEngine;
using CurrencySystem;

namespace Praktyki.KowadloUlepszen
{
    /// <summary>
    /// Zarządza poziomami ulepszeń poszczególnych broni gracza.
    /// Zapobiega modyfikacji współdzielonych assetów ScriptableObject (WeaponItem).
    /// </summary>
    public class WeaponUpgradeManager : MonoBehaviour
    {
        public static WeaponUpgradeManager Instance { get; private set; }

        [Header("Konfiguracja Ulepszeń")]
        [Tooltip("Maksymalny poziom ulepszenia każdej broni")]
        [SerializeField] private int maxLevel = 6;

        [Tooltip("Dodatkowe obrażenia za każdy poziom ulepszenia")]
        [SerializeField] private int damageBonusPerLevel = 5;

        [Tooltip("Koszt ulepszenia broni białej na kolejny poziom (indeks 0 = ulepszenie na poziom +1)")]
        [SerializeField] private int[] upgradeCosts = new int[] { 20, 40, 70, 110, 160, 230 };

        [Tooltip("Koszt ulepszenia kuszy na kolejny poziom (o 15 kapsli więcej na start i wyższe skalowanie)")]
        [SerializeField] private int[] crossbowUpgradeCosts = new int[] { 35, 65, 105, 160, 230, 320 };

        [Header("Bonus Mistrzowski (Poziom +6 MAX)")]
        [Tooltip("Dodatkowy procent szansy na trafienie krytyczne przy maksymalnym poziomie broni")]
        [SerializeField] private float masterworkCritBonus = 0.15f; // +15% do szansy na krytyk

        // Słownik przechowujący poziom ulepszenia danej broni: [ID/Nazwa broni -> Poziom]
        private readonly Dictionary<string, int> weaponLevels = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public event Action<string, int> OnWeaponUpgraded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Sprawdza czy dany przedmiot jest bronią podlegającą ulepszeniu (broń biała lub kusza).
        /// </summary>
        public bool IsUpgradeableWeapon(Item item, out int baseDamage, out string weaponName)
        {
            baseDamage = 0;
            weaponName = string.Empty;
            if (item == null) return false;

            if (item is WeaponItem weapon)
            {
                baseDamage = weapon.damage;
                weaponName = !string.IsNullOrEmpty(weapon.itemName) ? weapon.itemName : weapon.name;
                return true;
            }

            if (item is Praktyki.Kusza.CrossbowItem crossbow)
            {
                baseDamage = crossbow.weaponDamage;
                weaponName = !string.IsNullOrEmpty(crossbow.itemName) ? crossbow.itemName : crossbow.name;
                return true;
            }

            // Fallback po nazwie na wypadek customowych wariantów kuszy
            if (item.name.IndexOf("Kusz", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                baseDamage = 20;
                weaponName = !string.IsNullOrEmpty(item.itemName) ? item.itemName : item.name;
                return true;
            }

            return false;
        }

        public bool IsCrossbow(Item item)
        {
            if (item == null) return false;
            return item is Praktyki.Kusza.CrossbowItem || item.name.IndexOf("Kusz", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Zwraca unikalny identyfikator broni na podstawie Itemu.
        /// </summary>
        public string GetWeaponKey(Item item)
        {
            if (item == null) return string.Empty;
            return !string.IsNullOrEmpty(item.itemName) ? item.itemName : item.name;
        }

        /// <summary>
        /// Zwraca aktualny poziom ulepszenia danej broni (od 0 do maxLevel).
        /// </summary>
        public int GetUpgradeLevel(Item item)
        {
            if (item == null) return 0;
            return GetUpgradeLevel(GetWeaponKey(item));
        }

        public int GetUpgradeLevel(string weaponKey)
        {
            if (string.IsNullOrEmpty(weaponKey)) return 0;
            return weaponLevels.TryGetValue(weaponKey, out int lvl) ? Mathf.Clamp(lvl, 0, maxLevel) : 0;
        }

        /// <summary>
        /// Zwraca bonus do obrażeń dla danej broni na podstawie jej poziomu.
        /// </summary>
        public int GetBonusDamage(Item item)
        {
            if (item == null) return 0;
            return GetUpgradeLevel(item) * damageBonusPerLevel;
        }

        /// <summary>
        /// Zwraca bazowe obrażenia broni powiększone o ulepszenia.
        /// </summary>
        public int GetTotalDamage(WeaponItem weapon)
        {
            if (weapon == null) return 0;
            return weapon.damage + GetBonusDamage(weapon);
        }

        /// <summary>
        /// Zwraca koszt ulepszenia broni z jej aktualnego poziomu na kolejny.
        /// Kusza kosztuje 15 kapsli więcej na start i ma wyższe skalowanie.
        /// </summary>
        public int GetUpgradeCost(Item item, int currentLevel)
        {
            if (currentLevel >= maxLevel) return 0;

            if (IsCrossbow(item))
            {
                if (crossbowUpgradeCosts != null && currentLevel >= 0 && currentLevel < crossbowUpgradeCosts.Length)
                {
                    return crossbowUpgradeCosts[currentLevel];
                }
                return 35 + (currentLevel * 45);
            }

            if (upgradeCosts != null && currentLevel >= 0 && currentLevel < upgradeCosts.Length)
            {
                return upgradeCosts[currentLevel];
            }
            return 20 + (currentLevel * 30);
        }

        public int GetUpgradeCost(int currentLevel)
        {
            return GetUpgradeCost(null, currentLevel);
        }

        public bool IsMaxLevel(Item item)
        {
            return GetUpgradeLevel(item) >= maxLevel;
        }

        public bool HasMasterworkBonus(Item item)
        {
            return IsMaxLevel(item);
        }

        public float GetMasterworkCritBonus(Item item)
        {
            return HasMasterworkBonus(item) ? masterworkCritBonus : 0f;
        }

        /// <summary>
        /// Próbuje ulepszyć broń (białą lub kuszę) o 1 poziom za kapsle.
        /// </summary>
        public bool TryUpgradeWeapon(Item item, out string feedbackMessage)
        {
            if (!IsUpgradeableWeapon(item, out int baseDamage, out string weaponName))
            {
                feedbackMessage = "TO NIE JEST BROŃ, KTÓRĄ MOŻNA ULEPSZYĆ!";
                return false;
            }

            string key = GetWeaponKey(item);
            int currentLvl = GetUpgradeLevel(key);

            if (currentLvl >= maxLevel)
            {
                feedbackMessage = $"{weaponName.ToUpper()} OSIĄGNĄŁ JUŻ MAKSYMALNY POZIOM (+{maxLevel})!";
                return false;
            }

            int cost = GetUpgradeCost(item, currentLvl);

            PlayerCurrency currency = PlayerCurrency.Instance ?? FindFirstObjectByType<PlayerCurrency>();
            if (currency == null)
            {
                feedbackMessage = "BŁĄD: BRAK SYSTEMU KAPSLI GRACZA!";
                return false;
            }

            if (!currency.TrySpendCaps(cost))
            {
                feedbackMessage = $"ZA MAŁO KAPSLI! POTRZEBA: {cost} KAPSLI (POSIADASZ: {currency.CurrentCaps})";
                return false;
            }

            int newLvl = currentLvl + 1;
            weaponLevels[key] = newLvl;

            int newTotalDamage = baseDamage + (newLvl * damageBonusPerLevel);
            feedbackMessage = $"POMYŚLNIE ULEPSZONO {weaponName.ToUpper()} NA POZIOM +{newLvl}! (OBRAŻENIA: {newTotalDamage} DMG)";

            Debug.Log($"<color=#39FF14>[WeaponUpgradeManager]</color> Ulepszono broń: {key} -> Poziom +{newLvl} (+{newLvl * damageBonusPerLevel} DMG) za {cost} kapsli.");

            OnWeaponUpgraded?.Invoke(key, newLvl);
            return true;
        }

        #region Armor Upgrades

        public bool IsUpgradeableArmor(Item item, out Praktyki.Zbroja.ArmorItem armor)
        {
            if (item is Praktyki.Zbroja.ArmorItem a)
            {
                armor = a;
                return true;
            }
            armor = null;
            return false;
        }

        public int GetArmorEffectiveDefense(Praktyki.Zbroja.ArmorItem armor)
        {
            if (armor == null) return 0;
            int lvl = GetUpgradeLevel(armor);
            int bonus = lvl * 1; // +1 DEF na poziom
            if (lvl >= maxLevel) bonus += 1; // Bonus mistrzowski: +1 ekstra DEF
            return armor.defense + bonus;
        }

        public int GetArmorEffectiveHealth(Praktyki.Zbroja.ArmorItem armor)
        {
            if (armor == null) return 0;
            int lvl = GetUpgradeLevel(armor);
            return armor.bonusHealth + (lvl * 5); // +5 HP na poziom
        }

        public float GetArmorEffectiveSpeedModifier(Praktyki.Zbroja.ArmorItem armor)
        {
            if (armor == null) return 0f;
            int lvl = GetUpgradeLevel(armor);
            // Każdy poziom zmniejsza karę do prędkości o 0.8% (zbroja staje się lżejsza i lepiej dopasowana)
            float reduction = lvl * 0.008f;
            float effective = armor.speedBonus + reduction;
            if (armor.speedBonus < 0f && effective > 0f) effective = 0f;
            return effective;
        }

        public float GetArmorEffectiveStaminaDrainPenalty(Praktyki.Zbroja.ArmorItem armor)
        {
            if (armor == null) return 0f;
            int lvl = GetUpgradeLevel(armor);
            // Każdy poziom zmniejsza karę do zużycia staminy o 2.0%
            float reduction = lvl * 0.02f;
            if (armor.staminaDrainPenalty < 0f) return armor.staminaDrainPenalty - reduction;
            return Mathf.Max(0f, armor.staminaDrainPenalty - reduction);
        }

        public int GetArmorUpgradeCost(Praktyki.Zbroja.ArmorItem armor, int currentLevel)
        {
            int[] armorCosts = new int[] { 25, 45, 75, 115, 165, 230 };
            if (currentLevel >= 0 && currentLevel < armorCosts.Length)
                return armorCosts[currentLevel];
            return 25 + (currentLevel * 35);
        }

        public bool TryUpgradeArmor(Praktyki.Zbroja.ArmorItem armor, out string feedbackMessage)
        {
            if (armor == null)
            {
                feedbackMessage = "TO NIE JEST PANCERZ!";
                return false;
            }

            string key = GetWeaponKey(armor);
            int currentLvl = GetUpgradeLevel(key);

            if (currentLvl >= maxLevel)
            {
                feedbackMessage = $"{armor.itemName.ToUpper()} OSIĄGNĄŁ JUŻ MAKSYMALNY POZIOM (+{maxLevel})!";
                return false;
            }

            int cost = GetArmorUpgradeCost(armor, currentLvl);

            PlayerCurrency currency = PlayerCurrency.Instance ?? FindFirstObjectByType<PlayerCurrency>();
            if (currency == null)
            {
                feedbackMessage = "BŁĄD: BRAK SYSTEMU KAPSLI GRACZA!";
                return false;
            }

            if (!currency.TrySpendCaps(cost))
            {
                feedbackMessage = $"ZA MAŁO KAPSLI! POTRZEBA: {cost} KAPSLI (POSIADASZ: {currency.CurrentCaps})";
                return false;
            }

            int newLvl = currentLvl + 1;
            weaponLevels[key] = newLvl;

            int newDef = GetArmorEffectiveDefense(armor);
            feedbackMessage = $"POMYŚLNIE PRZEKUTO {armor.itemName.ToUpper()} NA POZIOM +{newLvl}! (OBRONA: +{newDef} DEF)";

            Debug.Log($"<color=#64B5F6>[WeaponUpgradeManager]</color> Ulepszono zbroję: {key} -> Poziom +{newLvl} (+{newDef} DEF) za {cost} kapsli.");

            if (Praktyki.EQ.MinecraftInventoryUI.Instance != null)
            {
                Praktyki.EQ.MinecraftInventoryUI.Instance.RecalculateArmorStats();
                Praktyki.EQ.MinecraftInventoryUI.Instance.RefreshUI();
            }

            OnWeaponUpgraded?.Invoke(key, newLvl);
            return true;
        }

        #endregion


        /// <summary>
        /// Bezpośrednie ustawienie poziomu ulepszenia broni (używane podczas odczytu zapisu gry).
        /// </summary>
        public void SetWeaponLevel(string weaponKey, int level)
        {
            if (string.IsNullOrEmpty(weaponKey)) return;
            weaponLevels[weaponKey] = Mathf.Clamp(level, 0, maxLevel);
        }

        /// <summary>
        /// Zwraca stan do zapisu w SaveData.
        /// </summary>
        public List<SaveSystem.WeaponUpgradeSaveData> GetSaveData()
        {
            var list = new List<SaveSystem.WeaponUpgradeSaveData>();
            foreach (var kvp in weaponLevels)
            {
                list.Add(new SaveSystem.WeaponUpgradeSaveData(kvp.Key, kvp.Value));
            }
            return list;
        }

        /// <summary>
        /// Wczytuje stan ulepszeń z zapisu gry.
        /// </summary>
        public void LoadSaveData(List<SaveSystem.WeaponUpgradeSaveData> dataList)
        {
            weaponLevels.Clear();
            if (dataList == null) return;

            foreach (var entry in dataList)
            {
                if (!string.IsNullOrEmpty(entry.weaponId))
                {
                    weaponLevels[entry.weaponId] = Mathf.Clamp(entry.level, 0, maxLevel);
                }
            }
            Debug.Log($"<color=#00FFFF>[WeaponUpgradeManager]</color> Wczytano stan ulepszeń dla {weaponLevels.Count} broni.");
        }

        /// <summary>
        /// Zwraca tekst opisujący ulepszenie unikalnej mechaniki broni przy awansie na kolejny poziom.
        /// </summary>
        public string GetWeaponSpecialUpgradeText(Item item, int currentLevel, int nextLevel)
        {
            if (item == null) return string.Empty;
            string key = GetWeaponKey(item);

            if (key.IndexOf("Sierp", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                int curRupture = 14 + (currentLevel * 2);
                int nextRupture = 14 + (nextLevel * 2);
                return $"WYSSANIE (RUPTURE): {curRupture}% -> {nextRupture}% MAX HP | CIĘCIE: +0.5%";
            }

            if (key.IndexOf("Zatrut", StringComparison.OrdinalIgnoreCase) >= 0 || key.IndexOf("Poison", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                int curChance = 35 + (currentLevel * 5);
                int nextChance = 35 + (nextLevel * 5);
                int curDmg = 3 + currentLevel;
                int nextDmg = 3 + nextLevel;
                return $"SZANSA NA TRUCIZNĘ: {curChance}% -> {nextChance}% (+5%) | DMG TRUCIZNY: {curDmg} -> {nextDmg}/S";
            }

            if (key.IndexOf("Młot", StringComparison.OrdinalIgnoreCase) >= 0 || key.IndexOf("Mlot", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                float curCost = Mathf.Max(2f, 7.0f - (currentLevel * 0.8f));
                float nextCost = Mathf.Max(2f, 7.0f - (nextLevel * 0.8f));
                int curReq = Mathf.Max(50, 90 - (currentLevel * 6));
                int nextReq = Mathf.Max(50, 90 - (nextLevel * 6));
                return $"KOSZT STAMINY (LMP): {curCost:0.0} -> {nextCost:0.0} (-0.8) | MIN. STAMINA PPM: {curReq}% -> {nextReq}%";
            }

            if (key.IndexOf("Włócz", StringComparison.OrdinalIgnoreCase) >= 0 || key.IndexOf("Wlocz", StringComparison.OrdinalIgnoreCase) >= 0 || key.IndexOf("Spear", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                float curDur = 3.5f + (currentLevel * 0.4f);
                float nextDur = 3.5f + (nextLevel * 0.4f);
                float curKb = 10f + (currentLevel * 1.5f);
                float nextKb = 10f + (nextLevel * 1.5f);
                return $"CZAS SZARŻY: {curDur:0.0}S -> {nextDur:0.0}S (+0.4S) | SIŁA ODRZUTU: {curKb:0.0} -> {nextKb:0.0}";
            }

            if (key.IndexOf("Sztylet", StringComparison.OrdinalIgnoreCase) >= 0 || key.IndexOf("Dagger", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                float curBleedDur = 3.0f + (currentLevel * 0.5f);
                float nextBleedDur = 3.0f + (nextLevel * 0.5f);
                int curChance = 5 + (currentLevel * 3);
                int nextChance = 5 + (nextLevel * 3);
                return $"KRWAWIENIE: {curBleedDur:0.0}S -> {nextBleedDur:0.0}S (+0.5S) | SZANSA: {curChance}% -> {nextChance}%";
            }

            if (IsCrossbow(item))
            {
                int curSpeed = 42 + (currentLevel * 3);
                int nextSpeed = 42 + (nextLevel * 3);
                return $"PRĘDKOŚĆ BEŁTU: {curSpeed} M/S -> {nextSpeed} M/S (+3 M/S)";
            }

            return string.Empty;
        }

        /// <summary>
        /// Zwraca opis aktualnych bonusów unikalnej mechaniki broni na poziomie maksymalnym.
        /// </summary>
        public string GetWeaponSpecialCurrentText(Item item, int currentLevel)
        {
            if (item == null) return string.Empty;
            string key = GetWeaponKey(item);

            if (key.IndexOf("Sierp", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                int curRupture = 14 + (currentLevel * 2);
                float curHit = 1.5f + (currentLevel * 0.5f);
                return $"WYSSANIE (RUPTURE): {curRupture}% MAX HP | CIĘCIE: {curHit:0.0}% HP (MAKSYMALNA MOC)";
            }

            if (key.IndexOf("Zatrut", StringComparison.OrdinalIgnoreCase) >= 0 || key.IndexOf("Poison", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                int curChance = 35 + (currentLevel * 5);
                int curDmg = 3 + currentLevel;
                return $"SZANSA NA TRUCIZNĘ: {curChance}% | DMG TRUCIZNY: {curDmg}/S (MAKSYMALNA MOC)";
            }

            if (key.IndexOf("Młot", StringComparison.OrdinalIgnoreCase) >= 0 || key.IndexOf("Mlot", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                float curCost = Mathf.Max(2f, 7.0f - (currentLevel * 0.8f));
                int curReq = Mathf.Max(50, 90 - (currentLevel * 6));
                return $"KOSZT STAMINY (LMP): {curCost:0.0} | MIN. STAMINA PPM: {curReq}% (MAKSYMALNA MOC)";
            }

            if (key.IndexOf("Włócz", StringComparison.OrdinalIgnoreCase) >= 0 || key.IndexOf("Wlocz", StringComparison.OrdinalIgnoreCase) >= 0 || key.IndexOf("Spear", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                float curDur = 3.5f + (currentLevel * 0.4f);
                float curKb = 10f + (currentLevel * 1.5f);
                return $"CZAS SZARŻY: {curDur:0.0}S | SIŁA ODRZUTU: {curKb:0.0} (MAKSYMALNA MOC)";
            }

            if (key.IndexOf("Sztylet", StringComparison.OrdinalIgnoreCase) >= 0 || key.IndexOf("Dagger", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                float curBleedDur = 3.0f + (currentLevel * 0.5f);
                int curChance = 5 + (currentLevel * 3);
                return $"KRWAWIENIE: {curBleedDur:0.0}S | SZANSA: {curChance}% | KRYTYK: +{currentLevel * 2}% (MAKSYMALNA MOC)";
            }

            if (IsCrossbow(item))
            {
                int curSpeed = 42 + (currentLevel * 3);
                return $"PRĘDKOŚĆ BEŁTU: {curSpeed} M/S (MAKSYMALNA MOC)";
            }

            return string.Empty;
        }
    }
}
