﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossAssist
{
    public static class MapAssist
    {
        public static List<Vector2> whitelistPos;
        public static List<int> whitelistType;

        internal static void FullMapInitialize()
        {
            whitelistPos = new List<Vector2>();
            whitelistType = new List<int>();
        }

        public static void DrawFullscreenMap(Mod mod, ref string mouseText)
        {
            UpdateMapLocations();
            DrawIcons();
        }

        private static void UpdateMapLocations()
        {
            whitelistPos.Clear();
            whitelistType.Clear();

            for (int i = 0; i < Main.maxItems; i++)
            {
                if (!Main.item[i].active) continue;
                if (IsWhiteListItem(Main.item[i]))
                {
                    whitelistPos.Add(Main.item[i].Center);
                    whitelistType.Add(Main.item[i].type);
                }
            }
        }

        private static void DrawIcons()
        {
            Texture2D drawTexture = null;
            Vector2 drawPosition = new Vector2();
            
            foreach (Vector2 item in whitelistPos)
            {
                drawTexture = Main.itemTexture[whitelistType[whitelistPos.IndexOf(item)]];
                drawPosition = CalculateDrawPos(new Vector2(item.X / 16, item.Y / 16));
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

        public static bool IsWhiteListItem(Item item)
        {
            if (item.consumable && item.Name == "Treasure Bag" && item.expert) return true;
            if (item.rare == 9 && item.damage <= 0 && item.Name.Contains("Fragment")) return true;
            if (item.type == ItemID.ShadowScale || item.type == ItemID.TissueSample) return true;
            return false;
        }
    }
}