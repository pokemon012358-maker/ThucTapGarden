using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BalconyGarden
{
    public class ShopUI : MonoBehaviour
    {
        [Header("Tham chiếu")]
        public GameObject panelRoot;
        public Transform itemsContainer;
        public GameObject shopItemPrefab;
        public Button closeButton;

        private void Start()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseShop);
            }
        }

        public void OpenShop()
        {
            if (panelRoot != null) panelRoot.SetActive(true);
            RefreshShop();
        }

        public void CloseShop()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        }

        public void RefreshShop()
        {
            if (itemsContainer == null || shopItemPrefab == null) return;

            // Xóa các item cũ
            foreach (Transform child in itemsContainer)
            {
                Destroy(child.gameObject);
            }

            var registeredPlants = GameManager.Instance.registeredPlants;
            int playerLevel = GameManager.Instance.level;

            foreach (var plant in registeredPlants)
            {
                var itemObj = Instantiate(shopItemPrefab, itemsContainer);
                var texts = itemObj.GetComponentsInChildren<TextMeshProUGUI>();
                var images = itemObj.GetComponentsInChildren<Image>();
                var buyBtn = itemObj.GetComponentInChildren<Button>();

                // Tìm và gán dữ liệu
                TextMeshProUGUI titleText = texts.Length > 0 ? texts[0] : null;
                TextMeshProUGUI priceText = texts.Length > 1 ? texts[1] : null;
                TextMeshProUGUI descText = texts.Length > 2 ? texts[2] : null;

                if (titleText != null)
                {
                    titleText.text = plant.plantName;
                }

                if (descText != null)
                {
                    string typeStr = plant.plantType == PlantType.Vegetable ? "Rau củ" : "Hoa";
                    descText.text = $"{typeStr} • Lớn: {plant.growthTimeSeconds:F0}s • Mở: Lv{plant.unlockLevel}";
                }

                if (priceText != null)
                {
                    priceText.text = $"{plant.buyPrice} Xu";
                }

                // Gán icon
                Image iconImg = null;
                foreach (var img in images)
                {
                    if (img.gameObject.name.Contains("Icon"))
                    {
                        iconImg = img;
                        break;
                    }
                }
                if (iconImg != null && plant.seedPacketSprite != null)
                {
                    iconImg.sprite = plant.seedPacketSprite;
                }

                // Kiểm tra level mở khóa
                bool isUnlocked = playerLevel >= plant.unlockLevel;
                if (buyBtn != null)
                {
                    buyBtn.interactable = isUnlocked;
                    if (!isUnlocked && priceText != null)
                    {
                        priceText.text = $"Khóa (Lv{plant.unlockLevel})";
                    }

                    string plantIdCopy = plant.plantId;
                    int priceCopy = plant.buyPrice;

                    buyBtn.onClick.AddListener(() =>
                    {
                        if (GameManager.Instance.SpendCoins(priceCopy))
                        {
                            InventorySystem.Instance.AddSeed(plantIdCopy, 1);
                            if (AudioManager.Instance != null) AudioManager.Instance.PlayCoin();
                            if (UIManager.Instance != null)
                            {
                                UIManager.Instance.ShowFloatingMessage($"Đã mua 1 Hạt {plant.plantName}!");
                                UIManager.Instance.RefreshBottomBar();
                            }
                        }
                        else
                        {
                            if (UIManager.Instance != null)
                            {
                                UIManager.Instance.ShowFloatingMessage("Không đủ Xu! Hãy thu hoạch và bán thêm nông sản.");
                            }
                        }
                    });
                }
            }
        }
    }
}
