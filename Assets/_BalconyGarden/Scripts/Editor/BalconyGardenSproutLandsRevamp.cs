#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

namespace BalconyGarden.Editor
{
    public static class BalconyGardenSproutLandsRevamp
    {
        private const string ScenePath = "Assets/_BalconyGarden/Scenes/BalconyGardenMain.unity";
        private const string SpritesPath = "Assets/_BalconyGarden/Sprites";
        private const string PlantsSOPath = "Assets/_BalconyGarden/ScriptableObjects/Plants";
        private const string NeighborsSOPath = "Assets/_BalconyGarden/ScriptableObjects/Neighbors";

        [MenuItem("Balcony Garden/🌟 Áp dụng Sprout Lands UI Pack gốc + Font", false, 0)]
        public static void ApplySproutLandsUIMenu()
        {
            ApplySproutLandsUI(true);
        }

        [InitializeOnLoadMethod]
        private static void AutoApplyOnInit()
        {
            EditorApplication.delayCall += () =>
            {
                // Chỉ dựng scene lần đầu. Không ghi đè scene đã có khi mở lại Unity / compile script.
                if (!File.Exists(ScenePath))
                {
                    ApplySproutLandsUI(false);
                }
            };
        }

        public static void ApplySproutLandsUI(bool showDialog = false)
        {
            Debug.Log("[BalconyGarden] Đang nạp UI Pack Sprout Lands gốc và font pixel...");
            SproutLandsPackAssets.PrepareAndLoad();

            Debug.Log("[BalconyGarden] Đang giữ texture cây/chậu, dựng lại UI bằng sprite pack...");
            SproutLandsTextureRefiner.GenerateCozyPixelTextures();
            RebuildSceneWithCozySproutLands();

            if (showDialog)
            {
                EditorUtility.DisplayDialog("Đã áp dụng Sprout Lands UI Pack!",
                    "Giao diện dùng sprite gốc từ Basic Pack + font pixelFont-7-8x14-sproutLands.\n\n" +
                    "Tiếng Việt dùng LiberationSans làm fallback vì font pack chỉ có chữ cái tiếng Anh và số.\n\n" +
                    "Nhấn PLAY để xem.",
                    "OK");
            }
        }

        private static void RebuildSceneWithCozySproutLands()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SproutLandsPackAssets.PrepareAndLoad();

            Sprite woodPanel = SproutLandsPackAssets.PanelBlank;
            Sprite parchmentPanel = SproutLandsPackAssets.DialogBar != null ? SproutLandsPackAssets.DialogBar : SproutLandsPackAssets.PanelHeader;
            Sprite slotBox = SproutLandsPackAssets.SlotLarge != null ? SproutLandsPackAssets.SlotLarge : SproutLandsPackAssets.SlotSmall;
            Sprite btnNormal = SproutLandsPackAssets.ButtonNormal;
            Sprite potSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Environment/pot_empty.png");
            Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Environment/balcony_bg.png");

            // 1. Camera
            var camObj = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            var cam = camObj.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.72f, 0.88f, 0.95f);
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            camObj.transform.position = new Vector3(0, 0, -10);

