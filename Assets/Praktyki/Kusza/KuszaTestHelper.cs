using UnityEngine;
using UnityEngine.InputSystem;

namespace Praktyki.Kusza
{
    /// <summary>
    /// Skrypt ułatwiający natychmiastowe testowanie kuszy i strzał w grze.
    /// Wystarczy wrzucić na dowolny obiekt na scenie lub gracz może użyć klawiszy skrótu:
    /// K - dodaje kuszę do ekwipunku
    /// L - dodaje 10 sztuk strzał do ekwipunku (test stackowania)
    /// </summary>
    public class KuszaTestHelper : MonoBehaviour
    {
        [Header("Testowe Assety")]
        [SerializeField] private Item kuszaItem;
        [SerializeField] private Item strzalaItem;

        [Header("Konfiguracja")]
        [SerializeField] private int arrowsToAdd = 10;

        private void Start()
        {
            // Automatyczne załadowanie z Resources jeśli nie przypisano
            if (kuszaItem == null) kuszaItem = Resources.Load<Item>("Items/Kusza");
            if (strzalaItem == null) strzalaItem = Resources.Load<Item>("Items/Strzala");
        }

        private void Update()
        {
            if (Keyboard.current == null || Inventory.instance == null) return;

            // Klawisz K - dodaj Kuszę
            if (Keyboard.current.kKey.wasPressedThisFrame)
            {
                if (kuszaItem == null) kuszaItem = Resources.Load<Item>("Items/Kusza");
                if (kuszaItem != null)
                {
                    Inventory.instance.Add(kuszaItem);
                    Debug.Log("<color=#00FFFF>[KuszaTestHelper] Dodano Kuszę do ekwipunku (Klawisz K)!</color>");
                }
            }

            // Klawisz L - dodaj 10 Strzał (test stackowania)
            if (Keyboard.current.lKey.wasPressedThisFrame)
            {
                if (strzalaItem == null) strzalaItem = Resources.Load<Item>("Items/Strzala");
                if (strzalaItem != null)
                {
                    Inventory.instance.Add(strzalaItem, arrowsToAdd);
                    Debug.Log($"<color=#00FFFF>[KuszaTestHelper] Dodano {arrowsToAdd}x Strzała do ekwipunku (Klawisz L)!</color>");
                }
            }
        }
    }
}
