using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BalconyGarden
{
    public class BreedingUI : MonoBehaviour
    {
        [Header("Tham chiếu chung")]
        public GameObject panelRoot;
        public Button closeButton;

        [Header("Chuyển Tab")]
        public Button tabCodexButton;
        public Button tabBreedingButton;
        public GameObject codexTabRoot;
        public GameObject breedingTabRoot;

        [Header("Tab Sổ tay hoa (Codex)")]
        public Transform codexContainer;
        public GameObject codexCardPrefab;

        [Header("Tab Lai tạo hoa")]
        public TMP_Dropdown flowerDropdownA;
        public TMP_Dropdown flowerDropdownB;
        public Image previewFlowerA;
        public Image previewFlowerB;
        public Button breedButton;
        public TextMeshProUGUI breedResultText;

        private List<string> availableFlowerIds = new List<string>();

        private void Start()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (tabCodexButton != null) tabCodexButton.onClick.AddListener(() => SwitchTab(true));
            if (tabBreedingButton != null) tabBreedingButton.onClick.AddListener(() => SwitchTab(false));
            if (breedButton != null) breedButton.onClick.AddListener(OnBreedClick);

            if (flowerDropdownA != null) flowerDropdownA.onValueChanged.AddListener((val) => UpdateBreedingPreviews());
            if (flowerDropdownB != null) flowerDropdownB.onValueChanged.AddListener((val) => UpdateBreedingPreviews());
        }

        public void Open()
        {
            if (panelRoot != null) panelRoot.SetActive(true);
            SwitchTab(true);
        }

        public void Close()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        }

        private void SwitchTab(bool isCodex)
        {
            if (codexTabRoot != null) codexTabRoot.SetActive(isCodex);
            if (breedingTabRoot != null) breedingTabRoot.SetActive(!isCodex);

            if (isCodex)
            {
                RefreshCodex();
            }
            else
            {
                RefreshBreedingOptions();
            }
        }

        private void RefreshCodex()
        {
            if (codexContainer == null || codexCardPrefab == null) return;

            foreach (Transform child in codexContainer)
            {
                Destroy(child.gameObject);
            }

            var allPlants = GameManager.Instance.registeredPlants;

            foreach (var plant in allPlants)
            {
                if (plant.plantType != PlantType.Flower) continue; // Chỉ hiển thị hoa

                bool isDiscovered = InventorySystem.Instance.HasDiscoveredFlower(plant.plantId);

                var card = Instantiate(codexCardPrefab, codexContainer);
                var texts = card.GetComponentsInChildren<TextMeshProUGUI>();
                var images = card.GetComponentsInChildren<Image>();

                TextMeshProUGUI nameTxt = texts.Length > 0 ? texts[0] : null;
                TextMeshProUGUI descTxt = texts.Length > 1 ? texts[1] : null;

                if (nameTxt != null)
                {
                    nameTxt.text = isDiscovered ? plant.plantName : "??? (Chưa mở khóa)";
                }

                if (descTxt != null)
                {
                    descTxt.text = isDiscovered ? plant.description : "Lai tạo hoặc gieo hạt để khám phá loài hoa này!";
                }

                Image iconImg = null;
                foreach (var img in images)
                {
                    if (img.gameObject.name.Contains("Icon"))
                    {
                        iconImg = img;
                        break;
                    }
                }

                if (iconImg != null)
                {
                    if (isDiscovered && plant.cropItemSprite != null)
                    {
                        iconImg.sprite = plant.cropItemSprite;
                        iconImg.color = Color.white;
                    }
                    else
                    {
                        iconImg.sprite = plant.cropItemSprite;
                        iconImg.color = new Color(0.2f, 0.2f, 0.2f, 0.6f); // Bóng đen bí ẩn
                    }
                }
            }
        }

        private void RefreshBreedingOptions()
        {
            availableFlowerIds.Clear();
            var crops = InventorySystem.Instance.GetAllCrops();

            foreach (var kvp in crops)
            {
                if (kvp.Value > 0)
                {
                    var plant = GameManager.Instance.GetPlantDataById(kvp.Key);
                    if (plant != null && plant.plantType == PlantType.Flower)
                    {
                        availableFlowerIds.Add(plant.plantId);
                    }
                }
            }

            var options = new List<TMP_Dropdown.OptionData>();
            foreach (var fId in availableFlowerIds)
            {
                var plant = GameManager.Instance.GetPlantDataById(fId);
                int count = InventorySystem.Instance.GetCropCount(fId);
                options.Add(new TMP_Dropdown.OptionData($"{plant.plantName} (x{count})"));
            }

            if (options.Count == 0)
            {
                options.Add(new TMP_Dropdown.OptionData("Không có hoa nào trong kho"));
            }

            if (flowerDropdownA != null)
            {
                flowerDropdownA.ClearOptions();
                flowerDropdownA.AddOptions(options);
            }

            if (flowerDropdownB != null)
            {
                flowerDropdownB.ClearOptions();
                flowerDropdownB.AddOptions(options);
                if (options.Count > 1) flowerDropdownB.value = 1;
            }

            UpdateBreedingPreviews();

            if (breedResultText != null)
            {
                breedResultText.text = "Chọn 2 bông hoa từ kho để tiến hành lai tạo giống hoa mới.";
            }
        }

        private void UpdateBreedingPreviews()
        {
            if (availableFlowerIds.Count == 0)
            {
                if (previewFlowerA != null) previewFlowerA.gameObject.SetActive(false);
                if (previewFlowerB != null) previewFlowerB.gameObject.SetActive(false);
                return;
            }

            int indexA = flowerDropdownA != null ? flowerDropdownA.value : 0;
            int indexB = flowerDropdownB != null ? flowerDropdownB.value : 0;

            if (indexA < availableFlowerIds.Count && previewFlowerA != null)
            {
                var plantA = GameManager.Instance.GetPlantDataById(availableFlowerIds[indexA]);
                previewFlowerA.gameObject.SetActive(true);
                if (plantA != null && plantA.cropItemSprite != null) previewFlowerA.sprite = plantA.cropItemSprite;
            }

            if (indexB < availableFlowerIds.Count && previewFlowerB != null)
            {
                var plantB = GameManager.Instance.GetPlantDataById(availableFlowerIds[indexB]);
                previewFlowerB.gameObject.SetActive(true);
                if (plantB != null && plantB.cropItemSprite != null) previewFlowerB.sprite = plantB.cropItemSprite;
            }
        }

        private void OnBreedClick()
        {
            if (availableFlowerIds.Count == 0)
            {
                if (breedResultText != null) breedResultText.text = "Bạn chưa có bông hoa nào trong kho!";
                return;
            }

            int idxA = flowerDropdownA.value;
            int idxB = flowerDropdownB.value;

            if (idxA >= availableFlowerIds.Count || idxB >= availableFlowerIds.Count) return;

            string flowerA = availableFlowerIds[idxA];
            string flowerB = availableFlowerIds[idxB];

            if (BreedingSystem.Instance.TryBreed(flowerA, flowerB, out string newSeedId, out string message))
            {
                var newPlant = GameManager.Instance.GetPlantDataById(newSeedId);
                string newName = newPlant != null ? newPlant.plantName : newSeedId;
                if (breedResultText != null)
                {
                    breedResultText.text = $"🎉 {message}\nNhận được: 1x Hạt giống {newName}!";
                }
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowFloatingMessage($"Lai tạo thành công: Hạt {newName}! ✨");
                    UIManager.Instance.RefreshBottomBar();
                }
                RefreshBreedingOptions();
            }
            else
            {
                if (breedResultText != null)
                {
                    breedResultText.text = $"⚠️ {message}";
                }
            }
        }
    }
}
