using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace BalconyGarden
{
    public enum ToolType
    {
        Seed,
        WateringCan,
        Shovel,
        Sickle
    }

    public class DraggableToolItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Cấu hình công cụ / hạt giống")]
        public ToolType toolType = ToolType.Seed;
        public string plantId = "cai_xanh";
        public Image iconImage;
        public TextMeshProUGUI countText;
        public TextMeshProUGUI nameText;

        [Header("Kéo thả")]
        public Canvas parentCanvas;

        private GameObject dragGhost;
        private RectTransform ghostRect;
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        public void Setup(ToolType type, string id, Sprite icon, int count, string displayName, Canvas canvas)
        {
            this.toolType = type;
            this.plantId = id;
            this.parentCanvas = canvas;

            if (iconImage != null && icon != null)
            {
                iconImage.sprite = icon;
                iconImage.gameObject.SetActive(true);
            }

            if (countText != null)
            {
                countText.text = count > 0 ? $"x{count}" : "";
                countText.gameObject.SetActive(count > 0 || type == ToolType.Seed);
            }

            if (nameText != null)
            {
                nameText.text = displayName;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (toolType == ToolType.Seed && InventorySystem.Instance.GetSeedCount(plantId) <= 0)
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowFloatingMessage("Hết hạt giống này rồi! Hãy mua thêm ở Cửa hàng.");
                }
                eventData.pointerDrag = null;
                return;
            }

            // Tạo icon bóng ma (ghost) bay theo con trỏ chuột / ngón tay
            dragGhost = new GameObject("DragGhost", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            dragGhost.transform.SetParent(parentCanvas != null ? parentCanvas.transform : transform.root, false);

            ghostRect = dragGhost.GetComponent<RectTransform>();
            ghostRect.sizeDelta = new Vector2(80, 80);

            var ghostImage = dragGhost.GetComponent<Image>();
            ghostImage.sprite = iconImage != null ? iconImage.sprite : null;
            ghostImage.color = new Color(1f, 1f, 1f, 0.85f);
            ghostImage.raycastTarget = false;

            var ghostGroup = dragGhost.GetComponent<CanvasGroup>();
            ghostGroup.blocksRaycasts = false;

            UpdateGhostPosition(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragGhost != null)
            {
                UpdateGhostPosition(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (dragGhost != null)
            {
                Destroy(dragGhost);
            }

            // Raycast kiểm tra ô chậu bên dưới con trỏ
            PlantPlot targetPlot = RaycastForPlot(eventData);

            if (targetPlot != null)
            {
                ApplyToolToPlot(targetPlot);
            }
        }

        private void UpdateGhostPosition(PointerEventData eventData)
        {
            if (ghostRect == null) return;

            if (parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                ghostRect.position = eventData.position;
            }
            else if (parentCanvas != null && parentCanvas.worldCamera != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentCanvas.transform as RectTransform,
                    eventData.position,
                    parentCanvas.worldCamera,
                    out Vector2 localPoint);
                ghostRect.localPosition = localPoint;
            }
            else
            {
                ghostRect.position = eventData.position;
            }
        }

        private PlantPlot RaycastForPlot(PointerEventData eventData)
        {
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
            {
                var plot = result.gameObject.GetComponentInParent<PlantPlot>();
                if (plot != null)
                {
                    return plot;
                }
            }

            return null;
        }

        private void ApplyToolToPlot(PlantPlot plot)
        {
            if (!plot.isUnlocked)
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowFloatingMessage($"Chậu này đang khóa! Mở khóa với {plot.unlockCost} Xu.");
                }
                return;
            }

            switch (toolType)
            {
                case ToolType.Seed:
                    if (plot.currentPlant != null)
                    {
                        if (UIManager.Instance != null)
                        {
                            UIManager.Instance.ShowFloatingMessage("Chậu này đang có cây!");
                        }
                        return;
                    }

                    if (InventorySystem.Instance.UseSeed(plantId, 1))
                    {
                        var pData = GameManager.Instance.GetPlantDataById(plantId);
                        if (pData != null)
                        {
                            plot.PlantSeed(pData);
                            if (UIManager.Instance != null)
                            {
                                UIManager.Instance.ShowFloatingMessage($"Đã gieo {pData.plantName}! 🌱");
                                UIManager.Instance.RefreshBottomBar();
                            }
                        }
                    }
                    break;

                case ToolType.WateringCan:
                    if (plot.currentPlant == null)
                    {
                        if (UIManager.Instance != null)
                        {
                            UIManager.Instance.ShowFloatingMessage("Chậu trống, hãy gieo hạt trước!");
                        }
                        return;
                    }

                    plot.Water();
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ShowFloatingMessage("Đã tưới nước đầy đủ! 💧");
                    }
                    break;

                case ToolType.Shovel:
                    if (plot.currentPlant != null)
                    {
                        plot.ClearPlot();
                        if (UIManager.Instance != null)
                        {
                            UIManager.Instance.ShowFloatingMessage("Đã xới đất dọn chậu! 🌿");
                        }
                    }
                    break;

                case ToolType.Sickle:
                    if (plot.currentPlant != null && plot.growthProgress >= 1.0f)
                    {
                        if (plot.Harvest(out PlantData harvested, out int count))
                        {
                            if (UIManager.Instance != null)
                            {
                                UIManager.Instance.ShowFloatingMessage($"Đã thu hoạch +{count} {harvested.plantName}! ✨");
                                UIManager.Instance.RefreshBottomBar();
                            }
                        }
                    }
                    else if (plot.currentPlant != null)
                    {
                        if (UIManager.Instance != null)
                        {
                            UIManager.Instance.ShowFloatingMessage("Cây chưa chín, hãy kiên nhẫn nhé!");
                        }
                    }
                    break;
            }
        }
    }
}
