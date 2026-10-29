#if UNITY_EDITOR
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
    public static class BalconyGardenSceneBuilder
    {
        private const string ScenePath = "Assets/_BalconyGarden/Scenes/BalconyGardenMain.unity";
        private const string SpritesPath = "Assets/_BalconyGarden/Sprites";
        private const string PlantsSOPath = "Assets/_BalconyGarden/ScriptableObjects/Plants";
        private const string NeighborsSOPath = "Assets/_BalconyGarden/ScriptableObjects/Neighbors";

        [MenuItem("Balcony Garden/3. Chạy TẤT CẢ (Tạo Sprite + Dựng Scene 1-Click)", false, 0)]
        public static void RunAllMenu()
        {
            RunAll(true);
        }

        public static void RunAll(bool showDialog = false)
        {
            BalconyGardenAssetGenerator.GenerateAllAssets(showDialog);
            BuildScene(showDialog);
        }

        [InitializeOnLoadMethod]
        private static void AutoSetupOnInit()
        {
            EditorApplication.delayCall += () =>
            {
                if (!System.IO.File.Exists(ScenePath))
                {
                    Debug.Log("[BalconyGarden] Đang tự động tạo Sprites, ScriptableObjects và dựng Scene Balcony Garden...");
                    RunAll(false);
                }
            };
        }

        [MenuItem("Balcony Garden/2. Dựng hoàn chỉnh Scene Balcony Garden", false, 2)]
        public static void BuildSceneMenu()
        {
            BuildScene(true);
        }

        public static void BuildScene(bool showDialog = false)
        {
            // Đảm bảo thư mục Scenes tồn tại
            string sceneDir = "Assets/_BalconyGarden/Scenes";
            if (!System.IO.Directory.Exists(sceneDir)) System.IO.Directory.CreateDirectory(sceneDir);

            // Tạo Scene mới
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Tạo Camera
            var camObj = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            var cam = camObj.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.85f, 0.94f, 0.98f);
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            camObj.transform.position = new Vector3(0, 0, -10);

            // 2. Tạo EventSystem
            var eventObj = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            // 3. Tạo Canvas chính
            var canvasObj = new GameObject("MainCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            // 4. Tạo GameManagers Root
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

            // Nạp ScriptableObjects vào GameManager và NeighborSystem
            LoadAssetsIntoManagers(gameMgr, neighborSys);

            // 5. Background Ban công
            var bgObj = CreateUIObject("Background", canvasObj.transform);
            var bgImg = bgObj.AddComponent<Image>();
            bgImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Environment/balcony_bg.png");
            bgImg.color = new Color(0.92f, 0.97f, 1.0f);
            SetStretch(bgObj.GetComponent<RectTransform>());

            // Ban công trang trí (Thanh lan can / Kệ gỗ ở giữa)
            var shelfObj = CreateUIObject("BalconyShelf", bgObj.transform);
            var shelfImg = shelfObj.AddComponent<Image>();
            shelfImg.color = new Color(0.72f, 0.54f, 0.38f, 0.8f);
            var shelfRect = shelfObj.GetComponent<RectTransform>();
            shelfRect.anchorMin = new Vector2(0.05f, 0.28f);
            shelfRect.anchorMax = new Vector2(0.95f, 0.82f);
            shelfRect.offsetMin = Vector2.zero;
            shelfRect.offsetMax = Vector2.zero;

            // 6. Dựng 6 Chậu Cây (2 cột x 3 hàng)
            var plotsContainer = CreateUIObject("PlotsContainer", shelfObj.transform);
            var plotsGrid = plotsContainer.AddComponent<GridLayoutGroup>();
            plotsGrid.cellSize = new Vector2(440, 290);
            plotsGrid.spacing = new Vector2(40, 35);
            plotsGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            plotsGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
            plotsGrid.childAlignment = TextAnchor.MiddleCenter;
            SetStretch(plotsContainer.GetComponent<RectTransform>());

            plotMgr.plots = new List<PlantPlot>();

            for (int i = 0; i < 6; i++)
            {
                var plot = CreatePlotItem(i, plotsContainer.transform);
                plotMgr.plots.Add(plot);
            }

            // 7. Top HUD (Xu, Hạt ngọc, Level, XP Bar, Debug toggle)
            var topBar = CreateTopBar(canvasObj.transform, uiMgr);

            // 8. Bottom Action Bar (Công cụ kéo thả, Khay hạt giống, Nút chức năng)
            var bottomBar = CreateBottomBar(canvasObj.transform, uiMgr, canvas);

            // 9. Popups (Shop, Inventory, Orders, Breeding, PlotInfo, LevelUp, Toasts)
            CreateAllPopups(canvasObj.transform, uiMgr);

            // Lưu scene
            EditorSceneManager.SaveScene(scene, ScenePath);

            // Thêm Scene vào Build Settings
            AddSceneToBuildSettings(ScenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (showDialog)
            {
                EditorUtility.DisplayDialog("Dựng Scene Hoàn Tất!",
                    "Scene 'BalconyGardenMain.unity' đã được dựng hoàn chỉnh và sẵn sàng chơi!\n\n" +
                    "Bạn chỉ cần nhấn nút PLAY trong Unity để trải nghiệm vòng lặp game đầy đủ.",
                    "Mở và chơi ngay");
            }
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

        private static PlantPlot CreatePlotItem(int index, Transform parent)
        {
            var plotObj = CreateUIObject($"Plot_{index}", parent);
            var plotRect = plotObj.GetComponent<RectTransform>();

            // Nền chậu
            var potBg = CreateUIObject("PotBase", plotObj.transform);
            var potImg = potBg.AddComponent<Image>();
            potImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Environment/pot_empty.png");
            SetStretch(potBg.GetComponent<RectTransform>());

            // Cây trồng (SpriteRenderer / Image)
            var plantObj = CreateUIObject("PlantVisual", plotObj.transform);
            var plantImg = plantObj.AddComponent<Image>();
            plantImg.preserveAspect = true;
            var plantRect = plantObj.GetComponent<RectTransform>();
            plantRect.anchorMin = new Vector2(0.15f, 0.25f);
            plantRect.anchorMax = new Vector2(0.85f, 0.95f);
            plantRect.offsetMin = Vector2.zero;
            plantRect.offsetMax = Vector2.zero;

            // Thanh nước (Water bar slider)
            var waterBar = CreateUIObject("WaterBar", plotObj.transform);
            var waterBarRect = waterBar.GetComponent<RectTransform>();
            waterBarRect.anchorMin = new Vector2(0.15f, 0.12f);
            waterBarRect.anchorMax = new Vector2(0.85f, 0.19f);
            waterBarRect.offsetMin = Vector2.zero;
            waterBarRect.offsetMax = Vector2.zero;
            var waterBgImg = waterBar.AddComponent<Image>();
            waterBgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.6f);

            var waterFill = CreateUIObject("Fill", waterBar.transform);
            var fillImg = waterFill.AddComponent<Image>();
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.color = new Color(0.2f, 0.7f, 1.0f);
            SetStretch(waterFill.GetComponent<RectTransform>());

            // Huy hiệu Nắng
            var sunBadge = CreateUIObject("SunBadge", plotObj.transform);
            var sunRect = sunBadge.GetComponent<RectTransform>();
            sunRect.anchorMin = new Vector2(0.05f, 0.85f);
            sunRect.anchorMax = new Vector2(0.55f, 0.98f);
            sunRect.offsetMin = Vector2.zero;
            sunRect.offsetMax = Vector2.zero;
            var sunText = sunBadge.AddComponent<TextMeshProUGUI>();
            sunText.fontSize = 20;
            sunText.color = new Color(0.9f, 0.5f, 0.1f);
            sunText.alignment = TextAlignmentOptions.Left;

            // Badge khát nước (Thirsty)
            var thirstyBadge = CreateUIObject("ThirstyBadge", plotObj.transform);
            var tRect = thirstyBadge.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0.75f, 0.75f);
            tRect.anchorMax = new Vector2(0.95f, 0.95f);
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;
            var tImg = thirstyBadge.AddComponent<Image>();
            tImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/UI/water_drop.png");

            // Badge chín sẵn sàng thu hoạch (Ripe)
            var ripeBadge = CreateUIObject("RipeBadge", plotObj.transform);
            var rRect = ripeBadge.GetComponent<RectTransform>();
            rRect.anchorMin = new Vector2(0.75f, 0.75f);
            rRect.anchorMax = new Vector2(0.95f, 0.95f);
            rRect.offsetMin = Vector2.zero;
            rRect.offsetMax = Vector2.zero;
            var rImg = ripeBadge.AddComponent<Image>();
            rImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/UI/sun.png");

            // Badge héo
            var witherBadge = CreateUIObject("WitherBadge", plotObj.transform);
            var wRect = witherBadge.GetComponent<RectTransform>();
            wRect.anchorMin = new Vector2(0.75f, 0.75f);
            wRect.anchorMax = new Vector2(0.95f, 0.95f);
            wRect.offsetMin = Vector2.zero;
            wRect.offsetMax = Vector2.zero;
            var wImg = witherBadge.AddComponent<Image>();
            wImg.color = new Color(0.9f, 0.2f, 0.2f);

            // Khóa overlay (Lock Overlay)
            var lockOverlay = CreateUIObject("LockOverlay", plotObj.transform);
            SetStretch(lockOverlay.GetComponent<RectTransform>());
            var lockBg = lockOverlay.AddComponent<Image>();
            lockBg.color = new Color(0.1f, 0.1f, 0.1f, 0.75f);

            var lockIcon = CreateUIObject("LockIcon", lockOverlay.transform);
            var liRect = lockIcon.GetComponent<RectTransform>();
            liRect.sizeDelta = new Vector2(60, 60);
            var liImg = lockIcon.AddComponent<Image>();
            liImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/UI/lock.png");

            var lockTxtObj = CreateUIObject("LockCostText", lockOverlay.transform);
            var ltRect = lockTxtObj.GetComponent<RectTransform>();
            ltRect.anchoredPosition = new Vector2(0, -50);
            var lockTxt = lockTxtObj.AddComponent<TextMeshProUGUI>();
            lockTxt.text = "100 Xu";
            lockTxt.fontSize = 26;
            lockTxt.alignment = TextAlignmentOptions.Center;
            lockTxt.color = Color.yellow;

            // Nút bấm chậu
            var btn = plotObj.AddComponent<Button>();

            // Thành phần PlantPlot
            var plot = plotObj.AddComponent<PlantPlot>();
            plot.plotIndex = index;
            plot.potImage = potImg;
            plot.plantImage = plantImg;
            plot.waterFillImage = fillImg;
            plot.waterBarContainer = waterBar;
            plot.sunBadgeText = sunText;
            plot.thirstyBadge = thirstyBadge;
            plot.ripeBadge = ripeBadge;
            plot.witheredBadge = witherBadge;
            plot.lockOverlay = lockOverlay;
            plot.lockCostText = lockTxt;

            btn.onClick.AddListener(plot.OnClickPlot);

            return plot;
        }

        private static GameObject CreateTopBar(Transform parent, UIManager uiMgr)
        {
            var topBar = CreateUIObject("TopHUD", parent);
            var rect = topBar.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.91f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var bg = topBar.AddComponent<Image>();
            bg.color = new Color(0.98f, 0.96f, 0.92f, 0.95f);

            var layout = topBar.AddComponent<HorizontalLayoutGroup>();
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.padding = new RectOffset(20, 20, 10, 10);
            layout.spacing = 15;

            // Level & XP
            var levelBox = CreateUIObject("LevelBox", topBar.transform);
            var lRect = levelBox.GetComponent<RectTransform>();
            lRect.sizeDelta = new Vector2(260, 110);
            var lvlText = CreateText("LvText", levelBox.transform, "Lv.1", 30, Color.black, TextAlignmentOptions.Left);
            lvlText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 25);
            uiMgr.levelText = lvlText;

            var xpSliderObj = CreateUIObject("XPSlider", levelBox.transform);
            var xpRect = xpSliderObj.GetComponent<RectTransform>();
            xpRect.sizeDelta = new Vector2(250, 20);
            xpRect.anchoredPosition = new Vector2(0, -10);
            var xpSlider = xpSliderObj.AddComponent<Slider>();
            var xpBg = xpSliderObj.AddComponent<Image>();
            xpBg.color = new Color(0.8f, 0.8f, 0.8f);

            var xpFill = CreateUIObject("Fill", xpSliderObj.transform);
            var xpFillImg = xpFill.AddComponent<Image>();
            xpFillImg.color = new Color(0.3f, 0.8f, 0.3f);
            SetStretch(xpFill.GetComponent<RectTransform>());
            xpSlider.fillRect = xpFill.GetComponent<RectTransform>();
            uiMgr.xpSlider = xpSlider;

            var xpValText = CreateText("XPText", levelBox.transform, "0/50 XP", 18, Color.gray, TextAlignmentOptions.Center);
            xpValText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -32);
            uiMgr.xpText = xpValText;

            // Xu Box
            var coinBox = CreateUIObject("CoinBox", topBar.transform);
            coinBox.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 110);
            var coinIcon = CreateIcon("Icon", coinBox.transform, AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/UI/coin.png"), new Vector2(40, 40), new Vector2(-60, 0));
            var coinText = CreateText("Coins", coinBox.transform, "50", 28, new Color(0.7f, 0.5f, 0.05f), TextAlignmentOptions.Left);
            coinText.GetComponent<RectTransform>().anchoredPosition = new Vector2(20, 0);
            uiMgr.coinsText = coinText;

            // Ngọc Box
            var gemBox = CreateUIObject("GemBox", topBar.transform);
            gemBox.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 110);
            var gemIcon = CreateIcon("Icon", gemBox.transform, AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/UI/gem.png"), new Vector2(40, 40), new Vector2(-50, 0));
            var gemText = CreateText("Gems", gemBox.transform, "5", 28, new Color(0.1f, 0.6f, 0.8f), TextAlignmentOptions.Left);
            gemText.GetComponent<RectTransform>().anchoredPosition = new Vector2(15, 0);
            uiMgr.gemsText = gemText;

            // Nút Debug Fast Speed
            var speedBtnObj = CreateUIObject("SpeedButton", topBar.transform);
            speedBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 70);
            var sbImg = speedBtnObj.AddComponent<Image>();
            sbImg.color = new Color(0.9f, 0.45f, 0.15f);
            var sbBtn = speedBtnObj.AddComponent<Button>();
            var sbTxt = CreateText("Label", speedBtnObj.transform, "⚡ Tốc độ: 1X", 22, Color.white, TextAlignmentOptions.Center);
            uiMgr.debugFastGrowthButton = sbBtn;
            uiMgr.debugFastGrowthText = sbTxt;

            return topBar;
        }

        private static GameObject CreateBottomBar(Transform parent, UIManager uiMgr, Canvas canvas)
        {
            var bottomBar = CreateUIObject("BottomHUD", parent);
            var rect = bottomBar.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0.26f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var bg = bottomBar.AddComponent<Image>();
            bg.color = new Color(0.98f, 0.96f, 0.93f, 0.98f);

            // Hàng 1: Khay công cụ kéo thả & khay hạt giống
            var toolsSection = CreateUIObject("ToolsSection", bottomBar.transform);
            var tsRect = toolsSection.GetComponent<RectTransform>();
            tsRect.anchorMin = new Vector2(0.02f, 0.38f);
            tsRect.anchorMax = new Vector2(0.98f, 0.96f);
            tsRect.offsetMin = Vector2.zero;
            tsRect.offsetMax = Vector2.zero;

            var hLayout = toolsSection.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 15;
            hLayout.childControlWidth = false;
            hLayout.childControlHeight = false;

            // Công cụ 1: Bình tưới (Draggable)
            uiMgr.wateringCanTool = CreateDraggableTool(toolsSection.transform, ToolType.WateringCan, "Bình tưới", AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Tools/watering_can.png"), canvas);

            // Công cụ 2: Xẻng (Draggable)
            uiMgr.shovelTool = CreateDraggableTool(toolsSection.transform, ToolType.Shovel, "Xẻng dọn", AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Tools/shovel.png"), canvas);

            // Công cụ 3: Liềm thu hoạch (Draggable)
            uiMgr.sickleTool = CreateDraggableTool(toolsSection.transform, ToolType.Sickle, "Thu hoạch", AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Tools/sickle.png"), canvas);

            // Khay chứa các hạt giống kéo thả (Seed Tray)
            var seedTray = CreateUIObject("SeedTrayContainer", toolsSection.transform);
            var stLayout = seedTray.AddComponent<HorizontalLayoutGroup>();
            stLayout.spacing = 10;
            stLayout.childControlWidth = false;
            stLayout.childControlHeight = false;
            uiMgr.seedTrayContainer = seedTray.transform;

            // Template hạt giống kéo thả
            uiMgr.draggableSeedPrefab = CreateSeedPrefabTemplate();

            // Hàng 2: Các nút chức năng (Cửa hàng, Kho, Hàng xóm, Sổ hoa)
            var navSection = CreateUIObject("NavSection", bottomBar.transform);
            var nsRect = navSection.GetComponent<RectTransform>();
            nsRect.anchorMin = new Vector2(0.02f, 0.04f);
            nsRect.anchorMax = new Vector2(0.98f, 0.34f);
            nsRect.offsetMin = Vector2.zero;
            nsRect.offsetMax = Vector2.zero;

            var navLayout = navSection.AddComponent<HorizontalLayoutGroup>();
            navLayout.spacing = 20;
            navLayout.childForceExpandWidth = true;
            navLayout.childForceExpandHeight = true;

            uiMgr.shopButton = CreateNavButton(navSection.transform, "Cửa hàng", AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/UI/shop_icon.png"));
            uiMgr.inventoryButton = CreateNavButton(navSection.transform, "Kho đồ", AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/UI/bag_icon.png"));
            uiMgr.ordersButton = CreateNavButton(navSection.transform, "Hàng xóm", AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/UI/order_icon.png"));
            uiMgr.flowerBookButton = CreateNavButton(navSection.transform, "Sổ hoa", AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/UI/book_icon.png"));

            return bottomBar;
        }

        private static DraggableToolItem CreateDraggableTool(Transform parent, ToolType type, string label, Sprite icon, Canvas canvas)
        {
            var itemObj = CreateUIObject($"Tool_{type}", parent);
            itemObj.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 220);

            var bg = itemObj.AddComponent<Image>();
            bg.color = new Color(0.92f, 0.92f, 0.92f);

            var iconObj = CreateIcon("Icon", itemObj.transform, icon, new Vector2(90, 90), new Vector2(0, 30));
            var nameTxt = CreateText("Name", itemObj.transform, label, 20, Color.black, TextAlignmentOptions.Center);
            nameTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50);

            var tool = itemObj.AddComponent<DraggableToolItem>();
            tool.toolType = type;
            tool.iconImage = iconObj.GetComponent<Image>();
            tool.nameText = nameTxt;
            tool.parentCanvas = canvas;

            return tool;
        }

        private static GameObject CreateSeedPrefabTemplate()
        {
            var seedObj = new GameObject("DraggableSeedItem_Prefab", typeof(RectTransform));
            seedObj.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 220);

            var bg = seedObj.AddComponent<Image>();
            bg.color = new Color(0.96f, 0.92f, 0.85f);

            var iconObj = CreateIcon("Icon", seedObj.transform, null, new Vector2(90, 90), new Vector2(0, 30));
            var countTxt = CreateText("Count", seedObj.transform, "x1", 20, new Color(0.2f, 0.5f, 0.1f), TextAlignmentOptions.Right);
            countTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(40, 70);

            var nameTxt = CreateText("Name", seedObj.transform, "Hạt giống", 18, Color.black, TextAlignmentOptions.Center);
            nameTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50);

            var tool = seedObj.AddComponent<DraggableToolItem>();
            tool.toolType = ToolType.Seed;
            tool.iconImage = iconObj.GetComponent<Image>();
            tool.countText = countTxt;
            tool.nameText = nameTxt;

            // Lưu thành Prefab
            string prefabPath = "Assets/_BalconyGarden/Prefabs/DraggableSeedItem.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(seedObj, prefabPath);
            Object.DestroyImmediate(seedObj);
            return prefab;
        }

        private static Button CreateNavButton(Transform parent, string title, Sprite icon)
        {
            var btnObj = CreateUIObject($"Btn_{title}", parent);
            var img = btnObj.AddComponent<Image>();
            img.color = new Color(0.88f, 0.94f, 0.88f);
            var btn = btnObj.AddComponent<Button>();

            if (icon != null)
            {
                CreateIcon("Icon", btnObj.transform, icon, new Vector2(45, 45), new Vector2(-40, 0));
            }

            var txt = CreateText("Label", btnObj.transform, title, 26, new Color(0.2f, 0.4f, 0.2f), TextAlignmentOptions.Center);
            txt.GetComponent<RectTransform>().anchoredPosition = new Vector2(25, 0);

            return btn;
        }

        private static void CreateAllPopups(Transform parent, UIManager uiMgr)
        {
            // 1. Shop Popup
            uiMgr.shopUI = CreateShopPopup(parent);

            // 2. Inventory Popup
            uiMgr.inventoryUI = CreateInventoryPopup(parent);

            // 3. Orders Popup
            uiMgr.ordersUI = CreateOrdersPopup(parent);

            // 4. Breeding Popup
            uiMgr.breedingUI = CreateBreedingPopup(parent);

            // 5. Plot Info Modal
            uiMgr.plotInfoUI = CreatePlotInfoModal(parent);

            // 6. Level Up Modal
            uiMgr.levelUpPopup = CreateLevelUpModal(parent, uiMgr);

            // 7. Toast Container
            var toastContainer = CreateUIObject("ToastContainer", parent);
            var tcRect = toastContainer.GetComponent<RectTransform>();
            tcRect.anchorMin = new Vector2(0.1f, 0.45f);
            tcRect.anchorMax = new Vector2(0.9f, 0.55f);
            tcRect.offsetMin = Vector2.zero;
            tcRect.offsetMax = Vector2.zero;
            uiMgr.toastContainer = toastContainer.transform;

            uiMgr.toastPrefab = CreateToastPrefab();
        }

        private static ShopUI CreateShopPopup(Transform parent)
        {
            var root = CreateModalRoot("ShopPopup", parent, "CỬA HÀNG HẠT GIỐNG");
            var shopUI = root.AddComponent<ShopUI>();
            shopUI.panelRoot = root;
            shopUI.closeButton = root.transform.Find("Header/CloseBtn").GetComponent<Button>();

            var content = root.transform.Find("ScrollArea/Viewport/Content");
            shopUI.itemsContainer = content;

            // Template Item Shop
            var itemObj = CreateUIObject("ShopItemTemplate", content);
            itemObj.GetComponent<RectTransform>().sizeDelta = new Vector2(850, 140);
            var itemBg = itemObj.AddComponent<Image>();
            itemBg.color = new Color(0.96f, 0.96f, 0.96f);

            var icon = CreateIcon("Icon", itemObj.transform, null, new Vector2(80, 80), new Vector2(-340, 0));
            var title = CreateText("Title", itemObj.transform, "Tên Cây", 28, Color.black, TextAlignmentOptions.Left);
            title.GetComponent<RectTransform>().anchoredPosition = new Vector2(-150, 25);
            var price = CreateText("Price", itemObj.transform, "10 Xu", 24, new Color(0.8f, 0.5f, 0.0f), TextAlignmentOptions.Left);
            price.GetComponent<RectTransform>().anchoredPosition = new Vector2(-150, -25);
            var desc = CreateText("Desc", itemObj.transform, "Mô tả", 20, Color.gray, TextAlignmentOptions.Left);
            desc.GetComponent<RectTransform>().anchoredPosition = new Vector2(100, 0);

            var buyBtnObj = CreateUIObject("BuyBtn", itemObj.transform);
            buyBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(140, 60);
            buyBtnObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(320, 0);
            var bbImg = buyBtnObj.AddComponent<Image>();
            bbImg.color = new Color(0.3f, 0.75f, 0.3f);
            var buyBtn = buyBtnObj.AddComponent<Button>();
            CreateText("BuyTxt", buyBtnObj.transform, "Mua", 24, Color.white, TextAlignmentOptions.Center);

            shopUI.shopItemPrefab = itemObj;
            itemObj.SetActive(false);
            root.SetActive(false);

            return shopUI;
        }

        private static InventoryUI CreateInventoryPopup(Transform parent)
        {
            var root = CreateModalRoot("InventoryPopup", parent, "KHO NÔNG SẢN");
            var invUI = root.AddComponent<InventoryUI>();
            invUI.panelRoot = root;
            invUI.closeButton = root.transform.Find("Header/CloseBtn").GetComponent<Button>();

            // Nút bán tất cả
            var sellAllObj = CreateUIObject("SellAllBtn", root.transform);
            var saRect = sellAllObj.GetComponent<RectTransform>();
            saRect.anchorMin = new Vector2(0.6f, 0.88f);
            saRect.anchorMax = new Vector2(0.85f, 0.96f);
            saRect.offsetMin = Vector2.zero;
            saRect.offsetMax = Vector2.zero;
            var saImg = sellAllObj.AddComponent<Image>();
            saImg.color = new Color(0.95f, 0.6f, 0.2f);
            invUI.sellAllButton = sellAllObj.AddComponent<Button>();
            CreateText("Label", sellAllObj.transform, "Bán tất cả 💰", 24, Color.white, TextAlignmentOptions.Center);

            var content = root.transform.Find("ScrollArea/Viewport/Content");
            invUI.cropsContainer = content;

            // Template Crop Item
            var itemObj = CreateUIObject("CropItemTemplate", content);
            itemObj.GetComponent<RectTransform>().sizeDelta = new Vector2(850, 130);
            itemObj.AddComponent<Image>().color = new Color(0.95f, 0.95f, 0.95f);

            CreateIcon("Icon", itemObj.transform, null, new Vector2(80, 80), new Vector2(-340, 0));
            var nameTxt = CreateText("Name", itemObj.transform, "Tên Cây", 26, Color.black, TextAlignmentOptions.Left);
            nameTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(-150, 20);
            var countTxt = CreateText("Count", itemObj.transform, "x5", 22, Color.gray, TextAlignmentOptions.Left);
            countTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(-150, -20);
            var valTxt = CreateText("Value", itemObj.transform, "Giá: 15 Xu", 22, new Color(0.7f, 0.5f, 0.1f), TextAlignmentOptions.Left);
            valTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(80, 0);

            var sellBtnObj = CreateUIObject("SellBtn", itemObj.transform);
            sellBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(130, 55);
            sellBtnObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(330, 0);
            sellBtnObj.AddComponent<Image>().color = new Color(0.3f, 0.7f, 0.3f);
            sellBtnObj.AddComponent<Button>();
            CreateText("Label", sellBtnObj.transform, "Bán 1", 22, Color.white, TextAlignmentOptions.Center);

            invUI.cropItemPrefab = itemObj;
            itemObj.SetActive(false);
            root.SetActive(false);

            return invUI;
        }

        private static OrdersUI CreateOrdersPopup(Transform parent)
        {
            var root = CreateModalRoot("OrdersPopup", parent, "ĐƠN HÀNG HÀNG XÓM");
            var ordersUI = root.AddComponent<OrdersUI>();
            ordersUI.panelRoot = root;
            ordersUI.closeButton = root.transform.Find("Header/CloseBtn").GetComponent<Button>();

            var content = root.transform.Find("ScrollArea/Viewport/Content");
            ordersUI.ordersContainer = content;

            // Template Order Card
            var cardObj = CreateUIObject("OrderCardTemplate", content);
            cardObj.GetComponent<RectTransform>().sizeDelta = new Vector2(850, 220);
            cardObj.AddComponent<Image>().color = new Color(0.96f, 0.95f, 0.92f);

            CreateIcon("Avatar", cardObj.transform, null, new Vector2(90, 90), new Vector2(-340, 40));
            CreateIcon("CropIcon", cardObj.transform, null, new Vector2(60, 60), new Vector2(-340, -50));

            var nameTxt = CreateText("Name", cardObj.transform, "Bà Lan", 28, new Color(0.2f, 0.2f, 0.2f), TextAlignmentOptions.Left);
            nameTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100, 60);

            var dialogueTxt = CreateText("Dialogue", cardObj.transform, "\"Rau cải của cháu ngon quá...\"", 22, new Color(0.3f, 0.3f, 0.3f), TextAlignmentOptions.Left);
            dialogueTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(60, 20);
            dialogueTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 50);

            var reqTxt = CreateText("Request", cardObj.transform, "Cần: 3 Cải Xanh", 24, Color.black, TextAlignmentOptions.Left);
            reqTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100, -45);

            var rewardTxt = CreateText("Reward", cardObj.transform, "Thưởng: +50 Xu, +20 XP", 22, new Color(0.8f, 0.5f, 0.1f), TextAlignmentOptions.Left);
            rewardTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(140, -45);

            var deliverBtnObj = CreateUIObject("DeliverBtn", cardObj.transform);
            deliverBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 65);
            deliverBtnObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(330, -35);
            deliverBtnObj.AddComponent<Image>().color = new Color(0.25f, 0.75f, 0.35f);
            deliverBtnObj.AddComponent<Button>();
            CreateText("Label", deliverBtnObj.transform, "Giao Hàng", 22, Color.white, TextAlignmentOptions.Center);

            ordersUI.orderCardPrefab = cardObj;
            cardObj.SetActive(false);
            root.SetActive(false);

            return ordersUI;
        }

        private static BreedingUI CreateBreedingPopup(Transform parent)
        {
            var root = CreateModalRoot("BreedingPopup", parent, "SỔ TAY HOA & LAI TẠO");
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
            tab1Obj.AddComponent<Image>().color = new Color(0.8f, 0.9f, 0.8f);
            bUI.tabCodexButton = tab1Obj.AddComponent<Button>();
            CreateText("Label", tab1Obj.transform, "📖 Sổ Tay Hoa", 24, Color.black, TextAlignmentOptions.Center);

            var tab2Obj = CreateUIObject("TabBreeding", tabRow.transform);
            tab2Obj.GetComponent<RectTransform>().sizeDelta = new Vector2(350, 60);
            tab2Obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(190, 0);
            tab2Obj.AddComponent<Image>().color = new Color(0.95f, 0.85f, 0.9f);
            bUI.tabBreedingButton = tab2Obj.AddComponent<Button>();
            CreateText("Label", tab2Obj.transform, "🔬 Phòng Lai Tạo", 24, Color.black, TextAlignmentOptions.Center);

            // Nội dung Tab 1: Codex
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
            codexCard.AddComponent<Image>().color = new Color(0.95f, 0.95f, 0.95f);
            CreateIcon("Icon", codexCard.transform, null, new Vector2(80, 80), new Vector2(-340, 0));
            var cName = CreateText("Name", codexCard.transform, "Tên Hoa", 26, Color.black, TextAlignmentOptions.Left);
            cName.GetComponent<RectTransform>().anchoredPosition = new Vector2(-150, 20);
            var cDesc = CreateText("Desc", codexCard.transform, "Mô tả hoa", 20, Color.gray, TextAlignmentOptions.Left);
            cDesc.GetComponent<RectTransform>().anchoredPosition = new Vector2(40, -15);
            bUI.codexCardPrefab = codexCard;
            codexCard.SetActive(false);

            // Nội dung Tab 2: Breeding
            var breedingRoot = CreateUIObject("BreedingTabRoot", root.transform);
            var brRect = breedingRoot.GetComponent<RectTransform>();
            brRect.anchorMin = new Vector2(0.05f, 0.05f);
            brRect.anchorMax = new Vector2(0.95f, 0.86f);
            brRect.offsetMin = Vector2.zero;
            brRect.offsetMax = Vector2.zero;
            bUI.breedingTabRoot = breedingRoot;

            var infoTxt = CreateText("BreedingInfo", breedingRoot.transform, "Chọn 2 bông hoa từ kho để ghép giống:", 26, Color.black, TextAlignmentOptions.Center);
            infoTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 300);

            // Dropdown A & Preview
            var ddA = CreateDropdown("DropdownA", breedingRoot.transform, new Vector2(-220, 160));
            bUI.flowerDropdownA = ddA;
            var prevA = CreateIcon("PreviewA", breedingRoot.transform, null, new Vector2(120, 120), new Vector2(-220, 30));
            bUI.previewFlowerA = prevA.GetComponent<Image>();

            // Biểu tượng dấu cộng +
            var plusTxt = CreateText("Plus", breedingRoot.transform, "➕", 40, Color.gray, TextAlignmentOptions.Center);
            plusTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 30);

            // Dropdown B & Preview
            var ddB = CreateDropdown("DropdownB", breedingRoot.transform, new Vector2(220, 160));
            bUI.flowerDropdownB = ddB;
            var prevB = CreateIcon("PreviewB", breedingRoot.transform, null, new Vector2(120, 120), new Vector2(220, 30));
            bUI.previewFlowerB = prevB.GetComponent<Image>();

            // Nút Lai tạo
            var breedBtnObj = CreateUIObject("BreedBtn", breedingRoot.transform);
            breedBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 80);
            breedBtnObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -120);
            breedBtnObj.AddComponent<Image>().color = new Color(0.85f, 0.35f, 0.7f);
            bUI.breedButton = breedBtnObj.AddComponent<Button>();
            CreateText("Label", breedBtnObj.transform, "Bắt Đầu Lai Tạo 🌸", 26, Color.white, TextAlignmentOptions.Center);

            // Kết quả
            var resTxt = CreateText("ResultTxt", breedingRoot.transform, "Kết quả lai tạo sẽ hiển thị tại đây...", 24, new Color(0.2f, 0.5f, 0.2f), TextAlignmentOptions.Center);
            resTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -250);
            bUI.breedResultText = resTxt;

            root.SetActive(false);
            return bUI;
        }

        private static PlotInfoUI CreatePlotInfoModal(Transform parent)
        {
            var root = CreateModalRoot("PlotInfoModal", parent, "CHI TIẾT CHẬU CÂY");
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 900);
            var infoUI = root.AddComponent<PlotInfoUI>();
            infoUI.panelRoot = root;
            infoUI.closeButton = root.transform.Find("Header/CloseBtn").GetComponent<Button>();

            // Bỏ scroll area của modal này
            var sa = root.transform.Find("ScrollArea");
            if (sa != null) Object.DestroyImmediate(sa.gameObject);

            var preview = CreateIcon("Preview", root.transform, null, new Vector2(180, 180), new Vector2(0, 200));
            infoUI.plantPreviewImage = preview.GetComponent<Image>();

            infoUI.plantNameText = CreateText("Name", root.transform, "Tên Cây", 34, Color.black, TextAlignmentOptions.Center);
            infoUI.plantNameText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 80);

            infoUI.stageText = CreateText("Stage", root.transform, "Trạng thái: Đang lớn", 24, Color.gray, TextAlignmentOptions.Center);
            infoUI.stageText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 30);

            infoUI.progressText = CreateText("Progress", root.transform, "Tiến độ: 50%", 22, Color.black, TextAlignmentOptions.Center);
            infoUI.progressText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -15);

            infoUI.waterText = CreateText("Water", root.transform, "Độ ẩm đất: 80%", 22, new Color(0.1f, 0.5f, 0.8f), TextAlignmentOptions.Center);
            infoUI.waterText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -55);

            infoUI.sunPreferenceText = CreateText("Sun", root.transform, "Ưa thích: Nắng vừa", 22, new Color(0.8f, 0.5f, 0.1f), TextAlignmentOptions.Center);
            infoUI.sunPreferenceText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -95);

            infoUI.soilTypeText = CreateText("Soil", root.transform, "Loại đất: Đất thường", 22, new Color(0.5f, 0.35f, 0.2f), TextAlignmentOptions.Center);
            infoUI.soilTypeText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -135);

            // Nút Thao tác
            var btnRow = CreateUIObject("ActionBtns", root.transform);
            var brRect = btnRow.GetComponent<RectTransform>();
            brRect.anchoredPosition = new Vector2(0, -250);
            brRect.sizeDelta = new Vector2(700, 80);
            var brLayout = btnRow.AddComponent<HorizontalLayoutGroup>();
            brLayout.spacing = 15;
            brLayout.childForceExpandWidth = true;
            brLayout.childForceExpandHeight = true;

            var wBtn = CreateActionButton("WaterBtn", btnRow.transform, "Tưới Nước 💧", new Color(0.2f, 0.6f, 0.9f));
            infoUI.waterButton = wBtn;

            var hBtn = CreateActionButton("HarvestBtn", btnRow.transform, "Thu Hoạch 🌸", new Color(0.3f, 0.8f, 0.3f));
            infoUI.harvestButton = hBtn;

            var speedBtn = CreateActionButton("SpeedBtn", btnRow.transform, "Lớn Ngay (1💎)", new Color(0.85f, 0.45f, 0.9f));
            infoUI.speedUpGemsButton = speedBtn;

            var cBtn = CreateActionButton("ClearBtn", btnRow.transform, "Dọn Chậu 🗑️", new Color(0.8f, 0.4f, 0.4f));
            infoUI.clearButton = cBtn;

            root.SetActive(false);
            return infoUI;
        }

        private static GameObject CreateLevelUpModal(Transform parent, UIManager uiMgr)
        {
            var modal = CreateUIObject("LevelUpModal", parent);
            SetStretch(modal.GetComponent<RectTransform>());
            modal.AddComponent<Image>().color = new Color(0, 0, 0, 0.7f);

            var box = CreateUIObject("Box", modal.transform);
            box.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 500);
            box.AddComponent<Image>().color = Color.white;

            var title = CreateText("Title", box.transform, "CHÚC MỪNG LÊN CẤP!", 36, new Color(0.9f, 0.6f, 0.1f), TextAlignmentOptions.Center);
            title.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 150);

            var desc = CreateText("Desc", box.transform, "Bạn nhận được quà tặng!", 28, Color.black, TextAlignmentOptions.Center);
            desc.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 40);
            uiMgr.levelUpTitleText = desc;

            var closeBtnObj = CreateUIObject("ClaimBtn", box.transform);
            closeBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 70);
            closeBtnObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -140);
            closeBtnObj.AddComponent<Image>().color = new Color(0.3f, 0.8f, 0.3f);
            var btn = closeBtnObj.AddComponent<Button>();
            CreateText("Label", closeBtnObj.transform, "Nhận Thưởng ✨", 26, Color.white, TextAlignmentOptions.Center);
            uiMgr.levelUpCloseButton = btn;

            modal.SetActive(false);
            return modal;
        }

        private static GameObject CreateToastPrefab()
        {
            var toast = new GameObject("Toast_Prefab", typeof(RectTransform));
            toast.GetComponent<RectTransform>().sizeDelta = new Vector2(650, 90);

            var bg = toast.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

            var txt = CreateText("Message", toast.transform, "Thông báo!", 26, Color.white, TextAlignmentOptions.Center);
            SetStretch(txt.GetComponent<RectTransform>());

            string path = "Assets/_BalconyGarden/Prefabs/ToastNotification.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(toast, path);
            Object.DestroyImmediate(toast);
            return prefab;
        }

        // --- UI Helpers ---

        private static GameObject CreateModalRoot(string name, Transform parent, string title)
        {
            var root = CreateUIObject(name, parent);
            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.05f, 0.12f);
            rect.anchorMax = new Vector2(0.95f, 0.88f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var bg = root.AddComponent<Image>();
            bg.color = new Color(0.98f, 0.97f, 0.95f, 0.99f);

            // Header
            var header = CreateUIObject("Header", root.transform);
            var hRect = header.GetComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0f, 0.92f);
            hRect.anchorMax = new Vector2(1f, 1f);
            hRect.offsetMin = Vector2.zero;
            hRect.offsetMax = Vector2.zero;
            header.AddComponent<Image>().color = new Color(0.88f, 0.93f, 0.88f);

            var titleTxt = CreateText("Title", header.transform, title, 32, new Color(0.2f, 0.4f, 0.2f), TextAlignmentOptions.Center);
            SetStretch(titleTxt.GetComponent<RectTransform>());

            var closeBtn = CreateUIObject("CloseBtn", header.transform);
            var cbRect = closeBtn.GetComponent<RectTransform>();
            cbRect.anchorMin = new Vector2(0.9f, 0.1f);
            cbRect.anchorMax = new Vector2(0.98f, 0.9f);
            cbRect.offsetMin = Vector2.zero;
            cbRect.offsetMax = Vector2.zero;
            closeBtn.AddComponent<Image>().color = new Color(0.85f, 0.3f, 0.3f);
            closeBtn.AddComponent<Button>();
            CreateText("X", closeBtn.transform, "✕", 26, Color.white, TextAlignmentOptions.Center);

            // Scroll Area
            CreateScrollArea("ScrollArea", root.transform);

            return root;
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

        private static Button CreateActionButton(string name, Transform parent, string label, Color color)
        {
            var btnObj = CreateUIObject(name, parent);
            btnObj.AddComponent<Image>().color = color;
            var btn = btnObj.AddComponent<Button>();
            CreateText("Label", btnObj.transform, label, 20, Color.white, TextAlignmentOptions.Center);
            return btn;
        }

        private static TMP_Dropdown CreateDropdown(string name, Transform parent, Vector2 pos)
        {
            var ddObj = CreateUIObject(name, parent);
            ddObj.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 50);
            ddObj.GetComponent<RectTransform>().anchoredPosition = pos;
            ddObj.AddComponent<Image>().color = Color.white;
            var dd = ddObj.AddComponent<TMP_Dropdown>();

            var label = CreateText("Label", ddObj.transform, "Chọn hoa...", 20, Color.black, TextAlignmentOptions.Left);
            dd.captionText = label;

            return dd;
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float fontSize, Color color, TextAlignmentOptions alignment)
        {
            var obj = CreateUIObject(name, parent);
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
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

        private static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool found = false;
            foreach (var s in scenes)
            {
                if (s.path == scenePath) { found = true; break; }
            }
            if (!found)
            {
                scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }
    }
}
#endif
