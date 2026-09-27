using UnityEngine;

namespace Praktyki.OtwieranieZamkow
{
    [CreateAssetMenu(fileName = "Wytrych", menuName = "Inventory/Wytrych")]
    public class LockpickItem : Item
    {
        private void Reset()
        {
            itemName = "Wytrych";
            isStackable = true;
            maxStack = 64;
            description = "Smukły, stalowy wytrych służący do otwierania zamków i kłódek. Wymaga wprawnej ręki – nieudana próba może go złamać!";
        }
    }
}
