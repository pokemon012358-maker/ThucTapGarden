using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BalconyGarden
{
    public enum SoilType
    {
        Normal,    // Đất thường
        Nutrient,  // Đất dinh dưỡng (lớn nhanh hơn 20%)
        Drainage   // Đất thoát nước (giữ ẩm lâu hơn 50%)
    }

    public class PlantPlot : MonoBehaviour
    {
        [Header("Thông tin ô chậu")]
        public int plotIndex = 0;
        public bool isUnlocked = true;
        public int unlockCost = 100;
        public SunlightLevel sunlightLevel = SunlightLevel.Medium;
        public SoilType soilType = SoilType.Normal;

        [Header("Trạng thái cây hiện tại")]
        public PlantData currentPlant;
        [Range(0f, 1f)] public float growthProgress = 0f;
        [Range(0f, 1f)] public float waterLevel = 1.0f;
        public bool isWithered = false;

        private float dryTimer = 0f;
        private const float DryTimeToWither = 45f; // Khô nước 45s sẽ héo

        [Header("Tham chiếu UI / Hiển thị")]
        public Image potImage;
        public Image plantImage;
        public Image waterFillImage;
        public GameObject waterBarContainer;
        public GameObject thirstyBadge;
        public GameObject ripeBadge;
        public GameObject witheredBadge;
        public GameObject lockOverlay;
        public TextMeshProUGUI lockCostText;
        public TextMeshProUGUI sunBadgeText;

        private const float PlantPixelScale = 0.5f;   // <-- thêm dòng này

        public event Action<PlantPlot> OnPlotUpdated;

        private void Start()
        {
            UpdateSunlightBadge();
            UpdateVisuals();
        }

        private void Update()
        {
            if (!isUnlocked || currentPlant == null) return;

            float dt = TimeManager.Instance != null ? TimeManager.Instance.GetDeltaTime() : Time.deltaTime;
            TickGrowth(dt);
        }

        public void TickGrowth(float dt)
        {
            if (isWithered) return;

            // Nếu đã chín thì không tăng tiến độ nữa
            if (growthProgress >= 1.0f)
            {
                UpdateVisuals();
                return;
            }

            // Tiêu thụ nước
            float waterDrain = currentPlant.waterConsumptionRate;
            if (soilType == SoilType.Drainage) waterDrain *= 0.65f; // Giữ ẩm lâu hơn

            waterLevel = Mathf.Clamp01(waterLevel - waterDrain * dt);

            // Kiểm tra thiếu nước
            if (waterLevel <= 0f)
            {
                dryTimer += dt;
                if (dryTimer >= DryTimeToWither)
                {
                    isWithered = true;
                    UpdateVisuals();
                    OnPlotUpdated?.Invoke(this);
                    return;
                }
            }
            else
            {
                dryTimer = 0f;

                // Tốc độ tăng trưởng
                float speedMultiplier = 1.0f;

                // Thưởng / phạt ánh nắng
                if (currentPlant.preferredSun == sunlightLevel)
                {
                    speedMultiplier *= 1.25f; // Hợp nắng lớn nhanh hơn 25%
                }
                else if (Mathf.Abs((int)currentPlant.preferredSun - (int)sunlightLevel) >= 2)
                {
                    speedMultiplier *= 0.8f; // Trái nắng lớn chậm hơn
                }

                // Thưởng đất
                if (soilType == SoilType.Nutrient)
                {
                    speedMultiplier *= 1.2f; // Đất dinh dưỡng nhanh hơn 20%
                }

                // Cheat test nhanh nếu bật
                if (GameManager.Instance != null && GameManager.Instance.debugFastGrowth)
                {
                    speedMultiplier *= GameManager.Instance.debugGrowthMultiplier;
                }

                float growthRate = (1.0f / Mathf.Max(5f, currentPlant.growthTimeSeconds)) * speedMultiplier;
                growthProgress = Mathf.Clamp01(growthProgress + growthRate * dt);
            }

            UpdateVisuals();
            OnPlotUpdated?.Invoke(this);
        }

        public void SimulateOffline(float offlineSeconds)
        {
            if (!isUnlocked || currentPlant == null || isWithered || growthProgress >= 1.0f) return;

            // Ước lượng nước tiêu thụ khi offline
            float waterDrainRate = currentPlant.waterConsumptionRate * (soilType == SoilType.Drainage ? 0.65f : 1.0f);
            float secondsUntilDry = waterDrainRate > 0 ? (waterLevel / waterDrainRate) : offlineSeconds;

            if (offlineSeconds <= secondsUntilDry)
            {
                // Vẫn đủ nước trong toàn bộ thời gian offline
                waterLevel = Mathf.Clamp01(waterLevel - waterDrainRate * offlineSeconds);
                float speed = (currentPlant.preferredSun == sunlightLevel) ? 1.25f : 1.0f;
                float growthRate = (1.0f / currentPlant.growthTimeSeconds) * speed;
                growthProgress = Mathf.Clamp01(growthProgress + growthRate * offlineSeconds);
            }
            else
            {
                // Nước bị cạn giữa chừng
                float wetSeconds = secondsUntilDry;
                float speed = (currentPlant.preferredSun == sunlightLevel) ? 1.25f : 1.0f;
                float growthRate = (1.0f / currentPlant.growthTimeSeconds) * speed;
                growthProgress = Mathf.Clamp01(growthProgress + growthRate * wetSeconds);
                waterLevel = 0f;

                float drySeconds = offlineSeconds - wetSeconds;
                if (drySeconds > DryTimeToWither)
                {
                    isWithered = true;
                }
            }

            UpdateVisuals();
        }

        public bool PlantSeed(PlantData plant)
        {
            if (!isUnlocked || currentPlant != null) return false;

            currentPlant = plant;
            growthProgress = 0f;
            waterLevel = 1.0f; // Tự động tưới nước lần đầu khi gieo
            isWithered = false;
            dryTimer = 0f;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPlanting();
            }

            UpdateVisuals();
            OnPlotUpdated?.Invoke(this);
            return true;
        }

        public bool Water()
        {
            if (!isUnlocked || currentPlant == null) return false;

            waterLevel = 1.0f;
            dryTimer = 0f;

            if (isWithered)
            {
                // Tưới nước hồi phục cây héo
                isWithered = false;
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayWatering();
            }

            UpdateVisuals();
            OnPlotUpdated?.Invoke(this);
            return true;
        }

        public bool Harvest(out PlantData harvestedPlant, out int yieldCount)
        {
            harvestedPlant = null;
            yieldCount = 0;

            if (!isUnlocked || currentPlant == null || growthProgress < 1.0f || isWithered)
            {
                return false;
            }

            harvestedPlant = currentPlant;
            yieldCount = currentPlant.harvestYield;

            // Thưởng XP & sản phẩm vào kho
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddXP(currentPlant.xpReward);
            }
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.AddCrop(currentPlant.plantId, yieldCount);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayHarvest();
            }

            // Làm sạch chậu sau thu hoạch
            currentPlant = null;
            growthProgress = 0f;
            waterLevel = 1.0f;
            isWithered = false;

            UpdateVisuals();
            OnPlotUpdated?.Invoke(this);
            return true;
        }

        public bool ClearPlot()
        {
            if (!isUnlocked || currentPlant == null) return false;

            currentPlant = null;
            growthProgress = 0f;
            waterLevel = 1.0f;
            isWithered = false;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayClick();
            }

            UpdateVisuals();
            OnPlotUpdated?.Invoke(this);
            return true;
        }

        public bool Unlock()
        {
            if (isUnlocked) return false;

            if (GameManager.Instance.SpendCoins(unlockCost))
            {
                isUnlocked = true;
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayCoin();
                }
                UpdateVisuals();
                OnPlotUpdated?.Invoke(this);
                return true;
            }

            return false;
        }

        public void UpdateVisuals()
        {
            if (lockOverlay != null)
            {
                lockOverlay.SetActive(!isUnlocked);
                if (lockCostText != null)
                {
                    lockCostText.text = $"{unlockCost} Xu";
                }
            }

            if (!isUnlocked)
            {
                if (plantImage != null) plantImage.gameObject.SetActive(false);
                if (waterBarContainer != null) waterBarContainer.SetActive(false);
                if (thirstyBadge != null) thirstyBadge.SetActive(false);
                if (ripeBadge != null) ripeBadge.SetActive(false);
                if (witheredBadge != null) witheredBadge.SetActive(false);
                return;
            }

            if (currentPlant != null)
            {
                if (plantImage != null)
                {
                    plantImage.gameObject.SetActive(true);
                    Sprite targetSprite = currentPlant.GetSpriteForProgress(growthProgress, isWithered);
                    if (targetSprite != null)
                    {
                        plantImage.sprite = targetSprite;
                        plantImage.preserveAspect = true;
                        plantImage.SetNativeSize();                       // khung = đúng kích thước ảnh gốc
                        plantImage.rectTransform.sizeDelta *= PlantPixelScale;  // phóng đều mọi giai đoạn
                    }
                }

                if (waterBarContainer != null)
                {
                    waterBarContainer.SetActive(true);
                    if (waterFillImage != null)
                    {
                        waterFillImage.fillAmount = waterLevel;
                    }
                }

                // Trạng thái khát nước
                if (thirstyBadge != null)
                {
                    thirstyBadge.SetActive(waterLevel < 0.25f && !isWithered && growthProgress < 1.0f);
                }

                // Trạng thái đã chín
                if (ripeBadge != null)
                {
                    ripeBadge.SetActive(growthProgress >= 1.0f && !isWithered);
                }

                // Trạng thái héo
                if (witheredBadge != null)
                {
                    witheredBadge.SetActive(isWithered);
                }
            }
            else
            {
                if (plantImage != null) plantImage.gameObject.SetActive(false);
                if (waterBarContainer != null) waterBarContainer.SetActive(false);
                if (thirstyBadge != null) thirstyBadge.SetActive(false);
                if (ripeBadge != null) ripeBadge.SetActive(false);
                if (witheredBadge != null) witheredBadge.SetActive(false);
            }
        }

        private void UpdateSunlightBadge()
        {
            if (sunBadgeText != null)
            {
                switch (sunlightLevel)
                {
                    case SunlightLevel.Low:
                        sunBadgeText.text = "Low Sunlight";
                        break;
                    case SunlightLevel.Medium:
                        sunBadgeText.text = "Medium Sunlight";
                        break;
                    case SunlightLevel.High:
                        sunBadgeText.text = "High Sunlight";
                        break;
                }
            }
        }

        public void OnClickPlot()
        {
            if (!isUnlocked)
            {
                // Thử mở khóa
                Unlock();
                return;
            }

            if (currentPlant == null)
            {
                // Mở giao diện khay chọn hạt giống
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.SelectPlotForPlanting(this);
                }
            }
            else if (growthProgress >= 1.0f && !isWithered)
            {
                // Thu hoạch khi bấm
                if (Harvest(out PlantData plant, out int count))
                {
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ShowFloatingMessage($"+{count} {plant.plantName}!");
                    }
                }
            }
            else if (isWithered || waterLevel < 0.35f)
            {
                // Tưới nước hồi phục
                Water();
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowFloatingMessage("Watered the plant!");
                }
            }
            else
            {
                // Xem chi tiết ô chậu
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.OpenPlotInfo(this);
                }
            }
        }

    }
}
