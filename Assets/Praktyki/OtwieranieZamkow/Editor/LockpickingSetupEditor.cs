#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Praktyki.OtwieranieZamkow.Editor
{
    public static class LockpickingSetupEditor
    {
        [MenuItem("Tools/Otwieranie Zamków/1. Skonfiguruj Kłódkę (Zaznacz obiekt Kłódki)")]
        public static void SetupSelectedPadlock()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                // Szukamy obiektu o nazwie "klodka" w hierarchii
                selected = GameObject.Find("klodka");
                if (selected == null)
                {
                    EditorUtility.DisplayDialog("Brak zaznaczenia", "Zaznacz w oknie Hierarchy obiekt kłódki!", "OK");
                    return;
                }
            }

            Undo.RegisterFullObjectHierarchyUndo(selected, "Konfiguracja Klodki");

            // 1. Collider
            Collider col = selected.GetComponent<Collider>();
            if (col == null)
            {
                MeshFilter mf = selected.GetComponentInChildren<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    MeshCollider mc = Undo.AddComponent<MeshCollider>(selected);
                    mc.convex = true;
                    mc.sharedMesh = mf.sharedMesh;
                }
                else
                {
                    BoxCollider bc = Undo.AddComponent<BoxCollider>(selected);
                    bc.size = new Vector3(2f, 3f, 1f);
                }
            }

            // 2. PadlockController
            PadlockController padController = selected.GetComponent<PadlockController>();
            if (padController == null)
            {
                padController = Undo.AddComponent<PadlockController>(selected);
            }

            // 3. Przypisanie przedmiotu Wytrych
            Item wytrychItem = Resources.Load<Item>("Items/Wytrych");
            if (wytrychItem == null) wytrychItem = AssetDatabase.LoadAssetAtPath<Item>("Assets/Praktyki/OtwieranieZamkow/Wytrych.asset");

            SerializedObject sObj = new SerializedObject(padController);
            if (wytrychItem != null)
            {
                SerializedProperty itemProp = sObj.FindProperty("lockpickItem");
                if (itemProp != null) itemProp.objectReferenceValue = wytrychItem;
            }

            // Szukanie drzwi w scenie
            DoorController door = selected.GetComponentInParent<DoorController>();
            if (door == null)
            {
                DoorController[] allDoors = Object.FindObjectsByType<DoorController>(FindObjectsSortMode.None);
                float bestDist = 5f;
                foreach (var d in allDoors)
                {
                    float dist = Vector3.Distance(selected.transform.position, d.transform.position);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        door = d;
                    }
                }
            }

            if (door != null)
            {
                SerializedProperty doorProp = sObj.FindProperty("targetDoor");
                if (doorProp != null) doorProp.objectReferenceValue = door;
                door.SetLocked(true);
                EditorUtility.SetDirty(door);
            }

            sObj.ApplyModifiedProperties();
            EditorUtility.SetDirty(selected);

            EditorUtility.DisplayDialog("Sukces!", 
                $"Kłódka '{selected.name}' została pomyślnie skonfigurowana!\n\n" +
                $"- Posiada Collider\n" +
                $"- Posiada PadlockController\n" +
                $"- Przypisano drzwi: {(door != null ? door.gameObject.name : "Nie znaleziono w pobliżu, przypisz ręcznie w Inspectorze")}\n" +
                $"- Drzwi zostały zablokowane!", "Super!");
        }

        [MenuItem("Tools/Otwieranie Zamków/2. Dodaj Wytrych (Pickup) obok gracza")]
        public static void SpawnLockpickPickup()
        {
            GameObject prefab = Resources.Load<GameObject>("Items/WytrychPickup");
            if (prefab == null) prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Praktyki/OtwieranieZamkow/WytrychPickup.prefab");

            if (prefab == null)
            {
                EditorUtility.DisplayDialog("Błąd", "Nie znaleziono prefabu WytrychPickup!", "OK");
                return;
            }

            Vector3 spawnPos = Vector3.zero;
            Camera sceneCam = SceneView.lastActiveSceneView != null ? SceneView.lastActiveSceneView.camera : null;
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                spawnPos = player.transform.position + player.transform.forward * 1.5f + Vector3.up * 0.5f;
            }
            else if (sceneCam != null)
            {
                spawnPos = sceneCam.transform.position + sceneCam.transform.forward * 3f;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = spawnPos;
            Undo.RegisterCreatedObjectUndo(instance, "Stworzenie WytrychPickup");
            Selection.activeGameObject = instance;

            Debug.Log($"<color=#32CD32>[LockpickingSetupEditor]</color> WytrychPickup utworzony na pozycji: {spawnPos}");
        }
    }
}
#endif
