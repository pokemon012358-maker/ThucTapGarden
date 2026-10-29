#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BalconyGarden.Editor
{
    public static class BalconyGardenAssetGenerator
    {
        private const string RootPath = "Assets/_BalconyGarden";
        private const string SpritesPath = "Assets/_BalconyGarden/Sprites";
        private const string PlantsSOPath = "Assets/_BalconyGarden/ScriptableObjects/Plants";
        private const string NeighborsSOPath = "Assets/_BalconyGarden/ScriptableObjects/Neighbors";

        [MenuItem("Balcony Garden/1. Tạo toàn bộ Sprite & ScriptableObjects", false, 1)]
        public static void GenerateAllAssetsMenu()
        {
            GenerateAllAssets(true);
        }

        public static void GenerateAllAssets(bool showDialog = false)
        {
            EnsureDirectories();
            GenerateAllSprites();
            AssetDatabase.Refresh();

            // Cấu hình Texture Importer thành Sprite 2D
            ConfigureAllTextureImporters();
            AssetDatabase.Refresh();

            // Tạo các ScriptableObjects và gán sprite
            CreateAllPlantScriptableObjects();
            CreateAllNeighborScriptableObjects();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (showDialog)
            {
                EditorUtility.DisplayDialog("Thành công!", 
                    "Đã tạo toàn bộ Sprites mẫu và ScriptableObjects!\n\n" +
                    "Sau này khi bạn vẽ xong hình, chỉ cần đè file PNG trong thư mục 'Assets/_BalconyGarden/Sprites' hoặc kéo Sprite mới vào ScriptableObject.", 
                    "Tuyệt vời");
            }
        }

        private static void EnsureDirectories()
        {
            string[] dirs = {
                RootPath,
                SpritesPath,
                SpritesPath + "/Plants",
                SpritesPath + "/Tools",
                SpritesPath + "/Environment",
                SpritesPath + "/UI",
                SpritesPath + "/Neighbors",
                RootPath + "/ScriptableObjects",
                PlantsSOPath,
                NeighborsSOPath,
                RootPath + "/Prefabs"
            };

            foreach (var d in dirs)
            {
                if (!Directory.Exists(d))
                {
                    Directory.CreateDirectory(d);
                }
            }
        }

        private static void GenerateAllSprites()
        {
            // 1. Sprites môi trường
            SaveTexture(CreateColorTexture(1080, 1920, new Color(0.85f, 0.94f, 0.98f)), SpritesPath + "/Environment/balcony_bg.png");
            SaveTexture(CreatePotTexture(220, 180, new Color(0.82f, 0.52f, 0.35f)), SpritesPath + "/Environment/pot_empty.png");
            SaveTexture(CreateColorTexture(200, 40, new Color(0.45f, 0.28f, 0.18f)), SpritesPath + "/Environment/shelf.png");

            // 2. Sprites công cụ
            SaveTexture(CreateCircleTexture(128, 128, new Color(0.2f, 0.6f, 0.9f), "💧"), SpritesPath + "/Tools/watering_can.png");
            SaveTexture(CreateCircleTexture(128, 128, new Color(0.6f, 0.4f, 0.25f), "⛏"), SpritesPath + "/Tools/shovel.png");
            SaveTexture(CreateCircleTexture(128, 128, new Color(0.95f, 0.75f, 0.2f), "🌾"), SpritesPath + "/Tools/sickle.png");
            SaveTexture(CreateRoundedBoxTexture(128, 128, new Color(0.95f, 0.88f, 0.75f), new Color(0.6f, 0.45f, 0.25f)), SpritesPath + "/Tools/seed_packet.png");

            // 3. Sprites UI
            SaveTexture(CreateCircleTexture(96, 96, new Color(1.0f, 0.84f, 0.0f), "Xu"), SpritesPath + "/UI/coin.png");
            SaveTexture(CreateCircleTexture(96, 96, new Color(0.2f, 0.85f, 0.95f), "💎"), SpritesPath + "/UI/gem.png");
            SaveTexture(CreateCircleTexture(80, 80, new Color(0.3f, 0.7f, 1.0f), "💧"), SpritesPath + "/UI/water_drop.png");
            SaveTexture(CreateCircleTexture(80, 80, new Color(1.0f, 0.6f, 0.1f), "☀️"), SpritesPath + "/UI/sun.png");
            SaveTexture(CreateRoundedBoxTexture(96, 96, new Color(0.95f, 0.7f, 0.8f), Color.white), SpritesPath + "/UI/order_icon.png");
            SaveTexture(CreateRoundedBoxTexture(96, 96, new Color(0.7f, 0.85f, 0.95f), Color.white), SpritesPath + "/UI/book_icon.png");
            SaveTexture(CreateRoundedBoxTexture(96, 96, new Color(0.98f, 0.85f, 0.55f), Color.white), SpritesPath + "/UI/shop_icon.png");
            SaveTexture(CreateRoundedBoxTexture(96, 96, new Color(0.8f, 0.95f, 0.75f), Color.white), SpritesPath + "/UI/bag_icon.png");
            SaveTexture(CreateCircleTexture(80, 80, new Color(0.3f, 0.3f, 0.3f, 0.8f), "🔒"), SpritesPath + "/UI/lock.png");

            // 4. Sprites 5 cây trồng MVP + 3 hoa lai
            GeneratePlantSprites("cai_xanh", new Color(0.3f, 0.75f, 0.25f), new Color(0.4f, 0.85f, 0.35f));
            GeneratePlantSprites("cuc_vang", new Color(0.98f, 0.82f, 0.15f), new Color(1.0f, 0.92f, 0.3f));
            GeneratePlantSprites("ca_chua", new Color(0.92f, 0.28f, 0.25f), new Color(1.0f, 0.35f, 0.3f));
            GeneratePlantSprites("huong_duong", new Color(0.95f, 0.68f, 0.1f), new Color(1.0f, 0.8f, 0.2f));
            GeneratePlantSprites("hoa_hong", new Color(0.92f, 0.18f, 0.35f), new Color(1.0f, 0.3f, 0.45f));
            
            // Hoa lai
            GeneratePlantSprites("cuc_cam", new Color(0.95f, 0.5f, 0.1f), new Color(1.0f, 0.65f, 0.2f));
            GeneratePlantSprites("cuc_mat_troi", new Color(1.0f, 0.45f, 0.05f), new Color(1.0f, 0.75f, 0.1f));
            GeneratePlantSprites("hoa_hong_xanh", new Color(0.2f, 0.45f, 0.95f), new Color(0.35f, 0.65f, 1.0f));

            // 5. Sprites hàng xóm
            SaveTexture(CreateNeighborAvatar(180, 180, new Color(0.95f, 0.75f, 0.75f), "Bà Lan"), SpritesPath + "/Neighbors/ba_lan.png");
            SaveTexture(CreateNeighborAvatar(180, 180, new Color(0.75f, 0.85f, 0.95f), "Anh Minh"), SpritesPath + "/Neighbors/anh_minh.png");
            SaveTexture(CreateNeighborAvatar(180, 180, new Color(0.95f, 0.8f, 0.95f), "Chị Hoa"), SpritesPath + "/Neighbors/chi_hoa.png");
            SaveTexture(CreateNeighborAvatar(180, 180, new Color(0.85f, 0.9f, 0.8f), "Ông Tư"), SpritesPath + "/Neighbors/ong_tu.png");
        }

        private static void GeneratePlantSprites(string plantId, Color mainColor, Color brightColor)
        {
            string plantFolder = $"{SpritesPath}/Plants/{plantId}";
            if (!Directory.Exists(plantFolder)) Directory.CreateDirectory(plantFolder);

            // Giai đoạn 1: Hạt
            SaveTexture(CreateCircleTexture(140, 140, new Color(0.45f, 0.32f, 0.22f), "•"), $"{plantFolder}/seed.png");
            // Giai đoạn 2: Mầm non
            SaveTexture(CreateSproutTexture(160, 160, new Color(0.55f, 0.85f, 0.35f)), $"{plantFolder}/sprout.png");
            // Giai đoạn 3: Cây đang lớn
            SaveTexture(CreateMatureTexture(180, 180, mainColor), $"{plantFolder}/mature.png");
            // Giai đoạn 4: Chín / Nở hoa
            SaveTexture(CreateBloomTexture(200, 200, brightColor), $"{plantFolder}/ripe.png");
            // Giai đoạn héo
            SaveTexture(CreateWitheredTexture(180, 180, new Color(0.65f, 0.58f, 0.42f)), $"{plantFolder}/withered.png");
            // Icon túi hạt
            SaveTexture(CreateSeedPacketTexture(128, 128, mainColor), $"{plantFolder}/seed_packet.png");
            // Sản phẩm thu hoạch
            SaveTexture(CreateHarvestCropTexture(128, 128, brightColor), $"{plantFolder}/crop.png");
        }

        private static void ConfigureAllTextureImporters()
        {
            string[] pngGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { SpritesPath });
            foreach (var guid in pngGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = 100;
                    importer.alphaIsTransparency = true;
                    importer.mipmapEnabled = false;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.SaveAndReimport();
                }
            }
        }

        private static void CreateAllPlantScriptableObjects()
        {
            // 1. Cải xanh
            CreateOrUpdatePlant("cai_xanh", "Cải Xanh", PlantType.Vegetable, "Rau cải xanh tươi giòn, giàu vitamin, dễ trồng.", 120f, 0.009f, SunlightLevel.Medium, 5, 12, 1, 15, 2);

            // 2. Cúc vàng
            CreateOrUpdatePlant("cuc_vang", "Cúc Vàng", PlantType.Flower, "Hoa cúc vàng rực rỡ, mang lại niềm vui và bình yên.", 300f, 0.007f, SunlightLevel.High, 10, 28, 1, 30, 2);

            // 3. Cà chua
            CreateOrUpdatePlant("ca_chua", "Cà Chua", PlantType.Vegetable, "Cà chua mọng nước, nguyên liệu số một cho các bữa ăn.", 900f, 0.008f, SunlightLevel.High, 25, 70, 3, 65, 3);

            // 4. Hướng dương
            CreateOrUpdatePlant("huong_duong", "Hướng Dương", PlantType.Flower, "Đóa hướng dương luôn hướng về ánh mặt trời chói chang.", 1800f, 0.006f, SunlightLevel.High, 40, 120, 3, 110, 2);

            // 5. Hoa hồng
            CreateOrUpdatePlant("hoa_hong", "Hoa Hồng", PlantType.Flower, "Nữ hoàng của các loài hoa, hương thơm ngào ngạt quý phái.", 3600f, 0.005f, SunlightLevel.Medium, 80, 260, 10, 260, 3);

            // 6. Giống lai: Cúc Cam
            CreateOrUpdatePlant("cuc_cam", "Cúc Cam", PlantType.Flower, "Hoa lai giữa Cúc Vàng và Hoa Hồng. Sắc cam ấm áp.", 1500f, 0.006f, SunlightLevel.High, 60, 180, 5, 150, 2);

            // 7. Giống lai: Cúc Mặt Trời
            CreateOrUpdatePlant("cuc_mat_troi", "Cúc Mặt Trời", PlantType.Flower, "Hoa lai quý hiếm giữa Cúc Vàng và Hướng Dương.", 2400f, 0.005f, SunlightLevel.High, 90, 280, 7, 220, 2);

            // 8. Giống lai: Hoa Hồng Xanh
            CreateOrUpdatePlant("hoa_hong_xanh", "Hoa Hồng Xanh", PlantType.Flower, "Giống hoa truyền thuyết lai tạo từ Hoa Hồng và Hướng Dương.", 4500f, 0.004f, SunlightLevel.Low, 150, 500, 10, 450, 4);
        }

        private static void CreateOrUpdatePlant(string id, string name, PlantType type, string desc, float growthSec, float waterRate, SunlightLevel sun, int buy, int sell, int unlockLv, int xp, int yieldCount)
        {
            string assetPath = $"{PlantsSOPath}/PlantData_{id}.asset";
            PlantData data = AssetDatabase.LoadAssetAtPath<PlantData>(assetPath);

            if (data == null)
            {
                data = ScriptableObject.CreateInstance<PlantData>();
                AssetDatabase.CreateAsset(data, assetPath);
            }

            data.plantId = id;
            data.plantName = name;
            data.plantType = type;
            data.description = desc;
            data.growthTimeSeconds = growthSec;
            data.waterConsumptionRate = waterRate;
            data.preferredSun = sun;
            data.buyPrice = buy;
            data.sellPrice = sell;
            data.unlockLevel = unlockLv;
            data.xpReward = xp;
            data.harvestYield = yieldCount;

            // Gán các Sprites
            string folder = $"{SpritesPath}/Plants/{id}";
            data.stageSeedSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{folder}/seed.png");
            data.stageSproutSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{folder}/sprout.png");
            data.stageMatureSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{folder}/mature.png");
            data.stageRipeSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{folder}/ripe.png");
            data.stageWitheredSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{folder}/withered.png");
            data.seedPacketSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{folder}/seed_packet.png");
            data.cropItemSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{folder}/crop.png");

            EditorUtility.SetDirty(data);
        }

        private static void CreateAllNeighborScriptableObjects()
        {
            // 1. Bà Lan
            CreateOrUpdateNeighbor("ba_lan", "Bà Lan",
                "Bà hàng xóm tầng dưới hiền từ, hay nấu canh rau cải và ngắm hoa cúc.",
                new[] { "cai_xanh", "cuc_vang" },
                new[] { "Cháu trồng rau mát tay quá, nhìn mướt cả mắt!", "Nhìn khóm cúc vàng của cháu bà lại nhớ quê ngày xưa...", "Mỗi lần lên ban công gặp cháu là bà thấy bình yên hẳn." });

            // 2. Anh Minh
            CreateOrUpdateNeighbor("anh_minh", "Anh Minh",
                "Sinh viên kiến trúc phòng bên, bận làm đồ án nhưng rất mê cà chua tươi.",
                new[] { "ca_chua", "cai_xanh" },
                new[] { "Cảm ơn nhé! Tối nay mình có cà chua làm mỳ Ý rồi!", "Đồ án thức đêm mà có rau củ tươi của bạn thì đỉnh chóp!", "Bạn thiết kế ban công xanh này trông có gu lắm đó!" });

            // 3. Chị Hoa
            CreateOrUpdateNeighbor("chi_hoa", "Chị Hoa",
                "Chủ một tiệm hoa nhỏ ở góc phố, luôn săn lùng những giống hoa độc lạ.",
                new[] { "cuc_vang", "huong_duong", "hoa_hong", "cuc_cam", "hoa_hong_xanh" },
                new[] { "Chị rất thích cách em chăm hoa, bông nào cũng đều và thắm.", "Em lai tạo được giống hoa mới chưa? Chị háo hức quá!", "Những bông hồng xanh của em là niềm mơ ước của tiệm chị đấy!" });

            // 4. Ông Tư
            CreateOrUpdateNeighbor("ong_tu", "Ông Tư",
                "Cán bộ hưu trí mê cây cảnh và trà đạo, rất nghiêm khắc nhưng ấm áp.",
                new[] { "hoa_hong", "huong_duong" },
                new[] { "Trồng cây rèn tính kiên nhẫn, người trẻ như cháu thế là quý.", "Gốc hoa hồng này cháu bón đất tốt đấy, nụ rất mập.", "Ban công này mát mẻ, khi nào rảnh qua uống tách trà với ông." });
        }

        private static void CreateOrUpdateNeighbor(string id, string name, string bio, string[] favPlants, string[] dialogues)
        {
            string assetPath = $"{NeighborsSOPath}/NeighborData_{id}.asset";
            NeighborData data = AssetDatabase.LoadAssetAtPath<NeighborData>(assetPath);

            if (data == null)
            {
                data = ScriptableObject.CreateInstance<NeighborData>();
                AssetDatabase.CreateAsset(data, assetPath);
            }

            data.neighborId = id;
            data.neighborName = name;
            data.bio = bio;
            data.avatarSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Neighbors/{id}.png");

            data.favoritePlantIds = new System.Collections.Generic.List<string>(favPlants);
            data.storyDialogues = new System.Collections.Generic.List<NeighborStoryDialogue>();

            for (int i = 0; i < dialogues.Length; i++)
            {
                data.storyDialogues.Add(new NeighborStoryDialogue
                {
                    requiredFriendship = i * 30,
                    dialogueText = dialogues[i]
                });
            }

            EditorUtility.SetDirty(data);
        }

        // --- Helper tạo Texture2D ---

        private static void SaveTexture(Texture2D tex, string path)
        {
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            Object.DestroyImmediate(tex);
        }

        private static Texture2D CreateColorTexture(int width, int height, Color col)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = col;
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateCircleTexture(int width, int height, Color col, string label = "")
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color transparent = new Color(0, 0, 0, 0);
            float radius = width * 0.45f;
            Vector2 center = new Vector2(width * 0.5f, height * 0.5f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= radius)
                    {
                        // Gradient nhẹ tạo chiều sâu
                        float shade = 1.0f - (dist / radius) * 0.2f;
                        tex.SetPixel(x, y, new Color(col.r * shade, col.g * shade, col.b * shade, col.a));
                    }
                    else
                    {
                        tex.SetPixel(x, y, transparent);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateRoundedBoxTexture(int width, int height, Color fill, Color border)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color transparent = new Color(0, 0, 0, 0);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isBorder = (x < 4 || x >= width - 4 || y < 4 || y >= height - 4);
                    tex.SetPixel(x, y, isBorder ? border : fill);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreatePotTexture(int width, int height, Color col)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color transparent = new Color(0, 0, 0, 0);
            Color soilCol = new Color(0.35f, 0.22f, 0.15f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Dáng chậu hình thang nhẹ
                    float tY = (float)y / height;
                    float inset = (1.0f - tY) * 20f;

                    if (x >= inset && x <= width - inset && y >= 10 && y <= height - 10)
                    {
                        if (y > height - 35)
                        {
                            tex.SetPixel(x, y, soilCol); // Đất trên miệng chậu
                        }
                        else
                        {
                            float shade = 0.85f + 0.25f * (float)x / width;
                            tex.SetPixel(x, y, new Color(col.r * shade, col.g * shade, col.b * shade, 1f));
                        }
                    }
                    else
                    {
                        tex.SetPixel(x, y, transparent);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateSproutTexture(int width, int height, Color green)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color transparent = new Color(0, 0, 0, 0);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Thân mầm ở giữa
                    bool isStem = (x >= width / 2 - 4 && x <= width / 2 + 4 && y <= height * 0.65f);
                    // 2 lá mầm
                    bool isLeafLeft = (x < width / 2 && y > height * 0.45f && y < height * 0.8f && Vector2.Distance(new Vector2(x, y), new Vector2(width * 0.35f, height * 0.6f)) < 25f);
                    bool isLeafRight = (x > width / 2 && y > height * 0.45f && y < height * 0.8f && Vector2.Distance(new Vector2(x, y), new Vector2(width * 0.65f, height * 0.6f)) < 25f);

                    if (isStem || isLeafLeft || isLeafRight)
                    {
                        tex.SetPixel(x, y, green);
                    }
                    else
                    {
                        tex.SetPixel(x, y, transparent);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateMatureTexture(int width, int height, Color foliageColor)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color transparent = new Color(0, 0, 0, 0);
            Vector2 center = new Vector2(width * 0.5f, height * 0.55f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Thân cây
                    bool isStem = (x >= width / 2 - 5 && x <= width / 2 + 5 && y <= height * 0.5f);
                    float dist = Vector2.Distance(new Vector2(x, y), center);

                    if (isStem)
                    {
                        tex.SetPixel(x, y, new Color(0.2f, 0.55f, 0.2f));
                    }
                    else if (dist < width * 0.35f)
                    {
                        float shade = 0.9f + 0.2f * Mathf.Sin(x * 0.2f);
                        tex.SetPixel(x, y, new Color(foliageColor.r * shade, foliageColor.g * shade, foliageColor.b * shade, 1f));
                    }
                    else
                    {
                        tex.SetPixel(x, y, transparent);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateBloomTexture(int width, int height, Color flowerColor)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color transparent = new Color(0, 0, 0, 0);
            Vector2 center = new Vector2(width * 0.5f, height * 0.6f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Thân và lá nền
                    bool isStem = (x >= width / 2 - 5 && x <= width / 2 + 5 && y <= height * 0.45f);
                    float distFromCenter = Vector2.Distance(new Vector2(x, y), center);

                    if (isStem)
                    {
                        tex.SetPixel(x, y, new Color(0.25f, 0.65f, 0.25f));
                    }
                    else if (distFromCenter < width * 0.38f)
                    {
                        // Cánh hoa nở to
                        if (distFromCenter < width * 0.12f)
                        {
                            tex.SetPixel(x, y, new Color(1.0f, 0.95f, 0.4f)); // Nhụy vàng sáng
                        }
                        else
                        {
                            tex.SetPixel(x, y, flowerColor);
                        }
                    }
                    else
                    {
                        tex.SetPixel(x, y, transparent);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateWitheredTexture(int width, int height, Color witherColor)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color transparent = new Color(0, 0, 0, 0);
            Vector2 center = new Vector2(width * 0.45f, height * 0.45f); // Nghiêng rũ xuống

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist < width * 0.3f && y < height * 0.7f)
                    {
                        tex.SetPixel(x, y, witherColor);
                    }
                    else
                    {
                        tex.SetPixel(x, y, transparent);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateSeedPacketTexture(int width, int height, Color col)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color border = new Color(0.4f, 0.3f, 0.2f);
            Color bg = new Color(0.98f, 0.94f, 0.85f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isBorder = (x < 5 || x >= width - 5 || y < 5 || y >= height - 5);
                    if (isBorder)
                    {
                        tex.SetPixel(x, y, border);
                    }
                    else if (x > 25 && x < width - 25 && y > 25 && y < height - 25)
                    {
                        tex.SetPixel(x, y, col); // Nhãn màu cây ở giữa
                    }
                    else
                    {
                        tex.SetPixel(x, y, bg);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateHarvestCropTexture(int width, int height, Color col)
        {
            return CreateCircleTexture(width, height, col);
        }

        private static Texture2D CreateNeighborAvatar(int width, int height, Color skinCol, string name)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color transparent = new Color(0, 0, 0, 0);
            Vector2 center = new Vector2(width * 0.5f, height * 0.5f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= width * 0.45f)
                    {
                        tex.SetPixel(x, y, skinCol);
                    }
                    else
                    {
                        tex.SetPixel(x, y, transparent);
                    }
                }
            }
            tex.Apply();
            return tex;
        }
    }
}
#endif
