using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaveSystem
{
    [Serializable]
    public class Vector3Serializable
    {
        public float x;
        public float y;
        public float z;

        public Vector3Serializable() { }

        public Vector3Serializable(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }

    [Serializable]
    public class PlayerSaveData
    {
        public Vector3Serializable position = new Vector3Serializable();
        public Vector3Serializable rotation = new Vector3Serializable();
        public int level = 1;
        public int experience = 0;
        public int requiredExperience = 100;
        public int strength = 10;
        public int vitality = 10;
        public int currentHealth = 100;
        public int capsCount = 0;
        public int activeSlotIndex = 0;
        public List<string> inventoryItems = new List<string>();

        // System Ulepszeń Statystyk (Stats Upgrade)
        public int availableStatPoints = 0;
        public int healthUpgrades = 0;
        public int staminaUpgrades = 0;
        public int staminaRegenUpgrades = 0;
        public int speedUpgrades = 0;
        public int strengthUpgrades = 0;
        public int critChanceUpgrades = 0;

        // System Ulepszeń Broni (Kowadło)
        public List<WeaponUpgradeSaveData> weaponUpgrades = new List<WeaponUpgradeSaveData>();
    }

    [Serializable]
    public class WeaponUpgradeSaveData
    {
        public string weaponId;
        public int level;

        public WeaponUpgradeSaveData() { }

        public WeaponUpgradeSaveData(string id, int lvl)
        {
            weaponId = id;
            level = lvl;
        }
    }

    [Serializable]
    public class EnemySaveData
    {
        public string saveID;
        public string enemyName;
        public Vector3Serializable position = new Vector3Serializable();
        public Vector3Serializable rotation = new Vector3Serializable();
        public int currentHealth;
        public bool isDead;
    }

    [Serializable]
    public class SaveData
    {
        public int slotId = 1;
        public string saveName = "Game Save";
        public string saveDateTime = "";
        public string sceneName = "";

        public PlayerSaveData playerData = new PlayerSaveData();
        public List<EnemySaveData> enemiesData = new List<EnemySaveData>();

        public SaveData()
        {
            saveDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
