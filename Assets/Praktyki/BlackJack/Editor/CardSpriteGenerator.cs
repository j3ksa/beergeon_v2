#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BlackJack.Editor
{
    public static class CardSpriteGenerator
    {
        [InitializeOnLoadMethod]
        public static void EnsureCardSprites()
        {
            string dir = "Assets/Praktyki/BlackJack/Sprites";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string heartPath = Path.Combine(dir, "Heart.png");
            string diamondPath = Path.Combine(dir, "Diamond.png");
            string clubPath = Path.Combine(dir, "Club.png");
            string spadePath = Path.Combine(dir, "Spade.png");

            bool created = false;
            if (!File.Exists(heartPath)) { File.WriteAllBytes(heartPath, CreateHeartTexture().EncodeToPNG()); created = true; }
            if (!File.Exists(diamondPath)) { File.WriteAllBytes(diamondPath, CreateDiamondTexture().EncodeToPNG()); created = true; }
            if (!File.Exists(clubPath)) { File.WriteAllBytes(clubPath, CreateClubTexture().EncodeToPNG()); created = true; }
            if (!File.Exists(spadePath)) { File.WriteAllBytes(spadePath, CreateSpadeTexture().EncodeToPNG()); created = true; }

            if (created)
            {
                AssetDatabase.Refresh();
                ConfigureSprite(heartPath);
                ConfigureSprite(diamondPath);
                ConfigureSprite(clubPath);
                ConfigureSprite(spadePath);
            }
        }

        private static void ConfigureSprite(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 100;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }
        }

        private static Texture2D CreateHeartTexture()
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color red = new Color(0.85f, 0.12f, 0.12f, 1f);
            Color clear = Color.clear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Znormalizowane współrzędne [-1, 1]
                    float nx = (x - size / 2f) / (size / 2.2f);
                    float ny = (y - size / 2.5f) / (size / 2.2f);

                    // Równanie serca: (x^2 + y^2 - 1)^3 - x^2 * y^3 <= 0
                    float a = nx * nx + ny * ny - 0.7f;
                    bool inHeart = (a * a * a - nx * nx * ny * ny * ny) <= 0f;

                    tex.SetPixel(x, y, inHeart ? red : clear);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateDiamondTexture()
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color red = new Color(0.85f, 0.12f, 0.12f, 1f);
            Color clear = Color.clear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = Mathf.Abs(x - size / 2f) / (size / 2.2f);
                    float ny = Mathf.Abs(y - size / 2f) / (size / 1.7f);

                    bool inDiamond = (nx + ny) <= 0.85f;
                    tex.SetPixel(x, y, inDiamond ? red : clear);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateClubTexture()
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color black = new Color(0.12f, 0.12f, 0.12f, 1f);
            Color clear = Color.clear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float cx = x - size / 2f;
                    float cy = y - size / 2.2f;

                    // Trzy koła (góra, lewo, prawo)
                    bool topCircle = Vector2.Distance(new Vector2(cx, cy), new Vector2(0, 10)) <= 11f;
                    bool leftCircle = Vector2.Distance(new Vector2(cx, cy), new Vector2(-10, -3)) <= 11f;
                    bool rightCircle = Vector2.Distance(new Vector2(cx, cy), new Vector2(10, -3)) <= 11f;

                    // Nóżka
                    bool stem = (Mathf.Abs(cx) <= (20f - y) * 0.45f) && y >= 6 && y <= 22;

                    bool inClub = topCircle || leftCircle || rightCircle || stem;
                    tex.SetPixel(x, y, inClub ? black : clear);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateSpadeTexture()
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color black = new Color(0.12f, 0.12f, 0.12f, 1f);
            Color clear = Color.clear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - size / 2f) / (size / 2.2f);
                    float ny = -(y - size / 1.7f) / (size / 2.2f); // Odwrócone serce

                    float a = nx * nx + ny * ny - 0.7f;
                    bool inSpadeHead = (a * a * a - nx * nx * ny * ny * ny) <= 0f && y >= 18;

                    // Nóżka
                    float cx = x - size / 2f;
                    bool stem = (Mathf.Abs(cx) <= (22f - y) * 0.45f) && y >= 6 && y <= 22;

                    bool inSpade = inSpadeHead || stem;
                    tex.SetPixel(x, y, inSpade ? black : clear);
                }
            }
            tex.Apply();
            return tex;
        }
    }
}
#endif
