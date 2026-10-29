using System;
using System.Collections.Generic;
using UnityEngine;

namespace BalconyGarden
{
    [Serializable]
    public class BreedingRecipe
    {
        public string parentFlowerIdA;
        public string parentFlowerIdB;
        public string resultPlantId;
        [Range(0f, 1f)]
        public float successRate = 0.8f;
        public string fallbackPlantId; // Nếu thất bại thì ra giống này
    }

    public class BreedingSystem : MonoBehaviour
    {
        public static BreedingSystem Instance { get; private set; }

        [Header("Công thức lai tạo hoa")]
        public List<BreedingRecipe> recipes = new List<BreedingRecipe>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializeDefaultRecipes();
        }

        private void InitializeDefaultRecipes()
        {
            if (recipes == null || recipes.Count == 0)
            {
                recipes = new List<BreedingRecipe>
                {
                    new BreedingRecipe
                    {
                        parentFlowerIdA = "cuc_vang",
                        parentFlowerIdB = "hoa_hong",
                        resultPlantId = "cuc_cam", // Cúc Cam lai tạo
                        successRate = 0.85f,
                        fallbackPlantId = "cuc_vang"
                    },
                    new BreedingRecipe
                    {
                        parentFlowerIdA = "cuc_vang",
                        parentFlowerIdB = "huong_duong",
                        resultPlantId = "cuc_mat_troi", // Cúc Mặt Trời lai tạo
                        successRate = 0.75f,
                        fallbackPlantId = "huong_duong"
                    },
                    new BreedingRecipe
                    {
                        parentFlowerIdA = "hoa_hong",
                        parentFlowerIdB = "huong_duong",
                        resultPlantId = "hoa_hong_xanh", // Hồng Xanh quý hiếm
                        successRate = 0.6f,
                        fallbackPlantId = "hoa_hong"
                    }
                };
            }
        }

        /// <summary>
        /// Thực hiện lai tạo từ 2 hoa mẹ
        /// </summary>
        public bool TryBreed(string flowerA, string flowerB, out string resultSeedId, out string message)
        {
            resultSeedId = "";
            message = "";

            // Kiểm tra kho có đủ 2 hoa mẹ không
            int countA = InventorySystem.Instance.GetCropCount(flowerA);
            int countB = InventorySystem.Instance.GetCropCount(flowerB);

            if (flowerA == flowerB)
            {
                if (countA < 2)
                {
                    message = "Bạn cần ít nhất 2 bông hoa cùng loại để nhân giống!";
                    return false;
                }
            }
            else
            {
                if (countA < 1 || countB < 1)
                {
                    message = "Bạn không có đủ hoa trong kho để lai tạo!";
                    return false;
                }
            }

            // Tìm công thức phù hợp
            BreedingRecipe match = recipes.Find(r =>
                (r.parentFlowerIdA == flowerA && r.parentFlowerIdB == flowerB) ||
                (r.parentFlowerIdA == flowerB && r.parentFlowerIdB == flowerA));

            // Tiêu thụ 2 hoa nguyên liệu
            if (flowerA == flowerB)
            {
                InventorySystem.Instance.UseCrop(flowerA, 2);
            }
            else
            {
                InventorySystem.Instance.UseCrop(flowerA, 1);
                InventorySystem.Instance.UseCrop(flowerB, 1);
            }

            float roll = UnityEngine.Random.value;

            if (match != null && roll <= match.successRate)
            {
                resultSeedId = match.resultPlantId;
                message = "Lai tạo thành công mỹ mãn! Bạn nhận được hạt giống mới.";
            }
            else if (match != null && !string.IsNullOrEmpty(match.fallbackPlantId))
            {
                resultSeedId = match.fallbackPlantId;
                message = "Lai tạo biến dị nhẹ, bạn nhận được hạt giống thường.";
            }
            else
            {
                // Ngẫu nhiên ra 1 trong 2 cây mẹ
                resultSeedId = (UnityEngine.Random.value > 0.5f) ? flowerA : flowerB;
                message = "Không có công thức đặc biệt, nhận lại hạt của cây mẹ.";
            }

            // Thêm hạt giống mới vào kho và ghi nhận vào sổ tay hoa
            InventorySystem.Instance.AddSeed(resultSeedId, 1);
            InventorySystem.Instance.DiscoverFlower(resultSeedId);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBreeding();
            }

            return true;
        }
    }
}
