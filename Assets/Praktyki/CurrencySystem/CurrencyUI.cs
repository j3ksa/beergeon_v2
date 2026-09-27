using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CurrencySystem
{
    public class CurrencyUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI capsText;
        [SerializeField] private Text fallbackCapsText;
        [SerializeField] private Image capIcon;

        [Header("Player Currency Reference")]
        [SerializeField] private PlayerCurrency playerCurrency;

        private void OnEnable()
        {
            FindPlayerCurrency();
            if (playerCurrency != null)
            {
                playerCurrency.OnCapsChanged.AddListener(UpdateCapsDisplay);
                UpdateCapsDisplay(playerCurrency.CurrentCaps);
            }
        }

        private void OnDisable()
        {
            if (playerCurrency != null)
            {
                playerCurrency.OnCapsChanged.RemoveListener(UpdateCapsDisplay);
            }
        }

        private void Start()
        {
            if (playerCurrency == null)
            {
                FindPlayerCurrency();
            }

            if (playerCurrency != null)
            {
                UpdateCapsDisplay(playerCurrency.CurrentCaps);
            }
        }

        private void FindPlayerCurrency()
        {
            if (playerCurrency != null) return;

            playerCurrency = PlayerCurrency.Instance;
            if (playerCurrency == null)
            {
                playerCurrency = Object.FindFirstObjectByType<PlayerCurrency>();
            }
        }

        public void UpdateCapsDisplay(int capsCount)
        {
            if (capsText != null)
            {
                capsText.text = capsCount.ToString();
            }

            if (fallbackCapsText != null)
            {
                fallbackCapsText.text = capsCount.ToString();
            }
        }

        /// <summary>
        /// Opcjonalna metoda do zmiany ikonki kapsla z poziomu skryptu, jeśli potrzebne.
        /// </summary>
        public void SetCapIcon(Sprite icon)
        {
            if (capIcon != null)
            {
                capIcon.sprite = icon;
            }
        }
    }
}
