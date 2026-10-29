using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BalconyGarden
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("Tham chiếu")]
        public GameObject panelRoot;
        public Transform cropsContainer;
        public GameObject cropItemPrefab;
        public Button closeButton;
        public Button sellAllButton;

        private void Start()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseInventory);
            }
            if (sellAllButton != null)
            {
                sellAllButton.onClick.AddListener(SellAllCrops);
            }
        }

        public void OpenInventory()
        {
            if (panelRoot != null) panelRoot.SetActive(true);
            RefreshInventory();
        }

        public void CloseInventory()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        }

        public void RefreshInventory()
        {
            if (cropsContainer == null || cropItemPrefab == null) return;

            foreach (Transform child in cropsContainer)
            {
                Destroy(child.gameObject);
            }

            var crops = InventorySystem.Instance.GetAllCrops();

            if (crops.Count == 0)
            {
                // Kho trống
                return;
            }

            foreach (var kvp in crops)
            {
                string plantId = kvp.Key;
                int count = kvp.Value;
                if (count <= 0) continue;

                var plant = GameManager.Instance.GetPlantDataById(plantId);
                if (plant == null) continue;

                var itemObj = Instantiate(cropItemPrefab, cropsContainer);
                var texts = itemObj.GetComponentsInChildren<TextMeshProUGUI>();
                var images = itemObj.GetComponentsInChildren<Image>();
                var sellBtn = itemObj.GetComponentInChildren<Button>();

                TextMeshProUGUI nameText = texts.Length > 0 ? texts[0] : null;
                TextMeshProUGUI countText = texts.Length > 1 ? texts[1] : null;
                TextMeshProUGUI valueText = texts.Length > 2 ? texts[2] : null;

                if (nameText != null) nameText.text = plant.plantName;
                if (countText != null) countText.text = $"Số lượng: x{count}";
                if (valueText != null) valueText.text = $"Giá bán: {plant.sellPrice} Xu/cây";

                Image iconImg = null;
                foreach (var img in images)
                {
                    if (img.gameObject.name.Contains("Icon"))
                    {
                        iconImg = img;
                        break;
                    }
                }
                if (iconImg != null && plant.cropItemSprite != null)
                {
                    iconImg.sprite = plant.cropItemSprite;
                }

                if (sellBtn != null)
                {
                    sellBtn.onClick.AddListener(() =>
                    {
                        if (InventorySystem.Instance.UseCrop(plantId, 1))
                        {
                            GameManager.Instance.AddCoins(plant.sellPrice);
                            if (AudioManager.Instance != null) AudioManager.Instance.PlayCoin();
                            if (UIManager.Instance != null)
                            {
                                UIManager.Instance.ShowFloatingMessage($"Đã bán 1 {plant.plantName} (+{plant.sellPrice} Xu)!");
                            }
                            RefreshInventory();
                        }
                    });
                }
            }
        }

        private void SellAllCrops()
        {
            var crops = InventorySystem.Instance.GetAllCrops();
            int totalCoinsEarned = 0;
            int totalItemsSold = 0;

            foreach (var kvp in crops)
            {
                string plantId = kvp.Key;
                int count = kvp.Value;
                if (count <= 0) continue;

                var plant = GameManager.Instance.GetPlantDataById(plantId);
                if (plant != null)
                {
                    int coins = plant.sellPrice * count;
                    totalCoinsEarned += coins;
                    totalItemsSold += count;
                    InventorySystem.Instance.UseCrop(plantId, count);
                }
            }

            if (totalCoinsEarned > 0)
            {
                GameManager.Instance.AddCoins(totalCoinsEarned);
                if (AudioManager.Instance != null) AudioManager.Instance.PlayCoin();
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowFloatingMessage($"Đã bán {totalItemsSold} nông sản (+{totalCoinsEarned} Xu)!");
                }
                RefreshInventory();
            }
            else
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowFloatingMessage("Kho không có nông sản để bán!");
                }
            }
        }
    }
}
