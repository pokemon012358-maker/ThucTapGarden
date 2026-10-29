#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BalconyGarden.Editor
{
    public static class SproutLandsTextureRefiner
    {
        private const string SpritesPath = "Assets/_BalconyGarden/Sprites";

        public static void GenerateCozyPixelTextures()
        {
            string envPath = $"{SpritesPath}/Environment";
            string uiPath = $"{SpritesPath}/UI";
            string toolsPath = $"{SpritesPath}/Tools";

            if (!Directory.Exists(envPath)) Directory.CreateDirectory(envPath);
            if (!Directory.Exists(uiPath)) Directory.CreateDirectory(uiPath);
            if (!Directory.Exists(toolsPath)) Directory.CreateDirectory(toolsPath);

            // 1. Hình nền Ban Công Ấm Cúng (1080 x 1920)
            SaveTexture(CreateCozyBalconyBackground(1080, 1920), $"{envPath}/balcony_bg.png");

            // 2. Chậu Cây Đất Nung Xinh Xắn (Terracotta Pot với Đất Màu Mỡ)
            SaveTexture(CreateTerracottaPot(240, 200), $"{envPath}/pot_empty.png");

            // 3. Khung Gỗ 9-Slice Sạch Sẽ (KHÔNG CÓ CHỮ "SETTINGS")
            SaveTexture(CreateWoodFrame9Slice(64, 64), $"{uiPath}/wood_panel_9slice.png");

            // 4. Khung Giấy Da 9-Slice (Parchment Paper Panel)
            SaveTexture(CreateParchmentFrame9Slice(64, 64), $"{uiPath}/parchment_panel_9slice.png");

            // 5. Ô Slot Hộp Đồ Gỗ 9-Slice (Cho khay công cụ & hạt giống)
            SaveTexture(CreateSlotBox9Slice(48, 48), $"{uiPath}/slot_box_9slice.png");

            // 6. Nút Bấm Gỗ 9-Slice (Wood Button Normal & Pressed)
            SaveTexture(CreateWoodButton9Slice(48, 48, false), $"{uiPath}/wood_button_normal.png");
            SaveTexture(CreateWoodButton9Slice(48, 48, true), $"{uiPath}/wood_button_pressed.png");

            // 7. Các Icon Pixel Rõ Nét
            SaveTexture(CreatePixelCoin(64, 64), $"{uiPath}/coin.png");
            SaveTexture(CreatePixelGem(64, 64), $"{uiPath}/gem.png");
            SaveTexture(CreatePixelWaterDrop(64, 64), $"{uiPath}/water_drop.png");
            SaveTexture(CreatePixelSun(64, 64), $"{uiPath}/sun.png");
            SaveTexture(CreatePixelLock(64, 64), $"{uiPath}/lock.png");
            SaveTexture(CreatePixelWateringCan(96, 96), $"{toolsPath}/watering_can.png");
            SaveTexture(CreatePixelShovel(96, 96), $"{toolsPath}/shovel.png");
            SaveTexture(CreatePixelSickle(96, 96), $"{toolsPath}/sickle.png");

            AssetDatabase.Refresh();

            // Cấu hình 9-slice borders và FilterMode.Point
            ConfigureTexture($"{uiPath}/wood_panel_9slice.png", new Vector4(16, 16, 16, 16));
            ConfigureTexture($"{uiPath}/parchment_panel_9slice.png", new Vector4(12, 12, 12, 12));
            ConfigureTexture($"{uiPath}/slot_box_9slice.png", new Vector4(10, 10, 10, 10));
            ConfigureTexture($"{uiPath}/wood_button_normal.png", new Vector4(8, 8, 8, 8));
            ConfigureTexture($"{uiPath}/wood_button_pressed.png", new Vector4(8, 8, 8, 8));

            ConfigureTexture($"{envPath}/balcony_bg.png", Vector4.zero);
            ConfigureTexture($"{envPath}/pot_empty.png", Vector4.zero);

            AssetDatabase.Refresh();
        }

        private static void ConfigureTexture(string path, Vector4 border)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.filterMode = FilterMode.Point; // Pixel art sắc nét
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.spriteBorder = border;
                importer.SaveAndReimport();
            }
        }

        private static void SaveTexture(Texture2D tex, string path)
        {
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            Object.DestroyImmediate(tex);
        }

        // --- HÀM TẠO BACKGROUND BAN CÔNG COZY ---
        private static Texture2D CreateCozyBalconyBackground(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color skyTop = new Color(0.72f, 0.88f, 0.95f);     // Trời xanh trong lành
            Color skyHorizon = new Color(0.92f, 0.96f, 0.94f); // Chân trời dịu
            Color floorWood = new Color(0.76f, 0.58f, 0.42f);  // Sàn gỗ ban công
            Color floorDark = new Color(0.55f, 0.38f, 0.25f);  // Vân gỗ

            for (int y = 0; y < h; y++)
            {
                float t = (float)y / h;
                for (int x = 0; x < w; x++)
                {
                    if (y > h * 0.22f)
                    {
                        // Bầu trời pastel chuyển màu mượt mà
                        float skyT = (y - h * 0.22f) / (h * 0.78f);
                        Color c = Color.Lerp(skyHorizon, skyTop, skyT);

                        // Thêm vài vệt mây trắng mềm mại ở xa
                        if (y > h * 0.75f && y < h * 0.88f)
                        {
                            float cloud = Mathf.Sin(x * 0.015f) * Mathf.Cos(y * 0.02f);
                            if (cloud > 0.6f) c = Color.Lerp(c, Color.white, (cloud - 0.6f) * 1.5f);
                        }
                        tex.SetPixel(x, y, c);
                    }
                    else
                    {
                        // Sàn gỗ ban công ở dưới đáy
                        bool isPlankBorder = (y % 60 < 3) || (x % 180 < 3);
                        tex.SetPixel(x, y, isPlankBorder ? floorDark : floorWood);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        // --- HÀM TẠO CHẬU CÂY ĐẤT NUNG XINH XẮN ---
        private static Texture2D CreateTerracottaPot(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color transparent = new Color(0, 0, 0, 0);
            Color potBorder = new Color(0.48f, 0.22f, 0.12f);  // Viền nâu sẫm
            Color potBody = new Color(0.85f, 0.48f, 0.28f);    // Thân chậu gốm nung
            Color potHighlight = new Color(0.95f, 0.62f, 0.38f);// Ánh sáng phản chiếu
            Color soil = new Color(0.32f, 0.20f, 0.12f);       // Đất màu mỡ
            Color soilLight = new Color(0.42f, 0.28f, 0.18f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float tY = (float)y / h;
                    float inset = (1.0f - tY) * 26f; // Vát hình thang miệng rộng đáy hẹp

                    if (y >= 10 && y <= h - 15 && x >= 20 + inset && x <= w - 20 - inset)
                    {
                        bool isEdge = (x <= 24 + inset || x >= w - 24 - inset || y <= 14 || y >= h - 19);

                        if (isEdge)
                        {
                            tex.SetPixel(x, y, potBorder);
                        }
                        else if (y >= h - 45)
                        {
                            // Lớp đất tơi xốp trên miệng chậu
                            bool soilDot = ((x + y) % 6 == 0);
                            tex.SetPixel(x, y, soilDot ? soilLight : soil);
                        }
                        else
                        {
                            // Thân gốm có gradient ánh sáng
                            float light = (float)x / w;
                            Color c = (light < 0.45f) ? Color.Lerp(potBody, potHighlight, light * 2f) : potBody;
                            tex.SetPixel(x, y, c);
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

        // --- HÀM TẠO KHUNG GỖ 9-SLICE (SẠCH, KHÔNG CHỮ) ---
        private static Texture2D CreateWoodFrame9Slice(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color darkWood = new Color(0.38f, 0.22f, 0.12f);
            Color midWood = new Color(0.65f, 0.45f, 0.28f);
            Color lightWood = new Color(0.78f, 0.58f, 0.38f);
            Color innerCream = new Color(0.96f, 0.94f, 0.88f); // Nền giấy da kem sáng ở giữa

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int borderDist = Mathf.Min(Mathf.Min(x, w - 1 - x), Mathf.Min(y, h - 1 - y));

                    if (borderDist < 3)
                    {
                        tex.SetPixel(x, y, darkWood); // Viền ngoài cùng
                    }
                    else if (borderDist < 8)
                    {
                        tex.SetPixel(x, y, (x + y) % 8 == 0 ? lightWood : midWood); // Gờ gỗ
                    }
                    else if (borderDist < 11)
                    {
                        tex.SetPixel(x, y, darkWood); // Rãnh lõm
                    }
                    else
                    {
                        tex.SetPixel(x, y, innerCream); // Lòng giấy da kem sáng
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        // --- HÀM TẠO KHUNG GIẤY DA 9-SLICE ---
        private static Texture2D CreateParchmentFrame9Slice(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color borderDark = new Color(0.55f, 0.40f, 0.26f);
            Color paperBg = new Color(0.98f, 0.96f, 0.91f);
            Color paperShade = new Color(0.93f, 0.90f, 0.82f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int borderDist = Mathf.Min(Mathf.Min(x, w - 1 - x), Mathf.Min(y, h - 1 - y));
                    if (borderDist < 3)
                    {
                        tex.SetPixel(x, y, borderDark);
                    }
                    else if (borderDist < 6)
                    {
                        tex.SetPixel(x, y, paperShade);
                    }
                    else
                    {
                        tex.SetPixel(x, y, paperBg);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        // --- HÀM TẠO Ô SLOT HỘP ĐỒ GỖ 9-SLICE ---
        private static Texture2D CreateSlotBox9Slice(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color darkBorder = new Color(0.35f, 0.22f, 0.12f);
            Color beveled = new Color(0.75f, 0.58f, 0.42f);
            Color slotInner = new Color(0.88f, 0.84f, 0.76f); // Đáy ô lõm

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int borderDist = Mathf.Min(Mathf.Min(x, w - 1 - x), Mathf.Min(y, h - 1 - y));
                    if (borderDist < 2)
                    {
                        tex.SetPixel(x, y, darkBorder);
                    }
                    else if (borderDist < 6)
                    {
                        // Hiệu ứng bóng lõm (trên tối, dưới sáng)
                        tex.SetPixel(x, y, (y > h / 2) ? darkBorder : beveled);
                    }
                    else
                    {
                        tex.SetPixel(x, y, slotInner);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        // --- HÀM TẠO NÚT BẤM GỖ 9-SLICE ---
        private static Texture2D CreateWoodButton9Slice(int w, int h, bool pressed)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color dark = new Color(0.32f, 0.18f, 0.08f);
            Color light = new Color(0.88f, 0.68f, 0.45f);
            Color face = new Color(0.78f, 0.56f, 0.36f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int borderDist = Mathf.Min(Mathf.Min(x, w - 1 - x), Mathf.Min(y, h - 1 - y));
                    if (borderDist < 2)
                    {
                        tex.SetPixel(x, y, dark);
                    }
                    else if (borderDist < 5)
                    {
                        // Nút nổi: trên sáng, dưới tối. Nút ấn: đảo ngược
                        bool isHighlight = pressed ? (y < h / 2) : (y > h / 2);
                        tex.SetPixel(x, y, isHighlight ? light : dark);
                    }
                    else
                    {
                        tex.SetPixel(x, y, face);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        // --- HÀM TẠO CÁC ICON PIXEL GỌN GÀNG ---
        private static Texture2D CreatePixelCoin(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color transparent = new Color(0, 0, 0, 0);
            Color border = new Color(0.55f, 0.38f, 0.05f);
            Color gold = new Color(1.0f, 0.82f, 0.15f);
            Color lightGold = new Color(1.0f, 0.95f, 0.45f);
            Vector2 c = new Vector2(w * 0.5f, h * 0.5f);
            float r = w * 0.42f;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), c);
                    if (d <= r)
                    {
                        if (d >= r - 3) tex.SetPixel(x, y, border);
                        else if (x < w * 0.45f && y > h * 0.45f) tex.SetPixel(x, y, lightGold);
                        else tex.SetPixel(x, y, gold);
                    }
                    else tex.SetPixel(x, y, transparent);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreatePixelGem(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color transparent = new Color(0, 0, 0, 0);
            Color border = new Color(0.08f, 0.35f, 0.55f);
            Color cyan = new Color(0.25f, 0.82f, 0.95f);
            Color lightCyan = new Color(0.75f, 0.95f, 1.0f);
            Vector2 c = new Vector2(w * 0.5f, h * 0.5f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // Hình thoi (Diamond gem)
                    float d = Mathf.Abs(x - c.x) + Mathf.Abs(y - c.y);
                    if (d <= w * 0.42f)
                    {
                        if (d >= w * 0.42f - 3) tex.SetPixel(x, y, border);
                        else if (y > c.y && x < c.x) tex.SetPixel(x, y, lightCyan);
                        else tex.SetPixel(x, y, cyan);
                    }
                    else tex.SetPixel(x, y, transparent);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreatePixelWaterDrop(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color transparent = new Color(0, 0, 0, 0);
            Color border = new Color(0.08f, 0.32f, 0.65f);
            Color blue = new Color(0.25f, 0.65f, 0.98f);
            Color lightBlue = new Color(0.75f, 0.90f, 1.0f);
            Vector2 c = new Vector2(w * 0.5f, h * 0.42f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), c);
                    bool inCircle = (d <= w * 0.35f && y <= c.y);
                    bool inTip = (y > c.y && Mathf.Abs(x - c.x) <= (1.0f - (y - c.y) / (h * 0.45f)) * (w * 0.35f));

                    if (inCircle || inTip)
                    {
                        bool isEdge = (d >= w * 0.35f - 3 && inCircle) || (y > h * 0.82f);
                        if (isEdge) tex.SetPixel(x, y, border);
                        else if (x < c.x && y > c.y * 0.8f) tex.SetPixel(x, y, lightBlue);
                        else tex.SetPixel(x, y, blue);
                    }
                    else tex.SetPixel(x, y, transparent);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreatePixelSun(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color transparent = new Color(0, 0, 0, 0);
            Color border = new Color(0.75f, 0.38f, 0.05f);
            Color orange = new Color(1.0f, 0.65f, 0.12f);
            Vector2 c = new Vector2(w * 0.5f, h * 0.5f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), c);
                    if (d <= w * 0.35f)
                    {
                        tex.SetPixel(x, y, d >= w * 0.35f - 2 ? border : orange);
                    }
                    else tex.SetPixel(x, y, transparent);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreatePixelLock(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color transparent = new Color(0, 0, 0, 0);
            Color darkWood = new Color(0.35f, 0.22f, 0.12f);
            Color brass = new Color(0.92f, 0.75f, 0.25f);
            Color silver = new Color(0.75f, 0.78f, 0.82f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // Quai khóa hình bán nguyệt ở trên
                    bool isShackle = (y >= h * 0.45f && y <= h * 0.85f && Mathf.Abs(x - w / 2) >= w * 0.15f && Mathf.Abs(x - w / 2) <= w * 0.32f);
                    // Thân khóa hình hộp ở dưới
                    bool isBody = (y >= h * 0.12f && y <= h * 0.55f && x >= w * 0.15f && x <= w * 0.85f);

                    if (isShackle) tex.SetPixel(x, y, silver);
                    else if (isBody)
                    {
                        bool isEdge = (x <= w * 0.18f || x >= w * 0.82f || y <= h * 0.15f || y >= h * 0.52f);
                        tex.SetPixel(x, y, isEdge ? darkWood : brass);
                    }
                    else tex.SetPixel(x, y, transparent);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreatePixelWateringCan(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color transparent = new Color(0, 0, 0, 0);
            Color canBody = new Color(0.28f, 0.65f, 0.88f);
            Color canDark = new Color(0.15f, 0.40f, 0.65f);
            Color handle = new Color(0.45f, 0.48f, 0.52f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // Thân bình tưới hình trụ
                    bool isBody = (x >= w * 0.25f && x <= w * 0.75f && y >= h * 0.2f && y <= h * 0.68f);
                    // Vòi tưới bên phải nghiêng
                    bool isSpout = (x > w * 0.72f && x <= w * 0.92f && y >= h * 0.45f && y <= h * 0.75f && (x - y) > 5 && (x - y) < 25);
                    // Tay cầm bên trái
                    bool isHandle = (x >= w * 0.08f && x <= w * 0.28f && y >= h * 0.3f && y <= h * 0.65f && (x < w * 0.15f || y > h * 0.6f || y < h * 0.35f));

                    if (isBody || isSpout)
                    {
                        bool isEdge = (x == (int)(w * 0.25f) || x == (int)(w * 0.75f) || y == (int)(h * 0.2f) || y == (int)(h * 0.68f));
                        tex.SetPixel(x, y, isEdge ? canDark : canBody);
                    }
                    else if (isHandle)
                    {
                        tex.SetPixel(x, y, handle);
                    }
                    else tex.SetPixel(x, y, transparent);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreatePixelShovel(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color transparent = new Color(0, 0, 0, 0);
            Color blade = new Color(0.68f, 0.72f, 0.76f);
            Color bladeDark = new Color(0.38f, 0.42f, 0.46f);
            Color shaft = new Color(0.65f, 0.42f, 0.25f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // Cán xẻng nghiêng chéo
                    bool isShaft = (Mathf.Abs((x - y)) < 5 && y > h * 0.35f && y < h * 0.88f);
                    // Lưỡi xẻng ở góc dưới
                    bool isBlade = (x >= w * 0.15f && x <= w * 0.5f && y >= h * 0.1f && y <= h * 0.42f && (x + y) < w * 0.7f);

                    if (isBlade)
                    {
                        bool isEdge = (x <= w * 0.18f || y <= h * 0.13f);
                        tex.SetPixel(x, y, isEdge ? bladeDark : blade);
                    }
                    else if (isShaft)
                    {
                        tex.SetPixel(x, y, shaft);
                    }
                    else tex.SetPixel(x, y, transparent);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreatePixelSickle(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color transparent = new Color(0, 0, 0, 0);
            Color goldBlade = new Color(0.95f, 0.82f, 0.25f);
            Color goldDark = new Color(0.65f, 0.45f, 0.15f);
            Color handle = new Color(0.45f, 0.28f, 0.18f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Vector2 c = new Vector2(w * 0.45f, h * 0.55f);
                    float d = Vector2.Distance(new Vector2(x, y), c);
                    // Lưỡi liềm hình vòng cung
                    bool isSickle = (d >= w * 0.25f && d <= w * 0.42f && x >= c.x - 10 && y >= c.y - 15);
                    // Cán gỗ
                    bool isHandle = (x >= w * 0.28f && x <= w * 0.42f && y >= h * 0.12f && y <= h * 0.45f);

                    if (isSickle)
                    {
                        tex.SetPixel(x, y, d >= w * 0.39f ? goldDark : goldBlade);
                    }
                    else if (isHandle)
                    {
                        tex.SetPixel(x, y, handle);
                    }
                    else tex.SetPixel(x, y, transparent);
                }
            }
            tex.Apply();
            return tex;
        }
    }
}
#endif
