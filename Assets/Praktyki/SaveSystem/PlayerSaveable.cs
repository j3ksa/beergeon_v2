using UnityEngine;

namespace SaveSystem
{
    [RequireComponent(typeof(SaveableEntity))]
    public class PlayerSaveable : MonoBehaviour
    {
        private PlayerStats playerStats;

        private void Awake()
        {
            FindPlayerStats();
        }

        private void FindPlayerStats()
        {
            if (playerStats == null) playerStats = GetComponent<PlayerStats>();
            if (playerStats == null) playerStats = GetComponentInChildren<PlayerStats>();
            if (playerStats == null) playerStats = FindFirstObjectByType<PlayerStats>();
        }

        public PlayerSaveData SaveState()
        {
            FindPlayerStats();
            PlayerSaveData data = new PlayerSaveData();
            data.position = new Vector3Serializable(transform.position);
            data.rotation = new Vector3Serializable(transform.eulerAngles);

            if (playerStats != null)
            {
                data.level = playerStats.Level;
                data.experience = playerStats.Experience;
                data.requiredExperience = playerStats.GetXpForNextLevel();
                data.strength = playerStats.Strength;
                data.vitality = playerStats.Vitality;
                Debug.Log($"[PlayerSaveable] Zapisano gracza: Lvl {data.level}, XP {data.experience}/{data.requiredExperience}");
            }
            else
            {
                Debug.LogWarning("[PlayerSaveable] Nie znaleziono PlayerStats podczas zapisu!");
            }

            PlayerHealth playerHealth = GetComponent<PlayerHealth>();
            if (playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                data.currentHealth = playerHealth.CurrentHealth;
            }

            Inventory inventory = GetComponent<Inventory>();
            if (inventory == null) inventory = FindFirstObjectByType<Inventory>();
            if (inventory != null)
            {
                data.activeSlotIndex = inventory.activeSlotIndex;
                data.inventoryItems.Clear();
                foreach (var item in inventory.items)
                {
                    if (item != null) data.inventoryItems.Add(item.name);
                }
            }

            CurrencySystem.PlayerCurrency currency = GetComponent<CurrencySystem.PlayerCurrency>();
            if (currency == null) currency = CurrencySystem.PlayerCurrency.Instance ?? FindFirstObjectByType<CurrencySystem.PlayerCurrency>();
            if (currency != null)
            {
                data.capsCount = currency.CurrentCaps;
            }

            PlayerStatsUpgrade statsUpgrade = GetComponent<PlayerStatsUpgrade>();
            if (statsUpgrade == null) statsUpgrade = FindFirstObjectByType<PlayerStatsUpgrade>();
            if (statsUpgrade != null)
            {
                data.availableStatPoints = statsUpgrade.AvailablePoints;
                data.healthUpgrades = statsUpgrade.HealthUpgrades;
                data.staminaUpgrades = statsUpgrade.StaminaUpgrades;
                data.staminaRegenUpgrades = statsUpgrade.StaminaRegenUpgrades;
                data.speedUpgrades = statsUpgrade.SpeedUpgrades;
                data.strengthUpgrades = statsUpgrade.StrengthUpgrades;
                data.critChanceUpgrades = statsUpgrade.CritChanceUpgrades;
            }

            if (Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance != null)
            {
                data.weaponUpgrades = Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance.GetSaveData();
            }

            return data;
        }

        public void LoadState(PlayerSaveData data)
        {
            if (data == null) return;
            FindPlayerStats();

            // Ustawienie pozycji gracza (CharacterController wyłączony na czas teleportacji)
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            transform.position = data.position.ToVector3();
            transform.eulerAngles = data.rotation.ToVector3();

            if (cc != null) cc.enabled = true;

            if (playerStats != null)
            {
                playerStats.SetStats(data.level, data.experience, data.strength, data.vitality);
            }

            PlayerStatsUpgrade statsUpgrade = GetComponent<PlayerStatsUpgrade>();
            if (statsUpgrade == null) statsUpgrade = FindFirstObjectByType<PlayerStatsUpgrade>();
            if (statsUpgrade != null)
            {
                statsUpgrade.LoadUpgradeState(
                    data.availableStatPoints,
                    data.healthUpgrades,
                    data.staminaUpgrades,
                    data.staminaRegenUpgrades,
                    data.speedUpgrades,
                    data.strengthUpgrades,
                    data.critChanceUpgrades
                );
            }

            PlayerHealth pHealth = GetComponent<PlayerHealth>();
            if (pHealth == null) pHealth = FindFirstObjectByType<PlayerHealth>();
            if (pHealth != null && data.currentHealth > 0)
            {
                pHealth.SetCurrentHealth(data.currentHealth);
            }

            Inventory inventory = GetComponent<Inventory>();
            if (inventory == null) inventory = FindFirstObjectByType<Inventory>();
            if (inventory != null)
            {
                inventory.items.Clear();
                foreach (string itemName in data.inventoryItems)
                {
                    Item item = Resources.Load<Item>($"Items/{itemName}");
                    if (item != null)
                    {
                        inventory.items.Add(item);
                    }
                    else
                    {
                        Debug.LogWarning($"Nie znaleziono przedmiotu w Resources/Items: {itemName}");
                    }
                }
                inventory.activeSlotIndex = Mathf.Clamp(data.activeSlotIndex, 0, Mathf.Max(0, inventory.items.Count - 1));
                inventory.EnsureItemCountsValid();
                inventory.OnInventoryChanged?.Invoke();
                inventory.OnActiveSlotChanged?.Invoke();

                PlayerHand playerHand = GetComponentInChildren<PlayerHand>() ?? FindFirstObjectByType<PlayerHand>();
                if (playerHand != null)
                {
                    playerHand.UpdateHandVisuals();
                }
            }

            CurrencySystem.PlayerCurrency currency = GetComponent<CurrencySystem.PlayerCurrency>();
            if (currency == null) currency = CurrencySystem.PlayerCurrency.Instance ?? FindFirstObjectByType<CurrencySystem.PlayerCurrency>();
            if (currency != null)
            {
                currency.SetCaps(data.capsCount);
            }

            if (Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance != null && data.weaponUpgrades != null)
            {
                Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance.LoadSaveData(data.weaponUpgrades);
            }

            Debug.Log($"[PlayerSaveable] Przywrócono pozycję gracza: {transform.position}, Poziom: {data.level}, XP: {data.experience}, Kapsle: {data.capsCount}");
        }
    }
}
