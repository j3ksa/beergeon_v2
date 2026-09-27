using UnityEngine;

namespace Praktyki.Kusza
{
    [CreateAssetMenu(fileName = "Kusza", menuName = "Inventory/Kusza/Kusza")]
    public class CrossbowItem : EquippableItem
    {
        [Header("Grafiki Kuszy")]
        [Tooltip("Grafika pustej kuszy")]
        public Sprite emptyCrossbowSprite;
        [Tooltip("Grafika naładowanej kuszy")]
        public Sprite loadedCrossbowSprite;

        [Header("Amunicja")]
        [Tooltip("Przedmiot strzały wymagany do załadowania")]
        public Item arrowItem;

        [Header("Pocisk 3D")]
        [Tooltip("Prefab wystrzeliwanej strzały 3D")]
        public GameObject arrowProjectilePrefab;
        public float projectileSpeed = 42f;
        public int weaponDamage = 20;

        [Header("Audio")]
        public AudioClip reloadSound;
        public AudioClip shootSound;
        public AudioClip emptyClickSound;

        [Header("Stan Naładowania")]
        public bool isLoaded = false;

        private float lastActionTime = 0f;

        private void OnEnable()
        {
            isLoaded = false;
            lastActionTime = -999f;
            UpdateVisualProperties();
        }

        public void UpdateVisualProperties()
        {
            Sprite targetSprite = isLoaded ? loadedCrossbowSprite : emptyCrossbowSprite;
            if (targetSprite != null)
            {
                heldSprite = targetSprite;
                icon = targetSprite;
            }
        }

        public override void AltUse(PlayerController player, PlayerHand hand)
        {
            HandleAction(player, hand);
        }

        public override void Use(PlayerController player, PlayerHand hand)
        {
            HandleAction(player, hand);
        }

        private void HandleAction(PlayerController player, PlayerHand hand)
        {
            // Bezpiecznik: jeśli Time.time zostało zresetowane (restart gry/sceny w edytorze)
            if (lastActionTime > Time.time)
            {
                lastActionTime = -999f;
            }

            if (Time.time < lastActionTime + cooldown)
            {
                Debug.Log($"<color=#FFFF00>[Kusza] Cooldown trwa jeszcze {(lastActionTime + cooldown - Time.time):F2}s</color>");
                return;
            }

            Debug.Log($"<color=#00FFFF>[Kusza] Wykonano akcję (isLoaded: {isLoaded})</color>");

            if (isLoaded)
            {
                Shoot(player, hand);
            }
            else
            {
                TryReload(player, hand);
            }
        }

        private bool IsArrow(Item it)
        {
            if (it == null) return false;
            if (it is ArrowItem) return true;
            string n = it.name.ToLower();
            string iname = it.itemName.ToLower();
            return n.Contains("strza") || iname.Contains("strza") || n.Contains("arrow") || iname.Contains("arrow");
        }

        public bool TryReload(PlayerController player, PlayerHand hand)
        {
            if (isLoaded)
            {
                Debug.Log("<color=#FFA500>[Kusza] Kusza jest już załadowana!</color>");
                return false;
            }

            Inventory inv = Inventory.instance;
            if (inv == null)
            {
                Debug.LogError("[Kusza] Błąd: Brak Inventory.instance w scenie!");
                return false;
            }

            Debug.Log($"<color=#FFFF00>[Kusza] Rozpoczynam ładowanie kuszy... Ekwipunek zawiera {inv.items.Count} slotów.</color>");

            // Znajdź strzałę w ekwipunku
            Item arrowToConsume = null;
            for (int i = 0; i < inv.items.Count; i++)
            {
                Item currentIt = inv.items[i];
                if (currentIt != null)
                {
                    Debug.Log($"[Kusza] Slot {i}: '{currentIt.itemName}' (obiekt: '{currentIt.name}') -> Czy strzała: {IsArrow(currentIt)}");
                }
                if (IsArrow(currentIt))
                {
                    arrowToConsume = currentIt;
                    break;
                }
            }
            if (arrowToConsume == null && arrowItem != null && inv.HasItem(arrowItem, 1))
            {
                arrowToConsume = arrowItem;
            }

            if (arrowToConsume != null)
            {
                inv.ConsumeItem(arrowToConsume, 1);
                isLoaded = true;
                lastActionTime = Time.time;
                UpdateVisualProperties();

                if (hand != null)
                {
                    hand.UpdateHandVisuals();
                }
                inv.OnInventoryChanged?.Invoke();

                if (reloadSound != null)
                {
                    Vector3 soundPos = player != null ? player.transform.position : Camera.main.transform.position;
                    AudioSource.PlayClipAtPoint(reloadSound, soundPos);
                }

                Debug.Log("<color=#55FF55><b>[Kusza]</b> Pomyślnie załadowano strzałę do kuszy!</color>");
                return true;
            }
            else
            {
                lastActionTime = Time.time;
                if (emptyClickSound != null)
                {
                    Vector3 soundPos = player != null ? player.transform.position : Camera.main.transform.position;
                    AudioSource.PlayClipAtPoint(emptyClickSound, soundPos);
                }

                Debug.LogWarning("<color=#FF5555><b>[Kusza]</b> Brak strzał w ekwipunku do załadowania!</color>");
                return false;
            }
        }

