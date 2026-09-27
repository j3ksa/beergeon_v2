using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveSystem
{
    public class SaveManager : MonoBehaviour
    {
        private static SaveManager _instance;
        public static SaveManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<SaveManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("SaveManager_Auto");
                        _instance = go.AddComponent<SaveManager>();
                        Debug.Log("[SaveManager] Automatycznie utworzono obiekt SaveManager.");
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SaveGame(int slotId = 1)
        {
            SaveData saveData = new SaveData();
            saveData.sceneName = SceneManager.GetActiveScene().name;

            // Zapis Gracza
            PlayerSaveable player = FindFirstObjectByType<PlayerSaveable>();
            if (player != null)
            {
                saveData.playerData = player.SaveState();
            }
            else
            {
                // Fallback: Jeśli brakuje skryptu PlayerSaveable na obiekcie gracza
                PlayerStats pStats = FindFirstObjectByType<PlayerStats>();
                if (pStats != null)
                {
                    saveData.playerData.level = pStats.Level;
                    saveData.playerData.experience = pStats.Experience;
                    saveData.playerData.requiredExperience = pStats.GetXpForNextLevel();
                    saveData.playerData.strength = pStats.Strength;
                    saveData.playerData.vitality = pStats.Vitality;
                    saveData.playerData.position = new Vector3Serializable(pStats.transform.position);
                    saveData.playerData.rotation = new Vector3Serializable(pStats.transform.eulerAngles);

                    PlayerHealth pHealth = FindFirstObjectByType<PlayerHealth>();
                    if (pHealth != null) saveData.playerData.currentHealth = pHealth.CurrentHealth;

                    Inventory inv = FindFirstObjectByType<Inventory>();
                    if (inv != null)
                    {
                        saveData.playerData.inventoryItems.Clear();
                        foreach (var item in inv.items)
                        {
                            if (item != null) saveData.playerData.inventoryItems.Add(item.name);
                        }
                    }

                    CurrencySystem.PlayerCurrency pCurrency = FindFirstObjectByType<CurrencySystem.PlayerCurrency>();
                    if (pCurrency != null) saveData.playerData.capsCount = pCurrency.CurrentCaps;

                    if (Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance != null)
                    {
                        saveData.playerData.weaponUpgrades = Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance.GetSaveData();
                    }

                    Debug.Log($"[SaveManager Fallback] Zapisano PlayerStats bezpośrednio: Lvl {pStats.Level}, XP {pStats.Experience}, Caps {saveData.playerData.capsCount}");
                }
            }

            // Zapis Przeciwników
            EnemySaveable[] enemies = FindObjectsByType<EnemySaveable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (EnemySaveable enemy in enemies)
            {
                saveData.enemiesData.Add(enemy.SaveState());
            }

            SaveSystem.Save(slotId, saveData);
        }

        private SaveData pendingSaveData;

        public void LoadGame(int slotId = 1)
        {
            SaveData saveData = SaveSystem.Load(slotId);
            if (saveData == null)
            {
                Debug.LogWarning($"[SaveManager] Nie znaleziono zapisu w slocie {slotId}!");
                return;
            }

            pendingSaveData = saveData;
            string currentScene = SceneManager.GetActiveScene().name;

            if (!string.IsNullOrEmpty(saveData.sceneName) && saveData.sceneName != currentScene)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneManager.LoadSceneAsync(saveData.sceneName);
            }
            else
            {
                ApplySaveData(pendingSaveData);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (pendingSaveData != null)
            {
                ApplySaveData(pendingSaveData);
            }
        }

        private void ApplySaveData(SaveData saveData)
        {
            // Odmrożenie gry po odczycie z pauzy
            Time.timeScale = 1f;
            AudioListener.pause = false;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            PauseMenu pauseMenu = FindFirstObjectByType<PauseMenu>();
            if (pauseMenu != null)
            {
                pauseMenu.Resume();
            }

            // Odtworzenie stanu Gracza
            PlayerSaveable player = FindFirstObjectByType<PlayerSaveable>();
            if (player != null)
            {
                player.LoadState(saveData.playerData);
            }
            else
            {
                PlayerStats pStats = FindFirstObjectByType<PlayerStats>();
                if (pStats != null)
                {
                    pStats.SetStats(saveData.playerData.level, saveData.playerData.experience, saveData.playerData.strength, saveData.playerData.vitality);
                    CharacterController cc = pStats.GetComponent<CharacterController>();
                    if (cc != null) cc.enabled = false;
                    pStats.transform.position = saveData.playerData.position.ToVector3();
                    pStats.transform.eulerAngles = saveData.playerData.rotation.ToVector3();
                    if (cc != null) cc.enabled = true;

                    PlayerHealth pHealth = FindFirstObjectByType<PlayerHealth>();
                    if (pHealth != null && saveData.playerData.currentHealth > 0)
                    {
                        pHealth.SetCurrentHealth(saveData.playerData.currentHealth);
                    }

                    Inventory inv = FindFirstObjectByType<Inventory>();
                    if (inv != null)
                    {
                        inv.items.Clear();
                        foreach (string itemName in saveData.playerData.inventoryItems)
                        {
                            Item item = Resources.Load<Item>($"Items/{itemName}");
                            if (item != null)
                            {
                                inv.items.Add(item);
                            }
                        }
                        inv.activeSlotIndex = Mathf.Clamp(saveData.playerData.activeSlotIndex, 0, Mathf.Max(0, inv.items.Count - 1));
                        inv.EnsureItemCountsValid();
                        inv.OnInventoryChanged?.Invoke();
                        inv.OnActiveSlotChanged?.Invoke();

                        PlayerHand hand = FindFirstObjectByType<PlayerHand>();
                        if (hand != null)
                        {
                            hand.UpdateHandVisuals();
                        }
                    }

                    CurrencySystem.PlayerCurrency pCurrency = FindFirstObjectByType<CurrencySystem.PlayerCurrency>();
                    if (pCurrency != null) pCurrency.SetCaps(saveData.playerData.capsCount);

                    if (Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance != null && saveData.playerData.weaponUpgrades != null)
                    {
                        Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance.LoadSaveData(saveData.playerData.weaponUpgrades);
                    }

                    Debug.Log($"[SaveManager Fallback] Przywrócono PlayerStats bezpośrednio: Lvl {saveData.playerData.level}, XP {saveData.playerData.experience}, Caps {saveData.playerData.capsCount}");
                }
            }

            // Odtworzenie stanu Przeciwników
            Dictionary<string, EnemySaveData> enemyDataDict = new Dictionary<string, EnemySaveData>();
            foreach (var eData in saveData.enemiesData)
            {
                if (!string.IsNullOrEmpty(eData.saveID))
                {
                    enemyDataDict[eData.saveID] = eData;
                }
            }

            EnemySaveable[] enemies = FindObjectsByType<EnemySaveable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (EnemySaveable enemy in enemies)
            {
                if (enemyDataDict.TryGetValue(enemy.SaveID, out EnemySaveData eData))
                {
                    enemy.LoadState(eData);
                }
            }

            Debug.Log("[SaveManager] Stan gry wczytany pomyślnie.");
        }
    }
}
