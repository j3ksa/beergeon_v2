using UnityEngine;
using UnityEngine.InputSystem;

namespace Praktyki.MlotekDoKotletow
{
    /// <summary>
    /// Skrypt pomocniczy do błyskawicznego testowania Młotka Do Kotletów w Play Mode.
    /// Klawisz skrótu:
    /// [M] - natychmiast dodaje Młotek Do Kotletów do ekwipunku gracza!
    /// </summary>
    public class MlotekDoKotletowTestHelper : MonoBehaviour
    {
        [Header("Testowy Przedmiot")]
        [SerializeField] private Item mlotekItem;

        private void Start()
        {
            if (mlotekItem == null)
            {
                mlotekItem = Resources.Load<Item>("Items/MlotekDoKotletow");
            }
        }

        private void Update()
        {
            if (Keyboard.current == null || Inventory.instance == null) return;

            // Klawisz M - dodaj Młotek Do Kotletów
            if (Keyboard.current.mKey.wasPressedThisFrame)
            {
                if (mlotekItem == null)
                {
                    mlotekItem = Resources.Load<Item>("Items/MlotekDoKotletow");
                }

                if (mlotekItem != null)
                {
                    bool added = Inventory.instance.Add(mlotekItem);
                    if (added)
                    {
                        Debug.Log("<color=#39FF14><b>[MlotekDoKotletowTestHelper]</b> Pomyślnie dodano Młotek Do Kotletów do ekwipunku (Klawisz M)!</color>");
                    }
                    else
                    {
                        Debug.LogWarning("[MlotekDoKotletowTestHelper] Ekwipunek jest pełny!");
                    }
                }
                else
                {
                    Debug.LogError("[MlotekDoKotletowTestHelper] Nie znaleziono assetu Items/MlotekDoKotletow w folderze Resources!");
                }
            }
        }
    }
}
