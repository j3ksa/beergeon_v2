using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BlackJack
{
    public class CardUI : MonoBehaviour
    {
        [Header("Root Panels")]
        [SerializeField] private GameObject frontPanel;
        [SerializeField] private GameObject backPanel;

        [Header("Card Front Elements")]
        [SerializeField] private Image cardBackground;
        [SerializeField] private TextMeshProUGUI topLeftText;
        [SerializeField] private TextMeshProUGUI centerSymbolText;
        [SerializeField] private TextMeshProUGUI bottomRightText;

        [Header("Colors")]
        [SerializeField] private Color redSuitColor = new Color(0.85f, 0.15f, 0.15f, 1f);
        [SerializeField] private Color blackSuitColor = new Color(0.12f, 0.12f, 0.12f, 1f);
        [SerializeField] private Color cardFrontColor = new Color(0.98f, 0.97f, 0.94f, 1f);

        public Card CurrentCard { get; private set; }
        public bool IsFaceUp { get; private set; }

        private void Awake()
        {
            AutoAssignReferences();
        }

        private void AutoAssignReferences()
        {
            if (frontPanel == null)
            {
                Transform front = transform.Find("Front");
                if (front != null) frontPanel = front.gameObject;
            }

            if (backPanel == null)
            {
                Transform back = transform.Find("Back");
                if (back != null) backPanel = back.gameObject;
            }

            if (frontPanel != null)
            {
                if (cardBackground == null)
                {
                    cardBackground = frontPanel.GetComponent<Image>();
                }
                if (topLeftText == null)
                {
                    Transform t = frontPanel.transform.Find("TopLeftText");
                    if (t != null) topLeftText = t.GetComponent<TextMeshProUGUI>();
                }
                if (centerSymbolText == null)
                {
                    Transform t = frontPanel.transform.Find("CenterSymbolText");
                    if (t != null) centerSymbolText = t.GetComponent<TextMeshProUGUI>();
                }
                if (bottomRightText == null)
                {
                    Transform t = frontPanel.transform.Find("BottomRightText");
                    if (t != null) bottomRightText = t.GetComponent<TextMeshProUGUI>();
                }
            }
        }

        /// <summary>
        /// Konfiguruje wygląd karty na podstawie figury i koloru.
        /// </summary>
        public void SetCard(Card card, bool faceUp = true)
        {
            CurrentCard = card;
            IsFaceUp = faceUp;

            AutoAssignReferences();

            Color textColor = card.IsRed ? redSuitColor : blackSuitColor;

            string cornerString = $"{card.RankString}\n{card.SuitSymbol}";

            if (topLeftText != null)
            {
                topLeftText.text = cornerString;
                topLeftText.color = textColor;
            }

            if (bottomRightText != null)
            {
                bottomRightText.text = cornerString;
                bottomRightText.color = textColor;
            }

            if (centerSymbolText != null)
            {
                centerSymbolText.text = card.SuitSymbol;
                centerSymbolText.color = textColor;
            }

            if (cardBackground != null)
            {
                cardBackground.color = cardFrontColor;
            }

            ApplyFaceState(faceUp);
        }

        public void Reveal(bool animate = true)
        {
            if (IsFaceUp) return;
            IsFaceUp = true;

            if (animate && gameObject.activeInHierarchy)
            {
                StopAllCoroutines();
                StartCoroutine(FlipRoutine(true));
            }
            else
            {
                ApplyFaceState(true);
            }
        }

        public void Hide(bool animate = true)
        {
            if (!IsFaceUp) return;
            IsFaceUp = false;

            if (animate && gameObject.activeInHierarchy)
            {
                StopAllCoroutines();
                StartCoroutine(FlipRoutine(false));
            }
            else
            {
                ApplyFaceState(false);
            }
        }

        private void ApplyFaceState(bool faceUp)
        {
            if (frontPanel != null) frontPanel.SetActive(faceUp);
            if (backPanel != null) backPanel.SetActive(!faceUp);
            transform.localScale = Vector3.one;
        }

        private IEnumerator FlipRoutine(bool targetFaceUp)
        {
            float duration = 0.2f;
            float elapsed = 0f;

            // Pierwsza faza: kurczenie w osi X do 0
            while (elapsed < duration / 2f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / (duration / 2f);
                transform.localScale = new Vector3(Mathf.Lerp(1f, 0f, t), 1f, 1f);
                yield return null;
            }

            // Przełączenie strony w połowie obrotu
            if (frontPanel != null) frontPanel.SetActive(targetFaceUp);
            if (backPanel != null) backPanel.SetActive(!targetFaceUp);

            // Druga faza: rozszerzanie z 0 do 1
            elapsed = 0f;
            while (elapsed < duration / 2f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / (duration / 2f);
                transform.localScale = new Vector3(Mathf.Lerp(0f, 1f, t), 1f, 1f);
                yield return null;
            }

            transform.localScale = Vector3.one;
        }
    }
}