        public void Shoot(PlayerController player, PlayerHand hand)
        {
            if (!isLoaded) return;

            isLoaded = false;
            lastActionTime = Time.time;
            UpdateVisualProperties();

            if (hand != null)
            {
                hand.UpdateHandVisuals();
            }
            if (Inventory.instance != null)
            {
                Inventory.instance.OnInventoryChanged?.Invoke();
            }

            // Znajdź pozycję wystrzału z kamery gracza
            Camera cam = Camera.main;
            if (player != null)
            {
                Camera playerCam = player.GetComponentInChildren<Camera>();
                if (playerCam != null) cam = playerCam;
            }

            if (cam != null && arrowProjectilePrefab != null)
            {
                Vector3 spawnPos = cam.transform.position + cam.transform.forward * 0.6f;
                Quaternion spawnRot = Quaternion.LookRotation(cam.transform.forward);

                GameObject arrowObj = Instantiate(arrowProjectilePrefab, spawnPos, spawnRot);
                ArrowProjectile projectile = arrowObj.GetComponent<ArrowProjectile>();
                if (projectile != null)
                {
                    // Oblicz obrażenia uwzględniając bonusy ulepszeń i siły gracza
                    int bonusDamage = 0;
                    if (Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance != null)
                    {
                        bonusDamage = Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance.GetBonusDamage(this);
                    }
                    int baseCrossbowDmg = weaponDamage + bonusDamage;
                    int totalDamage = baseCrossbowDmg;
                    if (player != null)
                    {
                        PlayerStatsUpgrade statsUpgrade = player.GetComponent<PlayerStatsUpgrade>() ?? Object.FindFirstObjectByType<PlayerStatsUpgrade>();
                        PlayerStats pStats = player.GetComponent<PlayerStats>();
                        if (statsUpgrade != null)
                        {
                            totalDamage = statsUpgrade.CalculateFinalDamage(baseCrossbowDmg);
                        }
                        else if (pStats != null)
                        {
                            totalDamage = pStats.GetDamage(baseCrossbowDmg);
                        }
                    }

                    // Bonus Mistrzowski na poziomie +6 (+15% krytyk)
                    int upgradeLvl = 0;
                    if (Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance != null)
                    {
                        upgradeLvl = Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance.GetUpgradeLevel(this);
                        if (Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance.HasMasterworkBonus(this))
                        {
                            if (Random.value < Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance.GetMasterworkCritBonus(this))
                            {
                                totalDamage = Mathf.RoundToInt(totalDamage * 1.5f);
                                Debug.Log($"<color=#FFD700>[Kusza] MISTRZOWSKI KRYTYK STRZAŁY (+6)! Zwiększono obrażenia do {totalDamage}</color>");
                            }
                        }
                    }

                    float effectiveSpeed = projectileSpeed + (upgradeLvl * 3f);
                    projectile.Initialize(cam.transform.forward, effectiveSpeed, totalDamage, player != null ? player.gameObject : null);
                }
            }

            if (shootSound != null)
            {
                Vector3 soundPos = player != null ? player.transform.position : (cam != null ? cam.transform.position : Vector3.zero);
                AudioSource.PlayClipAtPoint(shootSound, soundPos);
            }

            Debug.Log("<color=#FFA500><b>[Kusza]</b> Wystrzelono strzałę!</color>");
        }
    }
}
