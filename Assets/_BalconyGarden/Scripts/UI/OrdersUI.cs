using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BalconyGarden
{
    public class OrdersUI : MonoBehaviour
    {
        [Header("Tham chiếu")]
        public GameObject panelRoot;
        public Transform ordersContainer;
        public GameObject orderCardPrefab;
        public Button closeButton;

        private void Start()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseOrders);
            }
        }

        public void OpenOrders()
        {
            if (panelRoot != null) panelRoot.SetActive(true);
            RefreshOrders();
        }

        public void CloseOrders()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        }

        public void RefreshOrders()
        {
            if (ordersContainer == null || orderCardPrefab == null) return;

            foreach (Transform child in ordersContainer)
            {
                Destroy(child.gameObject);
            }

            var orders = NeighborSystem.Instance.GetActiveOrders();

            foreach (var order in orders)
            {
                if (order.isCompleted) continue;

                var neighbor = NeighborSystem.Instance.GetNeighborData(order.neighborId);
                var plant = GameManager.Instance.GetPlantDataById(order.requiredPlantId);
                if (neighbor == null || plant == null) continue;

                var cardObj = Instantiate(orderCardPrefab, ordersContainer);
                var texts = cardObj.GetComponentsInChildren<TextMeshProUGUI>();
                var images = cardObj.GetComponentsInChildren<Image>();
                var deliverBtn = cardObj.GetComponentInChildren<Button>();

                int currentHave = InventorySystem.Instance.GetCropCount(order.requiredPlantId);
                bool canFulfill = currentHave >= order.requiredAmount;

                // Gán text
                TextMeshProUGUI nameText = texts.Length > 0 ? texts[0] : null;
                TextMeshProUGUI dialogueText = texts.Length > 1 ? texts[1] : null;
                TextMeshProUGUI requestText = texts.Length > 2 ? texts[2] : null;
                TextMeshProUGUI rewardText = texts.Length > 3 ? texts[3] : null;

                if (nameText != null)
                {
                    int friendship = NeighborSystem.Instance.GetFriendship(order.neighborId);
                    nameText.text = $"{neighbor.neighborName} (Thân thiết: {friendship}❤️)";
                }

                if (dialogueText != null)
                {
                    dialogueText.text = $"\"{NeighborSystem.Instance.GetCurrentDialogue(neighbor)}\"";
                }

                if (requestText != null)
                {
                    string colorTag = canFulfill ? "<color=#4CAF50>" : "<color=#FF5252>";
                    requestText.text = $"Cần: {plant.plantName} ({colorTag}{currentHave}/{order.requiredAmount}</color>)";
                }

                if (rewardText != null)
                {
                    rewardText.text = $"Thưởng: +{order.rewardCoins} Xu • +{order.rewardXP} XP • +{order.rewardFriendship} ❤️";
                }

                // Gán ảnh đại diện hàng xóm và icon nông sản
                foreach (var img in images)
                {
                    if (img.gameObject.name.Contains("Avatar") && neighbor.avatarSprite != null)
                    {
                        img.sprite = neighbor.avatarSprite;
                    }
                    else if (img.gameObject.name.Contains("CropIcon") && plant.cropItemSprite != null)
                    {
                        img.sprite = plant.cropItemSprite;
                    }
                }

                // Nút giao hàng
                if (deliverBtn != null)
                {
                    deliverBtn.interactable = canFulfill;
                    var orderCopy = order;

                    deliverBtn.onClick.AddListener(() =>
                    {
                        if (NeighborSystem.Instance.TryFulfillOrder(orderCopy))
                        {
                            if (UIManager.Instance != null)
                            {
                                UIManager.Instance.ShowFloatingMessage($"Giao hàng thành công cho {neighbor.neighborName}! 🎁");
                            }
                            RefreshOrders();
                        }
                    });
                }
            }
        }
    }
}