            // 2. EventSystem
            var eventObj = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            // 3. Canvas chính (Chuẩn Portrait 1080x1920)
            var canvasObj = new GameObject("MainCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            // 4. Managers
            var managersObj = new GameObject("--- GAME MANAGERS ---");
            var gameMgr = managersObj.AddComponent<GameManager>();
            var timeMgr = managersObj.AddComponent<TimeManager>();
            var plotMgr = managersObj.AddComponent<PlotManager>();
            var invSys = managersObj.AddComponent<InventorySystem>();
            var neighborSys = managersObj.AddComponent<NeighborSystem>();
            var breedingSys = managersObj.AddComponent<BreedingSystem>();
            var audioMgr = managersObj.AddComponent<AudioManager>();
            var uiMgr = managersObj.AddComponent<UIManager>();
            uiMgr.mainCanvas = canvas;

            LoadAssetsIntoManagers(gameMgr, neighborSys);

            // 5. Hình nền Ban Công Ấm Cúng
            var bgObj = CreateUIObject("Background", canvasObj.transform);
            var bgImg = bgObj.AddComponent<Image>();
            bgImg.sprite = bgSprite;
            SetStretch(bgObj.GetComponent<RectTransform>());

            // 6. Kệ gỗ Ban công (Balcony Planter Stand)
            var shelfObj = CreateUIObject("BalconyStand", bgObj.transform);
            var shelfRect = shelfObj.GetComponent<RectTransform>();
            shelfRect.anchorMin = new Vector2(0.03f, 0.27f);
            shelfRect.anchorMax = new Vector2(0.97f, 0.89f);
            shelfRect.offsetMin = Vector2.zero;
            shelfRect.offsetMax = Vector2.zero;

            var shelfImg = shelfObj.AddComponent<Image>();
            SproutLandsPackAssets.ApplySliced(shelfImg, woodPanel, 0.32f);
            shelfImg.color = Color.white;

            // Dải ruy băng tiêu đề "VƯỜN BAN CÔNG"
            var titleBanner = CreateUIObject("TitleBanner", shelfObj.transform);
            var tbRect = titleBanner.GetComponent<RectTransform>();
            tbRect.anchorMin = new Vector2(0.18f, 0.935f);
            tbRect.anchorMax = new Vector2(0.82f, 0.985f);
            tbRect.offsetMin = Vector2.zero;
            tbRect.offsetMax = Vector2.zero;

            var tbImg = titleBanner.AddComponent<Image>();
            SproutLandsPackAssets.ApplySliced(tbImg, parchmentPanel, 0.28f);

            var titleTxt = CreateText("Label", titleBanner.transform, "- VƯỜN BAN CÔNG CỦA BẠN -", 28, new Color(0.38f, 0.20f, 0.08f), TextAlignmentOptions.Center, FontStyles.Bold);
            SetStretch(titleTxt.GetComponent<RectTransform>());

            // 7. Lưới 6 Chậu Cây (Planter Grid)
            var gridObj = CreateUIObject("PlotsGrid", shelfObj.transform);
            var grRect = gridObj.GetComponent<RectTransform>();
            grRect.anchorMin = new Vector2(0.03f, 0.02f);
            grRect.anchorMax = new Vector2(0.97f, 0.92f);
            grRect.offsetMin = Vector2.zero;
            grRect.offsetMax = Vector2.zero;

            var grid = gridObj.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(455, 280);
            grid.spacing = new Vector2(25, 20);
            grid.childAlignment = TextAnchor.MiddleCenter;

            plotMgr.plots = new List<PlantPlot>();
            for (int i = 0; i < 6; i++)
            {
                var plot = CreateCozyPlot(i, gridObj.transform, slotBox, potSprite, btnNormal);
                plotMgr.plots.Add(plot);
            }

            // 8. Top HUD (Thanh trạng thái đầu game)
            CreateCozyTopHUD(canvasObj.transform, uiMgr, woodPanel, slotBox, btnNormal);

            // 9. Bottom Action Hotbar (Khay công cụ & Hạt giống & Nút bấm)
            CreateCozyBottomHotbar(canvasObj.transform, uiMgr, canvas, woodPanel, slotBox, btnNormal);

            // 10. Popups (Cửa hàng, Kho, Hàng xóm, Sổ hoa, Chi tiết ô chậu, Lên cấp)
            CreateCozyPopups(canvasObj.transform, uiMgr, woodPanel, parchmentPanel, slotBox, btnNormal);

            // Cập nhật trạng thái hiển thị chuẩn cho 6 ô chậu (3 ô đầu mở, 3 ô sau khóa)
            for (int i = 0; i < plotMgr.plots.Count; i++)
            {
                var p = plotMgr.plots[i];
                p.isUnlocked = (i < 3);
                if (i == 3) p.unlockCost = 100;
                else if (i == 4) p.unlockCost = 250;
                else if (i == 5) p.unlockCost = 600;

                if (i < 2) p.sunlightLevel = SunlightLevel.High;
                else if (i < 4) p.sunlightLevel = SunlightLevel.Medium;
                else p.sunlightLevel = SunlightLevel.Low;

                p.UpdateVisuals();
            }

            // Lưu scene
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static PlantPlot CreateCozyPlot(int index, Transform parent, Sprite slotBox, Sprite potSprite, Sprite btnSprite)
        {
            var plotObj = CreateUIObject($"Plot_{index}", parent);
            plotObj.GetComponent<RectTransform>().sizeDelta = new Vector2(455, 280);

            // Khung hộp Planter Box Sprout Lands
            var frameImg = plotObj.AddComponent<Image>();
            SproutLandsPackAssets.ApplySliced(frameImg, slotBox, 0.4f);

            // Chậu gốm đất nung với đất màu mỡ ở đáy
            var potBase = CreateUIObject("PotBase", plotObj.transform);
            var pbRect = potBase.GetComponent<RectTransform>();
            pbRect.anchorMin = new Vector2(0.12f, 0.08f);
            pbRect.anchorMax = new Vector2(0.88f, 0.72f);
            pbRect.offsetMin = Vector2.zero;
            pbRect.offsetMax = Vector2.zero;
            var potImg = potBase.AddComponent<Image>();
            potImg.sprite = potSprite;
            potImg.preserveAspect = true;

            // Hình ảnh Cây trồng (nằm cân đối phía trên chậu đất)
            var plantObj = CreateUIObject("PlantVisual", plotObj.transform);
            var plantRect = plantObj.GetComponent<RectTransform>();
            plantRect.anchorMin = new Vector2(0.15f, 0.25f);
            plantRect.anchorMax = new Vector2(0.85f, 0.92f);
            plantRect.offsetMin = Vector2.zero;
            plantRect.offsetMax = Vector2.zero;
            var plantImg = plantObj.AddComponent<Image>();
            plantImg.preserveAspect = true;

            // Thanh độ ẩm đất (Water Bar)
            var waterBar = CreateUIObject("WaterBar", plotObj.transform);
            var wbRect = waterBar.GetComponent<RectTransform>();
            wbRect.anchorMin = new Vector2(0.18f, 0.06f);
            wbRect.anchorMax = new Vector2(0.82f, 0.13f);
            wbRect.offsetMin = Vector2.zero;
            wbRect.offsetMax = Vector2.zero;

            var wbBg = waterBar.AddComponent<Image>();
            wbBg.color = new Color(0.28f, 0.18f, 0.10f); // Khung rãnh gỗ đậm

            var waterFill = CreateUIObject("Fill", waterBar.transform);
            var wfRect = waterFill.GetComponent<RectTransform>();
            wfRect.anchorMin = new Vector2(0.04f, 0.15f);
            wfRect.anchorMax = new Vector2(0.96f, 0.85f);
            wfRect.offsetMin = Vector2.zero;
            wfRect.offsetMax = Vector2.zero;

            var fillImg = waterFill.AddComponent<Image>();
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.color = new Color(0.25f, 0.65f, 0.95f); // Xanh dương tươi mát

            // Huy hiệu Nắng (Top Left)
            var sunBadge = CreateUIObject("SunBadge", plotObj.transform);
            var sunRect = sunBadge.GetComponent<RectTransform>();
            sunRect.anchorMin = new Vector2(0.06f, 0.82f);
            sunRect.anchorMax = new Vector2(0.60f, 0.96f);
            sunRect.offsetMin = Vector2.zero;
            sunRect.offsetMax = Vector2.zero;
            var sunTxt = sunBadge.AddComponent<TextMeshProUGUI>();
            sunTxt.fontSize = 20;
            sunTxt.fontStyle = FontStyles.Bold;
            sunTxt.color = new Color(0.85f, 0.45f, 0.05f);
            sunTxt.alignment = TextAlignmentOptions.Left;

            // Huy hiệu Khát nước (Top Right)
            var thirstyBadge = CreateBadge("ThirstyBadge", plotObj.transform, $"{SpritesPath}/UI/water_drop.png", new Vector2(0.80f, 0.76f), new Vector2(0.94f, 0.95f));

            // Huy hiệu Chín sẵn sàng thu hoạch (Top Right)
            var ripeBadge = CreateBadge("RipeBadge", plotObj.transform, $"{SpritesPath}/UI/sun.png", new Vector2(0.80f, 0.76f), new Vector2(0.94f, 0.95f));

            // Huy hiệu Héo
            var witherBadge = CreateBadge("WitherBadge", plotObj.transform, $"{SpritesPath}/UI/lock.png", new Vector2(0.80f, 0.76f), new Vector2(0.94f, 0.95f));
            witherBadge.GetComponent<Image>().color = new Color(0.9f, 0.25f, 0.25f);

            // Khung Khóa Chậu (Chỉ hiện ở ô 4, 5, 6 khi chưa mở khóa)
            var lockOverlay = CreateUIObject("LockOverlay", plotObj.transform);
            SetStretch(lockOverlay.GetComponent<RectTransform>());
            var lockBg = lockOverlay.AddComponent<Image>();
            lockBg.color = new Color(0.22f, 0.15f, 0.08f, 0.88f); // Màu gỗ sẫm ấm áp

            var lockIcon = CreateUIObject("LockIcon", lockOverlay.transform);
            lockIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(55, 55);
            lockIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 25);
            var liImg = lockIcon.AddComponent<Image>();
            liImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/UI/lock.png");
            liImg.preserveAspect = true;

            var lockTxtObj = CreateUIObject("LockCostText", lockOverlay.transform);
            var ltRect = lockTxtObj.GetComponent<RectTransform>();
            ltRect.anchoredPosition = new Vector2(0, -35);
            ltRect.sizeDelta = new Vector2(250, 45);
            var lockTxt = lockTxtObj.AddComponent<TextMeshProUGUI>();
            lockTxt.fontSize = 24;
            lockTxt.fontStyle = FontStyles.Bold;
            lockTxt.alignment = TextAlignmentOptions.Center;
            lockTxt.color = new Color(1.0f, 0.85f, 0.25f);

            var btn = plotObj.AddComponent<Button>();

            var plot = plotObj.AddComponent<PlantPlot>();
            plot.plotIndex = index;
            plot.potImage = potImg;
            plot.plantImage = plantImg;
            plot.waterFillImage = fillImg;
            plot.waterBarContainer = waterBar;
            plot.sunBadgeText = sunTxt;
            plot.thirstyBadge = thirstyBadge;
            plot.ripeBadge = ripeBadge;
            plot.witheredBadge = witherBadge;
            plot.lockOverlay = lockOverlay;
            plot.lockCostText = lockTxt;

            btn.onClick.AddListener(plot.OnClickPlot);
            return plot;
        }

