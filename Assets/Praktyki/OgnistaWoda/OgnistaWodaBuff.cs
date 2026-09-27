using System;
using System.Collections;
using UnityEngine;

namespace Praktyki.OgnistaWoda
{
    /// <summary>
    /// Komponent gracza zarządzający aktywnym buffem redukcji obrażeń z Ognistej Wody.
    /// </summary>
    [DisallowMultipleComponent]
    public class OgnistaWodaBuff : MonoBehaviour
    {
        private static OgnistaWodaBuff instance;

        [Header("Ustawienia Redukcji Obrażeń")]
        [Tooltip("Procent redukcji otrzymywanych obrażeń (np. 0.25 = 25% mniejsze obrażenia)")]
        [SerializeField] [Range(0.05f, 0.95f)] private float reductionPercent = 0.25f;

        [Tooltip("Czas trwania buffa w sekundach")]
        [SerializeField] private float buffDuration = 10.0f;

        private float remainingDuration = 0f;
        private bool isBuffActive = false;
        private Coroutine buffCoroutine;

        public static event Action<bool> OnBuffStateChanged;
        public static event Action OnOgnistaWodaDrank;

        public static bool IsActive => instance != null && instance.isBuffActive;
        public static float DamageReductionPercent => instance != null ? instance.reductionPercent : 0.25f;
        public static float RemainingDuration => instance != null ? instance.remainingDuration : 0f;
        public static OgnistaWodaBuff Instance => instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }

        /// <summary>
        /// Globalna metoda do nałożenia/odświeżenia buffa Ognistej Wody na graczu.
        /// </summary>
        public static void ApplyBuff(float reduction = 0.25f, float duration = 10.0f)
        {
            if (instance == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    instance = player.GetComponent<OgnistaWodaBuff>();
                    if (instance == null)
                    {
                        instance = player.AddComponent<OgnistaWodaBuff>();
                    }
                }
                else
                {
                    PlayerHealth ph = UnityEngine.Object.FindFirstObjectByType<PlayerHealth>();
                    if (ph != null)
                    {
                        instance = ph.GetComponent<OgnistaWodaBuff>();
                        if (instance == null)
                        {
                            instance = ph.gameObject.AddComponent<OgnistaWodaBuff>();
                        }
                    }
                }
            }

            if (instance != null)
            {
                instance.ActivateBuff(reduction, duration);
            }
            else
            {
                Debug.LogWarning("[OgnistaWoda] Nie znaleziono obiektu Gracza, aby nałożyć OgnistaWodaBuff!");
            }
        }

        public void ActivateBuff(float reduction, float duration)
        {
            reductionPercent = reduction;
            remainingDuration = duration;

            if (buffCoroutine != null)
            {
                StopCoroutine(buffCoroutine);
            }

            buffCoroutine = StartCoroutine(BuffRoutine());
            OnOgnistaWodaDrank?.Invoke();
        }

        private IEnumerator BuffRoutine()
        {
            isBuffActive = true;
            OnBuffStateChanged?.Invoke(true);

            // Wyzwolenie winiety UI
            OgnistaWodaUI.TriggerEffect(buffDuration);

            Debug.Log($"<color=#B0C4DE><b>[Ognista Woda]</b></color> Aktywowano ochronę! Otrzymywane obrażenia mniejsze o {reductionPercent * 100:F0}% przez {remainingDuration:F1}s.");

            while (remainingDuration > 0)
            {
                remainingDuration -= Time.deltaTime;
                yield return null;
            }

            ResetBuff();
        }

        public void ResetBuff()
        {
            if (buffCoroutine != null)
            {
                StopCoroutine(buffCoroutine);
                buffCoroutine = null;
            }

            if (isBuffActive)
            {
                isBuffActive = false;
                OnBuffStateChanged?.Invoke(false);
                Debug.Log("<color=#B0C4DE><b>[Ognista Woda]</b></color> Efekt Ognistej Wody wygasł. Obrona powróciła do normy.");
            }

            remainingDuration = 0f;
        }

        private void OnDisable()
        {
            ResetBuff();
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
