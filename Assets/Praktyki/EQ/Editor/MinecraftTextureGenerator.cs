using System.IO;
using UnityEngine;
using UnityEditor;
using Praktyki.Zbroja;

namespace Praktyki.EQ.Editor
{
    [InitializeOnLoad]
    public static class MinecraftTextureGenerator
    {
        static MinecraftTextureGenerator()
        {
            EditorApplication.delayCall += EnsureAssetsGenerated;
        }

        [MenuItem("Beergeon/Wygeneruj Tekstury i Zbroje Minecrafta")]
        public static void EnsureAssetsGenerated()
        {
            string texDir = "Assets/Praktyki/EQ/Textures";
            string armorDir = "Assets/Praktyki/Zbroja/Graphics";

            if (!Directory.Exists(texDir)) Directory.CreateDirectory(texDir);
            if (!Directory.Exists(armorDir)) Directory.CreateDirectory(armorDir);

            // 1. Panel Minecraft (9-sliceable 32x32)
            string panelPath = $"{texDir}/minecraft_panel.png";
            if (!File.Exists(panelPath))
            {
                Texture2D panelTex = CreatePanelTexture(32, 32);
                SaveAndImportTexture(panelTex, panelPath, new Vector4(4, 4, 4, 4));
            }

            // 2. Slot Minecraft (36x36)
            string slotPath = $"{texDir}/minecraft_slot.png";
            if (!File.Exists(slotPath))
            {
                Texture2D slotTex = CreateSlotTexture(36, 36);
                SaveAndImportTexture(slotTex, slotPath, Vector4.zero);
            }

            // 3. Slot Aktywny (36x36)
            string slotActivePath = $"{texDir}/minecraft_slot_active.png";
            if (!File.Exists(slotActivePath))
            {
                Texture2D activeTex = CreateActiveSlotTexture(36, 36);
                SaveAndImportTexture(activeTex, slotActivePath, Vector4.zero);
            }

            // 4. Silwetki slotów zbroi
            GenerateArmorSilhouette(36, 36, $"{texDir}/minecraft_armor_helmet.png", ArmorSlotType.Helmet);
            GenerateArmorSilhouette(36, 36, $"{texDir}/minecraft_armor_chestplate.png", ArmorSlotType.Chestplate);
            GenerateArmorSilhouette(36, 36, $"{texDir}/minecraft_armor_leggings.png", ArmorSlotType.Leggings);
            GenerateArmorSilhouette(36, 36, $"{texDir}/minecraft_armor_boots.png", ArmorSlotType.Boots);

            // 5. Silwetka gracza i kosz
            string playerPath = $"{texDir}/minecraft_player_silhouette.png";
            if (!File.Exists(playerPath))
            {
                Texture2D playerTex = CreatePlayerSilhouetteTexture(64, 120);
                SaveAndImportTexture(playerTex, playerPath, Vector4.zero);
            }

            string trashPath = $"{texDir}/minecraft_trash.png";
            if (!File.Exists(trashPath))
            {
                Texture2D trashTex = CreateTrashTexture(36, 36);
                SaveAndImportTexture(trashTex, trashPath, Vector4.zero);
            }

            // 6. Grafiki elementów zbroi
            string helmTexPath = $"{armorDir}/helm_zelazny.png";
            string klataTexPath = $"{armorDir}/klata_zelazna.png";
            string spodnieTexPath = $"{armorDir}/spodnie_zelazne.png";
            string butyTexPath = $"{armorDir}/buty_zelazne.png";

            GenerateArmorItemGraphic(32, 32, helmTexPath, ArmorSlotType.Helmet);
            GenerateArmorItemGraphic(32, 32, klataTexPath, ArmorSlotType.Chestplate);
            GenerateArmorItemGraphic(32, 32, spodnieTexPath, ArmorSlotType.Leggings);
            GenerateArmorItemGraphic(32, 32, butyTexPath, ArmorSlotType.Boots);

            // 7. Tworzenie ScriptableObjectów zbroi jeśli nie istnieją
            CreateArmorAsset("ZelaznyHelm", "Żelazny Hełm", ArmorSlotType.Helmet, 2, 10, 0f, helmTexPath);
            CreateArmorAsset("ZelaznaKlata", "Żelazny Napierśnik", ArmorSlotType.Chestplate, 5, 25, -0.02f, klataTexPath);
            CreateArmorAsset("ZelazneSpodnie", "Żelazne Nogawice", ArmorSlotType.Leggings, 3, 15, 0f, spodnieTexPath);
            CreateArmorAsset("ZelazneButy", "Żelazne Buty", ArmorSlotType.Boots, 2, 5, 0.05f, butyTexPath);

            AssetDatabase.Refresh();
            Debug.Log("<color=#55FF55>[Minecraft EQ] Pomyślnie zweryfikowano i wygenerowano grafiki oraz przedmioty zbroi!</color>");
        }

