using System.Collections;
using UnityEngine;

namespace Praktyki.SpeedPotion
{
    [DisallowMultipleComponent]
    public class PlayerSpeedBuff : MonoBehaviour
    {
        private PlayerController playerController;
        private float baseMoveSpeed;
        private float currentMultiplier = 1f;
        private float remainingDuration = 0f;
        private bool isBuffActive = false;
        private Coroutine buffCoroutine;

        public bool IsBuffActive => isBuffActive;
        public float RemainingDuration => remainingDuration;
        public float CurrentMultiplier => currentMultiplier;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            if (playerController != null)
            {
                baseMoveSpeed = playerController.MoveSpeed;
            }
        }

        public void ApplySpeedBuff(float multiplier, float duration)
        {
            if (playerController == null)
            {
                playerController = GetComponent<PlayerController>();
                if (playerController == null)
                {
                    Debug.LogError("[PlayerSpeedBuff] Brak komponentu PlayerController na obiekcie gracza!");
                    return;
                }
            }

            // Jeśli buff nie był aktywny, zapamiętujemy aktualną bazową prędkość
            if (!isBuffActive)
            {
                baseMoveSpeed = playerController.MoveSpeed;
            }

            // Ustawiamy maksymalny mnożnik i odnawiamy/wydłużamy czas trwania
            currentMultiplier = Mathf.Max(currentMultiplier, multiplier);
            remainingDuration = duration;

            if (buffCoroutine != null)
            {
                StopCoroutine(buffCoroutine);
            }

            buffCoroutine = StartCoroutine(SpeedBuffRoutine());
        }

        private IEnumerator SpeedBuffRoutine()
        {
            isBuffActive = true;
            playerController.MoveSpeed = baseMoveSpeed * currentMultiplier;
            Debug.Log($"[PlayerSpeedBuff] Prędkość gracza zmieniona z {baseMoveSpeed} na {playerController.MoveSpeed}");

            while (remainingDuration > 0)
            {
                remainingDuration -= Time.deltaTime;
                yield return null;
            }

            // Przywrócenie bazowej prędkości
            ResetBuff();
        }

        public void ResetBuff()
        {
            if (buffCoroutine != null)
            {
                StopCoroutine(buffCoroutine);
                buffCoroutine = null;
            }

            if (isBuffActive && playerController != null)
            {
                playerController.MoveSpeed = baseMoveSpeed;
                Debug.Log($"[PlayerSpeedBuff] Buff wygasł. Prędkość przywrócona do bazowej: {baseMoveSpeed}");
            }

            isBuffActive = false;
            currentMultiplier = 1f;
            remainingDuration = 0f;
        }

        private void OnDisable()
        {
            ResetBuff();
        }
    }
}
