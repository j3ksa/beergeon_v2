using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Praktyki.KowadloUlepszen;

namespace Praktyki.Sierp
{
    [CreateAssetMenu(fileName = "Sierp", menuName = "Inventory/Weapons/Sierp")]
    public class SierpItem : WeaponItem
    {
        [Header("Mechanika Combo LPM")]
        [Tooltip("Czas w sekundach na wyprowadzenie kolejnego ciosu w serii, zanim combo wygaśnie")]
        public float comboResetTime = 1.1f;

        [Tooltip("Mnożnik obrażeń dla 3. ciosu w serii (Finishera)")]
        public float thirdStrikeDamageMultiplier = 1.45f;

        [Tooltip("Czas trwania krwawienia nałożonego 3. ciosem (w sekundach)")]
        public float bleedDuration = 3.5f;

        [Tooltip("Obrażenia krwawienia na sekundę")]
        public int bleedDamagePerSecond = 4;

        [Header("Kradzież Życia (Lifesteal)")]
        [Tooltip("Procent maksymalnego zdrowia gracza odzyskiwany przy zwykłych cięciach (1 i 2 cios) - np. 0.02 = 2%")]
        public float regularHitHealPercent = 0.02f;

        [Tooltip("Procent maksymalnego zdrowia gracza odzyskiwany przy 3. ciosie (Finisher) - np. 0.05 = 5%")]
        public float finisherHealPercent = 0.05f;

        [Header("Mechanika Ładowanego PPM (Rupture / Hak)")]
        [Tooltip("Czas potrzebny na pełne naładowanie ataku PPM (w sekundach)")]
        public float chargeTime = 0.85f;

        [Tooltip("Koszt staminy na ładowany atak")]
        public float chargeStaminaCost = 15f;

        [Tooltip("Mnożnik bazowych obrażeń dla naładowanego ataku PPM (gdy cel NIE krwawi)")]
        public float chargedDamageMultiplier = 1.7f;

        [Tooltip("Bazowe obrażenia dodatkowe przy zerwaniu krwawienia (Rupture)")]
        public int ruptureBonusDamage = 18;

        [Tooltip("Procent maksymalnego zdrowia odzyskiwany przy zerwaniu krwawienia (Rupture)")]
        public float ruptureHealPercent = 0.20f;

        [Tooltip("Procent leczenia z naładowanego PPM gdy cel NIE krwawił")]
        public float chargedNonBleedHealPercent = 0.06f;

        [Header("Pasywka: Krew za Krew (<30% HP)")]
        [Tooltip("Próg zdrowia gracza aktywujący desperację (0.30 = poniżej 30% HP)")]
        public float desperationHealthThreshold = 0.30f;

        [Tooltip("Mnożnik kradzieży życia w stanie desperacji (1.5 = +50% do całego leczenia)")]
        public float desperationLifestealBonus = 1.5f;

        // Stan combo gracza (statyczny na instancję w ręku gracza)
        private static int comboStep = 0; // 0, 1, 2
        private static float lastStrikeTime = -999f;
        private static Coroutine chargeRoutineInstance = null;
        private static bool isChargingHeavy = false;

        private void Reset()
        {
            itemName = "Sierp";
            damage = 14;
            range = 2.1f;
            cooldown = 0.32f; // szybka, zwinna broń
            useItemRotation = true;
            rotationEuler = new Vector3(30, -35, -25);
            rotationSpeed = 12f;
            returnRotationSpeedMultiplier = 1.3f;
            handPositionOffset = new Vector3(0.08f, -0.08f, 0.22f);
            handScaleMultiplier = 1.05f;
            description = "Rolniczy sierp o zakrzywionym ostrzu. Każde cięcie wysysa życie (skalowane z Twoim Max HP). Seria 3 ciosów nakłada Krwawienie (otrzymanie ciosu przerywa combo!). Przytrzymaj PPM by naładować potężne szarpnięcie – zerwanie krwawienia zadaje krytyczny cios i leczy aż 20% Twojego Max HP!";
        }