        private static void CreateArmorAsset(string assetFileName, string displayName, ArmorSlotType slotType, int defense, int bonusHp, float speedBonus, string spritePath)
        {
            string assetPath = $"Assets/Praktyki/Zbroja/{assetFileName}.asset";
            if (File.Exists(assetPath)) return;

            ArmorItem armor = ScriptableObject.CreateInstance<ArmorItem>();
            armor.itemName = displayName;
            armor.slotType = slotType;
            armor.defense = defense;
            armor.bonusHealth = bonusHp;
            armor.speedBonus = speedBonus;
            armor.description = $"Klasyczna zbroja wykuta z żelaza.\n+ {defense} Obrony | + {bonusHp} Max HP";
            armor.isStackable = false;
            armor.maxStack = 1;

            Sprite spr = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (spr != null)
            {
                armor.icon = spr;
                armor.heldSprite = spr;
            }

            AssetDatabase.CreateAsset(armor, assetPath);
            AssetDatabase.SaveAssets();
        }

        private static void SaveAndImportTexture(Texture2D tex, string path, Vector4 border)
        {
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 32;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true;
                if (border != Vector4.zero)
                {
                    importer.spriteBorder = border;
                }
                importer.SaveAndReimport();
            }
        }

        private static Texture2D CreatePanelTexture(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color bg = new Color(0.776f, 0.776f, 0.776f, 1f); // #C6C6C6
            Color white = new Color(1f, 1f, 1f, 1f);
            Color dark = new Color(0.333f, 0.333f, 0.333f, 1f); // #555555
            Color black = new Color(0f, 0f, 0f, 1f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (x == 0 || y == 0 || x == w - 1 || y == h - 1)
                        tex.SetPixel(x, y, black);
                    else if (x <= 2 || y >= h - 3)
                        tex.SetPixel(x, y, white);
                    else if (x >= w - 3 || y <= 2)
                        tex.SetPixel(x, y, dark);
                    else
                        tex.SetPixel(x, y, bg);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateSlotTexture(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color slotBg = new Color(0.545f, 0.545f, 0.545f, 1f); // #8B8B8B
            Color dark = new Color(0.216f, 0.216f, 0.216f, 1f); // #373737
            Color light = new Color(1f, 1f, 1f, 1f);
            Color black = new Color(0f, 0f, 0f, 1f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (x == 0 || y == h - 1)
                        tex.SetPixel(x, y, black);
                    else if (x <= 2 || y >= h - 3)
                        tex.SetPixel(x, y, dark);
                    else if (x >= w - 2 || y <= 1)
                        tex.SetPixel(x, y, light);
                    else
                        tex.SetPixel(x, y, slotBg);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateActiveSlotTexture(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color gold = new Color(1f, 0.84f, 0f, 1f); // #FFD700
            Color clear = new Color(0f, 0f, 0f, 0f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (x < 3 || x >= w - 3 || y < 3 || y >= h - 3)
                        tex.SetPixel(x, y, gold);
                    else
                        tex.SetPixel(x, y, clear);
                }
            }
            tex.Apply();
            return tex;
        }

        private static void GenerateArmorSilhouette(int w, int h, string path, ArmorSlotType type)
        {
            if (File.Exists(path)) return;

            Texture2D tex = CreateSlotTexture(w, h);
            Color iconColor = new Color(0.35f, 0.35f, 0.35f, 0.75f); // Wgłębiona sylwetka

            int cx = w / 2;
            int cy = h / 2;

            if (type == ArmorSlotType.Helmet)
            {
                for (int y = cy - 6; y <= cy + 6; y++)
                {
                    for (int x = cx - 7; x <= cx + 7; x++)
                    {
                        // Wycięcie na oczy i usta
                        if (y <= cy - 1 && x >= cx - 4 && x <= cx + 4 && y >= cy - 4) continue;
                        if (y >= cy + 5 && (x <= cx - 6 || x >= cx + 6)) continue;
                        tex.SetPixel(x, y, iconColor);
                    }
                }
            }
            else if (type == ArmorSlotType.Chestplate)
            {
                for (int y = cy - 7; y <= cy + 7; y++)
                {
                    for (int x = cx - 8; x <= cx + 8; x++)
                    {
                        // Wycięcie na szyję
                        if (y >= cy + 4 && x >= cx - 3 && x <= cx + 3) continue;
                        // Ramiona i korpus
                        if (y <= cy - 4 && (x <= cx - 5 || x >= cx + 5)) continue;
                        tex.SetPixel(x, y, iconColor);
                    }
                }
            }
            else if (type == ArmorSlotType.Leggings)
            {
                for (int y = cy - 8; y <= cy + 8; y++)
                {
                    for (int x = cx - 7; x <= cx + 7; x++)
                    {
                        // Rozkrok
                        if (y <= cy + 1 && x >= cx - 2 && x <= cx + 2) continue;
                        tex.SetPixel(x, y, iconColor);
                    }
                }
            }
            else if (type == ArmorSlotType.Boots)
            {
                for (int y = cy - 6; y <= cy + 5; y++)
                {
                    for (int x = cx - 7; x <= cx + 7; x++)
                    {
                        // Przerwa między butami
                        if (x >= cx - 1 && x <= cx + 1) continue;
                        // Noski butów
                        if (y <= cy - 3 && (x == cx - 7 || x == cx + 7)) continue;
                        tex.SetPixel(x, y, iconColor);
                    }
                }
            }

            tex.Apply();
            SaveAndImportTexture(tex, path, Vector4.zero);
        }

        private static Texture2D CreatePlayerSilhouetteTexture(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);
            Color body = new Color(0.25f, 0.25f, 0.25f, 1f);
            Color border = new Color(0.15f, 0.15f, 0.15f, 1f);

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    tex.SetPixel(x, y, clear);

            int cx = w / 2;

            // Głowa: 20x20
            DrawBox(tex, cx - 10, h - 30, 20, 20, body, border);
            // Tułów: 22x32
            DrawBox(tex, cx - 11, h - 64, 22, 32, body, border);
            // Lewe ramię: 8x32
            DrawBox(tex, cx - 20, h - 64, 8, 32, body, border);
            // Prawe ramię: 8x32
            DrawBox(tex, cx + 12, h - 64, 8, 32, body, border);
            // Lewa noga: 10x38
            DrawBox(tex, cx - 11, h - 104, 10, 38, body, border);
            // Prawa noga: 10x38
            DrawBox(tex, cx + 1, h - 104, 10, 38, body, border);

            tex.Apply();
            return tex;
        }

        private static Texture2D CreateTrashTexture(int w, int h)
        {
            Texture2D tex = CreateSlotTexture(w, h);
            Color red = new Color(0.85f, 0.3f, 0.2f, 0.9f);
            int cx = w / 2;
            int cy = h / 2;

            // Kubieł kosza
            for (int y = cy - 6; y <= cy + 2; y++)
            {
                for (int x = cx - 5; x <= cx + 5; x++)
                {
                    if (y >= cy - 5 && y <= cy + 1 && x > cx - 5 && x < cx + 5)
                    {
                        if (x == cx - 2 || x == cx + 2) tex.SetPixel(x, y, red);
                        else tex.SetPixel(x, y, new Color(0.3f, 0.1f, 0.1f, 0.8f));
                    }
                    else
                    {
                        tex.SetPixel(x, y, red);
                    }
                }
            }
            // Pokrywa
            for (int x = cx - 7; x <= cx + 7; x++)
            {
                tex.SetPixel(x, cy + 4, red);
            }
            // Uchwyt
            for (int x = cx - 2; x <= cx + 2; x++)
            {
                tex.SetPixel(x, cy + 6, red);
            }

            tex.Apply();
            return tex;
        }

        private static void GenerateArmorItemGraphic(int w, int h, string path, ArmorSlotType type)
        {
            if (File.Exists(path)) return;

            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);
            Color ironLight = new Color(0.92f, 0.92f, 0.95f, 1f);
            Color ironMid = new Color(0.72f, 0.72f, 0.75f, 1f);
            Color ironDark = new Color(0.45f, 0.45f, 0.50f, 1f);
            Color outline = new Color(0.15f, 0.15f, 0.20f, 1f);

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    tex.SetPixel(x, y, clear);

            int cx = w / 2;
            int cy = h / 2;

            if (type == ArmorSlotType.Helmet)
            {
                for (int y = cy - 6; y <= cy + 6; y++)
                {
                    for (int x = cx - 7; x <= cx + 7; x++)
                    {
                        if (y <= cy - 1 && x >= cx - 3 && x <= cx + 3 && y >= cy - 4) continue;
                        if (y == cy + 6 || y == cy - 6 || x == cx - 7 || x == cx + 7)
                            tex.SetPixel(x, y, outline);
                        else if (x <= cx - 3 || y >= cy + 4)
                            tex.SetPixel(x, y, ironLight);
                        else
                            tex.SetPixel(x, y, ironMid);
                    }
                }
            }
            else if (type == ArmorSlotType.Chestplate)
            {
                for (int y = cy - 8; y <= cy + 8; y++)
                {
                    for (int x = cx - 9; x <= cx + 9; x++)
                    {
                        if (y >= cy + 5 && x >= cx - 3 && x <= cx + 3) continue;
                        if (y <= cy - 5 && (x <= cx - 6 || x >= cx + 6)) continue;

                        if (x == cx - 9 || x == cx + 9 || y == cy - 8 || y == cy + 8)
                            tex.SetPixel(x, y, outline);
                        else if (x < cx)
                            tex.SetPixel(x, y, ironLight);
                        else
                            tex.SetPixel(x, y, ironMid);
                    }
                }
            }
            else if (type == ArmorSlotType.Leggings)
            {
                for (int y = cy - 9; y <= cy + 9; y++)
                {
                    for (int x = cx - 8; x <= cx + 8; x++)
                    {
                        if (y <= cy + 1 && x >= cx - 2 && x <= cx + 2) continue;

                        if (x == cx - 8 || x == cx + 8 || y == cy - 9 || y == cy + 9)
                            tex.SetPixel(x, y, outline);
                        else if (x < cx)
                            tex.SetPixel(x, y, ironLight);
                        else
                            tex.SetPixel(x, y, ironMid);
                    }
                }
            }
            else if (type == ArmorSlotType.Boots)
            {
                for (int y = cy - 7; y <= cy + 6; y++)
                {
                    for (int x = cx - 8; x <= cx + 8; x++)
                    {
                        if (x >= cx - 1 && x <= cx + 1) continue;

                        if (x == cx - 8 || x == cx + 8 || y == cy - 7 || y == cy + 6)
                            tex.SetPixel(x, y, outline);
                        else if (y < cy)
                            tex.SetPixel(x, y, ironLight);
                        else
                            tex.SetPixel(x, y, ironMid);
                    }
                }
            }

            tex.Apply();
            SaveAndImportTexture(tex, path, Vector4.zero);
        }

        private static void DrawBox(Texture2D tex, int x, int y, int w, int h, Color fill, Color border)
        {
            for (int py = y; py < y + h; py++)
            {
                for (int px = x; px < x + w; px++)
                {
                    if (px == x || px == x + w - 1 || py == y || py == y + h - 1)
                        tex.SetPixel(px, py, border);
                    else
                        tex.SetPixel(px, py, fill);
                }
            }
        }
    }
}
