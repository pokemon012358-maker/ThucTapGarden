using System.Collections.Generic;
using UnityEngine;

namespace BalconyGarden
{
    public class PlotManager : MonoBehaviour
    {
        public static PlotManager Instance { get; private set; }

        [Header("Danh sách 6 chậu cây ban công")]
        public List<PlantPlot> plots = new List<PlantPlot>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void LoadData(GameSaveData data, float offlineSeconds)
        {
            if (plots == null || plots.Count == 0) return;

            for (int i = 0; i < plots.Count; i++)
            {
                var plot = plots[i];
                plot.plotIndex = i;

                // Cấu hình giá mở khóa mặc định
                if (i == 3) plot.unlockCost = 100;
                else if (i == 4) plot.unlockCost = 250;
                else if (i == 5) plot.unlockCost = 600;

                // Cấu hình nắng
                if (i < 2) plot.sunlightLevel = SunlightLevel.High;
                else if (i < 4) plot.sunlightLevel = SunlightLevel.Medium;
                else plot.sunlightLevel = SunlightLevel.Low;

                if (data.plots != null && i < data.plots.Count)
                {
                    var pData = data.plots[i];
                    plot.isUnlocked = pData.isUnlocked;
                    plot.soilType = (SoilType)pData.soilType;
                    plot.waterLevel = pData.waterLevel;
                    plot.growthProgress = pData.growthProgress;
                    plot.isWithered = pData.isWithered;

                    if (!string.IsNullOrEmpty(pData.plantId))
                    {
                        plot.currentPlant = GameManager.Instance.GetPlantDataById(pData.plantId);
                    }
                    else
                    {
                        plot.currentPlant = null;
                    }
                }
                else
                {
                    plot.isUnlocked = (i < 3);
                    plot.currentPlant = null;
                }

                // Tính thời gian offline
                if (offlineSeconds > 0f)
                {
                    plot.SimulateOffline(offlineSeconds);
                }

                plot.UpdateVisuals();
            }
        }

        public void PopulateSaveData(GameSaveData data)
        {
            data.plots = new List<PlotSaveData>();
            for (int i = 0; i < plots.Count; i++)
            {
                var p = plots[i];
                data.plots.Add(new PlotSaveData
                {
                    plotIndex = i,
                    isUnlocked = p.isUnlocked,
                    plantId = p.currentPlant != null ? p.currentPlant.plantId : "",
                    growthProgress = p.growthProgress,
                    waterLevel = p.waterLevel,
                    isWithered = p.isWithered,
                    soilType = (int)p.soilType
                });
            }
        }

        public PlantPlot GetPlot(int index)
        {
            if (index >= 0 && index < plots.Count)
            {
                return plots[index];
            }
            return null;
        }

        public List<PlantPlot> GetAllPlots() => plots;
    }
}