        public override void OnEquipped(PlayerController player, PlayerHand hand)
        {
            base.OnEquipped(player, hand);
            comboStep = 0;
            isChargingHeavy = false;

            // Subskrypcja na obrażenia gracza, aby zresetować combo w przypadku oberwania ciosu
            if (player != null)
            {
                PlayerHealth health = player.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.OnHealthChanged.RemoveListener(OnPlayerDamagedCheck);
                    health.OnHealthChanged.AddListener(OnPlayerDamagedCheck);
                }
            }
        }

        public override void OnUnequipped(PlayerController player, PlayerHand hand)
        {
            base.OnUnequipped(player, hand);
            comboStep = 0;
            if (isChargingHeavy && hand != null)
            {
                hand.StopAllCoroutines();
                hand.IsAttacking = false;
            }
            isChargingHeavy = false;

            if (player != null)
            {
                PlayerHealth health = player.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.OnHealthChanged.RemoveListener(OnPlayerDamagedCheck);
                }
            }
        }

        private void OnPlayerDamagedCheck(float currentPercent)
        {
            // Jeśli gracz otrzyma cios (i nie jest to leczenie), combo zostaje przerwane!
            if (comboStep > 0)
            {
                comboStep = 0;
                Debug.Log("<color=#FF6347>[Sierp] Otrzymano cios! Combo zostało przerwane!</color>");
            }
        }

        /// <summary>
        /// LPM - Szybkie cięcia w combo 1-2-3 (z dynamiczną animacją cięcia po łuku)
        /// </summary>
        public override void Use(PlayerController player, PlayerHand hand)
        {
            if (isChargingHeavy || hand == null) return;

            // Sprawdzenie czy poprzednie uderzenie nie przedawniło się
            if (Time.time - lastStrikeTime > comboResetTime)
            {
                comboStep = 0;
            }

            int currentCombo = comboStep;

            // Parametry animacji cięcia sierpem (naprzemienne lub zamaszyste)
            Vector3 prepPos = (currentCombo == 1) ? new Vector3(-0.08f, 0.05f, 0.05f) : new Vector3(0.12f, 0.08f, -0.05f);
            Vector3 prepRot = (currentCombo == 1) ? new Vector3(-20f, -45f, 30f) : new Vector3(25f, 40f, -35f);

            Vector3 slashPos = (currentCombo == 1) ? new Vector3(0.15f, -0.06f, 0.20f) : new Vector3(-0.16f, -0.08f, 0.22f);
            Vector3 slashRot = (currentCombo == 1) ? new Vector3(40f, 50f, -40f) : new Vector3(45f, -65f, 40f);

            float speed = (currentCombo == 2) ? 14f : 16f;

            hand.StartCoroutine(hand.SickleSlashRoutine(prepPos, prepRot, slashPos, slashRot, speed, () =>
            {
                // Sprawdzamy czy trafiliśmy cel
                bool hitAnything = false;
                hand.PerformRaycast(-1, (targetObj, damageable, hitInfo) =>
                {
                    hitAnything = true;
                    OnHit(targetObj, damageable, hitInfo);
                });

                // Jeśli machnęliśmy w powietrze (pudło), combo się natychmiast zeruje!
                if (!hitAnything)
                {
                    if (comboStep > 0)
                    {
                        Debug.Log("<color=#808080>[Sierp] Pudło! Machnięcie w powietrze zresetowało combo.</color>");
                    }
                    comboStep = 0;
                }
            }));
        }

        /// <summary>
        /// Wywoływane po udanym trafieniu wroga przez PlayerHand.PerformRaycast()
        /// </summary>
        public override void OnHit(GameObject target, IDamageable damageable, RaycastHit hit)
        {
            if (target == null || damageable == null) return;

            // Naliczamy kolejny stopień combo TYLKO po celnym trafieniu!
            int currentComboHit = comboStep;
            comboStep = (comboStep + 1) % 3;
            lastStrikeTime = Time.time;

            bool isFinisher = (currentComboHit == 2); // Trzecie celne trafienie w serii

            Debug.Log($"<color=#32CD32>[Sierp]</color> Celne trafienie! Combo: {currentComboHit + 1}/3 {(isFinisher ? "★ FINISHER! ★" : "")}");

            PlayerController player = Object.FindFirstObjectByType<PlayerController>();
            if (player == null) return;

            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            PlayerStats stats = player.GetComponent<PlayerStats>();
            PlayerStatsUpgrade statsUpgrade = player.GetComponent<PlayerStatsUpgrade>() ?? Object.FindFirstObjectByType<PlayerStatsUpgrade>();

            // Bazowe obrażenia z kowadłem
            int baseDmg = damage;
            int upgradeLvl = 0;
            if (WeaponUpgradeManager.Instance != null)
            {
                baseDmg += WeaponUpgradeManager.Instance.GetBonusDamage(this);
                upgradeLvl = WeaponUpgradeManager.Instance.GetUpgradeLevel(itemName);
            }

            int finalHitDmg = isFinisher ? Mathf.RoundToInt(baseDmg * thirdStrikeDamageMultiplier) : baseDmg;
            if (statsUpgrade != null) finalHitDmg = statsUpgrade.CalculateFinalDamage(finalHitDmg);
            else if (stats != null) finalHitDmg = stats.GetDamage(finalHitDmg);

            // Dodatkowy cios finishera (dodatkowy narzut)
            if (isFinisher)
            {
                damageable.TakeDamage(Mathf.Max(1, finalHitDmg - baseDmg)); // Hand zadał już bazowy damage, więc dopisujemy różnicę
                
                // Nałożenie Krwawienia na wroga
                BleedEffect bleed = target.GetComponent<BleedEffect>() ?? target.GetComponentInParent<BleedEffect>();
                if (bleed == null) bleed = target.AddComponent<BleedEffect>();
                bleed.ApplyBleed(bleedDuration, bleedDamagePerSecond + upgradeLvl);

                Debug.Log($"<color=#DC143C><b>[Sierp - FINISHER!]</b></color> Zadano potężne cięcie ({finalHitDmg} dmg) i nałożono KRWAWIENIE na {target.name}!");
            }

            // Kradzież życia (Lifesteal)
            if (playerHealth != null)
            {
                int maxHp = stats != null ? stats.GetMaxHealth() : 100;
                float healFraction = isFinisher ? finisherHealPercent : regularHitHealPercent;

                // Bonus z ulepszenia na kowadle (+0.5% max HP leczenia na poziom)
                healFraction += (upgradeLvl * 0.005f);

                // Pasywka: Krew za Krew (<30% HP)
                if (playerHealth.GetHealthPercent() <= desperationHealthThreshold)
                {
                    healFraction *= desperationLifestealBonus;
                    Debug.Log("<color=#FF4500><b>[Sierp - KREW ZA KREW!]</b> Bonus desperacji aktywny (+50% do wysysania życia)!</color>");
                }

                int healAmount = Mathf.Max(1, Mathf.RoundToInt(maxHp * healFraction));
                playerHealth.Heal(healAmount);
                Debug.Log($"<color=#00FF7F>[Sierp - Lifesteal]</color> Odzyskano <b>+{healAmount} HP</b> (Max HP: {maxHp}).");
            }
        }

        /// <summary>
        /// PPM - Ładowany Hak Żniwiarza (Charged Rupture)
        /// </summary>
        public override void AltUse(PlayerController player, PlayerHand hand)
        {
            if (isChargingHeavy || hand == null || player == null) return;

            PlayerStamina stamina = player.GetComponent<PlayerStamina>();
            if (stamina != null && stamina.CurrentStamina < chargeStaminaCost)
            {
                Debug.LogWarning("<color=#FFA500>[Sierp]</color> Zbyt mało staminy na ładowany atak!");
                return;
            }

            if (stamina != null)
            {
                stamina.DrainStamina(chargeStaminaCost);
            }

            chargeRoutineInstance = hand.StartCoroutine(ChargedAttackRoutine(player, hand));
        }

        private IEnumerator ChargedAttackRoutine(PlayerController player, PlayerHand hand)
        {
            isChargingHeavy = true;
            hand.IsAttacking = true;

            float chargeTimer = 0f;
            Transform handTransform = hand.transform;
            Vector3 startLocalPos = handTransform.localPosition;
            Quaternion startLocalRot = handTransform.localRotation;

            // Wycofanie sierpa mocno w prawo i w tył pod kątem (potężny zamach przedramienia)
            Vector3 pulledBackPos = startLocalPos + new Vector3(0.18f, 0.10f, -0.12f);
            Quaternion pulledBackRot = startLocalRot * Quaternion.Euler(30f, 65f, -45f);

            Debug.Log("<color=#FF8C00>[Sierp] Rozpoczęto ładowanie ciosu PPM... Trzymaj lub puść!</color>");

            // Pętla ładowania: gracz trzyma PPM lub czeka na pełne naładowanie
            while (chargeTimer < chargeTime)
            {
                // Jeśli gracz puścił PPM przedwcześnie, przerywamy ładowanie
                if (Mouse.current != null && !Mouse.current.rightButton.isPressed)
                {
                    Debug.Log("<color=#FFA500>[Sierp]</color> Zwolniono PPM za wcześnie – atak przerwany.");
                    handTransform.localPosition = startLocalPos;
                    handTransform.localRotation = startLocalRot;
                    isChargingHeavy = false;
                    hand.IsAttacking = false;
                    hand.UpdateHandVisuals();
                    yield break;
                }

                chargeTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(chargeTimer / chargeTime);

                // Efekt lekkiego drżenia przy pełnym naładowaniu
                Vector3 shake = (progress > 0.8f) ? (Vector3)(Random.insideUnitCircle * 0.005f) : Vector3.zero;
                handTransform.localPosition = Vector3.Lerp(startLocalPos, pulledBackPos, progress) + shake;
                handTransform.localRotation = Quaternion.Slerp(startLocalRot, pulledBackRot, progress);

                yield return null;
            }

            Debug.Log("<color=#00FF00><b>[Sierp] HAK W PEŁNI NAŁADOWANY! Czekam na zwolnienie PPM...</b></color>");

            // Czekamy na puszczenie PPM przez gracza
            while (Mouse.current != null && Mouse.current.rightButton.isPressed)
            {
                // Utrzymujemy stan pełnego napięcia z wibracją
                Vector3 shake = (Vector3)(Random.insideUnitCircle * 0.006f);
                handTransform.localPosition = pulledBackPos + shake;
                yield return null;
            }

            // GRACZ PUŚCIŁ PPM -> POTĘŻNE, ZAMASZYSTE CIĘCIE PO ŁUKU OD PRAWEJ DO LEWEJ
            Debug.Log("<color=#DC143C><b>[Sierp] WYPROWADZENIE HAKA ŻNIWIARZA!</b></color>");

            // Cięcie przez środek aż do lewej krawędzi (zadzierający hak)
            float strikeTime = 0.10f;
            float t = 0f;
            Vector3 forwardStrikePos = startLocalPos + new Vector3(-0.22f, -0.06f, 0.22f);
            Quaternion forwardStrikeRot = startLocalRot * Quaternion.Euler(55f, -75f, 50f);

            while (t < 1f)
            {
                t += Time.deltaTime / strikeTime;
                handTransform.localPosition = Vector3.Lerp(pulledBackPos, forwardStrikePos, t);
                handTransform.localRotation = Quaternion.Slerp(pulledBackRot, forwardStrikeRot, t);
                yield return null;
            }

            // DETEKCJA TRAFIENIA
            ExecuteRuptureHit(player);

            // Płynny powrót z lewej strony do pozycji spoczynkowej
            yield return new WaitForSeconds(0.08f);
            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.22f;
                handTransform.localPosition = Vector3.Lerp(forwardStrikePos, startLocalPos, t);
                handTransform.localRotation = Quaternion.Slerp(forwardStrikeRot, startLocalRot, t);
                yield return null;
            }

            handTransform.localPosition = startLocalPos;
            handTransform.localRotation = startLocalRot;
            isChargingHeavy = false;
            hand.IsAttacking = false;
            hand.UpdateHandVisuals();
        }

        private void ExecuteRuptureHit(PlayerController player)
        {
            Camera cam = Camera.main ?? player.GetComponentInChildren<Camera>();
            if (cam == null) return;

            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, range + 0.5f))
            {
                IDamageable target = hit.collider.GetComponent<IDamageable>() ?? hit.collider.GetComponentInParent<IDamageable>();
                if (target != null)
                {
                    PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                    PlayerStats stats = player.GetComponent<PlayerStats>();
                    PlayerStatsUpgrade statsUpgrade = player.GetComponent<PlayerStatsUpgrade>() ?? Object.FindFirstObjectByType<PlayerStatsUpgrade>();

                    int baseDmg = damage;
                    int upgradeLvl = 0;
                    if (WeaponUpgradeManager.Instance != null)
                    {
                        baseDmg += WeaponUpgradeManager.Instance.GetBonusDamage(this);
                        upgradeLvl = WeaponUpgradeManager.Instance.GetUpgradeLevel(itemName);
                    }

                    BleedEffect bleed = hit.collider.GetComponent<BleedEffect>() ?? hit.collider.GetComponentInParent<BleedEffect>();
                    bool isTargetBleeding = bleed != null && bleed.IsBleeding;

                    int strikeDmg = Mathf.RoundToInt(baseDmg * chargedDamageMultiplier);
                    float healPercent = chargedNonBleedHealPercent;

                    if (isTargetBleeding)
                    {
                        // RUPTURE! ZERWANIE KRWAWIENIA!
                        int ticks = bleed.ConsumeBleed();
                        int bonusRupture = ruptureBonusDamage + (ticks * bleed.DamagePerTick);
                        strikeDmg += bonusRupture;
                        healPercent = ruptureHealPercent + (upgradeLvl * 0.02f); // +2% leczenia na poziom kowadła!

                        Debug.LogWarning($"<color=#FF0000><b>[SIERP - RUPTURE!]</b> Zerwano krwawienie z {hit.collider.name}! Zadano aż <b>{strikeDmg} DMG</b>!</color>");
                    }
                    else
                    {
                        Debug.Log($"<color=#FFA500>[Sierp - Hak]</color> Trafiono cel bez krwawienia za {strikeDmg} dmg.");
                    }

                    int finalDmg = statsUpgrade != null ? statsUpgrade.CalculateFinalDamage(strikeDmg) : (stats != null ? stats.GetDamage(strikeDmg) : strikeDmg);
                    target.TakeDamage(finalDmg);

                    // Leczenie procentowe z Max HP gracza
                    if (playerHealth != null)
                    {
                        int maxHp = stats != null ? stats.GetMaxHealth() : 100;
                        if (playerHealth.GetHealthPercent() <= desperationHealthThreshold)
                        {
                            healPercent *= desperationLifestealBonus;
                            Debug.Log("<color=#FF4500><b>[Sierp - KREW ZA KREW!]</b> Desperacja zwiększa wampiryzm z Rupture!</color>");
                        }

                        int healAmount = Mathf.Max(1, Mathf.RoundToInt(maxHp * healPercent));
                        playerHealth.Heal(healAmount);
                        Debug.Log($"<color=#00FF00><b>[Sierp - Rupture Heal]</b> Uleczono <b>+{healAmount} HP</b> ({healPercent * 100:F0}% Twojego Max HP)!</color>");
                    }
                }
            }
        }
    }
}
