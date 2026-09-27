using UnityEngine;
using UnityEditor;

namespace Praktyki.EQ.Editor
{
    public static class MinecraftUIBuilderMenu
    {
        [MenuItem("Beergeon/Stwórz Minecraft EQ UI w Scenie")]
        public static void CreateMinecraftUIInCanvas()
        {
            MinecraftTextureGenerator.EnsureAssetsGenerated();

            GameObject root = MinecraftUIBuilder.BuildMinecraftUI(null);
            Selection.activeGameObject = root;
            Undo.RegisterCreatedObjectUndo(root, "Stworzenie Minecraft EQ UI");

            EditorUtility.DisplayDialog("Sukces!", "Pomyślnie wygenerowano kompletny Ekwipunek Minecrafta w Hierarchii!\n• 4 Sloty na Zbroję (Hełm, Klata, Spodnie, Buty)\n• Podgląd postaci (Sylwetka)\n• Panel szczegółowych statystyk gracza (HP, Stamina, Pancerz, Siła, Kapsle)\n• 36 Slotów (Hotbar + Plecak)\n• Kosz do wyrzucania [Q]\n• Narzędzie Tooltip i retro dźwięki Audio\n\nWciśnij 'I' podczas gry, aby otworzyć!", "Super!");
        }
    }
}
