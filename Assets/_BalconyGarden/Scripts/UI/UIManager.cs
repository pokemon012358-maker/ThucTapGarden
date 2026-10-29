using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BalconyGarden
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Canvas & Root")]
        public Canvas mainCanvas;

        [Header("Top HUD")]
        public TextMeshProUGUI coinsText;
        public TextMeshProUGUI gemsText;
        public TextMeshProUGUI levelText;
        public Slider xpSlider;
        public TextMeshProUGUI xpText;

        [Header("Bottom Tool & Seed Tray")]
        public Transform seedTrayContainer;
        public GameObject draggableSeedPrefab;
        public DraggableToolItem wateringCanTool;
        public DraggableToolItem shovelTool;
        public DraggableToolItem sickleTool;

        [Header("Bottom Navigation Buttons")]
        public Button shopButton;
        public Button inventoryButton;
        public Button ordersButton;
        public Button flowerBookButton;
        public Button debugFastGrowthButton;
        public TextMeshProUGUI debugFastGrowthText;

        [Header("Popups")]
        public ShopUI shopUI;
        public InventoryUI inventoryUI;
        public OrdersUI ordersUI;
        public BreedingUI breedingUI;
        public PlotInfoUI plotInfoUI;
        public GameObject levelUpPopup;
        public TextMeshProUGUI levelUpTitleText;
        public Button levelUpCloseButton;

        [Header("Toast / Thông báo nổi")]
        public GameObject toastPrefab;
        public Transform toastContainer;

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
            // Đăng ký sự kiện
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnCurrenciesChanged += UpdateCurrencies;
                GameManager.Instance.OnXPChanged += UpdateXPBar;
                GameManager.Instance.OnLevelUp += ShowLevelUpPopup;
            }

            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.OnInventoryChanged += RefreshBottomBar;
            }

            // Gán nút bấm mở popup
            if (shopButton != null && shopUI != null)
            {
                shopButton.onClick.AddListener(() => shopUI.OpenShop());
            }

            if (inventoryButton != null && inventoryUI != null)
            {
                inventoryButton.onClick.AddListener(() => inventoryUI.OpenInventory());
            }

            if (ordersButton != null && ordersUI != null)
            {
                ordersButton.onClick.AddListener(() => ordersUI.OpenOrders());
            }

            if (flowerBookButton != null && breedingUI != null)
            {
                flowerBookButton.onClick.AddListener(() => breedingUI.Open());
            }

            if (debugFastGrowthButton != null)
            {
                debugFastGrowthButton.onClick.AddListener(ToggleDebugFastGrowth);
                UpdateDebugButtonText();
            }

            if (levelUpCloseButton != null)
            {
                levelUpCloseButton.onClick.AddListener(() =>
                {
                    if (levelUpPopup != null) levelUpPopup.SetActive(false);
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                });
            }

            // Thiết lập các công cụ kéo thả cố định
            SetupFixedTools();

            // Cập nhật giao diện ban đầu
            UpdateCurrencies();
            UpdateXPBar();
            RefreshBottomBar();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnCurrenciesChanged -= UpdateCurrencies;
                GameManager.Instance.OnXPChanged -= UpdateXPBar;
                GameManager.Instance.OnLevelUp -= ShowLevelUpPopup;
            }

            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.OnInventoryChanged -= RefreshBottomBar;
            }
        }

        private void SetupFixedTools()
        {
            if (wateringCanTool != null)
            {
                wateringCanTool.Setup(ToolType.WateringCan, "", wateringCanTool.iconImage != null ? wateringCanTool.iconImage.sprite : null, 0, "Watercan", mainCanvas);
            }

            if (shovelTool != null)
            {
                shovelTool.Setup(ToolType.Shovel, "", shovelTool.iconImage != null ? shovelTool.iconImage.sprite : null, 0, "Shovel", mainCanvas);
            }

            if (sickleTool != null)
            {
                sickleTool.Setup(ToolType.Sickle, "", sickleTool.iconImage != null ? sickleTool.iconImage.sprite : null, 0, "Collect", mainCanvas);
            }
        }

        public void UpdateCurrencies()
        {
            if (GameManager.Instance == null) return;

            if (coinsText != null) coinsText.text = $"{GameManager.Instance.coins:N0}";
            if (gemsText != null) gemsText.text = $"{GameManager.Instance.gems:N0}";
            if (levelText != null) levelText.text = $"Lv.{GameManager.Instance.level}";
        }

        public void UpdateXPBar()
        {
            if (GameManager.Instance == null) return;

            int current = GameManager.Instance.currentXP;
            int required = GameManager.Instance.GetXPRequiredForNextLevel();

            if (xpSlider != null)
            {
                xpSlider.maxValue = required;
                xpSlider.value = current;
            }

            if (xpText != null)
            {
                xpText.text = $"{current}/{required} XP";
            }
        }

        public void RefreshBottomBar()
        {
            if (seedTrayContainer == null || draggableSeedPrefab == null) return;

            // Xóa các hạt giống cũ trong khay
            foreach (Transform child in seedTrayContainer)
            {
                Destroy(child.gameObject);
            }

            var seeds = InventorySystem.Instance.GetAllSeeds();

            foreach (var kvp in seeds)
            {
                string plantId = kvp.Key;
                int count = kvp.Value;
                if (count <= 0) continue;

                var plant = GameManager.Instance.GetPlantDataById(plantId);
                if (plant == null) continue;

                var seedObj = Instantiate(draggableSeedPrefab, seedTrayContainer);
                var draggable = seedObj.GetComponent<DraggableToolItem>();
                if (draggable != null)
                {
                    draggable.Setup(ToolType.Seed, plantId, plant.seedPacketSprite, count, plant.plantName, mainCanvas);
                }
            }
        }

        public void ShowLevelUpPopup(int newLevel)
        {
            if (levelUpPopup != null)
            {
                levelUpPopup.SetActive(true);
                if (levelUpTitleText != null)
                {
                    levelUpTitleText.text = $"CONGRATULATIONS ON LEVEL {newLevel}!\nReward: +{30 * newLevel} Coins, +2 Gems";
                }
            }
        }

        public void ShowFloatingMessage(string message)
        {
            if (toastPrefab != null && toastContainer != null)
            {
                var toastObj = Instantiate(toastPrefab, toastContainer);
                var txt = toastObj.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.text = message;
                Destroy(toastObj, 2.0f);
            }
            else
            {
                Debug.Log($"[BalconyGarden] {message}");
            }
        }

        public void SelectPlotForPlanting(PlantPlot plot)
        {
            ShowFloatingMessage("Drag seeds from the tray below into the pot to plant!");
        }

        public void OpenPlotInfo(PlantPlot plot)
        {
            if (plotInfoUI != null)
            {
                plotInfoUI.Open(plot);
            }
        }

        private void ToggleDebugFastGrowth()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.debugFastGrowth = !GameManager.Instance.debugFastGrowth;
                UpdateDebugButtonText();
                string state = GameManager.Instance.debugFastGrowth ? "ON (Fast x5)" : "OFF (Normal)";
                ShowFloatingMessage($"Super Speed Mode: {state}");
            }
        }

        private void UpdateDebugButtonText()
        {
            if (debugFastGrowthText != null && GameManager.Instance != null)
            {
                debugFastGrowthText.text = GameManager.Instance.debugFastGrowth ? "Speed: X5" : "Speed: 1X";
            }
        }
    }
}
