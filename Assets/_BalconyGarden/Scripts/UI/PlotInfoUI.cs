using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BalconyGarden
{
    public class PlotInfoUI : MonoBehaviour
    {
        [Header("Tham chiếu")]
        public GameObject panelRoot;
        public Image plantPreviewImage;
        public TextMeshProUGUI plantNameText;
        public TextMeshProUGUI stageText;
        public TextMeshProUGUI progressText;
        public TextMeshProUGUI waterText;
        public TextMeshProUGUI sunPreferenceText;
        public TextMeshProUGUI soilTypeText;

        [Header("Nút tương tác")]
        public Button waterButton;
        public Button harvestButton;
        public Button clearButton;
        public Button speedUpGemsButton;
        public Button closeButton;

        private PlantPlot activePlot;

        private void Start()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (waterButton != null) waterButton.onClick.AddListener(OnWaterClick);
            if (harvestButton != null) harvestButton.onClick.AddListener(OnHarvestClick);
            if (clearButton != null) clearButton.onClick.AddListener(OnClearClick);
            if (speedUpGemsButton != null) speedUpGemsButton.onClick.AddListener(OnSpeedUpClick);
        }

        private void Update()
        {
            if (panelRoot != null && panelRoot.activeSelf && activePlot != null)
            {
                RefreshInfo();
            }
        }

        public void Open(PlantPlot plot)
        {
            activePlot = plot;
            if (panelRoot != null) panelRoot.SetActive(true);
            RefreshInfo();
        }

        public void Close()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            activePlot = null;
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        }

        public void RefreshInfo()
        {
            if (activePlot == null) return;

            if (activePlot.currentPlant != null)
            {
                var plant = activePlot.currentPlant;
                if (plantNameText != null) plantNameText.text = plant.plantName;

                if (plantPreviewImage != null)
                {
                    plantPreviewImage.gameObject.SetActive(true);
                    plantPreviewImage.sprite = plant.GetSpriteForProgress(activePlot.growthProgress, activePlot.isWithered);
                }

                if (activePlot.isWithered)
                {
                    if (stageText != null) stageText.text = "<color=#FF5252>Trạng thái: Bị héo (Cần tưới nước ngay!)</color>";
                }
                else if (activePlot.growthProgress >= 1.0f)
                {
                    if (stageText != null) stageText.text = "<color=#4CAF50>Trạng thái: Đã chín / Nở rộ (Sẵn sàng thu hoạch)</color>";
                }
                else
                {
                    string stageName = activePlot.growthProgress < 0.25f ? "Gieo hạt" : (activePlot.growthProgress < 0.6f ? "Mầm non" : "Đang lớn");
                    if (stageText != null) stageText.text = $"Trạng thái: {stageName}";
                }

                if (progressText != null)
                {
                    progressText.text = $"Tiến độ: {(activePlot.growthProgress * 100f):F0}%";
                }

                if (waterText != null)
                {
                    waterText.text = $"Độ ẩm đất: {(activePlot.waterLevel * 100f):F0}%";
                }

                if (sunPreferenceText != null)
                {
                    string sunStr = plant.preferredSun == SunlightLevel.High ? "Nhiều nắng" : (plant.preferredSun == SunlightLevel.Medium ? "Nắng vừa" : "Ít nắng");
                    sunPreferenceText.text = $"Ưa thích: {sunStr}";
                }

                if (harvestButton != null)
                {
                    harvestButton.gameObject.SetActive(activePlot.growthProgress >= 1.0f && !activePlot.isWithered);
                }

                if (speedUpGemsButton != null)
                {
                    speedUpGemsButton.gameObject.SetActive(activePlot.growthProgress < 1.0f && !activePlot.isWithered);
                }
            }
            else
            {
                if (plantNameText != null) plantNameText.text = "Chậu cây trống";
                if (stageText != null) stageText.text = "Kéo hạt giống vào chậu để gieo trồng!";
                if (progressText != null) progressText.text = "";
                if (waterText != null) waterText.text = "";
                if (sunPreferenceText != null) sunPreferenceText.text = "";
                if (plantPreviewImage != null) plantPreviewImage.gameObject.SetActive(false);
                if (harvestButton != null) harvestButton.gameObject.SetActive(false);
                if (speedUpGemsButton != null) speedUpGemsButton.gameObject.SetActive(false);
            }

            if (soilTypeText != null)
            {
                string soilStr = activePlot.soilType == SoilType.Normal ? "Đất thường" : (activePlot.soilType == SoilType.Nutrient ? "Đất dinh dưỡng (+20% tốc độ)" : "Đất thoát nước (giữ ẩm lâu)");
                soilTypeText.text = $"Loại đất: {soilStr}";
            }
        }

        private void OnWaterClick()
        {
            if (activePlot != null)
            {
                activePlot.Water();
                if (UIManager.Instance != null) UIManager.Instance.ShowFloatingMessage("Đã tưới nước đầy đủ! 💧");
                RefreshInfo();
            }
        }

        private void OnHarvestClick()
        {
            if (activePlot != null && activePlot.Harvest(out PlantData plant, out int count))
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowFloatingMessage($"Đã thu hoạch +{count} {plant.plantName}! 🌸");
                    UIManager.Instance.RefreshBottomBar();
                }
                Close();
            }
        }

        private void OnClearClick()
        {
            if (activePlot != null)
            {
                activePlot.ClearPlot();
                if (UIManager.Instance != null) UIManager.Instance.ShowFloatingMessage("Đã dọn sạch ô chậu.");
                Close();
            }
        }

        private void OnSpeedUpClick()
        {
            if (activePlot != null && activePlot.currentPlant != null && activePlot.growthProgress < 1.0f)
            {
                // Dùng 1 Hạt ngọc để cây lớn ngay lập tức (Casual F2P gem rush)
                if (GameManager.Instance.SpendGems(1))
                {
                    activePlot.growthProgress = 1.0f;
                    activePlot.UpdateVisuals();
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayLevelUp();
                    if (UIManager.Instance != null) UIManager.Instance.ShowFloatingMessage("Dùng 1 Hạt ngọc làm cây lớn tức thì! ✨");
                    RefreshInfo();
                }
                else
                {
                    if (UIManager.Instance != null) UIManager.Instance.ShowFloatingMessage("Không đủ Hạt ngọc!");
                }
            }
        }
    }
}
