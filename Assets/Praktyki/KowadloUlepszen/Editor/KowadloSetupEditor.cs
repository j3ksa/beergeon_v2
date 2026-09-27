#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;

namespace Praktyki.KowadloUlepszen.Editor
{
    public static class KowadloSetupEditor
    {
        [MenuItem("Tools/Kowadło/1. Skonfiguruj Zaznaczony Obiekt jako Kowadło")]
        public static void SetupSelectedAsKowadlo()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Brak zaznaczenia", "Zaznacz w oknie Hierarchy obiekt swojego kowadła (lub sześcian placeholder)!", "OK");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(selected, "Konfiguracja Kowadla");

            // 1. Upewnij się, że obiekt ma odpowiedni Collider dopasowany do geometrii
            MeshFilter mf = selected.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                // Jeśli był stary/błędny BoxCollider, usuwamy go
                BoxCollider oldBox = selected.GetComponent<BoxCollider>();
                if (oldBox != null)
                {
                    Undo.DestroyObjectImmediate(oldBox);
                }

                MeshCollider mc = selected.GetComponent<MeshCollider>();
                if (mc == null)
                {
                    mc = Undo.AddComponent<MeshCollider>(selected);
                }
                mc.convex = true;
                mc.sharedMesh = mf.sharedMesh;
            }
            else
            {
                Collider col = selected.GetComponent<Collider>();
                if (col == null)
                {
                    BoxCollider box = Undo.AddComponent<BoxCollider>(selected);
                    box.size = Vector3.one;
                }
            }

            // 2. Dodaj / skonfiguruj KowadloController
            KowadloController controller = selected.GetComponent<KowadloController>();
            if (controller == null)
            {
                controller = Undo.AddComponent<KowadloController>(selected);
            }

            // 3. Dodaj / skonfiguruj AudioSource
            AudioSource audio = selected.GetComponent<AudioSource>();
            if (audio == null)
            {
                audio = Undo.AddComponent<AudioSource>(selected);
                audio.playOnAwake = false;
                audio.spatialBlend = 0.85f;
                audio.maxDistance = 15f;
            }

            // 4. Stwórz i precyzyjnie wypozycjonuj cząsteczki iskier na szczycie kowadła
            Transform existingSparks = selected.transform.Find("Kowadlo_Sparks");
            ParticleSystem sparksPS = null;
            GameObject sparksObj = null;

            if (existingSparks == null)
            {
                sparksObj = new GameObject("Kowadlo_Sparks");
                sparksObj.transform.SetParent(selected.transform, false);
                sparksPS = sparksObj.AddComponent<ParticleSystem>();
                Undo.RegisterCreatedObjectUndo(sparksObj, "Stworzenie Iskry Kowadla");
            }
            else
            {
                sparksObj = existingSparks.gameObject;
                sparksPS = sparksObj.GetComponent<ParticleSystem>();
                if (sparksPS == null) sparksPS = sparksObj.AddComponent<ParticleSystem>();
            }

            // Ustawienie iskier na szczycie modelu w przestrzeni świata i skierowanie w górę
            Renderer rend = selected.GetComponentInChildren<Renderer>();
            Vector3 topPos = (rend != null)
                ? new Vector3(rend.bounds.center.x, rend.bounds.max.y + 0.05f, rend.bounds.center.z)
                : selected.transform.position + Vector3.up * 0.55f;

            sparksObj.transform.position = topPos;
            sparksObj.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);

            // Kompensacja skali rodzica (np. przeskalowanie x53), by iskry miały naturalny rozmiar 1:1
            Vector3 parentLossy = selected.transform.lossyScale;
            if (Mathf.Abs(parentLossy.x) > 0.001f && Mathf.Abs(parentLossy.y) > 0.001f && Mathf.Abs(parentLossy.z) > 0.001f)
            {
                sparksObj.transform.localScale = new Vector3(
                    1f / Mathf.Abs(parentLossy.x),
                    1f / Mathf.Abs(parentLossy.y),
                    1f / Mathf.Abs(parentLossy.z)
                );
            }
            else
            {
                sparksObj.transform.localScale = Vector3.one;
            }

            if (sparksPS != null)
            {
                KowadloController.ConfigureStarParticleSystem(sparksPS);
            }

            // Przypisz cząsteczki do kontrolera przez SerializedObject
            SerializedObject sObj = new SerializedObject(controller);
            SerializedProperty psProp = sObj.FindProperty("sparksParticleSystem");
            if (psProp != null && sparksPS != null)
            {
                psProp.objectReferenceValue = sparksPS;
            }
            sObj.ApplyModifiedProperties();

            // 5. Upewnij się, że gracz ma WeaponUpgradeManager
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                if (player.GetComponent<WeaponUpgradeManager>() == null)
                {
                    Undo.AddComponent<WeaponUpgradeManager>(player);
                }
            }

            EditorUtility.SetDirty(selected);
            EditorUtility.DisplayDialog("Sukces!", $"Obiekt '{selected.name}' został pomyślnie skonfigurowany jako Kowadło Ulepszeń!\n\nPosiada Collider, AudioSource, Iskry oraz KowadloController.", "Gotowe");
        }
    }
}
#endif
