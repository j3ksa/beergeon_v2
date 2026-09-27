using System;
using System.Collections;
using UnityEngine;
using Praktyki.StaryHmielnik;

namespace Praktyki.MlekoStarego
{
    /// <summary>
    /// Zarządza aktywnym stanem odporności i oczyszczenia po wypiciu "Mleka Starego".
    /// Natychmiast zdejmuje zatrucie (HopPoisonDebuff) oraz spowolnienie (HopSlowDebuff)
    /// i zapewnia 10 sekund całkowitej ochrony/odporności przed ponownym zatruciem przez Starego Chmielnika.
    /// </summary>
    [DisallowMultipleComponent]
    public class MlekoStaregoBuff : MonoBehaviour
    {
        private static MlekoStaregoBuff instance;

        [Header("Ustawienia Ochrony")]
        [Tooltip("Czas trwania odporności na truciznę w sekundach (domyślnie 10s)")]
        [SerializeField] private float immunityDuration = 10.0f;

        private float remainingDuration = 0f;
        private bool isImmune = false;
        private Coroutine immunityCoroutine;

        public static event Action<bool> OnImmunityStateChanged;
        public static event Action OnMlekoStaregoDrank;

        public static bool IsImmune => instance != null && instance.isImmune;
        public static float RemainingDuration => instance != null ? instance.remainingDuration : 0f;
        public static MlekoStaregoBuff Instance => instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }

        /// <summary>
        /// Globalna metoda do aktywacji ochrony i oczyszczenia gracza.
        /// </summary>
        public static void ApplyBuff(float duration = 10.0f, bool cleanseSlow = true)
        {
            if (instance == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    instance = player.GetComponent<MlekoStaregoBuff>();
                    if (instance == null)
                    {
                        instance = player.AddComponent<MlekoStaregoBuff>();
                    }
                }
                else
                {
                    PlayerHealth ph = UnityEngine.Object.FindFirstObjectByType<PlayerHealth>();
                    if (ph != null)
                    {
                        instance = ph.GetComponent<MlekoStaregoBuff>();
                        if (instance == null)
                        {
                            instance = ph.gameObject.AddComponent<MlekoStaregoBuff>();
                        }
                    }
                }
            }

            if (instance != null)
            {
                instance.ActivateImmunity(duration, cleanseSlow);
            }
            else
            {
                Debug.LogWarning("[MlekoStarego] Nie znaleziono obiektu Gracza, aby nałożyć MlekoStaregoBuff!");
            }
        }

        public void ActivateImmunity(float duration, bool cleanseSlow)
        {
            if (duration <= 0f) duration = immunityDuration;

            // 1. Natychmiastowe oczyszczenie (Cleanses) zatrucia
            HopPoisonDebuff poison = GetComponent<HopPoisonDebuff>();
            if (poison != null)
            {
                poison.RemovePoison();
                Debug.Log("<color=#FFFFFF><b>[Mleko Starego]</b></color> Trucizna Starego Chmielnika została zneutralizowana!");
            }

            // Opcjonalnie zdejmij również spowolnienie (mleko oczyszcza organizm ze śluzu/zarodników pleśni)
            if (cleanseSlow)
            {
                HopSlowDebuff slow = GetComponent<HopSlowDebuff>();
                if (slow != null)
                {
                    slow.RemoveDebuff();
                    Debug.Log("<color=#FFFFFF><b>[Mleko Starego]</b></color> Spowolnienie zostało zniwelowane!");
                }
            }

            // 2. Włączenie ochrony na określony czas (10s)
            remainingDuration = duration;

            if (immunityCoroutine != null)
            {
                StopCoroutine(immunityCoroutine);
            }

            immunityCoroutine = StartCoroutine(ImmunityRoutine());
            OnMlekoStaregoDrank?.Invoke();
        }

        private IEnumerator ImmunityRoutine()
        {
            isImmune = true;
            OnImmunityStateChanged?.Invoke(true);

            // Wyzwolenie białego rozbłysku i aury na ekranie UI
            MlekoStaregoUI.TriggerEffect(remainingDuration);

            Debug.Log($"<color=#FFFFFF><b>[Mleko Starego]</b></color> Gracz zyskał odporność na truciznę na {remainingDuration:F1}s!");

            while (remainingDuration > 0)
            {
                remainingDuration -= Time.deltaTime;
                yield return null;
            }

            ResetImmunity();
        }

        public void ResetImmunity()
        {
            if (immunityCoroutine != null)
            {
                StopCoroutine(immunityCoroutine);
                immunityCoroutine = null;
            }

            if (isImmune)
            {
                isImmune = false;
                OnImmunityStateChanged?.Invoke(false);
                Debug.Log("<color=#FFFFFF><b>[Mleko Starego]</b></color> Ochrona przed trucizną wygasła.");
            }

            remainingDuration = 0f;
        }

        private void OnDisable()
        {
            ResetImmunity();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
