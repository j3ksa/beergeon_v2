using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Praktyki.KowadloUlepszen;
using UnityEngine.InputSystem;

namespace Praktyki.Wlocznia
{
    [CreateAssetMenu(fileName = "Wlocznia", menuName = "Inventory/Weapons/Wlocznia")]
    public class WloczniaItem : WeaponItem
    {
        [Header("Zasięg i Strefa Precyzji (Sweet Spot)")]
        [Tooltip("Maksymalny zasięg pchnięcia włócznią (w metrach)")]
        public float spearMaxRange = 3.2f;

        [Tooltip("Minimalny dystans od gracza, od którego liczy się Sweet Spot (ostrze włóczni)")]
        public float sweetSpotMinDistance = 2.2f;

        [Tooltip("Mnożnik obrażeń gdy trafimy przeciwnika blisko (trzonkiem) - np. 0.5 = 50%")]
        [Range(0.1f, 1f)]
        public float closeRangeMultiplier = 0.5f;

        [Tooltip("Mnożnik obrażeń gdy trafimy przeciwnika w sweet spot (ostrzem) - np. 1.25 = +25%")]
        [Range(1f, 2f)]
        public float sweetSpotMultiplier = 1.25f;

        [Header("Szarża (PPM - Charge)")]
        [Tooltip("Prędkość ruchu gracza podczas szarży")]
        public float chargeSpeed = 9.5f;

        [Tooltip("Maksymalny czas trwania szarży w sekundach")]
        public float maxChargeDuration = 3.5f;

        [Tooltip("Koszt staminy na sam start szarży")]
        public float chargeInitialStaminaCost = 6f;

        [Tooltip("Zużycie staminy na sekundę podczas biegu szarży")]
        public float chargeDrainPerSecond = 10f;

        [Tooltip("Mnożnik bazowych obrażeń dla pierwszego trafionego wroga")]
        public float firstHitDamageMultiplier = 1.6f;

        [Tooltip("Mnożnik obrażeń dla drugiego wroga w linii szarży")]
        public float secondHitDamageMultiplier = 0.8f;

        [Tooltip("Siła odrzutu wrogów potrąconych na bok w trakcie szarży")]
        public float chargeKnockbackForce = 10f;

        [Tooltip("Mnożnik dodatkowych obrażeń zadawanych nabitemu wrogowi przy wbiciu go w ścianę")]
        public float pinnedEnemyWallSlamMultiplier = 2.0f;

        [Tooltip("Obrażenia własne gracza przy uderzeniu w ścianę/przeszkodę")]
        public int wallCrashSelfDamage = 5;

        [Tooltip("Utrata staminy przy utracie równowagi na ścianie")]
        public float wallCrashStaminaLoss = 15f;

        [Header("Pasywne Spowolnienie")]
        [Tooltip("Modyfikator prędkości gracza gdy włócznia jest wyciągnięta (0.88 = -12% prędkości)")]
        [Range(0.5f, 1f)]
        public float equippedSpeedModifier = 0.88f;

        [Header("Trafienie Krytyczne")]
        [Tooltip("Bazowa szansa na trafienie krytyczne")]
        [Range(0f, 1f)]
        public float critChance = 0.08f;

        [Tooltip("Mnożnik obrażeń krytycznych")]
        public float critMultiplier = 1.6f;

        [Header("Animacja Dłoni")]
        [Tooltip("Wektor wysunięcia włóczni do przodu podczas pchnięcia")]
        public Vector3 thrustVector = new Vector3(0f, 0.15f, 0.6f);

        [Tooltip("Szybkość wysunięcia pchnięcia")]
        public float thrustSpeed = 12f;

        // Stan szarży w czasie działania
        private static Coroutine activeChargeCoroutine = null;
        private static bool isCharging = false;
        private static PlayerHand chargingHand = null;

        public static bool IsCharging => isCharging;

