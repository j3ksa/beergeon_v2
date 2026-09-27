using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Praktyki.StaryHmielnik
{
    public enum StaryHmielnikStyl
    {
        [InspectorName("Spleśniały Brąz (Zgniły stary chmiel)")]
        SplesnialyBraz,

        [InspectorName("Szara Pleśń (Grzybicza, zakurzona)")]
        SzaraPlesn,

        [InspectorName("Zeschły Brąz (Wysuszony, stary chmiel)")]
        ZeschlyBraz,

        [InspectorName("Ciemna Zgnilizna (Gnijący, mroczny)")]
        CiemnaZgnilizna,

        [InspectorName("Własne Kolory (Custom)")]
        Wlasny,

        [InspectorName("Oryginalny (Świeży, jasny zielony)")]
        Oryginalny
    }

    /// <summary>
    /// Zarządza wyglądem i barwą Starego Chmielnika.
    /// Umożliwia natychmiastową zmianę kolorów wszystkich części ciała (chmielowa głowa, korpus, kończyny)
    /// z poziomu inspektora jednym kliknięciem lub precyzyjnymi suwakami.
    /// Działa w czasie rzeczywistym w trybie edycji (Scene View) i podczas rozgrywki.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class StaryHmielnikVisuals : MonoBehaviour
    {
        [Header("Wybór Stylu / Presetu")]
        [Tooltip("Wybierz gotowy schemat kolorystyczny dla starego chmielnika")]
        [SerializeField] private StaryHmielnikStyl styl = StaryHmielnikStyl.SplesnialyBraz;

        [Header("Precyzyjne Kolory")]
        [Tooltip("Barwa chmielowej czupryny/liści na głowie (Warstwa 12)")]
        [SerializeField] private Color kolorGlowy = new Color(0.62f, 0.52f, 0.36f, 1f);

        [Tooltip("Barwa tułowia/kubraka (Warstwa 11_2)")]
        [SerializeField] private Color kolorKorpusu = new Color(0.58f, 0.48f, 0.34f, 1f);

        [Tooltip("Barwa ramion (Warstwa 11, Warstwa 11_1)")]
        [SerializeField] private Color kolorRamion = new Color(0.56f, 0.50f, 0.40f, 1f);

        [Tooltip("Barwa nóg (Warstwa 10, Warstwa 10_1)")]
        [SerializeField] private Color kolorNog = new Color(0.52f, 0.46f, 0.36f, 1f);

        // Zapamiętany ostatni styl, by wykrywać zmianę enuma w OnValidate
        [SerializeField, HideInInspector] private StaryHmielnikStyl poprzedniStyl = StaryHmielnikStyl.SplesnialyBraz;

        private void OnValidate()
        {
            if (styl != poprzedniStyl)
            {
                UstawStyl(styl);
                poprzedniStyl = styl;
            }
            else if (styl == StaryHmielnikStyl.Wlasny)
            {
                ZastosujAktualneKolory();
            }
            else
            {
                ZastosujAktualneKolory();
            }
        }

        private void Awake()
        {
            ZastosujAktualneKolory();
        }

        /// <summary>
        /// Ustawia kolory według wybranego presetu.
        /// </summary>
        public void UstawStyl(StaryHmielnikStyl nowyStyl)
        {
            styl = nowyStyl;
            poprzedniStyl = nowyStyl;

            switch (nowyStyl)
            {
                case StaryHmielnikStyl.SplesnialyBraz:
                    // Zrównoważony, zgniły spleśniały brąz - gaszone zielenie z tonami butwienia
                    kolorGlowy = new Color(0.62f, 0.52f, 0.36f, 1f);
                    kolorKorpusu = new Color(0.58f, 0.48f, 0.34f, 1f);
                    kolorRamion = new Color(0.56f, 0.50f, 0.40f, 1f);
                    kolorNog = new Color(0.52f, 0.46f, 0.36f, 1f);
                    break;

                case StaryHmielnikStyl.SzaraPlesn:
                    // Zgaszona, zapleśniała, szarawo-blada patyna pleśniowa
                    kolorGlowy = new Color(0.55f, 0.60f, 0.52f, 1f);
                    kolorKorpusu = new Color(0.52f, 0.55f, 0.48f, 1f);
                    kolorRamion = new Color(0.50f, 0.53f, 0.47f, 1f);
                    kolorNog = new Color(0.48f, 0.50f, 0.45f, 1f);
                    break;

                case StaryHmielnikStyl.ZeschlyBraz:
                    // Cieplejszy, mocno wysuszony, zbrunatniały chmiel (jak zeschłe szyszki)
                    kolorGlowy = new Color(0.70f, 0.52f, 0.32f, 1f);
                    kolorKorpusu = new Color(0.64f, 0.48f, 0.32f, 1f);
                    kolorRamion = new Color(0.60f, 0.48f, 0.35f, 1f);
                    kolorNog = new Color(0.55f, 0.44f, 0.32f, 1f);
                    break;

                case StaryHmielnikStyl.CiemnaZgnilizna:
                    // Mroczna, mocno zbutwiała zgnilizna
                    kolorGlowy = new Color(0.42f, 0.38f, 0.28f, 1f);
                    kolorKorpusu = new Color(0.40f, 0.34f, 0.25f, 1f);
                    kolorRamion = new Color(0.38f, 0.34f, 0.27f, 1f);
                    kolorNog = new Color(0.35f, 0.30f, 0.24f, 1f);
                    break;

                case StaryHmielnikStyl.Oryginalny:
                    // Czysta, świeża biel (oryginalne kolory tekstury chmielnika)
                    kolorGlowy = Color.white;
                    kolorKorpusu = Color.white;
                    kolorRamion = Color.white;
                    kolorNog = Color.white;
                    break;

                case StaryHmielnikStyl.Wlasny:
                    // Zachowaj aktualnie ustawione kolory
                    break;
            }

            ZastosujAktualneKolory();
        }

        /// <summary>
        /// Aplikuje barwy do odpowiednich komponentów SpriteRenderer w hierarchii.
        /// </summary>
        public void ZastosujAktualneKolory()
        {
            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
            if (renderers == null || renderers.Length == 0) return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Undo.RecordObjects(renderers, "Zmień Kolor Starego Chmielnika");
            }
#endif

            foreach (var r in renderers)
            {
                if (r == null) continue;
                string goName = r.gameObject.name;

                if (goName.Contains("Warstwa 12"))
                {
                    // Głowa / liście chmielu (Warstwa 12 oraz Warstwa 12_1)
                    r.color = kolorGlowy;
                }
                else if (goName.Contains("Warstwa 11_2"))
                {
                    // Korpus / kubrak
                    r.color = kolorKorpusu;
                }
                else if (goName.Contains("Warstwa 11"))
                {
                    // Ręce (Warstwa 11, Warstwa 11_1)
                    r.color = kolorRamion;
                }
                else if (goName.Contains("Warstwa 10"))
                {
                    // Nogi (Warstwa 10, Warstwa 10_1)
                    r.color = kolorNog;
                }
                else
                {
                    r.color = kolorKorpusu;
                }

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(r);
                }
#endif
            }
        }

        [ContextMenu("Zastosuj: Spleśniały Brąz")]
        public void UstawSplesnialyBraz() => UstawStyl(StaryHmielnikStyl.SplesnialyBraz);

        [ContextMenu("Zastosuj: Szara Pleśń")]
        public void UstawSzaraPlesn() => UstawStyl(StaryHmielnikStyl.SzaraPlesn);

        [ContextMenu("Zastosuj: Zeschły Brąz")]
        public void UstawZeschlyBraz() => UstawStyl(StaryHmielnikStyl.ZeschlyBraz);

        [ContextMenu("Zastosuj: Ciemna Zgnilizna")]
        public void UstawCiemnaZgnilizna() => UstawStyl(StaryHmielnikStyl.CiemnaZgnilizna);

        [ContextMenu("Zastosuj: Reset do oryginału")]
        public void ResetDoOryginalu() => UstawStyl(StaryHmielnikStyl.Oryginalny);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(StaryHmielnikVisuals))]
    public class StaryHmielnikVisualsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            StaryHmielnikVisuals visuals = (StaryHmielnikVisuals)target;

            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Szybkie Presety Kolorystyczne", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🌿 Spleśniały Brąz"))
            {
                Undo.RecordObject(visuals, "Zmień Styl");
                visuals.UstawStyl(StaryHmielnikStyl.SplesnialyBraz);
                EditorUtility.SetDirty(visuals);
            }
            if (GUILayout.Button("🍄 Szara Pleśń"))
            {
                Undo.RecordObject(visuals, "Zmień Styl");
                visuals.UstawStyl(StaryHmielnikStyl.SzaraPlesn);
                EditorUtility.SetDirty(visuals);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🍂 Zeschły Brąz"))
            {
                Undo.RecordObject(visuals, "Zmień Styl");
                visuals.UstawStyl(StaryHmielnikStyl.ZeschlyBraz);
                EditorUtility.SetDirty(visuals);
            }
            if (GUILayout.Button("🌑 Ciemna Zgnilizna"))
            {
                Undo.RecordObject(visuals, "Zmień Styl");
                visuals.UstawStyl(StaryHmielnikStyl.CiemnaZgnilizna);
                EditorUtility.SetDirty(visuals);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            if (GUILayout.Button("🌱 Reset do Oryginalnego (Świeża zieleń)"))
            {
                Undo.RecordObject(visuals, "Reset Koloru");
                visuals.UstawStyl(StaryHmielnikStyl.Oryginalny);
                EditorUtility.SetDirty(visuals);
            }

            EditorGUILayout.Space(5);
            if (GUILayout.Button("✓ Zastosuj Aktualne Kolory"))
            {
                visuals.ZastosujAktualneKolory();
            }
        }
    }
#endif
}
