using UnityEngine;

namespace Praktyki.Kusza
{
    [CreateAssetMenu(fileName = "Strzala", menuName = "Inventory/Kusza/Strzala")]
    public class ArrowItem : Item
    {
        [Header("Parametry Strzały")]
        [Tooltip("Dodatkowe obrażenia dodawane przez ten typ strzały")]
        public int bonusDamage = 0;

        private void Reset()
        {
            itemName = "Strzala";
            isStackable = true;
            maxStack = 64;
            description = "Ostra strzała do kuszy. Można ją załadować klikając PPM z kuszą w dłoni.";
        }
    }
}
