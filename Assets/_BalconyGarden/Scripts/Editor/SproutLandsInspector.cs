#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BalconyGarden.Editor
{
    public static class SproutLandsInspector
    {
        [MenuItem("Balcony Garden/Debug/Inspect Sprout Lands Assets")]
        public static void InspectAssets()
        {
            string basePath = "Assets/_BalconyGarden/Sprout Lands - UI Pack - Basic pack/Sprout Lands - UI Pack - Basic pack";
            string[] texturePaths = {
                $"{basePath}/Sprite sheets/Sprite sheet for Basic Pack.png",
                $"{basePath}/Sprite sheets/Setting menu.png",
                $"{basePath}/Sprite sheets/Dialouge UI/Premade dialog box  big.png",
                $"{basePath}/Sprite sheets/Dialouge UI/Premade dialog box medium.png",
                $"{basePath}/Sprite sheets/Dialouge UI/Premade dialog box small.png",
                $"{basePath}/Sprite sheets/Dialouge UI/dialog box big.png",
                $"{basePath}/Sprite sheets/buttons/Square Buttons 26x26.png",
                $"{basePath}/Sprite sheets/buttons/Small Square Buttons.png",
                $"{basePath}/Sprite sheets/Icons/All Icons.png"
            };

            var sb = new StringBuilder();
            foreach (var path in texturePaths)
            {
                var assets = AssetDatabase.LoadAllAssetsAtPath(path);
                sb.AppendLine($"--- {path} (Total sub-assets: {assets.Length}) ---");
                int count = 0;
                foreach (var a in assets)
                {
                    if (a is Sprite s)
                    {
                        sb.AppendLine($"  Sprite: '{s.name}' Rect: {s.rect} Border: {s.border}");
                        count++;
                        if (count > 20)
                        {
                            sb.AppendLine($"  ... and {assets.Length - 20} more sprites.");
                            break;
                        }
                    }
                }
            }

            Debug.Log(sb.ToString());
        }
    }
}
#endif
