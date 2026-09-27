using UnityEngine;
using UnityEngine.InputSystem;

namespace Praktyki.EQ
{
    public class MinecraftEQAutoBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            bool iPressed = Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame;

            if (iPressed)
            {
                if (MinecraftInventoryUI.Instance == null)
                {
                    Debug.Log("<color=#55FFFF>[Minecraft EQ] Wykryto kliknięcie [I] - automatyczne tworzenie UI w Canvasie...</color>");
                    EnsureUIExistsAndOpen();
                }
            }
        }

        public static void EnsureUIExistsAndOpen()
        {
            if (MinecraftInventoryUI.Instance == null)
            {
                MinecraftUIBuilder.BuildMinecraftUI(null);
            }

            if (MinecraftInventoryUI.Instance != null)
            {
                MinecraftInventoryUI.Instance.OpenInventory();
            }
        }
    }
}