        private void Reset()
        {
            itemName = "Włócznia";
            damage = 22; // solidny bazowy damage (z sweet spotem osiąga ~27-28, przy bliskim trafieniu ~11)
            range = 3.2f;
            spearMaxRange = 3.2f;
            sweetSpotMinDistance = 2.2f;
            closeRangeMultiplier = 0.5f;
            sweetSpotMultiplier = 1.25f;
            cooldown = 0.55f; // wolniejsza niż sztylet, wymaga wyczucia rytmu
            useItemRotation = true;
            rotationEuler = new Vector3(20, -25, -10);
            rotationSpeed = 8f;
            returnRotationSpeedMultiplier = 1.2f;
            handPositionOffset = new Vector3(0.06f, -0.05f, 0.25f);
            handScaleMultiplier = 1.15f;
            chargeSpeed = 9.5f;
            description = "Długa broń drzewcowa. Przeszywa wrogów w linii. Posiada Strefę Precyzji: uderzenie samym ostrzem (3.2m - 4.8m) zadaje +25% obrażeń, ale uderzenie blisko trzonkiem tylko 50%. PPM aktywuje szarżę (ponowny PPM anuluje, uderzenie w ścianę rani i zbija staminę). Nieco spowalnia ruch (-12%).";
        }

        public override void OnEquipped(PlayerController player, PlayerHand hand)
        {
            base.OnEquipped(player, hand);

            if (player != null)
            {
                player.WeaponSpeedModifier = equippedSpeedModifier;
                Debug.Log($"<color=#32CD32>[Włócznia]</color> Dobyto włóczni: redukcja prędkości ruchu o {Mathf.RoundToInt((1f - equippedSpeedModifier) * 100)}%.");
            }
        }

        public override void OnUnequipped(PlayerController player, PlayerHand hand)
        {
            base.OnUnequipped(player, hand);

            // Anulowanie szarży w razie schowania broni w trakcie
            if (isCharging && player != null)
            {
                StopActiveCharge(player, hand);
            }

            if (player != null)
            {
                player.WeaponSpeedModifier = 1f;
                Debug.Log("<color=#32CD32>[Włócznia]</color> Schowano włócznię: przywrócono standardową prędkość ruchu.");
            }
        }

        public override void Use(PlayerController player, PlayerHand hand)
        {
            if (isCharging)
            {
                // Jeśli gracz atakuje podczas szarży, ignorujemy
                return;
            }

            if (hand == null || player == null) return;

            // Wykonujemy animację pchnięcia w przód zamiast machnięcia
            hand.StartCoroutine(hand.SpearThrustRoutine(thrustVector, thrustSpeed, () =>
            {
                PerformPiercingThrust(player, hand);
            }));
        }

        public override void AltUse(PlayerController player, PlayerHand hand)
        {
            if (player == null) return;

            // 1. Jeśli trwa już szarża, powtórny klik PPM anuluje szarżę
            if (isCharging)
            {
                StopActiveCharge(player, hand);
                Debug.Log("<color=#00FFFF>[Włócznia]</color> Anulowano szarżę na żądanie gracza (PPM).");
                return;
            }

            // 2. Sprawdź staminę do rozpoczęcia szarży
            PlayerStamina stamina = player.GetComponent<PlayerStamina>();
            if (stamina != null && stamina.CurrentStamina < chargeInitialStaminaCost)
            {
                Debug.LogWarning("<color=#FFA500>[Włócznia]</color> Zbyt mało staminy na start szarży!");
                return;
            }

            if (stamina != null)
            {
                stamina.DrainStamina(chargeInitialStaminaCost);
            }

            chargingHand = hand;

            // Zablokowanie ruchu kamery w pionie (patrzenie prosto w przód) oraz ociężałe skręcanie (20% czułości)
            Camera cam = Camera.main ?? player.GetComponentInChildren<Camera>();
            CameraController camCtrl = cam != null ? cam.GetComponent<CameraController>() : Object.FindFirstObjectByType<CameraController>();
            if (camCtrl != null)
            {
                camCtrl.SetChargeLookState(true, 0.20f);
            }

            // Wysunięcie włóczni do przodu w pozycję bojową
            if (hand != null)
            {
                hand.StartCoroutine(hand.SpearChargeStanceRoutine(thrustVector, thrustSpeed * 1.5f));
            }

            // Rozpocznij szarżę
            activeChargeCoroutine = player.StartCoroutine(ChargeRoutine(player, hand));
        }

