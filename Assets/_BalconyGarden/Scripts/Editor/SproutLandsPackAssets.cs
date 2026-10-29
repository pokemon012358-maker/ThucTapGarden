#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace BalconyGarden.Editor
{
    public static class SproutLandsPackAssets
    {
        public const string PackRoot =
            "Assets/_BalconyGarden/Sprout Lands - UI Pack - Basic pack/Sprout Lands - UI Pack - Basic pack";

        public const string FontPath = PackRoot + "/fonts/pixelFont-7-8x14-sproutLands SDF.asset";
        public const string FallbackFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

        public static TMP_FontAsset PixelFont { get; private set; }

        public static Sprite PanelBlank { get; private set; }
        public static Sprite PanelHeader { get; private set; }
        public static Sprite DialogBar { get; private set; }
        public static Sprite SlotSmall { get; private set; }
        public static Sprite SlotLarge { get; private set; }
        public static Sprite ButtonNormal { get; private set; }
        public static Sprite ButtonPressed { get; private set; }
        public static Sprite CloseButton { get; private set; }
        public static Sprite IconShop { get; private set; }
        public static Sprite IconInventory { get; private set; }
        public static Sprite IconNeighbors { get; private set; }
        public static Sprite IconBook { get; private set; }
        public static Sprite IconClose { get; private set; }

        public static void PrepareAndLoad()
        {
            SetPointFilterAndBorder(PackRoot + "/emojis-free/emoji style ui/Inventory_Blocks_Spritesheet.png", 8);
            SetPointFilterAndBorder(PackRoot + "/Sprite sheets/Dialouge UI/dialog box medium.png", 12);
            SetPointFilter(PackRoot + "/Sprite sheets/Setting menu.png");
            SetPointFilter(PackRoot + "/Sprite sheets/buttons/Square Buttons 26x26.png");
            SetPointFilter(PackRoot + "/Sprite sheets/buttons/Small Square Buttons.png");
            SetPointFilter(PackRoot + "/Sprite sheets/Icons/All Icons.png");
            AssetDatabase.Refresh();

            PixelFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            var fallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FallbackFontPath);
            if (PixelFont != null && fallback != null)
            {
                if (PixelFont.fallbackFontAssetTable == null)
                    PixelFont.fallbackFontAssetTable = new List<TMP_FontAsset>();
                if (!PixelFont.fallbackFontAssetTable.Contains(fallback))
                {
                    PixelFont.fallbackFontAssetTable.Add(fallback);
                    EditorUtility.SetDirty(PixelFont);
                }
            }

            PanelBlank = LoadSprite(PackRoot + "/Sprite sheets/Setting menu.png", "Setting menu_1");
            PanelHeader = LoadSprite(PackRoot + "/Sprite sheets/Setting menu.png", "Setting menu_0");
            DialogBar = LoadSprite(PackRoot + "/Sprite sheets/Dialouge UI/dialog box medium.png", "dialog box medium_0");
            SlotSmall = LoadSprite(PackRoot + "/emojis-free/emoji style ui/Inventory_Blocks_Spritesheet.png", "Inventory_Blocks_Spritesheet_1");
            SlotLarge = LoadSprite(PackRoot + "/emojis-free/emoji style ui/Inventory_Blocks_Spritesheet.png", "Inventory_Blocks_Spritesheet_7");
            ButtonNormal = LoadSprite(PackRoot + "/Sprite sheets/buttons/Square Buttons 26x26.png", "Square Buttons 26x26_2");
            ButtonPressed = LoadSprite(PackRoot + "/Sprite sheets/buttons/Square Buttons 26x26.png", "Square Buttons 26x26_3");
            CloseButton = LoadSprite(PackRoot + "/Sprite sheets/buttons/Small Square Buttons.png", "Small Square Buttons_0");
            IconShop = LoadSprite(PackRoot + "/Sprite sheets/Icons/All Icons.png", "All Icons_4");
            IconInventory = LoadSprite(PackRoot + "/Sprite sheets/Icons/All Icons.png", "All Icons_5");
            IconNeighbors = LoadSprite(PackRoot + "/Sprite sheets/Icons/All Icons.png", "All Icons_2");
            IconBook = LoadSprite(PackRoot + "/Sprite sheets/Icons/All Icons.png", "All Icons_6");
            IconClose = LoadSprite(PackRoot + "/Sprite sheets/Icons/All Icons.png", "All Icons_7");
        }

        public static Sprite LoadSprite(string assetPath, string spriteName)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            if (assets == null) return null;

            foreach (var asset in assets)
            {
                if (asset is Sprite sprite && sprite.name == spriteName)
                    return sprite;
            }

            foreach (var asset in assets)
            {
                if (asset is Sprite sprite)
                    return sprite;
            }

            return null;
        }

        public static void ApplySliced(Image image, Sprite sprite, float pixelsPerUnitMultiplier = 0.38f)
        {
            if (image == null) return;
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = pixelsPerUnitMultiplier;
            image.color = Color.white;
        }

        public static void ApplyPixelFont(TextMeshProUGUI tmp)
        {
            if (tmp == null || PixelFont == null) return;
            tmp.font = PixelFont;
            tmp.fontStyle = FontStyles.Normal;
            tmp.extraPadding = true;
            tmp.richText = true;
        }

        private static void SetPointFilter(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        private static void SetPointFilterAndBorder(string path, int border)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;

            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.spriteImportMode = SpriteImportMode.Multiple;

            var sheets = importer.spritesheet;
            if (sheets != null)
            {
                for (int i = 0; i < sheets.Length; i++)
                {
                    var meta = sheets[i];
                    meta.border = new Vector4(border, border, border, border);
                    sheets[i] = meta;
                }
                importer.spritesheet = sheets;
            }

            importer.SaveAndReimport();
        }
    }
}
#endif
