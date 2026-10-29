using System;
using System.Collections.Generic;
using UnityEngine;

namespace BalconyGarden
{
    public class InventorySystem : MonoBehaviour
    {
        public static InventorySystem Instance { get; private set; }

        private Dictionary<string, int> seedStock = new Dictionary<string, int>();
        private Dictionary<string, int> cropStock = new Dictionary<string, int>();
        private HashSet<string> discoveredFlowers = new HashSet<string>();

        public event Action OnInventoryChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void LoadData(GameSaveData data)
        {
            seedStock.Clear();
            cropStock.Clear();
            discoveredFlowers.Clear();

            if (data.seedInventory != null)
            {
                foreach (var pair in data.seedInventory)
                {
                    seedStock[pair.id] = pair.count;
                }
            }

            if (data.cropInventory != null)
            {
                foreach (var pair in data.cropInventory)
                {
                    cropStock[pair.id] = pair.count;
                }
            }

            if (data.discoveredFlowerIds != null)
            {
                foreach (var id in data.discoveredFlowerIds)
                {
                    discoveredFlowers.Add(id);
                }
            }

            OnInventoryChanged?.Invoke();
        }

        public void PopulateSaveData(GameSaveData data)
        {
            data.seedInventory = new List<ItemCountPair>();
            foreach (var kvp in seedStock)
            {
                data.seedInventory.Add(new ItemCountPair(kvp.Key, kvp.Value));
            }

            data.cropInventory = new List<ItemCountPair>();
            foreach (var kvp in cropStock)
            {
                data.cropInventory.Add(new ItemCountPair(kvp.Key, kvp.Value));
            }

            data.discoveredFlowerIds = new List<string>(discoveredFlowers);
        }

        public void AddSeed(string plantId, int count = 1)
        {
            if (!seedStock.ContainsKey(plantId))
                seedStock[plantId] = 0;

            seedStock[plantId] += count;
            OnInventoryChanged?.Invoke();
        }

        public bool UseSeed(string plantId, int count = 1)
        {
            if (seedStock.TryGetValue(plantId, out int available) && available >= count)
            {
                seedStock[plantId] -= count;
                if (seedStock[plantId] <= 0)
                    seedStock.Remove(plantId);

                OnInventoryChanged?.Invoke();
                return true;
            }
            return false;
        }

        public int GetSeedCount(string plantId)
        {
            return seedStock.TryGetValue(plantId, out int count) ? count : 0;
        }

        public void AddCrop(string plantId, int count = 1)
        {
            if (!cropStock.ContainsKey(plantId))
                cropStock[plantId] = 0;

            cropStock[plantId] += count;

            // Nếu là hoa, ghi nhận vào bộ sưu tập
            DiscoverFlower(plantId);

            OnInventoryChanged?.Invoke();
        }

        public bool UseCrop(string plantId, int count = 1)
        {
            if (cropStock.TryGetValue(plantId, out int available) && available >= count)
            {
                cropStock[plantId] -= count;
                if (cropStock[plantId] <= 0)
                    cropStock.Remove(plantId);

                OnInventoryChanged?.Invoke();
                return true;
            }
            return false;
        }

        public int GetCropCount(string plantId)
        {
            return cropStock.TryGetValue(plantId, out int count) ? count : 0;
        }

        public void DiscoverFlower(string plantId)
        {
            if (!discoveredFlowers.Contains(plantId))
            {
                discoveredFlowers.Add(plantId);
                OnInventoryChanged?.Invoke();
            }
        }

        public bool HasDiscoveredFlower(string plantId)
        {
            return discoveredFlowers.Contains(plantId);
        }

        public Dictionary<string, int> GetAllSeeds() => new Dictionary<string, int>(seedStock);
        public Dictionary<string, int> GetAllCrops() => new Dictionary<string, int>(cropStock);
        public HashSet<string> GetDiscoveredFlowers() => new HashSet<string>(discoveredFlowers);
    }
}
