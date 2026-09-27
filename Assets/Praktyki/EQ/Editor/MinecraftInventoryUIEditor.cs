using UnityEngine;
using UnityEditor;

namespace Praktyki.EQ.Editor
{
    [CustomEditor(typeof(MinecraftInventoryUI))]
    public class MinecraftInventoryUIEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            MinecraftInventoryUI script = (MinecraftInventoryUI)target;

            GUILayout.Space(15);
            GUI.backgroundColor = new Color(0.2f, 0.85f, 0.4f);
            if (GUILayout.Button("⚡ Wygeneruj / Podłącz UI Minecrafta w Canvasie", GUILayout.Height(38)))
            {
                Canvas canvas = script.GetComponentInParent<Canvas>();
                if (canvas == null) canvas = FindFirstObjectByType<Canvas>();

                if (canvas != null)
                {
                    MinecraftUIBuilderMenu.CreateMinecraftUIInCanvas();
                }
                else
                {
                    EditorUtility.DisplayDialog("Błąd", "Nie znaleziono Canvasa w scenie! Utwórz najpierw Canvas UI.", "OK");
                }
            }

            GUI.backgroundColor = Color.white;
            GUILayout.Space(5);
            EditorGUILayout.HelpBox("Sterowanie Ekwipunkiem Minecrafta:\n• [I] lub [Esc] - Otwórz / Zamknij Ekwipunek\n• LPM - Podnieś stos / Połóż stos / Zamień przedmioty\n• PPM - Podziel stos na pół / Odłóż 1 sztukę\n• Shift + LPM - Szybkie przenoszenie między Hotbarem a Plecakiem\n• 1-9 - Szybka zamiana najechanego slotu z Hotbarem\n• [Q] - Wyrzucenie 1 sztuki do świata gry", MessageType.Info);
        }
    }
}
