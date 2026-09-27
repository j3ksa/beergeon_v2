using UnityEngine;
using UnityEngine.InputSystem;

namespace Praktyki.ZatrutyMiecz
{
    /// <summary>
    /// Skrypt pomocniczy ułatwiający natychmiastowe przetestowanie Zatrutego Miecza w grze.
    /// Wystarczy dodać go do dowolnego obiektu na scenie (lub gracza).
    /// Klawisz skrótu:
    /// [Z] - natychmiast dodaje Zatruty Miecz do ekwipunku gracza!
    /// </summary>
    public class ZatrutyMieczTestHelper : MonoBehaviour
    {
        [Header("Testowy Przedmiot")]
        [SerializeField] private Item zatrutyMieczItem;

        private void Start()
        {
            if (zatrutyMieczItem == null)
            {
                zatrutyMieczItem = Resources.Load<Item>("Items/ZatrutyMiecz");
            }
        }

        private void Update()
        {
            if (Keyboard.current == null || Inventory.instance == null) return;

            // Klawisz Z - dodaj Zatruty Miecz
            if (Keyboard.current.zKey.wasPressedThisFrame)
            {
                if (zatrutyMieczItem == null)
                {
                    zatrutyMieczItem = Resources.Load<Item>("Items/ZatrutyMiecz");
                }

                if (zatrutyMieczItem != null)
                {
                    bool added = Inventory.instance.Add(zatrutyMieczItem);
                    if (added)
                    {
                        Debug.Log("<color=#39FF14><b>[ZatrutyMieczTestHelper]</b> Pomyślnie dodano Zatruty Miecz do ekwipunku (Klawisz Z)!</color>");
                    }
                    else
                    {
                        Debug.LogWarning("[ZatrutyMieczTestHelper] Ekwipunek jest pełny!");
                    }
                }
                else
                {
                    Debug.LogError("[ZatrutyMieczTestHelper] Nie znaleziono assetu Items/ZatrutyMiecz w folderze Resources!");
                }
            }
        }
    }
}
