using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Praktyki.MlotekDoKotletow
{
    [CreateAssetMenu(fileName = "MlotekDoKotletow", menuName = "Inventory/Weapons/Młotek Do Kotletów")]
    public class MlotekDoKotletowItem : WeaponItem
    {
        [Header("Statystyki Zwykłego Ataku (LMP)")]
        [Tooltip("Koszt staminy przy zwykłym ataku")]
        public float staminaCostLMB = 7.0f;

        [Tooltip("Szansa na lekki odrzut i ogłuszenie przy ataku LMP (np. 0.10 = 10%)")]
        [Range(0f, 1f)]
        public float lightKnockbackChance = 0.10f;

        [Tooltip("Siła lekkiego odrzutu (LMP)")]
        public float lightKnockbackForce = 3.5f;

        [Tooltip("Czas trwania lekkiego ogłuszenia (LMP w sekundach)")]
        public float lightStunDuration = 0.4f;

        [Tooltip("Próg staminy poniżej którego cios nie pobiera staminy, lecz zadaje zmniejszone obrażenia (0.10 = 10%)")]
        [Range(0f, 0.5f)]
        public float lowStaminaThreshold = 0.10f;

        [Tooltip("Mnożnik obrażeń gdy stamina jest poniżej progu 10% (0.20 = 20% obrażeń)")]
        [Range(0.05f, 1f)]
        public float lowStaminaDamageMultiplier = 0.20f;

        [Header("Statystyki Silnego Ciosu (PPM - Rozbijacz Kotletów)")]
        [Tooltip("Dodatkowe punkty obrażeń przy silnym ciosie (dodawane do bazowego damage broni)")]
        public int heavyDamageBonus = 15; // 45 + 15 = 60

        [Tooltip("Czas odnowienia po silnym ciosie (w sekundach)")]
        public float heavyCooldown = 2.4f;

        [Tooltip("Minimalny procent staminy wymagany do wykonania silnego ataku (0.90 = 90%)")]
        [Range(0.5f, 1f)]
        public float minStaminaPercentRMB = 0.90f;

        [Tooltip("Mnożnik prędkości chodu gracza po silnym ciosie (0.5 = 50% prędkości)")]
        public float playerSlowMultiplier = 0.5f;

        [Tooltip("Czas trwania spowolnienia gracza (w sekundach)")]
        public float playerSlowDuration = 2.2f;

        [Tooltip("Siła potężnego odrzutu (PPM)")]
        public float heavyKnockbackForce = 8.5f;

        [Tooltip("Czas trwania potężnego ogłuszenia (PPM w sekundach)")]
        public float heavyStunDuration = 1.2f;

        [Header("Animacja Silnego Ciosu")]
        public Vector3 heavySwingEuler = new Vector3(90f, -80f, -40f);
        public float heavySwingSpeed = 3.8f;

        [Header("Ładowanie Silnego Ciosu (PPM Charge)")]
        [Tooltip("Czas ładowania silnego ciosu w sekundach (trzymanie PPM)")]
        public float chargeTime = 0.65f;

        private Coroutine chargeRoutineInstance = null;
        private bool isChargingHeavy = false;

        public override void OnUnequipped(PlayerController player, PlayerHand hand)
        {
            base.OnUnequipped(player, hand);

            if (chargeRoutineInstance != null && hand != null)
            {
                hand.StopCoroutine(chargeRoutineInstance);
                chargeRoutineInstance = null;
            }

            isChargingHeavy = false;
            if (hand != null)
            {
                hand.IsAttacking = false;
            }
        }

        private void Reset()
        {
            itemName = "Młotek Do Kotletów";
            damage = 45; // 40 (zatruty miecz) + 5
            range = 3.5f;
            cooldown = 1.0f; // 1 sekunda cooldownu
            useItemRotation = true;
            rotationEuler = new Vector3(90, -80, -40);
            rotationSpeed = 8.0f;
            returnRotationSpeedMultiplier = 0.45f; // Wydłużony powrót młotka
            handPositionOffset = new Vector3(-0.05f, 0.40f, 0f);
            handScaleMultiplier = 1.15f;
            description = "Masywny kuchenny tłuczek do mięsa. Zadaje 45 pkt obrażeń i zużywa odrobinę staminy z 10% szansą na odrzut. Poniżej 10% staminy nie zużywa jej, lecz zadaje tylko 20% obrażeń. Potężny cios (PPM) wymaga pełnej staminy, zadaje 60 obrażeń, odrzuca i ogłusza wrogów kosztem chwilowego spowolnienia gracza.";
        }

        public override void Use(PlayerController player, PlayerHand hand)
        {
            PlayerStamina stamina = player != null ? player.GetComponent<PlayerStamina>() : null;
            bool isLowStamina = stamina != null && stamina.GetStaminaPercent() < lowStaminaThreshold;

            int upgradeLvl = 0;
            if (Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance != null)
            {
                upgradeLvl = Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance.GetUpgradeLevel(this);
            }

            if (!isLowStamina)
            {
                // Pobierz staminę przy normalnym ciosie (zmniejszany koszt wraz z poziomem kowadła)
                if (stamina != null)
                {
                    float effectiveStaminaCost = Mathf.Max(2.0f, staminaCostLMB - (upgradeLvl * 0.8f));
                    stamina.DrainStamina(effectiveStaminaCost);
                }

                // Standardowe uderzenie raycast (pełne obrażenia z uwzględnieniem kowadła)
                hand.PerformRaycast();
            }
            else
            {
                // Poniżej 10% staminy: brak ubytku staminy, ale tylko 20% obrażeń
                int totalWeaponDmg = damage + (Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance != null ? Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance.GetBonusDamage(this) : 0);
                int exhaustedDamage = Mathf.Max(1, Mathf.RoundToInt(totalWeaponDmg * lowStaminaDamageMultiplier));
                Debug.Log($"<color=#FF9800>[Młotek Do Kotletów]</color> Wyczerpanie (stamina < 10%)! Brak kosztu staminy, cios zadaje 20% obrażeń ({exhaustedDamage} DMG).");
                hand.PerformRaycast(exhaustedDamage);
            }
        }

        public override void OnHit(GameObject target, IDamageable damageable, RaycastHit hit)
        {
            base.OnHit(target, damageable, hit);

            if (target == null && damageable == null) return;

            // Przy wyczerpaniu (< 10% staminy) brak odrzutu i ogłuszenia
            Camera cam = Camera.main;
            PlayerController player = cam != null ? cam.GetComponentInParent<PlayerController>() : null;
            if (player == null) player = Object.FindFirstObjectByType<PlayerController>();
            PlayerStamina stamina = player != null ? player.GetComponent<PlayerStamina>() : null;
            if (stamina != null && stamina.GetStaminaPercent() < lowStaminaThreshold)
            {
                return;
            }

            // Szansa 10% na lekki knockback i mikro-ogłuszenie
            if (Random.value <= lightKnockbackChance)
            {
                EnemyController enemy = target.GetComponent<EnemyController>() ?? target.GetComponentInParent<EnemyController>();
                if (enemy != null && enemy.gameObject.activeInHierarchy)
                {
                    Vector3 pushDir = cam != null ? cam.transform.forward : (target.transform.position - enemy.transform.position);
                    pushDir.y = 0;
                    if (pushDir.sqrMagnitude < 0.01f) pushDir = Vector3.forward;

                    enemy.ApplyKnockback(pushDir.normalized, lightKnockbackForce, lightStunDuration);
                    Debug.Log($"<color=#FFC107>[Młotek Do Kotletów]</color> Lekki odrzut i ogłuszenie na {enemy.gameObject.name} (10% szansy aktywowane)!");
                }
            }
        }

        public override void AltUse(PlayerController player, PlayerHand hand)
        {
            if (isChargingHeavy || hand == null || hand.IsAttacking || player == null) return;

            PlayerStamina stamina = player.GetComponent<PlayerStamina>();

            int upgradeLvl = 0;
            if (Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance != null)
            {
                upgradeLvl = Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance.GetUpgradeLevel(this);
            }

            // Weryfikacja staminy (próg zmniejsza się z 90% o 6% za każdy poziom ulepszenia na kowadle)
            float effectiveMinStamina = Mathf.Max(0.50f, minStaminaPercentRMB - (upgradeLvl * 0.06f));
            if (stamina != null && !stamina.HasStaminaPercent(effectiveMinStamina))
            {
                Debug.LogWarning($"<color=#FF5722>[Młotek Do Kotletów]</color> Za mało staminy na potężny cios! Wymagane min. {effectiveMinStamina * 100:0}% staminy (obecnie: {stamina.GetStaminaPercent() * 100:0}%).");
                return;
            }

            chargeRoutineInstance = hand.StartCoroutine(ChargedHammerRoutine(player, hand, stamina));
        }

        private IEnumerator ChargedHammerRoutine(PlayerController player, PlayerHand hand, PlayerStamina stamina)
        {
            isChargingHeavy = true;
            hand.IsAttacking = true;

            float chargeTimer = 0f;
            Transform handTransform = hand.transform;
            Vector3 startLocalPos = handTransform.localPosition;
            Quaternion startLocalRot = handTransform.localRotation;

            // Wycofanie młota mocno w prawo i lekko w tył (zamaszyste przygotowanie ciosu jak w sierpie)
            Vector3 pulledBackPos = startLocalPos + new Vector3(0.24f, 0.10f, -0.06f);
            Quaternion pulledBackRot = startLocalRot * Quaternion.Euler(25f, 60f, -40f);

            Debug.Log("<color=#FF8C00>[Młotek Do Kotletów] Rozpoczęto ładowanie Rozbijacza Kotletów... Trzymaj PPM!</color>");

            // Faza ładowania: gracz trzyma PPM
            while (chargeTimer < chargeTime)
            {
                // Jeśli gracz puści PPM przed czasem, przerywamy ładowanie bez kary
                if (Mouse.current != null && !Mouse.current.rightButton.isPressed)
                {
                    Debug.Log("<color=#FFA500>[Młotek Do Kotletów]</color> Zwolniono PPM za wcześnie – cios przerwany.");
                    handTransform.localPosition = startLocalPos;
                    handTransform.localRotation = startLocalRot;
                    isChargingHeavy = false;
                    hand.IsAttacking = false;
                    hand.UpdateHandVisuals();
                    yield break;
                }

                chargeTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(chargeTimer / chargeTime);

                // Drżenie ręki pod ciężarem unoszonego młota przy końcu ładowania
                Vector3 shake = (progress > 0.75f) ? (Vector3)(Random.insideUnitCircle * 0.006f) : Vector3.zero;
                handTransform.localPosition = Vector3.Lerp(startLocalPos, pulledBackPos, progress) + shake;
                handTransform.localRotation = Quaternion.Slerp(startLocalRot, pulledBackRot, progress);

                yield return null;
            }

            Debug.Log("<color=#00FF00><b>[Młotek Do Kotletów] ROZBIJACZ KOTLETÓW W PEŁNI NAŁADOWANY! Puść PPM, aby uderzyć!</b></color>");

            // Czekamy na puszczenie PPM przez gracza
            while (Mouse.current != null && Mouse.current.rightButton.isPressed)
            {
                Vector3 shake = (Vector3)(Random.insideUnitCircle * 0.007f);
                handTransform.localPosition = pulledBackPos + shake;
                yield return null;
            }

            // Gracz puścił PPM -> WYPROWADZENIE POTĘŻNEGO CIOSU Z PRAWEJ W PRZÓD I W LEWO!
            Debug.Log("<color=#FF5722><b>[Młotek Do Kotletów] MIAŻDŻĄCY ZAMACH Z PRAWEJ!</b></color>");

            // Zużycie staminy i spowolnienie
            if (stamina != null)
            {
                stamina.DrainAllStamina(extraRegenDelay: 0.8f);
            }
            if (player != null)
            {
                player.ApplyTemporarySlow(playerSlowMultiplier, playerSlowDuration);
            }

            // Gwałtowne wyprowadzenie ciosu po łuku (smash)
            float strikeTime = 0.10f;
            float t = 0f;
            Vector3 strikePos = startLocalPos + new Vector3(-0.18f, -0.08f, 0.26f);
            Quaternion strikeRot = startLocalRot * Quaternion.Euler(55f, -70f, 45f);

            while (t < 1f)
            {
                t += Time.deltaTime / strikeTime;
                handTransform.localPosition = Vector3.Lerp(pulledBackPos, strikePos, t);
                handTransform.localRotation = Quaternion.Slerp(pulledBackRot, strikeRot, t);
                yield return null;
            }

            // Zadanie obrażeń i odrzut
            PerformHeavySmash(player, hand);

            // Chwilowy impakt / zatrzymanie po zderzeniu z ziemią/wrogiem
            yield return new WaitForSeconds(0.10f);

            // Płynny powrót młotka do pozycji spoczynkowej
            t = 0f;
            float returnTime = 0.25f;
            while (t < 1f)
            {
                t += Time.deltaTime / returnTime;
                handTransform.localPosition = Vector3.Lerp(strikePos, startLocalPos, t);
                handTransform.localRotation = Quaternion.Slerp(strikeRot, startLocalRot, t);
                yield return null;
            }

            handTransform.localPosition = startLocalPos;
            handTransform.localRotation = startLocalRot;
            isChargingHeavy = false;
            hand.IsAttacking = false;
            hand.UpdateHandVisuals();
        }

        private void PerformHeavySmash(PlayerController player, PlayerHand hand)
        {
            Camera cam = Camera.main;
            Vector3 origin = cam != null ? cam.transform.position : (player != null ? player.transform.position + Vector3.up * 1.5f : Vector3.zero);
            Vector3 forward = cam != null ? cam.transform.forward : (player != null ? player.transform.forward : Vector3.forward);

            int upgradeBonus = 0;
            int upgradeLvl = 0;
            if (Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance != null)
            {
                upgradeBonus = Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance.GetBonusDamage(this);
                upgradeLvl = Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance.GetUpgradeLevel(this);
            }

            int totalBaseDamage = damage + heavyDamageBonus + upgradeBonus; // Pełne skalowanie z kowadłem
            float effectiveKnockback = heavyKnockbackForce + (upgradeLvl * 0.8f);

            // Sprawdzenie celów w stożku przed graczem (półkole o zasięgu broni)
            Collider[] colliders = Physics.OverlapSphere(origin + forward * (range * 0.5f), range * 0.75f);
            HashSet<GameObject> hitEnemies = new HashSet<GameObject>();
            bool hitAny = false;

            foreach (Collider col in colliders)
            {
                if (player != null && (col.gameObject == player.gameObject || col.transform.IsChildOf(player.transform)))
                {
                    continue;
                }

                EnemyController enemy = col.GetComponent<EnemyController>() ?? col.GetComponentInParent<EnemyController>();
                IDamageable damageable = col.GetComponent<IDamageable>() ?? col.GetComponentInParent<IDamageable>();

                GameObject enemyObj = enemy != null ? enemy.gameObject : (damageable as MonoBehaviour)?.gameObject;
                if (enemyObj != null && !hitEnemies.Contains(enemyObj) && enemyObj.activeInHierarchy)
                {
                    // Weryfikacja kąta widzenia (czy wróg znajduje się przed graczem w zakresie 75 stopni)
                    Vector3 toEnemy = (enemyObj.transform.position - origin).normalized;
                    if (Vector3.Angle(forward, toEnemy) <= 75f)
                    {
                        hitEnemies.Add(enemyObj);
                        hitAny = true;

                        // Obliczenie obrażeń ze skalowaniem siły gracza
                        int finalDamage = totalBaseDamage;
                        if (player != null)
                        {
                            PlayerStatsUpgrade statsUpgrade = player.GetComponent<PlayerStatsUpgrade>() ?? Object.FindFirstObjectByType<PlayerStatsUpgrade>();
                            PlayerStats stats = player.GetComponent<PlayerStats>();
                            finalDamage = statsUpgrade != null ? statsUpgrade.CalculateFinalDamage(totalBaseDamage) : (stats != null ? stats.GetDamage(totalBaseDamage) : totalBaseDamage);
                        }

                        if (damageable != null)
                        {
                            damageable.TakeDamage(finalDamage);
                            Debug.Log($"<color=#39FF14>[Młotek Do Kotletów - PPM]</color> Miażdżące trafienie w {enemyObj.name}! Zadano {finalDamage} DMG (Baza: {totalBaseDamage})");
                        }

                        if (enemy != null)
                        {
                            Vector3 pushDir = enemyObj.transform.position - (player != null ? player.transform.position : origin);
                            pushDir.y = 0;
                            if (pushDir.sqrMagnitude < 0.01f) pushDir = forward;

                            enemy.ApplyKnockback(pushDir.normalized, effectiveKnockback, heavyStunDuration);
                            Debug.Log($"<color=#FF5722>[Młotek Do Kotletów - PPM]</color> POTĘŻNY ODRZUT i OGŁUSZENIE przeciwnika {enemyObj.name} na {heavyStunDuration}s (Odrzut: {effectiveKnockback:0.0})!");
                        }
                    }
                }
            }

            // Fallback na pojedynczy punktowy Raycast, jeśli żaden wróg nie został trafiony obszarowo
            if (!hitAny)
            {
                hand.PerformRaycast(totalBaseDamage, (target, dmgable, hit) =>
                {
                    EnemyController enemy = target.GetComponent<EnemyController>() ?? target.GetComponentInParent<EnemyController>();
                    if (enemy != null)
                    {
                        Vector3 pushDir = forward;
                        pushDir.y = 0;
                        enemy.ApplyKnockback(pushDir.normalized, effectiveKnockback, heavyStunDuration);
                        Debug.Log($"<color=#FF5722>[Młotek Do Kotletów - PPM]</color> Bezpośredni odrzut i ogłuszenie przeciwnika {target.name}!");
                    }
                }, range);
            }
        }
    }
}