        private static void CreateCozyTopHUD(Transform parent, UIManager uiMgr, Sprite woodPanel, Sprite slotBox, Sprite btnSprite)
        {
            var topBar = CreateUIObject("TopHUD", parent);
            var rect = topBar.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.03f, 0.905f);
            rect.anchorMax = new Vector2(0.97f, 0.99f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var bg = topBar.AddComponent<Image>();
            SproutLandsPackAssets.ApplySliced(bg, woodPanel, 0.32f);

            var layout = topBar.AddComponent<HorizontalLayoutGroup>();
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.padding = new RectOffset(20, 20, 10, 10);
            layout.spacing = 15;

            // 1. Level & XP Box
            var levelBox = CreateUIObject("LevelBox", topBar.transform);
            levelBox.GetComponent<RectTransform>().sizeDelta = new Vector2(280, 110);
            var lbImg = levelBox.AddComponent<Image>();
            SproutLandsPackAssets.ApplySliced(lbImg, slotBox, 0.45f);

            var lvlText = CreateText("LvText", levelBox.transform, "Lv.1", 26, new Color(0.38f, 0.20f, 0.08f), TextAlignmentOptions.Left, FontStyles.Bold);
            lvlText.GetComponent<RectTransform>().anchoredPosition = new Vector2(15, 24);
            uiMgr.levelText = lvlText;

            var xpSliderObj = CreateUIObject("XPSlider", levelBox.transform);
            var xpRect = xpSliderObj.GetComponent<RectTransform>();
            xpRect.sizeDelta = new Vector2(250, 20);
            xpRect.anchoredPosition = new Vector2(0, -12);
            var xpSlider = xpSliderObj.AddComponent<Slider>();

            var xpBg = xpSliderObj.AddComponent<Image>();
            xpBg.color = new Color(0.32f, 0.22f, 0.14f);

            var xpFill = CreateUIObject("Fill", xpSliderObj.transform);
            var xpFillImg = xpFill.AddComponent<Image>();
            xpFillImg.color = new Color(0.40f, 0.82f, 0.35f); // Xanh mầm lá
            SetStretch(xpFill.GetComponent<RectTransform>());
            xpSlider.fillRect = xpFill.GetComponent<RectTransform>();
            uiMgr.xpSlider = xpSlider;

            var xpValText = CreateText("XPText", levelBox.transform, "0/50 XP", 16, new Color(0.48f, 0.35f, 0.22f), TextAlignmentOptions.Center, FontStyles.Bold);
            xpValText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -34);
            uiMgr.xpText = xpValText;

