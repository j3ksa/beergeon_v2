using System.Collections;
using UnityEngine;

namespace Praktyki.StaryHmielnik
{
    /// <summary>
    /// Nakłada efekt spowolnienia na gracza po trafieniu szyszką chmielu.
    /// Zmniejsza prędkość poruszania się na określony czas, a następnie bezpiecznie ją przywraca.
    /// </summary>
    [DisallowMultipleComponent]
    public class HopSlowDebuff : MonoBehaviour
    {
        private PlayerController playerController;
        private float originalSpeed;
        private float currentSlowMultiplier = 0.5f;
        private float remainingDuration = 0f;
        private bool isDebuffActive = false;
        private Coroutine debuffCoroutine;

        public bool IsActive => isDebuffActive;
        public float RemainingTime => remainingDuration;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            if (playerController != null)
            {
                originalSpeed = playerController.MoveSpeed;
            }
        }

        /// <summary>
        /// Aplikuje lub odświeża efekt spowolnienia.
        /// </summary>
        /// <param name="slowMultiplier">Mnożnik prędkości (np. 0.5f = 50% prędkości bazowej)</param>
        /// <param name="duration">Czas trwania w sekundach</param>
        public void ApplySlow(float slowMultiplier, float duration)
        {
            if (playerController == null)
            {
                playerController = GetComponent<PlayerController>();
                if (playerController == null) return;
            }

            // Jeśli to nowe nałożenie debuffu, zapamiętujemy bieżącą prędkość
            if (!isDebuffActive)
            {
                originalSpeed = playerController.MoveSpeed;
                currentSlowMultiplier = slowMultiplier;
            }
            else
            {
                // Wybieramy mocniejsze spowolnienie (mniejszy mnożnik)
                currentSlowMultiplier = Mathf.Min(currentSlowMultiplier, slowMultiplier);
            }

            // Odświeżamy czas trwania
            remainingDuration = Mathf.Max(remainingDuration, duration);

            if (debuffCoroutine != null)
            {
                StopCoroutine(debuffCoroutine);
            }

            debuffCoroutine = StartCoroutine(SlowRoutine());
        }

        private IEnumerator SlowRoutine()
        {
            isDebuffActive = true;
            playerController.MoveSpeed = originalSpeed * currentSlowMultiplier;
            HopSlowVignetteUI.TriggerEffect(remainingDuration);
            Debug.Log($"[HopSlowDebuff] Gracz spowolniony! Prędkość: {playerController.MoveSpeed} (mnożnik {currentSlowMultiplier}) na {remainingDuration}s.");

            while (remainingDuration > 0)
            {
                remainingDuration -= Time.deltaTime;
                yield return null;
            }

            RemoveDebuff();
        }

        public void RemoveDebuff()
        {
            if (debuffCoroutine != null)
            {
                StopCoroutine(debuffCoroutine);
                debuffCoroutine = null;
            }

            if (isDebuffActive && playerController != null)
            {
                playerController.MoveSpeed = originalSpeed;
                HopSlowVignetteUI.StopEffect();
                Debug.Log($"[HopSlowDebuff] Spowolnienie minęło. Prędkość przywrócona do {originalSpeed}.");
            }

            isDebuffActive = false;
            remainingDuration = 0f;
            currentSlowMultiplier = 1f;
        }

        private void OnDisable()
        {
            RemoveDebuff();
        }

        private void OnDestroy()
        {
            RemoveDebuff();
        }
    }
}
