using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossAssist
{
    public static class MapAssist
    {
        public static List<Vector2> bossBagPos;
        public static List<int> bossBagType;

        public static List<Vector2> fragmentPos;
        public static List<int> fragmentType;

        public static List<Vector2> scalesPos;
        public static List<int> scalesType;

        internal static void FullMapInitialize()
        {
            bossBagPos = new List<Vector2>();
            bossBagType = new List<int>();

            fragmentPos = new List<Vector2>();
            fragmentType = new List<int>();

            scalesPos = new List<Vector2>();
            scalesType = new List<int>();
        }

        public static void DrawFullscreenMap(Mod mod, ref string mouseText)
        {
            UpdateMapLocations();
            DrawIcons();
        }

        private static void UpdateMapLocations()
        {
            bossBagPos.Clear();
            bossBagType.Clear();

            fragmentPos.Clear();
            fragmentType.Clear();

            scalesPos.Clear();
            scalesType.Clear();

            for (int i = 0; i < Main.maxItems; i++)
            {
                if (!Main.item[i].active) continue;
                if (Main.item[i].consumable && Main.item[i].Name == "Treasure Bag" && Main.item[i].expert)
                {
                    bossBagPos.Add(Main.item[i].Center);
                    bossBagType.Add(Main.item[i].type);
                }
                if (Main.item[i].rare == 9 && Main.item[i].damage <= 0 && Main.item[i].Name.Contains("Fragment"))
                {
                    fragmentPos.Add(Main.item[i].Center);
                    fragmentType.Add(Main.item[i].type);
                }
                if (Main.item[i].type == ItemID.ShadowScale || Main.item[i].type == ItemID.TissueSample)
                {
                    scalesPos.Add(Main.item[i].Center);
                    scalesType.Add(Main.item[i].type);
                }
            }
        }

        private static void DrawIcons()
        {
            Texture2D drawTexture = null;
            Vector2 drawPosition = new Vector2();
            
            foreach (Vector2 bossBag in bossBagPos)
            {
                drawTexture = Main.itemTexture[bossBagType[bossBagPos.IndexOf(bossBag)]];
                drawPosition = CalculateDrawPos(new Vector2(bossBag.X / 16, bossBag.Y / 16));
                DrawTextureOnMap(drawTexture, drawPosition);
            }
            foreach (Vector2 frag in fragmentPos)
            {
                drawTexture = Main.itemTexture[fragmentType[fragmentPos.IndexOf(frag)]];
                drawPosition = CalculateDrawPos(new Vector2(frag.X / 16, frag.Y / 16));
                DrawTextureOnMap(drawTexture, drawPosition);
            }
            foreach (Vector2 loot in scalesPos)
            {
                drawTexture = Main.itemTexture[scalesType[scalesPos.IndexOf(loot)]];
                drawPosition = CalculateDrawPos(new Vector2(loot.X / 16, loot.Y / 16));
                DrawTextureOnMap(drawTexture, drawPosition);
            }
        }

        private static Vector2 CalculateDrawPos(Vector2 tilePos)
        {
            Vector2 halfScreen = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
            Vector2 relativePos = tilePos - Main.mapFullscreenPos;
            relativePos *= Main.mapFullscreenScale / 16;
            relativePos = relativePos * 16 + halfScreen;

            Vector2 drawPosition = new Vector2((int)relativePos.X, (int)relativePos.Y);
            return drawPosition;
        }

        private static void DrawTextureOnMap(Texture2D texture, Vector2 drawPosition)
        {
            Rectangle drawPos = new Rectangle((int)drawPosition.X, (int)drawPosition.Y, texture.Width, texture.Height);
            Vector2 originLoc = new Vector2(texture.Width / 2, texture.Height / 2);
            Main.spriteBatch.Draw(texture, drawPos, null, Color.White, 0f, originLoc, SpriteEffects.None, 0f);
        }
    }
}