            // 2. Xu Badge
            var coinBox = CreateUIObject("CoinBox", topBar.transform);
            coinBox.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 110);
            SproutLandsPackAssets.ApplySliced(coinBox.AddComponent<Image>(), SproutLandsPackAssets.SlotSmall, 0.45f);

            CreateIcon("Icon", coinBox.transform, AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/UI/coin.png"), new Vector2(40, 40), new Vector2(-55, 0));
            var coinText = CreateText("Coins", coinBox.transform, "50", 26, new Color(0.65f, 0.42f, 0.05f), TextAlignmentOptions.Left, FontStyles.Bold);
            coinText.GetComponent<RectTransform>().anchoredPosition = new Vector2(25, 0);
            uiMgr.coinsText = coinText;

            // 3. Ngọc Badge
            var gemBox = CreateUIObject("GemBox", topBar.transform);
            gemBox.GetComponent<RectTransform>().sizeDelta = new Vector2(170, 110);
            SproutLandsPackAssets.ApplySliced(gemBox.AddComponent<Image>(), SproutLandsPackAssets.SlotSmall, 0.45f);

            CreateIcon("Icon", gemBox.transform, AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/UI/gem.png"), new Vector2(38, 38), new Vector2(-48, 0));
            var gemText = CreateText("Gems", gemBox.transform, "5", 26, new Color(0.12f, 0.52f, 0.72f), TextAlignmentOptions.Left, FontStyles.Bold);
            gemText.GetComponent<RectTransform>().anchoredPosition = new Vector2(20, 0);
            uiMgr.gemsText = gemText;

            // 4. Nút Tốc độ (Speed Toggle)
            var speedBtnObj = CreateUIObject("SpeedButton", topBar.transform);
            speedBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(170, 75);
            var sbImg = speedBtnObj.AddComponent<Image>();
            SproutLandsPackAssets.ApplySliced(sbImg, btnSprite, 0.42f);

            var sbBtn = speedBtnObj.AddComponent<Button>();
            WireButtonSprites(sbBtn, sbImg);
            var sbTxt = CreateText("Label", speedBtnObj.transform, "1X", 22, new Color(0.38f, 0.20f, 0.08f), TextAlignmentOptions.Center, FontStyles.Bold);
            uiMgr.debugFastGrowthButton = sbBtn;
            uiMgr.debugFastGrowthText = sbTxt;
        }

        private static void CreateCozyBottomHotbar(Transform parent, UIManager uiMgr, Canvas canvas, Sprite woodPanel, Sprite slotBox, Sprite btnSprite)
        {
            var bottomBar = CreateUIObject("BottomHUD", parent);
            var rect = bottomBar.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.02f, 0.012f);
            rect.anchorMax = new Vector2(0.98f, 0.258f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var bg = bottomBar.AddComponent<Image>();
            SproutLandsPackAssets.ApplySliced(bg, woodPanel, 0.32f);

            // Hàng 1: Khay Công cụ & Hạt giống (Hotbar Rack)
            var toolsSection = CreateUIObject("ToolsSection", bottomBar.transform);
            var tsRect = toolsSection.GetComponent<RectTransform>();
            tsRect.anchorMin = new Vector2(0.02f, 0.40f);
            tsRect.anchorMax = new Vector2(0.98f, 0.95f);
            tsRect.offsetMin = Vector2.zero;
            tsRect.offsetMax = Vector2.zero;

            var hLayout = toolsSection.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 15;
            hLayout.childControlWidth = false;
            hLayout.childControlHeight = false;

            // 3 Công cụ chính
            uiMgr.wateringCanTool = CreateCozyDraggableTool(toolsSection.transform, ToolType.WateringCan, "Bình tưới", $"{SpritesPath}/Tools/watering_can.png", canvas, slotBox);
            uiMgr.shovelTool = CreateCozyDraggableTool(toolsSection.transform, ToolType.Shovel, "Xẻng dọn", $"{SpritesPath}/Tools/shovel.png", canvas, slotBox);
            uiMgr.sickleTool = CreateCozyDraggableTool(toolsSection.transform, ToolType.Sickle, "Thu hoạch", $"{SpritesPath}/Tools/sickle.png", canvas, slotBox);

            // Khay Hạt Giống Kéo Thả (Seed Tray)
            var seedTray = CreateUIObject("SeedTrayContainer", toolsSection.transform);
            var stLayout = seedTray.AddComponent<HorizontalLayoutGroup>();
            stLayout.spacing = 12;
            stLayout.childControlWidth = false;
            stLayout.childControlHeight = false;
            uiMgr.seedTrayContainer = seedTray.transform;

            uiMgr.draggableSeedPrefab = CreateCozySeedPrefabTemplate(slotBox);

            // Hàng 2: 4 Nút Điều Hướng Gỗ (Cửa hàng, Kho, Hàng xóm, Sổ hoa)
            var navSection = CreateUIObject("NavSection", bottomBar.transform);
            var nsRect = navSection.GetComponent<RectTransform>();
            nsRect.anchorMin = new Vector2(0.02f, 0.04f);
            nsRect.anchorMax = new Vector2(0.98f, 0.36f);
            nsRect.offsetMin = Vector2.zero;
            nsRect.offsetMax = Vector2.zero;

            var navLayout = navSection.AddComponent<HorizontalLayoutGroup>();
            navLayout.spacing = 16;
            navLayout.childForceExpandWidth = true;
            navLayout.childForceExpandHeight = true;

            uiMgr.shopButton = CreateCozyNavButton(navSection.transform, "Cửa hàng", SproutLandsPackAssets.IconShop, btnSprite);
            uiMgr.inventoryButton = CreateCozyNavButton(navSection.transform, "Kho đồ", SproutLandsPackAssets.IconInventory, btnSprite);
            uiMgr.ordersButton = CreateCozyNavButton(navSection.transform, "Hàng xóm", SproutLandsPackAssets.IconNeighbors, btnSprite);
            uiMgr.flowerBookButton = CreateCozyNavButton(navSection.transform, "Sổ hoa", SproutLandsPackAssets.IconBook, btnSprite);
        }

        private static DraggableToolItem CreateCozyDraggableTool(Transform parent, ToolType type, string label, string iconPath, Canvas canvas, Sprite slotBox)
        {
            var itemObj = CreateUIObject($"Tool_{type}", parent);
            itemObj.GetComponent<RectTransform>().sizeDelta = new Vector2(145, 205);

            var bg = itemObj.AddComponent<Image>();
            SproutLandsPackAssets.ApplySliced(bg, slotBox, 0.48f);

            var icon = CreateIcon("Icon", itemObj.transform, AssetDatabase.LoadAssetAtPath<Sprite>(iconPath), new Vector2(85, 85), new Vector2(0, 24));
            var nameTxt = CreateText("Name", itemObj.transform, label, 20, new Color(0.38f, 0.20f, 0.08f), TextAlignmentOptions.Center, FontStyles.Bold);
            nameTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -48);

            var tool = itemObj.AddComponent<DraggableToolItem>();
            tool.toolType = type;
            tool.iconImage = icon.GetComponent<Image>();
            tool.nameText = nameTxt;
            tool.parentCanvas = canvas;
            return tool;
        }

        private static GameObject CreateCozySeedPrefabTemplate(Sprite slotBox)
        {
            var seedObj = new GameObject("DraggableSeedItem_Prefab", typeof(RectTransform));
            seedObj.GetComponent<RectTransform>().sizeDelta = new Vector2(145, 205);

            var bg = seedObj.AddComponent<Image>();
            SproutLandsPackAssets.ApplySliced(bg, slotBox, 0.48f);

            var icon = CreateIcon("Icon", seedObj.transform, null, new Vector2(85, 85), new Vector2(0, 24));
            var countTxt = CreateText("Count", seedObj.transform, "x1", 20, new Color(0.2f, 0.55f, 0.1f), TextAlignmentOptions.Right, FontStyles.Bold);
            countTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(35, 62);

            var nameTxt = CreateText("Name", seedObj.transform, "Hạt giống", 18, new Color(0.38f, 0.20f, 0.08f), TextAlignmentOptions.Center, FontStyles.Bold);
            nameTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -48);

            var tool = seedObj.AddComponent<DraggableToolItem>();
            tool.toolType = ToolType.Seed;
            tool.iconImage = icon.GetComponent<Image>();
            tool.countText = countTxt;
            tool.nameText = nameTxt;

            string prefabPath = "Assets/_BalconyGarden/Prefabs/DraggableSeedItem.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(seedObj, prefabPath);
            Object.DestroyImmediate(seedObj);
            return prefab;
        }

        private static Button CreateCozyNavButton(Transform parent, string title, Sprite iconSprite, Sprite btnSprite)
        {
            var btnObj = CreateUIObject($"Btn_{title}", parent);
            var img = btnObj.AddComponent<Image>();
            SproutLandsPackAssets.ApplySliced(img, btnSprite, 0.42f);
            var btn = btnObj.AddComponent<Button>();
            WireButtonSprites(btn, img);

            CreateIcon("Icon", btnObj.transform, iconSprite, new Vector2(40, 40), new Vector2(-45, 0));
            var txt = CreateText("Label", btnObj.transform, title, 22, new Color(0.38f, 0.20f, 0.08f), TextAlignmentOptions.Center, FontStyles.Bold);
            txt.GetComponent<RectTransform>().anchoredPosition = new Vector2(25, 0);

            return btn;
        }

        private static void CreateCozyPopups(Transform parent, UIManager uiMgr, Sprite woodPanel, Sprite parchmentPanel, Sprite slotBox, Sprite btnSprite)
        {
            // 1. Shop Popup
            uiMgr.shopUI = CreateCozyShopPopup(parent, woodPanel, parchmentPanel, slotBox, btnSprite);

            // 2. Inventory Popup
            uiMgr.inventoryUI = CreateCozyInventoryPopup(parent, woodPanel, parchmentPanel, slotBox, btnSprite);

            // 3. Orders Popup
            uiMgr.ordersUI = CreateCozyOrdersPopup(parent, woodPanel, parchmentPanel, slotBox, btnSprite);

            // 4. Breeding Popup
            uiMgr.breedingUI = CreateCozyBreedingPopup(parent, woodPanel, parchmentPanel, slotBox, btnSprite);

            // 5. Plot Info Modal
            uiMgr.plotInfoUI = CreateCozyPlotInfoModal(parent, woodPanel, parchmentPanel, btnSprite);

            // 6. Level Up Modal
            uiMgr.levelUpPopup = CreateCozyLevelUpModal(parent, uiMgr, woodPanel, btnSprite);

            // 7. Toast Container
            var toastContainer = CreateUIObject("ToastContainer", parent);
            var tcRect = toastContainer.GetComponent<RectTransform>();
            tcRect.anchorMin = new Vector2(0.1f, 0.45f);
            tcRect.anchorMax = new Vector2(0.9f, 0.55f);
            tcRect.offsetMin = Vector2.zero;
            tcRect.offsetMax = Vector2.zero;
            uiMgr.toastContainer = toastContainer.transform;
            uiMgr.toastPrefab = CreateCozyToastPrefab(parchmentPanel);
        }

        private static ShopUI CreateCozyShopPopup(Transform parent, Sprite woodPanel, Sprite parchmentPanel, Sprite slotBox, Sprite btnSprite)
        {
            var root = CreateCozyModalRoot("ShopPopup", parent, "CỬA HÀNG HẠT GIỐNG", woodPanel, parchmentPanel);
            var shopUI = root.AddComponent<ShopUI>();
            shopUI.panelRoot = root;
            shopUI.closeButton = root.transform.Find("Header/CloseBtn").GetComponent<Button>();

            var content = root.transform.Find("ScrollArea/Viewport/Content");
            shopUI.itemsContainer = content;

            // Template Item Shop
            var itemObj = CreateUIObject("ShopItemTemplate", content);
            itemObj.GetComponent<RectTransform>().sizeDelta = new Vector2(850, 140);
            var itemBg = itemObj.AddComponent<Image>();
            SproutLandsPackAssets.ApplySliced(itemBg, slotBox, 0.45f);

            CreateIcon("Icon", itemObj.transform, null, new Vector2(80, 80), new Vector2(-340, 0));
            var title = CreateText("Title", itemObj.transform, "Tên Cây", 28, new Color(0.38f, 0.20f, 0.08f), TextAlignmentOptions.Left, FontStyles.Bold);
            title.GetComponent<RectTransform>().anchoredPosition = new Vector2(-150, 25);
            var price = CreateText("Price", itemObj.transform, "10 Xu", 24, new Color(0.72f, 0.42f, 0.05f), TextAlignmentOptions.Left, FontStyles.Bold);
            price.GetComponent<RectTransform>().anchoredPosition = new Vector2(-150, -25);
            var desc = CreateText("Desc", itemObj.transform, "Mô tả", 20, new Color(0.48f, 0.35f, 0.22f), TextAlignmentOptions.Left);
            desc.GetComponent<RectTransform>().anchoredPosition = new Vector2(100, 0);

            var buyBtnObj = CreateUIObject("BuyBtn", itemObj.transform);
            buyBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(140, 60);
            buyBtnObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(320, 0);
            var bbImg = buyBtnObj.AddComponent<Image>();
            SproutLandsPackAssets.ApplySliced(bbImg, btnSprite, 0.42f);
            var buyBtn = buyBtnObj.AddComponent<Button>();
            CreateText("BuyTxt", buyBtnObj.transform, "Mua", 24, new Color(0.2f, 0.55f, 0.1f), TextAlignmentOptions.Center, FontStyles.Bold);

            shopUI.shopItemPrefab = itemObj;
            itemObj.SetActive(false);
            root.SetActive(false);
            return shopUI;
        }

        private static InventoryUI CreateCozyInventoryPopup(Transform parent, Sprite woodPanel, Sprite parchmentPanel, Sprite slotBox, Sprite btnSprite)
        {
            var root = CreateCozyModalRoot("InventoryPopup", parent, "KHO NÔNG SẢN", woodPanel, parchmentPanel);
            var invUI = root.AddComponent<InventoryUI>();
            invUI.panelRoot = root;
            invUI.closeButton = root.transform.Find("Header/CloseBtn").GetComponent<Button>();

            // Nút Bán Tất Cả
            var sellAllObj = CreateUIObject("SellAllBtn", root.transform);
            var saRect = sellAllObj.GetComponent<RectTransform>();
            saRect.anchorMin = new Vector2(0.58f, 0.885f);
            saRect.anchorMax = new Vector2(0.86f, 0.965f);
            saRect.offsetMin = Vector2.zero;
            saRect.offsetMax = Vector2.zero;
            var saImg = sellAllObj.AddComponent<Image>();
            SproutLandsPackAssets.ApplySliced(saImg, btnSprite, 0.42f);
            invUI.sellAllButton = sellAllObj.AddComponent<Button>();
            CreateText("Label", sellAllObj.transform, "Bán tất cả 💰", 24, new Color(0.45f, 0.25f, 0.05f), TextAlignmentOptions.Center, FontStyles.Bold);

            var content = root.transform.Find("ScrollArea/Viewport/Content");
            invUI.cropsContainer = content;

            // Template Crop
            var itemObj = CreateUIObject("CropItemTemplate", content);
            itemObj.GetComponent<RectTransform>().sizeDelta = new Vector2(850, 130);
            var itemBg = itemObj.AddComponent<Image>();
            SproutLandsPackAssets.ApplySliced(itemBg, slotBox, 0.45f);

            CreateIcon("Icon", itemObj.transform, null, new Vector2(80, 80), new Vector2(-340, 0));
            var nameTxt = CreateText("Name", itemObj.transform, "Tên Cây", 26, new Color(0.38f, 0.20f, 0.08f), TextAlignmentOptions.Left, FontStyles.Bold);
            nameTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(-150, 20);
            var countTxt = CreateText("Count", itemObj.transform, "x5", 22, new Color(0.48f, 0.35f, 0.22f), TextAlignmentOptions.Left);
            countTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(-150, -20);
            var valTxt = CreateText("Value", itemObj.transform, "Giá: 15 Xu", 22, new Color(0.70f, 0.42f, 0.05f), TextAlignmentOptions.Left, FontStyles.Bold);
            valTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(80, 0);

            var sellBtnObj = CreateUIObject("SellBtn", itemObj.transform);
            sellBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(130, 55);
            sellBtnObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(330, 0);
            var sbImg = sellBtnObj.AddComponent<Image>();
            SproutLandsPackAssets.ApplySliced(sbImg, btnSprite, 0.42f);
            sellBtnObj.AddComponent<Button>();
            CreateText("Label", sellBtnObj.transform, "Bán 1", 22, new Color(0.2f, 0.55f, 0.1f), TextAlignmentOptions.Center, FontStyles.Bold);

            invUI.cropItemPrefab = itemObj;
            itemObj.SetActive(false);
            root.SetActive(false);
            return invUI;
        }

        private static OrdersUI CreateCozyOrdersPopup(Transform parent, Sprite woodPanel, Sprite parchmentPanel, Sprite slotBox, Sprite btnSprite)
        {
            var root = CreateCozyModalRoot("OrdersPopup", parent, "ĐƠN HÀNG HÀNG XÓM", woodPanel, parchmentPanel);
            var ordersUI = root.AddComponent<OrdersUI>();
            ordersUI.panelRoot = root;
            ordersUI.closeButton = root.transform.Find("Header/CloseBtn").GetComponent<Button>();

            var content = root.transform.Find("ScrollArea/Viewport/Content");
            ordersUI.ordersContainer = content;

            // Template Order
            var cardObj = CreateUIObject("OrderCardTemplate", content);
            cardObj.GetComponent<RectTransform>().sizeDelta = new Vector2(850, 220);
            var cBg = cardObj.AddComponent<Image>();
            SproutLandsPackAssets.ApplySliced(cBg, slotBox, 0.45f);

            CreateIcon("Avatar", cardObj.transform, null, new Vector2(90, 90), new Vector2(-340, 40));
            CreateIcon("CropIcon", cardObj.transform, null, new Vector2(60, 60), new Vector2(-340, -50));

            var nameTxt = CreateText("Name", cardObj.transform, "Bà Lan", 28, new Color(0.38f, 0.20f, 0.08f), TextAlignmentOptions.Left, FontStyles.Bold);
            nameTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100, 60);

            var dialogueTxt = CreateText("Dialogue", cardObj.transform, "\"Rau cải của cháu ngon quá...\"", 22, new Color(0.48f, 0.35f, 0.22f), TextAlignmentOptions.Left);
            dialogueTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(60, 20);
            dialogueTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 50);

            var reqTxt = CreateText("Request", cardObj.transform, "Cần: 3 Cải Xanh", 24, new Color(0.38f, 0.20f, 0.08f), TextAlignmentOptions.Left, FontStyles.Bold);
            reqTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100, -45);

            var rewardTxt = CreateText("Reward", cardObj.transform, "Thưởng: +50 Xu, +20 XP", 22, new Color(0.75f, 0.45f, 0.05f), TextAlignmentOptions.Left, FontStyles.Bold);
            rewardTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(140, -45);

            var deliverBtnObj = CreateUIObject("DeliverBtn", cardObj.transform);
            deliverBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 65);
            deliverBtnObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(330, -35);
            var dbImg = deliverBtnObj.AddComponent<Image>();
            SproutLandsPackAssets.ApplySliced(dbImg, btnSprite, 0.42f);
            deliverBtnObj.AddComponent<Button>();
            CreateText("Label", deliverBtnObj.transform, "Giao Hàng", 22, new Color(0.2f, 0.55f, 0.1f), TextAlignmentOptions.Center, FontStyles.Bold);

            ordersUI.orderCardPrefab = cardObj;
            cardObj.SetActive(false);
            root.SetActive(false);
            return ordersUI;
        }

        private static BreedingUI CreateCozyBreedingPopup(Transform parent, Sprite woodPanel, Sprite parchmentPanel, Sprite slotBox, Sprite btnSprite)
        {
            var root = CreateCozyModalRoot("BreedingPopup", parent, "SỔ TAY HOA & LAI TẠO", woodPanel, parchmentPanel);
            var bUI = root.AddComponent<BreedingUI>();
            bUI.panelRoot = root;
            bUI.closeButton = root.transform.Find("Header/CloseBtn").GetComponent<Button>();

            // Nút Tab 1 & Tab 2
            var tabRow = CreateUIObject("TabRow", root.transform);
            var trRect = tabRow.GetComponent<RectTransform>();
            trRect.anchorMin = new Vector2(0.1f, 0.88f);
            trRect.anchorMax = new Vector2(0.9f, 0.96f);
            trRect.offsetMin = Vector2.zero;
            trRect.offsetMax = Vector2.zero;

            var tab1Obj = CreateUIObject("TabCodex", tabRow.transform);
            tab1Obj.GetComponent<RectTransform>().sizeDelta = new Vector2(350, 60);
            tab1Obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-190, 0);
            var t1Img = tab1Obj.AddComponent<Image>();
            SproutLandsPackAssets.ApplySliced(t1Img, btnSprite, 0.42f);
            bUI.tabCodexButton = tab1Obj.AddComponent<Button>();
            CreateText("Label", tab1Obj.transform, "📖 Sổ Tay Hoa", 24, new Color(0.38f, 0.20f, 0.08f), TextAlignmentOptions.Center, FontStyles.Bold);

            var tab2Obj = CreateUIObject("TabBreeding", tabRow.transform);
            tab2Obj.GetComponent<RectTransform>().sizeDelta = new Vector2(350, 60);
            tab2Obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(190, 0);
            var t2Img = tab2Obj.AddComponent<Image>();
            SproutLandsPackAssets.ApplySliced(t2Img, btnSprite, 0.42f);
            bUI.tabBreedingButton = tab2Obj.AddComponent<Button>();
            CreateText("Label", tab2Obj.transform, "🔬 Phòng Lai Tạo", 24, new Color(0.38f, 0.20f, 0.08f), TextAlignmentOptions.Center, FontStyles.Bold);

            // Tab 1: Codex
            var codexRoot = CreateUIObject("CodexTabRoot", root.transform);
            var crRect = codexRoot.GetComponent<RectTransform>();
            crRect.anchorMin = new Vector2(0.05f, 0.05f);
            crRect.anchorMax = new Vector2(0.95f, 0.86f);
            crRect.offsetMin = Vector2.zero;
            crRect.offsetMax = Vector2.zero;
            bUI.codexTabRoot = codexRoot;

            var codexScroll = CreateScrollArea("Scroll", codexRoot.transform);
            bUI.codexContainer = codexScroll.transform.Find("Viewport/Content");

            var codexCard = CreateUIObject("CodexCardTemplate", bUI.codexContainer);
            codexCard.GetComponent<RectTransform>().sizeDelta = new Vector2(850, 130);
            var ccBg = codexCard.AddComponent<Image>();
            SproutLandsPackAssets.ApplySliced(ccBg, slotBox, 0.45f);
            CreateIcon("Icon", codexCard.transform, null, new Vector2(80, 80), new Vector2(-340, 0));
            var cName = CreateText("Name", codexCard.transform, "Tên Hoa", 26, new Color(0.38f, 0.20f, 0.08f), TextAlignmentOptions.Left, FontStyles.Bold);
            cName.GetComponent<RectTransform>().anchoredPosition = new Vector2(-150, 20);
            var cDesc = CreateText("Desc", codexCard.transform, "Mô tả hoa", 20, new Color(0.48f, 0.35f, 0.22f), TextAlignmentOptions.Left);
            cDesc.GetComponent<RectTransform>().anchoredPosition = new Vector2(40, -15);
            bUI.codexCardPrefab = codexCard;
            codexCard.SetActive(false);

            // Tab 2: Breeding
            var breedingRoot = CreateUIObject("BreedingTabRoot", root.transform);
            var brRect = breedingRoot.GetComponent<RectTransform>();
            brRect.anchorMin = new Vector2(0.05f, 0.05f);
            brRect.anchorMax = new Vector2(0.95f, 0.86f);
            brRect.offsetMin = Vector2.zero;
            brRect.offsetMax = Vector2.zero;
            bUI.breedingTabRoot = breedingRoot;

            var infoTxt = CreateText("BreedingInfo", breedingRoot.transform, "Chọn 2 bông hoa từ kho để lai giống:", 26, new Color(0.38f, 0.20f, 0.08f), TextAlignmentOptions.Center, FontStyles.Bold);
            infoTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 300);

            var ddA = CreateDropdown("DropdownA", breedingRoot.transform, new Vector2(-220, 160));
            bUI.flowerDropdownA = ddA;
            var prevA = CreateIcon("PreviewA", breedingRoot.transform, null, new Vector2(120, 120), new Vector2(-220, 30));
            bUI.previewFlowerA = prevA.GetComponent<Image>();

            var plusTxt = CreateText("Plus", breedingRoot.transform, "➕", 40, Color.gray, TextAlignmentOptions.Center);
            plusTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 30);

            var ddB = CreateDropdown("DropdownB", breedingRoot.transform, new Vector2(220, 160));
            bUI.flowerDropdownB = ddB;
            var prevB = CreateIcon("PreviewB", breedingRoot.transform, null, new Vector2(120, 120), new Vector2(220, 30));
            bUI.previewFlowerB = prevB.GetComponent<Image>();

            var breedBtnObj = CreateUIObject("BreedBtn", breedingRoot.transform);
            breedBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(320, 80);
            breedBtnObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -120);
            var bbImg = breedBtnObj.AddComponent<Image>();
            SproutLandsPackAssets.ApplySliced(bbImg, btnSprite, 0.42f);
            bUI.breedButton = breedBtnObj.AddComponent<Button>();
            CreateText("Label", breedBtnObj.transform, "Bắt Đầu Lai Tạo 🌸", 26, new Color(0.48f, 0.15f, 0.35f), TextAlignmentOptions.Center, FontStyles.Bold);

            var resTxt = CreateText("ResultTxt", breedingRoot.transform, "Kết quả lai tạo sẽ hiển thị tại đây...", 24, new Color(0.2f, 0.5f, 0.2f), TextAlignmentOptions.Center);
            resTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -250);
            bUI.breedResultText = resTxt;

            root.SetActive(false);
            return bUI;
        }

        private static PlotInfoUI CreateCozyPlotInfoModal(Transform parent, Sprite woodPanel, Sprite parchmentPanel, Sprite btnSprite)
        {
            var root = CreateCozyModalRoot("PlotInfoModal", parent, "CHI TIẾT CHẬU CÂY", woodPanel, parchmentPanel);
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 920);
            var infoUI = root.AddComponent<PlotInfoUI>();
            infoUI.panelRoot = root;
            infoUI.closeButton = root.transform.Find("Header/CloseBtn").GetComponent<Button>();

            var sa = root.transform.Find("ScrollArea");
            if (sa != null) Object.DestroyImmediate(sa.gameObject);

            var preview = CreateIcon("Preview", root.transform, null, new Vector2(180, 180), new Vector2(0, 200));
            infoUI.plantPreviewImage = preview.GetComponent<Image>();

            infoUI.plantNameText = CreateText("Name", root.transform, "Tên Cây", 34, new Color(0.38f, 0.20f, 0.08f), TextAlignmentOptions.Center, FontStyles.Bold);
            infoUI.plantNameText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 80);

            infoUI.stageText = CreateText("Stage", root.transform, "Trạng thái: Đang lớn", 24, new Color(0.48f, 0.35f, 0.22f), TextAlignmentOptions.Center);
            infoUI.stageText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 30);

            infoUI.progressText = CreateText("Progress", root.transform, "Tiến độ: 50%", 22, new Color(0.38f, 0.20f, 0.08f), TextAlignmentOptions.Center, FontStyles.Bold);
            infoUI.progressText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -15);

            infoUI.waterText = CreateText("Water", root.transform, "Độ ẩm đất: 80%", 22, new Color(0.15f, 0.55f, 0.85f), TextAlignmentOptions.Center, FontStyles.Bold);
            infoUI.waterText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -55);

            infoUI.sunPreferenceText = CreateText("Sun", root.transform, "Ưa thích: Nắng vừa", 22, new Color(0.85f, 0.45f, 0.05f), TextAlignmentOptions.Center);
            infoUI.sunPreferenceText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -95);

            infoUI.soilTypeText = CreateText("Soil", root.transform, "Loại đất: Đất thường", 22, new Color(0.55f, 0.35f, 0.18f), TextAlignmentOptions.Center);
            infoUI.soilTypeText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -135);

            var btnRow = CreateUIObject("ActionBtns", root.transform);
            var brRect = btnRow.GetComponent<RectTransform>();
            brRect.anchoredPosition = new Vector2(0, -250);
            brRect.sizeDelta = new Vector2(700, 80);
            var brLayout = btnRow.AddComponent<HorizontalLayoutGroup>();
            brLayout.spacing = 15;
            brLayout.childForceExpandWidth = true;
            brLayout.childForceExpandHeight = true;

            infoUI.waterButton = CreateCozyActionButton("WaterBtn", btnRow.transform, "Tưới Nước 💧", new Color(0.1f, 0.45f, 0.75f), btnSprite);
            infoUI.harvestButton = CreateCozyActionButton("HarvestBtn", btnRow.transform, "Thu Hoạch 🌸", new Color(0.2f, 0.55f, 0.15f), btnSprite);
            infoUI.speedUpGemsButton = CreateCozyActionButton("SpeedBtn", btnRow.transform, "Lớn Ngay (1💎)", new Color(0.5f, 0.15f, 0.55f), btnSprite);
            infoUI.clearButton = CreateCozyActionButton("ClearBtn", btnRow.transform, "Dọn Chậu 🗑️", new Color(0.65f, 0.15f, 0.15f), btnSprite);

            root.SetActive(false);
            return infoUI;
        }

        private static GameObject CreateCozyLevelUpModal(Transform parent, UIManager uiMgr, Sprite woodPanel, Sprite btnSprite)
        {
            var modal = CreateUIObject("LevelUpModal", parent);
            SetStretch(modal.GetComponent<RectTransform>());
            modal.AddComponent<Image>().color = new Color(0, 0, 0, 0.75f);

            var box = CreateUIObject("Box", modal.transform);
            box.GetComponent<RectTransform>().sizeDelta = new Vector2(720, 520);
            var bImg = box.AddComponent<Image>();
            SproutLandsPackAssets.ApplySliced(bImg, woodPanel, 0.32f);

            var title = CreateText("Title", box.transform, "CHÚC MỪNG LÊN CẤP! 🌟", 36, new Color(0.85f, 0.45f, 0.05f), TextAlignmentOptions.Center, FontStyles.Bold);
            title.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 150);

            var desc = CreateText("Desc", box.transform, "Bạn nhận được quà tặng!", 28, new Color(0.38f, 0.20f, 0.08f), TextAlignmentOptions.Center);
            desc.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 40);
            uiMgr.levelUpTitleText = desc;

            var claimBtnObj = CreateUIObject("ClaimBtn", box.transform);
            claimBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(280, 75);
            claimBtnObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -140);
            var cImg = claimBtnObj.AddComponent<Image>();
            SproutLandsPackAssets.ApplySliced(cImg, btnSprite, 0.42f);
            var btn = claimBtnObj.AddComponent<Button>();
            CreateText("Label", claimBtnObj.transform, "Nhận Thưởng ✨", 26, new Color(0.2f, 0.55f, 0.1f), TextAlignmentOptions.Center, FontStyles.Bold);
            uiMgr.levelUpCloseButton = btn;

            modal.SetActive(false);
            return modal;
        }

        private static GameObject CreateCozyToastPrefab(Sprite parchmentPanel)
        {
            var toast = new GameObject("Toast_Prefab", typeof(RectTransform));
            toast.GetComponent<RectTransform>().sizeDelta = new Vector2(680, 95);

            var bg = toast.AddComponent<Image>();
            SproutLandsPackAssets.ApplySliced(bg, parchmentPanel, 0.3f);

            var txt = CreateText("Message", toast.transform, "Thông báo!", 26, new Color(0.38f, 0.20f, 0.08f), TextAlignmentOptions.Center, FontStyles.Bold);
            SetStretch(txt.GetComponent<RectTransform>());

            string path = "Assets/_BalconyGarden/Prefabs/ToastNotification.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(toast, path);
            Object.DestroyImmediate(toast);
            return prefab;
        }

        private static GameObject CreateCozyModalRoot(string name, Transform parent, string title, Sprite woodPanel, Sprite parchmentPanel)
        {
            var root = CreateUIObject(name, parent);
            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.04f, 0.10f);
            rect.anchorMax = new Vector2(0.96f, 0.90f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var bg = root.AddComponent<Image>();
            SproutLandsPackAssets.ApplySliced(bg, woodPanel, 0.32f);

            // Header Ribbon
            var header = CreateUIObject("Header", root.transform);
            var hRect = header.GetComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0.04f, 0.91f);
            hRect.anchorMax = new Vector2(0.96f, 0.99f);
            hRect.offsetMin = Vector2.zero;
            hRect.offsetMax = Vector2.zero;

            var hImg = header.AddComponent<Image>();
            SproutLandsPackAssets.ApplySliced(hImg, parchmentPanel, 0.28f);

            var titleTxt = CreateText("Title", header.transform, title, 30, new Color(0.38f, 0.20f, 0.08f), TextAlignmentOptions.Center, FontStyles.Bold);
            SetStretch(titleTxt.GetComponent<RectTransform>());

            var closeBtn = CreateUIObject("CloseBtn", header.transform);
            var cbRect = closeBtn.GetComponent<RectTransform>();
            cbRect.anchorMin = new Vector2(0.88f, 0.12f);
            cbRect.anchorMax = new Vector2(0.98f, 0.88f);
            cbRect.offsetMin = Vector2.zero;
            cbRect.offsetMax = Vector2.zero;
            var closeImg = closeBtn.AddComponent<Image>();
            closeImg.sprite = SproutLandsPackAssets.CloseButton != null ? SproutLandsPackAssets.CloseButton : SproutLandsPackAssets.IconClose;
            closeImg.preserveAspect = true;
            closeImg.color = Color.white;
            closeBtn.AddComponent<Button>();

            CreateScrollArea("ScrollArea", root.transform);
            return root;
        }

        private static Button CreateCozyActionButton(string name, Transform parent, string label, Color textColor, Sprite btnSprite)
        {
            var btnObj = CreateUIObject(name, parent);
            var img = btnObj.AddComponent<Image>();
            SproutLandsPackAssets.ApplySliced(img, btnSprite, 0.42f);
            var btn = btnObj.AddComponent<Button>();
            WireButtonSprites(btn, img);
            CreateText("Label", btnObj.transform, label, 20, textColor, TextAlignmentOptions.Center, FontStyles.Bold);
            return btn;
        }

        private static GameObject CreateBadge(string name, Transform parent, string iconPath, Vector2 anchorMin, Vector2 anchorMax)
        {
            var badge = CreateUIObject(name, parent);
            var bRect = badge.GetComponent<RectTransform>();
            bRect.anchorMin = anchorMin;
            bRect.anchorMax = anchorMax;
            bRect.offsetMin = Vector2.zero;
            bRect.offsetMax = Vector2.zero;
            var img = badge.AddComponent<Image>();
            img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            img.preserveAspect = true;
            return badge;
        }

        private static GameObject CreateScrollArea(string name, Transform parent)
        {
            var scrollObj = CreateUIObject(name, parent);
            var sRect = scrollObj.GetComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0.02f, 0.02f);
            sRect.anchorMax = new Vector2(0.98f, 0.86f);
            sRect.offsetMin = Vector2.zero;
            sRect.offsetMax = Vector2.zero;

            var scrollRect = scrollObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            var viewport = CreateUIObject("Viewport", scrollObj.transform);
            SetStretch(viewport.GetComponent<RectTransform>());
            viewport.AddComponent<RectMask2D>();
            scrollRect.viewport = viewport.GetComponent<RectTransform>();

            var content = CreateUIObject("Content", viewport.transform);
            var cRect = content.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0, 1);
            cRect.anchorMax = new Vector2(1, 1);
            cRect.pivot = new Vector2(0.5f, 1);
            cRect.sizeDelta = new Vector2(0, 1000);

            var vLayout = content.AddComponent<VerticalLayoutGroup>();
            vLayout.spacing = 15;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = false;
            vLayout.childForceExpandWidth = true;
            vLayout.padding = new RectOffset(10, 10, 10, 10);

            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = cRect;
            return scrollObj;
        }

        private static TMP_Dropdown CreateDropdown(string name, Transform parent, Vector2 pos)
        {
            var ddObj = CreateUIObject(name, parent);
            ddObj.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 50);
            ddObj.GetComponent<RectTransform>().anchoredPosition = pos;
            SproutLandsPackAssets.ApplySliced(ddObj.AddComponent<Image>(), SproutLandsPackAssets.SlotSmall, 0.5f);
            var dd = ddObj.AddComponent<TMP_Dropdown>();
            var label = CreateText("Label", ddObj.transform, "Chọn hoa...", 20, new Color(0.38f, 0.20f, 0.08f), TextAlignmentOptions.Left);
            dd.captionText = label;
            return dd;
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static void WireButtonSprites(Button button, Image image)
        {
            if (button == null || image == null) return;
            button.targetGraphic = image;
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState
            {
                pressedSprite = SproutLandsPackAssets.ButtonPressed,
                highlightedSprite = SproutLandsPackAssets.ButtonNormal,
                selectedSprite = SproutLandsPackAssets.ButtonNormal
            };
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float fontSize, Color color, TextAlignmentOptions alignment, FontStyles style = FontStyles.Normal)
        {
            var obj = CreateUIObject(name, parent);
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = alignment;
            SproutLandsPackAssets.ApplyPixelFont(tmp);
            return tmp;
        }

        private static GameObject CreateIcon(string name, Transform parent, Sprite sprite, Vector2 size, Vector2 pos)
        {
            var obj = CreateUIObject(name, parent);
            var rect = obj.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = pos;
            var img = obj.AddComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            return obj;
        }

        private static void SetStretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void LoadAssetsIntoManagers(GameManager gameMgr, NeighborSystem neighborSys)
        {
            string[] plantGuids = AssetDatabase.FindAssets("t:PlantData", new[] { PlantsSOPath });
            gameMgr.registeredPlants = new List<PlantData>();
            foreach (var guid in plantGuids)
            {
                var p = AssetDatabase.LoadAssetAtPath<PlantData>(AssetDatabase.GUIDToAssetPath(guid));
                if (p != null) gameMgr.registeredPlants.Add(p);
            }

            string[] neighborGuids = AssetDatabase.FindAssets("t:NeighborData", new[] { NeighborsSOPath });
            neighborSys.registeredNeighbors = new List<NeighborData>();
            foreach (var guid in neighborGuids)
            {
                var n = AssetDatabase.LoadAssetAtPath<NeighborData>(AssetDatabase.GUIDToAssetPath(guid));
                if (n != null) neighborSys.registeredNeighbors.Add(n);
            }
        }
    }
}
#endif
