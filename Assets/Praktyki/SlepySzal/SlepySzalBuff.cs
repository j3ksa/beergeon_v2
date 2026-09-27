using System;
using UnityEngine;

namespace Praktyki.SlepySzal
{
    /// <summary>
    /// Komponent gracza zarządzający stanem Ślepego Szału (+200% obrażeń, 3x szybszy spadek zdrowia, tunelowa ślepota).
    /// Efekt jest trwały (brak timera) – trwa dopóki nie zostanie wyleczony dedykowaną miksturą lub gracz nie zginie.
    /// </summary>
    [DisallowMultipleComponent]
    public class SlepySzalBuff : MonoBehaviour
    {
        private static SlepySzalBuff instance;

        [Header("Ustawienia Ślepego Szału")]
        [Tooltip("Mnożnik obrażeń (3.0 = +200% obrażeń, czyli 3x większe obrażenia)")]
        [SerializeField] private float damageMultiplier = 3.0f;

        [Tooltip("Mnożnik pasywnego ubytku zdrowia (3.0 = zdrowie ubywa 3x szybciej)")]
        [SerializeField] private float decayMultiplier = 3.0f;

        private bool isBuffActive = false;

        public static event Action<bool> OnBuffStateChanged;

        public static bool IsActive => instance != null && instance.isBuffActive;
        public static float DamageMultiplier => instance != null ? instance.damageMultiplier : 3.0f;
        public static float DecayMultiplier => instance != null ? instance.decayMultiplier : 3.0f;
        public static SlepySzalBuff Instance => instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }

        /// <summary>
        /// Globalna metoda aktywująca Ślepy Szał na graczu.
        /// </summary>
        public static void ApplyBuff(float dmgMult = 3.0f, float decayMult = 3.0f, float clearRadius = 0.1f, float outerRadius = 0.22f)
        {
            if (instance == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    instance = player.GetComponent<SlepySzalBuff>();
                    if (instance == null)
                    {
                        instance = player.AddComponent<SlepySzalBuff>();
                    }
                }
                else
                {
                    PlayerHealth ph = UnityEngine.Object.FindFirstObjectByType<PlayerHealth>();
                    if (ph != null)
                    {
                        instance = ph.GetComponent<SlepySzalBuff>();
                        if (instance == null)
                        {
                            instance = ph.gameObject.AddComponent<SlepySzalBuff>();
                        }
                    }
                }
            }

            if (instance != null)
            {
                instance.ActivateBuff(dmgMult, decayMult, clearRadius, outerRadius);
            }
            else
            {
                Debug.LogWarning("[SlepySzal] Nie znaleziono obiektu Gracza, aby nałożyć SlepySzalBuff!");
            }
        }

        /// <summary>
        /// Globalna metoda lecząca/usuwająca Ślepy Szał (wywoływana przez przyszłą potkę leczącą/oczyszczającą).
        /// </summary>
        public static void CureBuff()
        {
            if (instance != null && instance.isBuffActive)
            {
                instance.DeactivateBuff();
                Debug.Log("<color=#32CD32><b>[Ślepy Szał]</b></color> Ślepy Szał został wyleczony! Statystyki i wzrok powróciły do normy.");
            }
        }

        public void ActivateBuff(float dmgMult, float decayMult, float clearRadius = 0.1f, float outerRadius = 0.22f)
        {
            damageMultiplier = dmgMult;
            decayMultiplier = decayMult;
            isBuffActive = true;

            OnBuffStateChanged?.Invoke(true);

            // Włączenie czarnej winiety tunelowej UI z podanymi promieniami
            SlepySzalUI.SetVisionRadius(clearRadius, outerRadius);
            SlepySzalUI.EnableTunnelVision(true);

            Debug.Log($"<color=#8B0000><b>[Ślepy Szał]</b></color> Aktywowano Ślepy Szał! Obrażenia x{damageMultiplier} (+{(damageMultiplier - 1f) * 100:F0}%), ubytek zdrowia x{decayMultiplier} szybszy, pole widzenia: {clearRadius}.");
        }

        public void DeactivateBuff()
        {
            if (!isBuffActive) return;

            isBuffActive = false;
            OnBuffStateChanged?.Invoke(false);

            // Wyłączenie winiety UI
            SlepySzalUI.EnableTunnelVision(false);
        }

        private void OnDisable()
        {
            DeactivateBuff();
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