        private void StopActiveCharge(PlayerController player, PlayerHand hand = null)
        {
            if (activeChargeCoroutine != null && player != null)
            {
                player.StopCoroutine(activeChargeCoroutine);
                activeChargeCoroutine = null;
            }
            isCharging = false;

            // Przywrócenie pełnej kontroli nad kamerą
            CameraController camCtrl = Object.FindFirstObjectByType<CameraController>();
            if (camCtrl != null)
            {
                camCtrl.SetChargeLookState(false);
            }

            PlayerHand targetHand = hand != null ? hand : chargingHand;
            if (targetHand != null)
            {
                targetHand.StartCoroutine(targetHand.SpearReleaseChargeRoutine(thrustVector, thrustSpeed * 1.2f));
            }
            chargingHand = null;
        }

        /// <summary>
        /// LPM - Pchnięcie przeszywające w linii z kalkulacją Strefy Precyzji (Sweet Spot)
        /// </summary>
        private void PerformPiercingThrust(PlayerController player, PlayerHand hand)
        {
            Camera cam = Camera.main;
            if (cam == null) cam = player.GetComponentInChildren<Camera>();
            if (cam == null) return;

            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            RaycastHit[] hits = Physics.RaycastAll(ray, spearMaxRange);

            // Sortujemy trafienia rosnąco według odległości od kamery
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            // Zestaw trafionych IDamageable, aby uniknąć wielokrotnego zranienia jednego wroga z wieloma colliderami
            HashSet<IDamageable> damagedEntities = new HashSet<IDamageable>();

            int baseDmg = damage;
            if (WeaponUpgradeManager.Instance != null)
            {
                baseDmg += WeaponUpgradeManager.Instance.GetBonusDamage(this);
            }

            PlayerStatsUpgrade statsUpgrade = player.GetComponent<PlayerStatsUpgrade>() ?? Object.FindFirstObjectByType<PlayerStatsUpgrade>();
            PlayerStats stats = player.GetComponent<PlayerStats>() ?? Object.FindFirstObjectByType<PlayerStats>();

            bool hitAnyEnemy = false;

            foreach (RaycastHit hit in hits)
            {
                // Ignorujemy collider samego gracza
                if (hit.collider.gameObject == player.gameObject || hit.collider.transform.IsChildOf(player.transform))
                    continue;

                // Jeśli trafiliśmy w twarde otoczenie (np. ściana przed wrogami), pchnięcie zatrzymuje się
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Default") && !hit.collider.isTrigger)
                {
                    IDamageable wallCheck = hit.collider.GetComponentInParent<IDamageable>();
                    if (wallCheck == null)
                    {
                        // Trafiono litą ścianę/przeszkodę
                        break;
                    }
                }

                IDamageable damageable = hit.collider.GetComponent<IDamageable>() ?? hit.collider.GetComponentInParent<IDamageable>();
                if (damageable != null && !damagedEntities.Contains(damageable))
                {
                    damagedEntities.Add(damageable);
                    hitAnyEnemy = true;

                    // Obliczenie modyfikatora Sweet Spot na podstawie odległości
                    float dist = hit.distance;
                    float distMultiplier = 1f;
                    string hitZoneName = "Zwykła";
                    if (dist >= sweetSpotMinDistance)
                    {
                        distMultiplier = sweetSpotMultiplier; // +25%
                        hitZoneName = "<color=#39FF14>SWEET SPOT (Ostrze!)</color>";
                    }
                    else
                    {
                        distMultiplier = closeRangeMultiplier; // 50%
                        hitZoneName = "<color=#FFA07A>Bliski zasięg (Trzonek)</color>";
                    }

                    int scaledBase = Mathf.RoundToInt(baseDmg * distMultiplier);
                    int finalDmg = statsUpgrade != null ? statsUpgrade.CalculateFinalDamage(scaledBase) : (stats != null ? stats.GetDamage(scaledBase) : scaledBase);

                    // Szansa na trafienie krytyczne
                    float totalCrit = critChance;
                    if (WeaponUpgradeManager.Instance != null && WeaponUpgradeManager.Instance.HasMasterworkBonus(this))
                    {
                        totalCrit += WeaponUpgradeManager.Instance.GetMasterworkCritBonus(this);
                    }

                    if (Random.value <= totalCrit)
                    {
                        finalDmg = Mathf.RoundToInt(finalDmg * critMultiplier);
                        Debug.Log($"<color=#FFD700><b>[WŁÓCZNIA - KRYTYK!]</b></color> Trafiono: {hit.collider.name} | Dystans: <b>{dist:F2}m</b> | Strefa: {hitZoneName} | <b>Obrażenia: {finalDmg}</b>");
                    }
                    else
                    {
                        Debug.Log($"<color=#00FF7F>[WŁÓCZNIA - Pchnięcie]</color> Trafiono: {hit.collider.name} | Dystans: <b>{dist:F2}m</b> | Strefa: {hitZoneName} | <b>Obrażenia: {finalDmg}</b>");
                    }

                    damageable.TakeDamage(finalDmg);
                    OnHit(hit.collider.gameObject, damageable, hit);
                }
            }

            if (!hitAnyEnemy)
            {
                Debug.Log($"<color=#888888>[WŁÓCZNIA - Pchnięcie]</color> Machnięcie w powietrze (brak wrogów w zasięgu {spearMaxRange:F1}m).");
            }
        }

