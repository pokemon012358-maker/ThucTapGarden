using UnityEngine;

namespace BalconyGarden
{
    public enum PlantType
    {
        Vegetable,
        Flower
    }

    public enum SunlightLevel
    {
        Low,
        Medium,
        High
    }

    [CreateAssetMenu(fileName = "NewPlantData", menuName = "Balcony Garden/Plant Data")]
    public class PlantData : ScriptableObject
    {
        [Header("Thông tin cơ bản")]
        public string plantId = "cai_xanh";
        public string plantName = "Cải Xanh";
        public PlantType plantType = PlantType.Vegetable;
        [TextArea] public string description = "Rau xanh tươi mát, dễ trồng cho người mới.";

        [Header("Thời gian & Tăng trưởng")]
        [Tooltip("Thời gian phát triển (giây)")]
        public float growthTimeSeconds = 120f;
        [Tooltip("Mức tiêu thụ nước mỗi giây (0..1)")]
        public float waterConsumptionRate = 0.008f;
        [Tooltip("Mức nắng ưa thích")]
        public SunlightLevel preferredSun = SunlightLevel.Medium;

        [Header("Kinh tế & Cấp độ")]
        public int buyPrice = 5;
        public int sellPrice = 12;
        public int unlockLevel = 1;
        public int xpReward = 15;
        public int harvestYield = 2; // Số lượng thu hoạch được

        [Header("Sprites (Thay thế ảnh của bạn tại đây)")]
        [Tooltip("Ảnh túi hạt giống hiển thị trong kho / khay gieo hạt")]
        public Sprite seedPacketSprite;
        [Tooltip("Giai đoạn 1: Hạt mới gieo")]
        public Sprite stageSeedSprite;
        [Tooltip("Giai đoạn 2: Mầm non")]
        public Sprite stageSproutSprite;
        [Tooltip("Giai đoạn 3: Cây đang lớn")]
        public Sprite stageMatureSprite;
        [Tooltip("Giai đoạn 4: Cây chín / nở hoa (sẵn sàng thu hoạch)")]
        public Sprite stageRipeSprite;
        [Tooltip("Giai đoạn héo khi thiếu nước")]
        public Sprite stageWitheredSprite;
        [Tooltip("Ảnh nông sản / bông hoa khi thu hoạch")]
        public Sprite cropItemSprite;

        /// <summary>
        /// Lấy sprite tương ứng theo tiến độ phát triển (0..1) hoặc trạng thái héo
        /// </summary>
        public Sprite GetSpriteForProgress(float progress, bool isWithered)
        {
            if (isWithered)
            {
                return stageWitheredSprite != null ? stageWitheredSprite : stageMatureSprite;
            }

            if (progress < 0.25f)
            {
                return stageSeedSprite;
            }
            if (progress < 0.60f)
            {
                return stageSproutSprite;
            }
            if (progress < 1.0f)
            {
                return stageMatureSprite;
            }

            return stageRipeSprite;
        }
    }
}
