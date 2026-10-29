using System;
using System.Collections.Generic;
using UnityEngine;

namespace BalconyGarden
{
    public class NeighborSystem : MonoBehaviour
    {
        public static NeighborSystem Instance { get; private set; }

        [Header("Dữ liệu hàng xóm")]
        public List<NeighborData> registeredNeighbors = new List<NeighborData>();

        private Dictionary<string, int> friendshipMap = new Dictionary<string, int>();
        private Dictionary<string, int> completedOrdersMap = new Dictionary<string, int>();
        private List<NeighborActiveOrder> activeOrders = new List<NeighborActiveOrder>();

        public event Action OnOrdersUpdated;

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
            friendshipMap.Clear();
            completedOrdersMap.Clear();

            if (data.neighbors != null)
            {
                foreach (var n in data.neighbors)
                {
                    friendshipMap[n.neighborId] = n.friendshipPoints;
                    completedOrdersMap[n.neighborId] = n.completedOrders;
                }
            }

            GenerateOrdersIfNeeded();
        }

        public void PopulateSaveData(GameSaveData data)
        {
            data.neighbors = new List<NeighborSaveData>();
            foreach (var n in registeredNeighbors)
            {
                int fp = friendshipMap.TryGetValue(n.neighborId, out int val) ? val : 0;
                int co = completedOrdersMap.TryGetValue(n.neighborId, out int cVal) ? cVal : 0;
                data.neighbors.Add(new NeighborSaveData
                {
                    neighborId = n.neighborId,
                    friendshipPoints = fp,
                    completedOrders = co
                });
            }
        }

        public void GenerateOrdersIfNeeded()
        {
            if (activeOrders == null) activeOrders = new List<NeighborActiveOrder>();

            if (registeredNeighbors == null || registeredNeighbors.Count == 0) return;

            // Đảm bảo mỗi hàng xóm có 1 đơn hàng đang chờ
            foreach (var neighbor in registeredNeighbors)
            {
                bool hasOrder = activeOrders.Exists(o => o.neighborId == neighbor.neighborId && !o.isCompleted);
                if (!hasOrder)
                {
                    GenerateNewOrderFor(neighbor);
                }
            }

            OnOrdersUpdated?.Invoke();
        }

        private void GenerateNewOrderFor(NeighborData neighbor)
        {
            string plantId = "cai_xanh";
            if (neighbor.favoritePlantIds != null && neighbor.favoritePlantIds.Count > 0)
            {
                int rndIndex = UnityEngine.Random.Range(0, neighbor.favoritePlantIds.Count);
                plantId = neighbor.favoritePlantIds[rndIndex];
            }

            int amount = UnityEngine.Random.Range(2, 5);
            var plant = GameManager.Instance.GetPlantDataById(plantId);
            int baseSell = plant != null ? plant.sellPrice : 15;

            var newOrder = new NeighborActiveOrder
            {
                neighborId = neighbor.neighborId,
                requiredPlantId = plantId,
                requiredAmount = amount,
                // Đơn hàng hàng xóm thưởng thêm 40% so với bán chợ
                rewardCoins = Mathf.RoundToInt(baseSell * amount * 1.4f),
                rewardXP = 20 * amount,
                rewardFriendship = 15,
                isCompleted = false
            };

            activeOrders.Add(newOrder);
        }

        public bool TryFulfillOrder(NeighborActiveOrder order)
        {
            if (order == null || order.isCompleted) return false;

            int available = InventorySystem.Instance.GetCropCount(order.requiredPlantId);
            if (available < order.requiredAmount)
            {
                return false;
            }

            // Tiêu hao nông sản trong kho
            InventorySystem.Instance.UseCrop(order.requiredPlantId, order.requiredAmount);

            // Thưởng xu & XP
            GameManager.Instance.AddCoins(order.rewardCoins);
            GameManager.Instance.AddXP(order.rewardXP);

            // Tăng điểm thân thiết
            AddFriendship(order.neighborId, order.rewardFriendship);

            order.isCompleted = true;
            activeOrders.Remove(order);

            // Sinh đơn mới sau khi hoàn thành
            var neighbor = GetNeighborData(order.neighborId);
            if (neighbor != null)
            {
                GenerateNewOrderFor(neighbor);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCoin();
            }

            OnOrdersUpdated?.Invoke();
            return true;
        }

        public void AddFriendship(string neighborId, int points)
        {
            if (!friendshipMap.ContainsKey(neighborId))
                friendshipMap[neighborId] = 0;

            friendshipMap[neighborId] += points;

            if (!completedOrdersMap.ContainsKey(neighborId))
                completedOrdersMap[neighborId] = 0;

            completedOrdersMap[neighborId]++;
        }

        public int GetFriendship(string neighborId)
        {
            return friendshipMap.TryGetValue(neighborId, out int val) ? val : 0;
        }

        public NeighborData GetNeighborData(string neighborId)
        {
            return registeredNeighbors.Find(n => n.neighborId == neighborId);
        }

        public List<NeighborActiveOrder> GetActiveOrders() => activeOrders;

        public string GetCurrentDialogue(NeighborData neighbor)
        {
            if (neighbor == null || neighbor.storyDialogues == null || neighbor.storyDialogues.Count == 0)
                return "Chào bạn! Ban công của bạn trồng cây đẹp quá!";

            int fp = GetFriendship(neighbor.neighborId);
            string dialogue = neighbor.storyDialogues[0].dialogueText;

            foreach (var d in neighbor.storyDialogues)
            {
                if (fp >= d.requiredFriendship)
                {
                    dialogue = d.dialogueText;
                }
            }

            return dialogue;
        }
    }
}
