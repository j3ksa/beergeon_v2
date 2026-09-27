#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;

namespace DamageNumbers.Editor
{
    public static class DamagePopupEditorUtils
    {
        private const string PrefabPath = "Assets/Praktyki/Cyferkiobrazen/DamagePopup.prefab";
        private const string FontPathPraktyki = "Assets/Praktyki/IbarraRealNova-VariableFont_wght SDF.asset";
        private const string FontPathResources = "Assets/Resources/IbarraRealNova-VariableFont_wght SDF.asset";

        [MenuItem("Tools/Cyferki Obrażeń/1. Utwórz Prefab Cyferek")]
        public static GameObject CreateOrUpdateDamagePopupPrefab()
        {
            // 1. Wczytaj czcionkę Ibarra Real Nova SDF
            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPathPraktyki);
            if (fontAsset == null)
            {
                fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPathResources);
            }

            if (fontAsset == null)
            {
                Debug.LogWarning("[Cyferki Obrażeń] Nie znaleziono pliku czcionki IbarraRealNova SDF pod ścieżką: " + FontPathPraktyki);
            }

            // 2. Stwórz obiekt w pamięci
            GameObject go = new GameObject("DamagePopup");

            var textMesh = go.AddComponent<TextMeshPro>();
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.fontSize = 2.4f;
            textMesh.text = "25";
            textMesh.color = new Color(1f, 0.95f, 0.6f, 1f);
            textMesh.raycastTarget = false;

            if (fontAsset != null)
            {
                textMesh.font = fontAsset;
            }

            // RectTransform / Transform optymalizacje
            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.sizeDelta = new Vector2(2f, 1f);
            }

            var popup = go.AddComponent<DamagePopup>();

            // Ustaw pola prywatne przez SerializedObject
            SerializedObject serializedPopup = new SerializedObject(popup);
            serializedPopup.FindProperty("textMesh").objectReferenceValue = textMesh;
            serializedPopup.ApplyModifiedPropertiesWithoutUndo();

            // 3. Zapisz jako prefab
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
            Object.DestroyImmediate(go);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=#39FF14><b>[Cyferki Obrażeń]</b></color> Prefab utworzony pomyślnie pod ścieżką: {PrefabPath}");
            EditorGUIUtility.PingObject(savedPrefab);

            return savedPrefab;
        }

        [MenuItem("Tools/Cyferki Obrażeń/2. Dodaj Menadżer Cyferek do Sceny")]
        public static void AddManagerToCurrentScene()
        {
            DamagePopupManager existingManager = Object.FindFirstObjectByType<DamagePopupManager>();
            if (existingManager != null)
            {
                Debug.Log("[Cyferki Obrażeń] DamagePopupManager już istnieje na scenie: " + existingManager.gameObject.name);
                Selection.activeGameObject = existingManager.gameObject;
                return;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                prefab = CreateOrUpdateDamagePopupPrefab();
            }

            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPathPraktyki);
            if (fontAsset == null)
            {
                fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPathResources);
            }

            GameObject managerGo = new GameObject("DamagePopupManager");
            DamagePopupManager manager = managerGo.AddComponent<DamagePopupManager>();

            SerializedObject so = new SerializedObject(manager);
            if (prefab != null)
            {
                so.FindProperty("popupPrefab").objectReferenceValue = prefab;
            }
            if (fontAsset != null)
            {
                so.FindProperty("fontAsset").objectReferenceValue = fontAsset;
            }
            so.ApplyModifiedProperties();

            Undo.RegisterCreatedObjectUndo(managerGo, "Utwórz DamagePopupManager");
            Selection.activeGameObject = managerGo;

            Debug.Log("<color=#39FF14><b>[Cyferki Obrażeń]</b></color> DamagePopupManager został dodany do otwartej sceny!");
        }
    }
}
#endif
