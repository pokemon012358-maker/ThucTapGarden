using System;
using System.Collections.Generic;
using UnityEngine;

namespace BalconyGarden
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Dữ liệu cây trồng đăng ký")]
        public List<PlantData> registeredPlants = new List<PlantData>();

        [Header("Trạng thái người chơi")]
        public int coins = 50;
        public int gems = 5;
        public int level = 1;
        public int currentXP = 0;

        [Header("Cài đặt test / Cheat nhanh")]
        public bool debugFastGrowth = false;
        public float debugGrowthMultiplier = 5.0f;

        public event Action OnCurrenciesChanged;
        public event Action<int> OnLevelUp;
        public event Action OnXPChanged;

        private float autoSaveTimer = 0f;
        private const float AutoSaveInterval = 15f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            LoadGame();
        }

        private void Update()
        {
            autoSaveTimer += Time.deltaTime;
            if (autoSaveTimer >= AutoSaveInterval)
            {
                autoSaveTimer = 0f;
                SaveGame();
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                SaveGame();
            }
        }

        private void OnApplicationQuit()
        {
            SaveGame();
        }

        public void SaveGame()
        {
            GameSaveData data = new GameSaveData
            {
                coins = this.coins,
                gems = this.gems,
                level = this.level,
                currentXP = this.currentXP
            };

            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.PopulateSaveData(data);
            }

            if (PlotManager.Instance != null)
            {
                PlotManager.Instance.PopulateSaveData(data);
            }

            if (NeighborSystem.Instance != null)
            {
                NeighborSystem.Instance.PopulateSaveData(data);
            }

            SaveSystem.SaveGame(data);
        }

        public void LoadGame()
        {
            GameSaveData data = SaveSystem.LoadGame();
            this.coins = data.coins;
            this.gems = data.gems;
            this.level = Mathf.Max(1, data.level);
            this.currentXP = data.currentXP;

            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.LoadData(data);
            }

            float offlineSeconds = 0f;
            if (TimeManager.Instance != null)
            {
                offlineSeconds = TimeManager.Instance.CalculateOfflineSeconds(data.lastSaveTimeUtc);
            }

            if (PlotManager.Instance != null)
            {
                PlotManager.Instance.LoadData(data, offlineSeconds);
            }

            if (NeighborSystem.Instance != null)
            {
                NeighborSystem.Instance.LoadData(data);
            }

            OnCurrenciesChanged?.Invoke();
            OnXPChanged?.Invoke();
        }

        public void AddCoins(int amount)
        {
            coins += amount;
            OnCurrenciesChanged?.Invoke();
        }

        public bool SpendCoins(int amount)
        {
            if (coins >= amount)
            {
                coins -= amount;
                OnCurrenciesChanged?.Invoke();
                return true;
            }
            return false;
        }

        public void AddGems(int amount)
        {
            gems += amount;
            OnCurrenciesChanged?.Invoke();
        }

        public bool SpendGems(int amount)
        {
            if (gems >= amount)
            {
                gems -= amount;
                OnCurrenciesChanged?.Invoke();
                return true;
            }
            return false;
        }

        public int GetXPRequiredForNextLevel()
        {
            return 50 + (level - 1) * 60;
        }

        public void AddXP(int amount)
        {
            currentXP += amount;
            int req = GetXPRequiredForNextLevel();

            while (currentXP >= req)
            {
                currentXP -= req;
                level++;
                req = GetXPRequiredForNextLevel();

                // Thưởng khi lên level
                AddCoins(30 * level);
                AddGems(2);

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayLevelUp();
                }

                OnLevelUp?.Invoke(level);
            }

            OnXPChanged?.Invoke();
        }

        public PlantData GetPlantDataById(string plantId)
        {
            if (string.IsNullOrEmpty(plantId)) return null;
            return registeredPlants.Find(p => p.plantId == plantId);
        }

        public void ResetGameData()
        {
            SaveSystem.DeleteSave();
            LoadGame();
        }
    }
}
