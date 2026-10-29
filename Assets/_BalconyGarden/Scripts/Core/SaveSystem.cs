using System;
using System.IO;
using UnityEngine;

namespace BalconyGarden
{
    public static class SaveSystem
    {
        private static string SaveFilePath => Path.Combine(Application.persistentDataPath, "balcony_garden_save.json");

        public static void SaveGame(GameSaveData data)
        {
            try
            {
                data.lastSaveTimeUtc = DateTime.UtcNow.ToString("o");
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SaveFilePath, json);
                // Debug.Log($"[SaveSystem] Saved game successfully to {SaveFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Error saving game: {ex.Message}");
            }
        }

        public static GameSaveData LoadGame()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    string json = File.ReadAllText(SaveFilePath);
                    GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
                    if (data != null)
                    {
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveSystem] Error loading game, creating new save: {ex.Message}");
            }

            return CreateNewSave();
        }

        public static bool HasSaveFile()
        {
            return File.Exists(SaveFilePath);
        }

        public static void DeleteSave()
        {
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
                Debug.Log("[SaveSystem] Save file deleted.");
            }
        }

        public static GameSaveData CreateNewSave()
        {
            var data = new GameSaveData
            {
                coins = 50,
                gems = 5,
                level = 1,
                currentXP = 0,
                lastSaveTimeUtc = DateTime.UtcNow.ToString("o"),
                seedInventory = new System.Collections.Generic.List<ItemCountPair>
                {
                    new ItemCountPair("cai_xanh", 3),
                    new ItemCountPair("cuc_vang", 2)
                },
                discoveredFlowerIds = new System.Collections.Generic.List<string>
                {
                    "cuc_vang"
                }
            };

            // 6 plots: 3 unlocked at start, 3 locked
            for (int i = 0; i < 6; i++)
            {
                data.plots.Add(new PlotSaveData
                {
                    plotIndex = i,
                    isUnlocked = (i < 3),
                    plantId = "",
                    growthProgress = 0f,
                    waterLevel = 1f,
                    isWithered = false,
                    soilType = 0
                });
            }

            return data;
        }
    }
}
