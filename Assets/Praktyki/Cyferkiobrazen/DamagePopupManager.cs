using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DamageNumbers
{
    /// <summary>
    /// Globalny menedżer i pula obiektów (Object Pool) dla unoszących się cyferek obrażeń.
    /// Zapewnia brak alokacji pamięci (GC) podczas dynamicznej walki oraz proste statyczne API.
    /// </summary>
    public class DamagePopupManager : MonoBehaviour
    {
        public static DamagePopupManager Instance { get; private set; }

        [Header("Prefab & Font")]
        [Tooltip("Prefab zawierający komponent DamagePopup i TextMeshPro. Jeśli pusty, zostanie utworzony automatycznie.")]
        [SerializeField] private GameObject popupPrefab;
        [Tooltip("Domyślna czcionka TMP (np. Ibarra Real Nova SDF).")]
        [SerializeField] private TMP_FontAsset fontAsset;

        [Header("Thresholds")]
        [Tooltip("Wartość obrażeń, od której trafienie jest automatycznie traktowane jako krytyk (chyba że to DoT).")]
        [SerializeField] private int critDamageThreshold = 25;

        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(1f, 0.95f, 0.6f, 1f);      // Jasne złoto / ciepły biały
        [SerializeField] private Color critColor = new Color(1f, 0.25f, 0.08f, 1f);       // Ognisty cynober / pomarańczowo-czerwony
        [SerializeField] private Color poisonColor = new Color(0.2f, 1f, 0.35f, 1f);      // Jadowita zieleń
        [SerializeField] private Color bleedColor = new Color(0.88f, 0.08f, 0.15f, 1f);    // Karmazynowa krew
        [SerializeField] private Color healColor = new Color(0.1f, 0.9f, 1f, 1f);         // Kojący błękit

        [Header("Font Sizes")]
        [SerializeField] private float normalFontSize = 2.4f;
        [SerializeField] private float critFontSize = 3.2f;
        [SerializeField] private float dotFontSize = 1.9f;

        [Header("Pooling")]
        [SerializeField] private int initialPoolSize = 25;

        private readonly Queue<DamagePopup> pool = new Queue<DamagePopup>();
        private Transform poolContainer;

        public int CritThreshold => critDamageThreshold;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            poolContainer = new GameObject("PopupPool").transform;
            poolContainer.SetParent(transform);

            // Automatyczne wczytanie czcionki z Resources, jeśli nie podano w Inspektorze
            if (fontAsset == null)
            {
                fontAsset = Resources.Load<TMP_FontAsset>("IbarraRealNova-VariableFont_wght SDF");
            }

            PrewarmPool();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void PrewarmPool()
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                DamagePopup popup = CreateNewPopupInstance();
                popup.gameObject.SetActive(false);
                pool.Enqueue(popup);
            }
        }

        private DamagePopup CreateNewPopupInstance()
        {
            GameObject obj;
            if (popupPrefab != null)
            {
                obj = Instantiate(popupPrefab, poolContainer);
            }
            else
            {
                // Dynamiczny fallback w razie braku przypisanego prefaba
                obj = new GameObject("DamagePopup_Auto");
                obj.transform.SetParent(poolContainer);
                var textMesh = obj.AddComponent<TextMeshPro>();
                textMesh.alignment = TextAlignmentOptions.Center;
                textMesh.raycastTarget = false;
                if (fontAsset != null) textMesh.font = fontAsset;
                obj.AddComponent<DamagePopup>();
            }

            var popupComponent = obj.GetComponent<DamagePopup>();
            if (popupComponent == null)
            {
                popupComponent = obj.AddComponent<DamagePopup>();
            }

            return popupComponent;
        }

        /// <summary>
        /// Główna metoda statyczna wywoływana z dowolnego miejsca w kodzie.
        /// </summary>
        public static void Show(Vector3 worldPosition, int damage, DamageType? explicitType = null, bool? explicitCrit = null)
        {
            if (Instance == null)
            {
                // Próba znalezienia w scenie
                Instance = FindFirstObjectByType<DamagePopupManager>();
                if (Instance == null)
                {
                    GameObject managerObj = new GameObject("[DamagePopupManager]");
                    Instance = managerObj.AddComponent<DamagePopupManager>();
                }
            }

            Instance.Spawn(worldPosition, damage, explicitType, explicitCrit);
        }

        public void Spawn(Vector3 worldPosition, int damage, DamageType? explicitType = null, bool? explicitCrit = null)
        {
            if (damage <= 0) return;

            DamageType resolvedType;
            bool resolvedCrit;

            if (explicitType.HasValue)
            {
                resolvedType = explicitType.Value;
                resolvedCrit = explicitCrit.HasValue ? explicitCrit.Value : (resolvedType == DamageType.Critical);
            }
            else
            {
                var contextResult = DamagePopupContext.Consume(damage, critDamageThreshold);
                resolvedType = contextResult.damageType;
                resolvedCrit = contextResult.isCrit;
            }

            // Dobór koloru i rozmiaru
            Color textColor;
            float fontSize;

            switch (resolvedType)
            {
                case DamageType.Critical:
                    textColor = critColor;
                    fontSize = critFontSize;
                    break;
                case DamageType.Poison:
                    textColor = poisonColor;
                    fontSize = dotFontSize;
                    break;
                case DamageType.Bleed:
                    textColor = bleedColor;
                    fontSize = dotFontSize;
                    break;
                case DamageType.Heal:
                    textColor = healColor;
                    fontSize = normalFontSize;
                    break;
                case DamageType.Normal:
                default:
                    if (resolvedCrit)
                    {
                        textColor = critColor;
                        fontSize = critFontSize;
                    }
                    else
                    {
                        textColor = normalColor;
                        fontSize = normalFontSize;
                    }
                    break;
            }

            // Pobranie z puli
            DamagePopup popup;
            if (pool.Count > 0)
            {
                popup = pool.Dequeue();
            }
            else
            {
                popup = CreateNewPopupInstance();
            }

            popup.transform.position = worldPosition;
            popup.gameObject.SetActive(true);
            popup.Setup(damage, resolvedType, resolvedCrit, textColor, fontSize, fontAsset);
        }

        public void ReturnToPool(DamagePopup popup)
        {
            if (popup == null) return;

            popup.gameObject.SetActive(false);
            popup.transform.SetParent(poolContainer);
            pool.Enqueue(popup);
        }
    }
}
