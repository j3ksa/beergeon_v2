using System.IO;
using UnityEngine;

namespace SaveSystem
{
    public static class SaveSystem
    {
        private static string SaveDirectory => Path.Combine(Application.persistentDataPath, "Saves");

        private static string GetSaveFilePath(int slotId)
        {
            return Path.Combine(SaveDirectory, $"save_slot_{slotId}.json");
        }

        public static void Save(int slotId, SaveData data)
        {
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }

            data.slotId = slotId;
            data.saveDateTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string json = JsonUtility.ToJson(data, true);
            string filePath = GetSaveFilePath(slotId);

            File.WriteAllText(filePath, json);
            Debug.Log($"[SaveSystem] Gra zapisana pomyślnie w slocie {slotId}: {filePath}");
        }

        public static SaveData Load(int slotId)
        {
            string filePath = GetSaveFilePath(slotId);

            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[SaveSystem] Brak pliku zapisu dla slotu {slotId}");
                return null;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                Debug.Log($"[SaveSystem] Wczytano zapis ze slotu {slotId}");
                return data;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SaveSystem] Błąd podczas wczytywania slotu {slotId}: {ex.Message}");
                return null;
            }
        }

        public static bool SaveExists(int slotId)
        {
            return File.Exists(GetSaveFilePath(slotId));
        }

        public static void DeleteSave(int slotId)
        {
            string filePath = GetSaveFilePath(slotId);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"[SaveSystem] Usunięto zapis ze slotu {slotId}");
            }
        }
    }
}
