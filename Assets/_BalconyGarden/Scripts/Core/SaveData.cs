using System;
using System.Collections.Generic;

namespace BalconyGarden
{
    [Serializable]
    public class PlotSaveData
    {
        public int plotIndex;
        public bool isUnlocked;
        public string plantId = "";
        public float growthProgress;
        public float waterLevel;
        public bool isWithered;
        public int soilType; // 0: Normal, 1: Nutrient, 2: Drainage
    }

    [Serializable]
    public class ItemCountPair
    {
        public string id;
        public int count;

        public ItemCountPair() { }
        public ItemCountPair(string id, int count)
        {
            this.id = id;
            this.count = count;
        }
    }

    [Serializable]
    public class NeighborSaveData
    {
        public string neighborId;
        public int friendshipPoints;
        public int completedOrders;
        public int storyUnlockedLevel;
    }

    [Serializable]
    public class GameSaveData
    {
        public int saveVersion = 1;
        public int coins = 50;
        public int gems = 5;
        public int level = 1;
        public int currentXP = 0;
        public string lastSaveTimeUtc = "";

        public List<PlotSaveData> plots = new List<PlotSaveData>();
        public List<ItemCountPair> seedInventory = new List<ItemCountPair>();
        public List<ItemCountPair> cropInventory = new List<ItemCountPair>();
        public List<string> discoveredFlowerIds = new List<string>();
        public List<NeighborSaveData> neighbors = new List<NeighborSaveData>();
    }
}