        /// <summary>
        /// PPM - Coroutine Szarży z włócznią
        /// </summary>
        private IEnumerator ChargeRoutine(PlayerController player, PlayerHand hand)
        {
            isCharging = true;
            CharacterController cc = player.GetComponent<CharacterController>();
            Camera cam = Camera.main ?? player.GetComponentInChildren<Camera>();

            float timer = 0f;
            HashSet<IDamageable> hitDuringCharge = new HashSet<IDamageable>();

            Debug.Log("<color=#00FFFF><b>[Włócznia] START SZARŻY!</b></color>");

            // Bazowe obrażenia do szarży i poziom ulepszenia kowadła
            int baseDmg = damage;
            int upgradeLvl = 0;
            if (WeaponUpgradeManager.Instance != null)
            {
                baseDmg += WeaponUpgradeManager.Instance.GetBonusDamage(this);
                upgradeLvl = WeaponUpgradeManager.Instance.GetUpgradeLevel(this);
            }

            float effectiveChargeDuration = maxChargeDuration + (upgradeLvl * 0.4f);
            float effectiveKnockback = chargeKnockbackForce + (upgradeLvl * 1.5f);
            float effectiveWallSlamMultiplier = pinnedEnemyWallSlamMultiplier + (upgradeLvl * 0.15f);

            PlayerStatsUpgrade statsUpgrade = player.GetComponent<PlayerStatsUpgrade>() ?? Object.FindFirstObjectByType<PlayerStatsUpgrade>();
            PlayerStats stats = player.GetComponent<PlayerStats>() ?? Object.FindFirstObjectByType<PlayerStats>();

            PlayerStamina stamina = player.GetComponent<PlayerStamina>();
            GameObject pinnedEnemyObj = null;
            IDamageable pinnedEnemyDamageable = null;

            while (timer < effectiveChargeDuration && isCharging)
            {
                timer += Time.deltaTime;

                // Ciągłe pobieranie staminy w czasie biegu
                if (stamina != null)
                {
                    bool hasStamina = stamina.DrainStamina(chargeDrainPerSecond * Time.deltaTime);
                    if (!hasStamina || stamina.CurrentStamina <= 0f)
                    {
                        Debug.Log("<color=#FFA500>[Włócznia] Wyczerpano staminę! Koniec szarży.</color>");
                        isCharging = false;
                        break;
                    }
                }

                Vector3 forwardDir = cam != null ? cam.transform.forward : player.transform.forward;
                forwardDir.y = 0f;
                forwardDir.Normalize();

                if (forwardDir.sqrMagnitude < 0.01f)
                {
                    forwardDir = player.transform.forward;
                    forwardDir.y = 0f;
                    forwardDir.Normalize();
                }

                Vector3 moveDelta = forwardDir * chargeSpeed * Time.deltaTime;

                // Ruch gracza za pomocą CharacterControllera
                if (cc != null && cc.enabled)
                {
                    cc.Move(moveDelta + Vector3.down * (5f * Time.deltaTime));
                }
                else
                {
                    player.transform.position += moveDelta;
                }

                // Jeśli mamy nabitego wroga, trzymamy go idealnie przed grotem włóczni i go ogłuszamy
                if (pinnedEnemyObj != null && pinnedEnemyObj.activeInHierarchy)
                {
                    EnemyController enemyCtrl = pinnedEnemyObj.GetComponent<EnemyController>() ?? pinnedEnemyObj.GetComponentInParent<EnemyController>();
                    if (enemyCtrl != null)
                    {
                        enemyCtrl.ApplyStun(0.3f);
                    }

                    // Obliczamy pozycję na ostrzu włóczni (~1.8m przed graczem)
                    Vector3 targetPinPos = player.transform.position + forwardDir * 1.8f;
                    CharacterController enemyCC = pinnedEnemyObj.GetComponent<CharacterController>() ?? pinnedEnemyObj.GetComponentInParent<CharacterController>();
                    
                    if (enemyCC != null && enemyCC.enabled && enemyCC.gameObject.activeInHierarchy)
                    {
                        Vector3 enemyDelta = targetPinPos - enemyCC.transform.position;
                        enemyDelta.y = -2f * Time.deltaTime; // docisk do podłoża
                        enemyCC.Move(enemyDelta);
                    }
                    else
                    {
                        pinnedEnemyObj.transform.position = new Vector3(targetPinPos.x, pinnedEnemyObj.transform.position.y, targetPinPos.z);
                    }
                }

                // Sprawdzanie kolizji w przód promieniem o grubości (SphereCast)
                Ray ray = new Ray(player.transform.position + Vector3.up * 1f, forwardDir);
                RaycastHit[] sphereHits = Physics.SphereCastAll(ray, 0.65f, 2.2f);

                bool hitWall = false;

                foreach (var hit in sphereHits)
                {
                    if (hit.collider.gameObject == player.gameObject || hit.collider.transform.IsChildOf(player.transform))
                        continue;

                    // Ignorujemy już nabitego wroga w bieżących trafieniach
                    if (pinnedEnemyObj != null && (hit.collider.gameObject == pinnedEnemyObj || hit.collider.transform.IsChildOf(pinnedEnemyObj.transform)))
                        continue;

                    IDamageable enemy = hit.collider.GetComponent<IDamageable>() ?? hit.collider.GetComponentInParent<IDamageable>();

                    if (enemy != null)
                    {
                        if (!hitDuringCharge.Contains(enemy))
                        {
                            hitDuringCharge.Add(enemy);

                            if (pinnedEnemyObj == null)
                            {
                                // 1. NABICIE WROGA NA OSTRZE (Pin)
                                pinnedEnemyObj = hit.collider.gameObject;
                                pinnedEnemyDamageable = enemy;

                                EnemyController enemyCtrl = pinnedEnemyObj.GetComponent<EnemyController>() ?? pinnedEnemyObj.GetComponentInParent<EnemyController>();
                                if (enemyCtrl != null)
                                {
                                    enemyCtrl.ApplyStun(effectiveChargeDuration);
                                }

                                int chargeDmg = Mathf.RoundToInt(baseDmg * firstHitDamageMultiplier);
                                int finalDmg = statsUpgrade != null ? statsUpgrade.CalculateFinalDamage(chargeDmg) : (stats != null ? stats.GetDamage(chargeDmg) : chargeDmg);
                                enemy.TakeDamage(finalDmg);

                                Debug.Log($"<color=#FF4500><b>[Włócznia - Szarża]</b> NABITO WROGA ({hit.collider.name}) NA OSTRZE!</color> Zadano {finalDmg} dmg i pchnięto cel przed sobą!");
                            }
                            else
                            {
                                // KOLEJNI PRZECIWNICY SĄ ODPYCHANI NA BOKI Z LEKKIMI OBRAŻENIAMI
                                int sideDmg = Mathf.RoundToInt(baseDmg * secondHitDamageMultiplier);
                                int finalSideDmg = statsUpgrade != null ? statsUpgrade.CalculateFinalDamage(sideDmg) : (stats != null ? stats.GetDamage(sideDmg) : sideDmg);
                                enemy.TakeDamage(finalSideDmg);

                                // Odrzut na bok (w lewo lub w prawo zależnie od pozycji)
                                Vector3 toEnemy = (hit.point - player.transform.position).normalized;
                                Vector3 sideDir = Vector3.Cross(Vector3.up, forwardDir).normalized;
                                if (Vector3.Dot(toEnemy, sideDir) < 0) sideDir = -sideDir;

                                Vector3 knockbackDir = (forwardDir * 0.4f + sideDir * 0.8f).normalized;
                                ApplyKnockback(hit.collider.gameObject, knockbackDir, effectiveKnockback);
                                Debug.Log($"<color=#FFA500><b>[Włócznia - Szarża]</b> Potrącono kolejnego wroga ({hit.collider.name}) na bok!</color> Zadano {finalSideDmg} dmg (Siła odrzutu: {effectiveKnockback:0.0}).");
                            }
                        }
                    }
                    else if (!hit.collider.isTrigger)
                    {
                        // Kolizja ze ścianą / przeszkodą otoczenia
                        hitWall = true;
                    }
                }

                if (hitWall)
                {
                    if (pinnedEnemyObj != null && pinnedEnemyDamageable != null && pinnedEnemyObj.activeInHierarchy)
                    {
                        // ZMIAŻDŻENIE NABITEGO WROGA O ŚCIANĘ (Potężny bonus!)
                        int wallSlamDmg = Mathf.RoundToInt(baseDmg * effectiveWallSlamMultiplier);
                        int finalSlamDmg = statsUpgrade != null ? statsUpgrade.CalculateFinalDamage(wallSlamDmg) : (stats != null ? stats.GetDamage(wallSlamDmg) : wallSlamDmg);
                        pinnedEnemyDamageable.TakeDamage(finalSlamDmg);

                        EnemyController enemyCtrl = pinnedEnemyObj.GetComponent<EnemyController>() ?? pinnedEnemyObj.GetComponentInParent<EnemyController>();
                        if (enemyCtrl != null)
                        {
                            enemyCtrl.ApplyStun(1.5f);
                        }

                        Debug.LogWarning($"<color=#FF0000><b>[WŁÓCZNIA - ROZTRZAŚNIĘCIE O ŚCIANĘ!]</b> Nabity wróg został zmiażdżony o mur za <b>{finalSlamDmg} DMG</b>!</color>");
                    }
                    else
                    {
                        // Gracz bez nabitego wroga uderzył sam w ścianę -> utrata równowagi, self-damage i utrata staminy
                        Debug.LogWarning("<color=#FF0000><b>[Włócznia] KRAKSA O ŚCIANĘ!</b> Gracz traci równowagę, staminę (-15) i otrzymuje 5 obrażeń!</color>");

                        PlayerHealth pHealth = player.GetComponent<PlayerHealth>();
                        if (pHealth != null)
                        {
                            pHealth.TakeDamage(wallCrashSelfDamage);
                        }

                        PlayerStamina pStamina = player.GetComponent<PlayerStamina>();
                        if (pStamina != null)
                        {
                            pStamina.DrainStamina(wallCrashStaminaLoss);
                        }
                    }

                    isCharging = false;
                    break;
                }

                yield return null;
            }

            // Jeśli szarża się skończyła z nabitym wrogiem (np. upłynął czas), odpychamy go delikatnie do przodu
            if (pinnedEnemyObj != null && pinnedEnemyObj.activeInHierarchy)
            {
                ApplyKnockback(pinnedEnemyObj, cam != null ? cam.transform.forward : player.transform.forward, 4f);
            }

            isCharging = false;
            activeChargeCoroutine = null;

            // Przywrócenie pełnej kontroli nad kamerą
            CameraController camController = Object.FindFirstObjectByType<CameraController>();
            if (camController != null)
            {
                camController.SetChargeLookState(false);
            }

            // Przywrócenie włóczni z pozycji szarży do normalnej pozycji spoczynkowej
            PlayerHand targetHand = hand != null ? hand : chargingHand;
            if (targetHand != null)
            {
                targetHand.StartCoroutine(targetHand.SpearReleaseChargeRoutine(thrustVector, thrustSpeed * 1.2f));
            }
            chargingHand = null;

            Debug.Log("<color=#00FFFF>[Włócznia] Koniec szarży.</color>");
        }

        private void ApplyKnockback(GameObject targetObj, Vector3 direction, float force)
        {
            if (targetObj == null || !targetObj.activeInHierarchy) return;

            Rigidbody rb = targetObj.GetComponent<Rigidbody>() ?? targetObj.GetComponentInParent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.AddForce(direction * force, ForceMode.Impulse);
            }
            else
            {
                // Fallback dla wrogów z CharacterController
                CharacterController targetCC = targetObj.GetComponent<CharacterController>() ?? targetObj.GetComponentInParent<CharacterController>();
                if (targetCC != null && targetCC.enabled && targetCC.gameObject.activeInHierarchy)
                {
                    targetCC.Move(direction * (force * 0.25f));
                }
            }
        }
    }
}
