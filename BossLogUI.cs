using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace BossAssist
{
    // "Open UI" buttons
    internal class BossAssistButton : UIImageButton
    {
        internal string buttonType;

        public BossAssistButton(Texture2D texture, string type) : base(texture)
        {
            buttonType = type;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle innerDimensions = GetInnerDimensions();
            Vector2 stringAdjust = Main.fontMouseText.MeasureString(buttonType);
            Vector2 pos = new Vector2(innerDimensions.X - (stringAdjust.X / 3), innerDimensions.Y - 24);
            base.DrawSelf(spriteBatch);
            if (IsMouseHovering) DynamicSpriteFontExtensionMethods.DrawString(spriteBatch, Main.fontMouseText, buttonType, pos, Color.White);
            if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) Main.player[Main.myPlayer].mouseInterface = true;
        }
    }

    internal class LogItemSlot : UIElement
    {
        internal string hoverText;
        internal Item item;
        private readonly int context;
        private readonly float scale;

        public LogItemSlot(Item item, string hoverText = "", int context = ItemSlot.Context.GuideItem, float scale = 1f)
        {
            this.context = context;
            this.scale = scale;
            this.item = item;
            this.hoverText = hoverText;

            Width.Set(Main.inventoryBack9Texture.Width * scale, 0f);
            Height.Set(Main.inventoryBack9Texture.Height * scale, 0f);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            float oldScale = Main.inventoryScale;
            Main.inventoryScale = scale;
            Rectangle rectangle = GetDimensions().ToRectangle();
            var backup = Main.inventoryBack6Texture;
            
            if (Main.expertMode) Main.inventoryBack6Texture = Main.inventoryBack14Texture;
            else Main.inventoryBack6Texture = BossAssist.instance.GetTexture("Resources/ExpertOnly");
            
            ItemSlot.Draw(spriteBatch, ref item, context, rectangle.TopLeft());
            
            Texture2D checkMark = ModLoader.GetTexture("BossAssist/Resources/Checkbox_Check");
            if (Main.LocalPlayer.GetModPlayer<PlayerAssist>().BossTrophies[BossLogUI.PageNum].itemList.FindIndex(x => x.type == item.type) != -1)
            {
                if (Id.Contains("collect") && Main.LocalPlayer.GetModPlayer<PlayerAssist>().BossTrophies[BossLogUI.PageNum].checkList[Main.LocalPlayer.GetModPlayer<PlayerAssist>().BossTrophies[BossLogUI.PageNum].itemList.FindIndex(x => x.type == item.type)])
                    spriteBatch.Draw(checkMark, new Rectangle(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2, checkMark.Width, checkMark.Height), new Color(255, 255, 255));
            }

            Main.inventoryBack6Texture = backup;
            if (IsMouseHovering)
            {
                if (hoverText != "By Hand")
                {
                    if (item.type != 0 || hoverText != "")
                    {
                        Color newcolor = ItemRarity.GetColor(item.rare);
                        float num3 = (float)(int)Main.mouseTextColor / 255f;
                        if (item.expert || item.expertOnly)
                        {
                            newcolor = new Color((byte)(Main.DiscoR * num3), (byte)(Main.DiscoG * num3), (byte)(Main.DiscoB * num3), Main.mouseTextColor);
                        }
                        Main.HoverItem = item;
                        Main.hoverItemName = "[c/" + newcolor.Hex3() + ":" + hoverText + "]";
                    }
                }
                else Main.hoverItemName = hoverText;
            }
            Main.inventoryScale = oldScale;
        }
    }

    internal class LootRow : UIElement
    {
        // Had to put the itemslots in a row in order to be put in a UIList with scroll functionality
        int order;

        public LootRow(int order)
        {
            this.order = order;
            Height.Pixels = 50;
            Width.Pixels = 800;
            Top.Pixels = 145;
        }

        public override int CompareTo(object obj)
        {
            LootRow other = obj as LootRow;
            return order.CompareTo(other.order);
        }
    }

    internal class BossLogPanel : UIElement
    {
        public static bool visible = false;
        public static int timerTrophy = 480;
        
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!visible) return;
            base.Draw(spriteBatch);
            if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface)
            {
                Main.player[Main.myPlayer].mouseInterface = true;
                Main.LocalPlayer.showItemIcon = false;
                Main.ItemIconCacheUpdate(0);
            }

            Rectangle pageRect = GetInnerDimensions().ToRectangle();

            if (Id == "PageOne" && BossLogUI.PageNum != -1)
            {
                BossInfo BossPage = BossAssist.instance.setup.SortedBosses[BossLogUI.PageNum];

                int bossType = BossPage.id;
                Texture2D temp = BossPage.pageTexture;

                Rectangle posRect = new Rectangle(pageRect.X + (pageRect.Width / 2) - (temp.Width / 2), pageRect.Y + (pageRect.Height / 2) - (temp.Height / 2), temp.Width, temp.Height);
                Rectangle cutRect = new Rectangle(0, 0, temp.Width, temp.Height);
                spriteBatch.Draw(temp, posRect, cutRect, new Color(255, 255, 255));
            }

            if (Id == "PageTwo" && BossLogUI.PageNum != -1 && BossLogUI.SubPageNum == 0)
            {
                // Boss Records Subpage
                Texture2D achievements = ModLoader.GetTexture("Terraria/UI/Achievements");
                
                int achX = 0;
                int achY = 0;

                for (int i = 0; i < 4; i++)
                {
                    if (i == 0)
                    {
                        achX = 4;
                        achY = 10;
                    }
                    else if (i == 1)
                    {
                        achX = 4;
                        achY = 8;
                    }
                    else if (i == 2)
                    {
                        achX = 4;
                        achY = 9;
                    }
                    else if (i == 3)
                    {
                        achX = 3;
                        achY = 0;
                    }
                    Rectangle posRect = new Rectangle(pageRect.X, pageRect.Y + 100 + (75 * i), 64, 64);
                    Rectangle cutRect = new Rectangle(66 * achX, 66 * achY, 64, 64);
                    spriteBatch.Draw(achievements, posRect, cutRect, new Color(255, 255, 255));
                }
            }

            if (Id == "PageTwo" && BossLogUI.PageNum != -1 && BossLogUI.SubPageNum == 1)
            {
                // Spawn Item Subpage
            }

            if (Id == "PageTwo" && BossLogUI.PageNum != -1 && BossLogUI.SubPageNum == 2)
            {
                // Loot Table Subpage
                Main.instance.LoadTiles(237);
                Texture2D bag = ModLoader.GetTexture("BossAssist/Resources/treasureBag");
                for (int i = 0; i < BossAssist.instance.setup.SortedBosses[BossLogUI.PageNum].loot.Count; i++)
                {
                    Item bagItem = new Item();
                    bagItem.SetDefaults(BossAssist.instance.setup.SortedBosses[BossLogUI.PageNum].loot[i]);
                    if (bagItem.expert && bagItem.Name.Contains("Treasure Bag"))
                    {
                        if (bagItem.type < ItemID.Count)
                        {
                            bag = ModLoader.GetTexture("Terraria/Item_" + bagItem.type);
                        }
                        else
                        {
                            bag = ModLoader.GetTexture(ItemLoader.GetItem(bagItem.type).Texture);
                            break;
                        }
                    }
                }

                for (int i = 0; i < 7; i++)
                {
                    Rectangle posRect = new Rectangle(pageRect.X + (pageRect.Width / 2) - 20 - (bag.Width / 2), pageRect.Y + 88, bag.Width, bag.Height);
                    spriteBatch.Draw(bag, posRect, new Color(255, 255, 255));
                }
            }

            if (Id == "PageTwo" && BossLogUI.PageNum != -1 && BossLogUI.SubPageNum == 3)
            {
                // Collectibles Subpage
                BossInfo BossPage = BossAssist.instance.setup.SortedBosses[BossLogUI.PageNum];
                BossCollection Collections = Main.LocalPlayer.GetModPlayer<PlayerAssist>().BossTrophies[BossLogUI.PageNum];

                Texture2D template = ModLoader.GetTexture("BossAssist/Resources/CollectionTemplate");
                if (Collections.itemList.FindIndex(x => x.Name.Contains("Music Box") && x.createTile > 0) != -1)
                {
                    if (Collections.checkList[Collections.itemList.FindIndex(x => x.Name.Contains("Music Box") && x.createTile > 0)])
                    {
                        template = ModLoader.GetTexture("BossAssist/Resources/CollectionTemplate_NoMusicBox");
                    }
                }
                
                spriteBatch.Draw(template, new Rectangle(pageRect.X + (pageRect.Width / 2) - (template.Width / 2) - 24, pageRect.Y + 84, template.Width, template.Height), new Color(255, 255, 255));
                
                if (BossPage.id != NPCID.Retinazer) // (We have a special case for the Twins)
                {
                    // Draw Mask
                    if (Collections.itemList.FindIndex(x => x.Name.Contains("Mask") && x.vanity) != -1)
                    {
                        if (Collections.checkList[Collections.itemList.FindIndex(x => x.Name.Contains("Mask") && x.vanity)])
                        {
                            Texture2D mask;
                            if (BossPage.collection[0] < ItemID.Count)
                            {
                                Item newItem = new Item();
                                newItem.SetDefaults(BossPage.collection[1]);
                                mask = ModLoader.GetTexture("Terraria/Armor_Head_" + newItem.headSlot);
                            }
                            else mask = ModLoader.GetTexture(ItemLoader.GetItem(BossPage.collection[0]).Texture + "_Head");

                            int frameCut = mask.Height / 24;
                            Rectangle posRect = new Rectangle(pageRect.X + (pageRect.Width / 2) - (mask.Width / 2) - 12, pageRect.Y + (pageRect.Height / 2) - (frameCut / 2) - 86, mask.Width, frameCut);
                            Rectangle cutRect = new Rectangle(0, 0, mask.Width, frameCut);
                            spriteBatch.Draw(mask, posRect, cutRect, new Color(255, 255, 255));
                        }
                    }

                    // Draw Trophy
                    if (Collections.itemList.FindIndex(x => x.Name.Contains("Trophy") && x.createTile > 0) != -1)
                    {
                        if (Collections.checkList[Collections.itemList.FindIndex(x => x.Name.Contains("Trophy") && x.createTile > 0)])
                        {
                            int offsetX = 0;
                            int offsetY = 0;
                            Main.instance.LoadTiles(240);
                            Texture2D trophy = Main.tileTexture[240];

                            if (BossPage.collection[0] < ItemID.Count)
                            {
                                if (BossPage.id == NPCID.EyeofCthulhu)
                                {
                                    offsetX = 0; // Position on tile table, times 3
                                    offsetY = 0;
                                }
                                else if (BossPage.id == NPCID.EaterofWorldsHead)
                                {
                                    offsetX = 3;
                                    offsetY = 0;
                                }
                                else if (BossPage.id == NPCID.BrainofCthulhu)
                                {
                                    offsetX = 6;
                                    offsetY = 0;
                                }
                                else if (BossPage.id == NPCID.SkeletronHead)
                                {
                                    offsetX = 9;
                                    offsetY = 0;
                                }
                                else if (BossPage.id == NPCID.QueenBee)
                                {
                                    offsetX = 12;
                                    offsetY = 0;
                                }
                                else if (BossPage.id == NPCID.WallofFlesh)
                                {
                                    offsetX = 15;
                                    offsetY = 0;
                                }
                                else if (BossPage.id == NPCID.TheDestroyer)
                                {
                                    offsetX = 18;
                                    offsetY = 0;
                                }
                                else if (BossPage.id == NPCID.SkeletronPrime)
                                {
                                    offsetX = 21;
                                    offsetY = 0;
                                }
                                else if (BossPage.id == NPCID.Plantera)
                                {
                                    offsetX = 30;
                                    offsetY = 0;
                                }
                                else if (BossPage.id == NPCID.Golem)
                                {
                                    offsetX = 33;
                                    offsetY = 0;
                                }
                                else if (BossPage.id == NPCID.KingSlime)
                                {
                                    offsetX = 54;
                                    offsetY = 3;
                                }
                                else if (BossPage.id == NPCID.DukeFishron)
                                {
                                    offsetX = 57;
                                    offsetY = 3;
                                }
                                else if (BossPage.id == NPCID.CultistBoss)
                                {
                                    offsetX = 60;
                                    offsetY = 3;
                                }
                                else if (BossPage.id == NPCID.MoonLordHead)
                                {
                                    offsetX = 69;
                                    offsetY = 3;
                                }
                                else if (BossPage.id == NPCID.DD2Betsy)
                                {
                                    offsetX = 75;
                                    offsetY = 3;
                                }

                                int backupX = offsetX;
                                int backupY = offsetY;

                                for (int i = 0; i < 9; i++)
                                {
                                    Rectangle posRect = new Rectangle(pageRect.X + 94 + (offsetX * 16) - (backupX * 16), pageRect.Y + 126 + (offsetY * 16) - (backupY * 16), 18, 18);
                                    Rectangle cutRect = new Rectangle(offsetX * 18, offsetY * 18, 18, 18);

                                    spriteBatch.Draw(trophy, posRect, cutRect, new Color(255, 255, 255));

                                    offsetX++;
                                    if (i == 2 || i == 5)
                                    {
                                        offsetX = backupX;
                                        offsetY++;
                                    }
                                }
                            }
                            else
                            {
                                Main.instance.LoadTiles(ItemLoader.GetItem(BossPage.collection[1]).item.createTile);
                                trophy = Main.tileTexture[ItemLoader.GetItem(BossPage.collection[1]).item.createTile];

                                offsetX = 0;
                                offsetY = 0;

                                for (int i = 0; i < 9; i++)
                                {
                                    Rectangle posRect = new Rectangle(pageRect.X + 94 + (offsetX * 16), pageRect.Y + 126 + (offsetY * 16), 18, 18);
                                    Rectangle cutRect = new Rectangle(offsetX * 18, offsetY * 18, 18, 18);

                                    spriteBatch.Draw(trophy, posRect, cutRect, new Color(255, 255, 255));

                                    offsetX++;
                                    if (i == 2 || i == 5)
                                    {
                                        offsetX = 0;
                                        offsetY++;
                                    }
                                }
                            }
                        }
                    }

                    // Draw Music Box
                    if (Collections.itemList.FindIndex(x => x.Name.Contains("Music Box") && x.createTile > 0) != -1)
                    {
                        if (Collections.checkList[Collections.itemList.FindIndex(x => x.Name.Contains("Music Box") && x.createTile > 0)])
                        {
                            int offsetX = 0;
                            int offsetY = 0;
                            Main.instance.LoadTiles(139);
                            Texture2D musicBox = Main.tileTexture[139];

                            if (BossPage.collection[2] < ItemID.Count)
                            {
                                if (BossPage.collection[2] == ItemID.MusicBoxBoss1)
                                {
                                    if (Main.music[MusicID.Boss1].IsPlaying) offsetX = 2;
                                    offsetY = 10;
                                }
                                else if (BossPage.collection.Any(x => x == ItemID.MusicBoxBoss2))
                                {
                                    if (Main.music[MusicID.Boss2].IsPlaying) offsetX = 2;
                                    offsetY = 20;
                                }
                                else if (BossPage.collection[2] == ItemID.MusicBoxBoss3)
                                {
                                    if (Main.music[MusicID.Boss3].IsPlaying) offsetX = 2;
                                    offsetY = 24;
                                }
                                else if (BossPage.collection[2] == ItemID.MusicBoxBoss4)
                                {
                                    if (Main.music[MusicID.Boss4].IsPlaying) offsetX = 2;
                                    offsetY = 32;
                                }
                                else if (BossPage.collection[2] == ItemID.MusicBoxBoss5)
                                {
                                    if (Main.music[MusicID.Boss5].IsPlaying) offsetX = 2;
                                    offsetY = 48;
                                }
                                else if (BossPage.collection[2] == ItemID.MusicBoxPlantera)
                                {
                                    if (Main.music[MusicID.Plantera].IsPlaying) offsetX = 2;
                                    offsetY = 46;
                                }
                                else if (BossPage.collection[2] == ItemID.MusicBoxDD2)
                                {
                                    if (Main.music[MusicID.OldOnesArmy].IsPlaying) offsetX = 2;
                                    offsetY = 78;
                                }
                                else if (BossPage.collection[2] == ItemID.MusicBoxLunarBoss)
                                {
                                    if (Main.music[MusicID.LunarBoss].IsPlaying) offsetX = 2;
                                    offsetY = 64;
                                }

                                int backupX = offsetX;
                                int backupY = offsetY;

                                for (int i = 0; i < 4; i++)
                                {
                                    Rectangle posRect = new Rectangle(pageRect.X + 206 + (offsetX * 16) - (backupX * 16), pageRect.Y + 158 + (offsetY * 16) - (backupY * 16), 18, 18);
                                    Rectangle cutRect = new Rectangle(offsetX * 18, (offsetY * 18) - 2, 18, 18);

                                    spriteBatch.Draw(musicBox, posRect, cutRect, new Color(255, 255, 255));

                                    offsetX++;
                                    if (i == 1)
                                    {
                                        offsetX = backupX;
                                        offsetY++;
                                    }
                                }
                            }
                            else
                            {
                                Main.instance.LoadTiles(ItemLoader.GetItem(BossPage.collection[2]).item.createTile);
                                musicBox = Main.tileTexture[ItemLoader.GetItem(BossPage.collection[2]).item.createTile];

                                for (int i = 0; i < 4; i++)
                                {
                                    Rectangle posRect = new Rectangle(pageRect.X + 206 + (offsetX * 16), pageRect.Y + 158 + (offsetY * 16), 18, 18);
                                    Rectangle cutRect = new Rectangle(offsetX * 18, (offsetY * 18) - 2, 18, 18);

                                    spriteBatch.Draw(musicBox, posRect, cutRect, new Color(255, 255, 255));

                                    offsetX++;
                                    if (i == 1)
                                    {
                                        offsetX = 0;
                                        offsetY++;
                                    }
                                }
                            }
                        }
                    }
                }
                else // if the Boss Page is currently on the Twins
                {
                    // Draw both Twins trophies
                    if (Collections.itemList.FindIndex(x => x.type == ItemID.RetinazerTrophy) != -1 && Collections.itemList.FindIndex(x => x.type == ItemID.SpazmatismTrophy) != -1)
                    {
                        if (Collections.checkList[Collections.itemList.FindIndex(x => x.type == ItemID.RetinazerTrophy)] || Collections.checkList[Collections.itemList.FindIndex(x => x.type == ItemID.SpazmatismTrophy)])
                        {
                            if (timerTrophy != 0) timerTrophy--;
                            else timerTrophy = 480;

                            Main.instance.LoadTiles(240);
                            Texture2D trophy = Main.tileTexture[240];
                            int offsetX = 0;
                            int offsetY = 0;

                            bool drawTrophy = true;

                            if (timerTrophy >= 240 && Collections.checkList[Collections.itemList.FindIndex(x => x.type == ItemID.RetinazerTrophy)])
                            {
                                offsetX = 24;
                                offsetY = 0;
                                drawTrophy = true;
                            }
                            else if (timerTrophy >= 240 && !Collections.checkList[Collections.itemList.FindIndex(x => x.type == ItemID.RetinazerTrophy)])
                            {
                                drawTrophy = false;
                            }

                            if (timerTrophy < 240 && Collections.checkList[Collections.itemList.FindIndex(x => x.type == ItemID.SpazmatismTrophy)])
                            {
                                offsetX = 27;
                                offsetY = 0;
                                drawTrophy = true;
                            }
                            else if (timerTrophy < 240 && !Collections.checkList[Collections.itemList.FindIndex(x => x.type == ItemID.SpazmatismTrophy)])
                            {
                                drawTrophy = false;
                            }

                            int backupX = offsetX;
                            int backupY = offsetY;

                            if (drawTrophy)
                            {
                                for (int i = 0; i < 9; i++)
                                {
                                    Rectangle posRect = new Rectangle(pageRect.X + 94 + (offsetX * 16) - (backupX * 16), pageRect.Y + 126 + (offsetY * 16) - (backupY * 16), 18, 18);
                                    Rectangle cutRect = new Rectangle(offsetX * 18, offsetY * 18, 18, 18);

                                    spriteBatch.Draw(trophy, posRect, cutRect, new Color(255, 255, 255));

                                    offsetX++;
                                    if (i == 2 || i == 5)
                                    {
                                        offsetX = backupX;
                                        offsetY++;
                                    }
                                }
                            }
                        }
                    }

                    // Draw Twins Mask
                    if (Collections.itemList.FindIndex(x => x.type == ItemID.TwinMask) != -1)
                    {
                        if (Collections.checkList[Collections.itemList.FindIndex(x => x.type == ItemID.TwinMask)])
                        {
                            Item newItem = new Item();
                            newItem.SetDefaults(BossPage.collection[0]);
                            Texture2D mask = ModLoader.GetTexture("Terraria/Armor_Head_" + newItem.headSlot);

                            int frameCut = mask.Height / 24;
                            Rectangle posRect = new Rectangle(pageRect.X + (pageRect.Width / 2) - (mask.Width / 2) - 12, pageRect.Y + (pageRect.Height / 2) - (frameCut / 2) - 86, mask.Width, frameCut);
                            Rectangle cutRect = new Rectangle(0, 0, mask.Width, frameCut);
                            spriteBatch.Draw(mask, posRect, cutRect, new Color(255, 255, 255));
                        }
                    }

                    // Draw Twins Music Box
                    if (Collections.itemList.FindIndex(x => x.type == ItemID.MusicBoxBoss2) != -1)
                    {
                        if (Collections.checkList[Collections.itemList.FindIndex(x => x.type == ItemID.MusicBoxBoss2)])
                        {
                            int offsetX = 0;
                            int offsetY = 20;
                            Main.instance.LoadTiles(139);
                            Texture2D musicBox = Main.tileTexture[139];

                            if (Main.music[MusicID.Boss2].IsPlaying) offsetX = 2;

                            int backupX = offsetX;
                            int backupY = offsetY;

                            for (int i = 0; i < 4; i++)
                            {
                                Rectangle posRect = new Rectangle(pageRect.X + 206 + (offsetX * 16) - (backupX * 16), pageRect.Y + 158 + (offsetY * 16) - (backupY * 16), 18, 18);
                                Rectangle cutRect = new Rectangle(offsetX * 18, (offsetY * 18) - 2, 18, 18);

                                spriteBatch.Draw(musicBox, posRect, cutRect, new Color(255, 255, 255));

                                offsetX++;
                                if (i == 1)
                                {
                                    offsetX = backupX;
                                    offsetY++;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    internal class FixedUIScrollbar : UIScrollbar
    {
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            UserInterface temp = UserInterface.ActiveInstance;
            UserInterface.ActiveInstance = BossAssist.instance.BossLogInterface;
            base.DrawSelf(spriteBatch);
            UserInterface.ActiveInstance = temp;
        }

        public override void MouseDown(UIMouseEvent evt)
        {
            UserInterface temp = UserInterface.ActiveInstance;
            UserInterface.ActiveInstance = BossAssist.instance.BossLogInterface;
            base.MouseDown(evt);
            UserInterface.ActiveInstance = temp;
        }

        public override void Click(UIMouseEvent evt)
        {
            UserInterface temp = UserInterface.ActiveInstance;
            UserInterface.ActiveInstance = BossAssist.instance.BossLogInterface;
            base.MouseDown(evt);
            UserInterface.ActiveInstance = temp;
        }
    }

    internal class BookUI : UIImage
    {
        public static bool visible = false;
        Texture2D book;

        public BookUI(Texture2D texture) : base(texture)
        {
            book = texture;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (!visible) return;
            base.DrawSelf(spriteBatch);
            if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface)
            {
                // Needed to remove mousetext from outside sources when using the Boss Log
                Main.player[Main.myPlayer].mouseInterface = true;
                Main.LocalPlayer.showItemIcon = false;
                Main.ItemIconCacheUpdate(0);
                Main.HoverItem = null;
                Main.HoveringOverAnNPC = false;
            }
        }
    }

    internal class TableOfContents : UIText
    {
        float order = 0;
        bool nextCheck;

        public TableOfContents(float order, string text, bool nextCheck, float textScale = 1, bool large = false) : base(text, textScale, large)
        {
            this.order = order;
            this.nextCheck = nextCheck;
            Recalculate();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            Texture2D progressBox = BossAssist.instance.GetTexture("Resources/Checkbox_Empty");
            CalculatedStyle innerDimensions = GetInnerDimensions();
            Vector2 pos = new Vector2(innerDimensions.X - 20, innerDimensions.Y - 5);
            if (!Id.Contains("Header")) spriteBatch.Draw(progressBox, pos, Color.White);
            if (order != -1f)
            {
                BossAssist BA = BossAssist.instance;
                if (BA.setup.SortedBosses[Convert.ToInt32(Id)].downed()) spriteBatch.Draw(BA.GetTexture("Resources/Checkbox_Check"), pos, Color.White);
                else if (!BA.setup.SortedBosses[Convert.ToInt32(Id)].downed() && nextCheck) spriteBatch.Draw(BA.GetTexture("Resources/Checkbox_Next"), pos, Color.White);

                if (IsMouseHovering && BA.setup.SortedBosses[Convert.ToInt32(Id)].downed()) TextColor = Color.DarkSeaGreen;
                else if (IsMouseHovering && !BA.setup.SortedBosses[Convert.ToInt32(Id)].downed()) TextColor = Color.IndianRed;
                else if (!IsMouseHovering && BA.setup.SortedBosses[Convert.ToInt32(Id)].downed()) TextColor = Colors.RarityGreen;
                else if (!IsMouseHovering && !BA.setup.SortedBosses[Convert.ToInt32(Id)].downed()) TextColor = Colors.RarityRed;
            }
        }

        public override int CompareTo(object obj)
        {
            TableOfContents other = obj as TableOfContents;
            return order.CompareTo(other.order);
        }
    }

    internal class SubpageButton : UIPanel
    {
        string buttonString;

        public SubpageButton(string type)
        {
            buttonString = type;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            BackgroundColor = Color.Brown;
            base.DrawSelf(spriteBatch);

            CalculatedStyle innerDimensions = GetInnerDimensions();
            Vector2 stringAdjust = Main.fontMouseText.MeasureString(buttonString);
            Vector2 pos = new Vector2(innerDimensions.X - (stringAdjust.X / 3) + Width.Pixels / 3, innerDimensions.Y - 10);
            DynamicSpriteFontExtensionMethods.DrawString(spriteBatch, Main.fontMouseText, buttonString, pos, Color.Gold);
        }
    }

    class BossLogUI : UIState
    {
        public BossAssistButton bosslogbutton;

        public BookUI bossLogPanel;
        public BossLogPanel PageOne;
        public BossLogPanel PageTwo;

        public SubpageButton recordButton;
        public SubpageButton spawnButton;
        public SubpageButton lootButton;
        public SubpageButton collectButton;

        public UIImage bossIcon;
        public UIImage spazHead; // Special Case
        public static UIImage[] itemIngredients;

        public UIText TextBossName;
        public UIText TextMod;
        public UIText TextDowned;
        public UIText TextRecord;
        public UIText TextKills;
        public UIText TextDeaths;
        public UIText TextBrink;

        public UIImageButton NextPage;
        public UIImageButton PrevPage;

        public UIList prehardmodeList;
        public UIList hardmodeList;
        public FixedUIScrollbar scrollOne;
        public FixedUIScrollbar scrollTwo;

        public UIList loottableList;
        public FixedUIScrollbar loottableScroll;

        public static int PageNum = 0; // Selected Boss Page
        public static int SubPageNum = 0; // Selected Topic Tab (Loot, Stats, etc.)
        public static bool visible = false;        

        public override void OnInitialize()
        {
            Texture2D bookTexture = BossAssist.instance.GetTexture("Resources/Button_BossLog");
            bosslogbutton = new BossAssistButton(bookTexture, "Boss Log");
            bosslogbutton.Width.Set(34, 0f);
            bosslogbutton.Height.Set(38, 0f);
            bosslogbutton.Left.Set(Main.PendingResolutionWidth - bosslogbutton.Width.Pixels - 190, 0f);
            bosslogbutton.Top.Pixels = Main.PendingResolutionHeight - bosslogbutton.Height.Pixels - 8;
            bosslogbutton.OnClick += new MouseEvent(OpenBossLog);
            bosslogbutton.OnRightClick += new MouseEvent(OpenNextBoss);
            Append(bosslogbutton);

            Texture2D bosslogTexture = BossAssist.instance.GetTexture("Resources/UI_BossLogPanel");
            bossLogPanel = new BookUI(bosslogTexture);
            bossLogPanel.Width.Pixels = 800;
            bossLogPanel.Height.Pixels = 500;
            bossLogPanel.Left.Pixels = (Main.screenWidth / 2) - (bossLogPanel.Width.Pixels / 2);
            bossLogPanel.Top.Pixels = (Main.screenHeight / 2) - (bossLogPanel.Height.Pixels / 2);

            PageOne = new BossLogPanel { Id = "PageOne" };
            PageOne.Width.Pixels = 375;
            PageOne.Height.Pixels = 480;
            PageOne.Left.Pixels = 20;
            PageOne.Top.Pixels = 32;

            Texture2D prevTexture = BossAssist.instance.GetTexture("Resources/Prev");
            PrevPage = new BossAssistButton(prevTexture, "");
            PrevPage.Width.Pixels = 14;
            PrevPage.Height.Pixels = 20;
            PrevPage.Left.Pixels = 1;
            PrevPage.Top.Pixels = 415;
            PrevPage.OnClick += new MouseEvent(PrevPageClicked);
            PageOne.Append(PrevPage);

            prehardmodeList = new UIList();
            prehardmodeList.Left.Pixels = 4;
            prehardmodeList.Top.Pixels = 4;
            prehardmodeList.Width.Pixels = PageOne.Width.Pixels - 25;
            prehardmodeList.Height.Pixels = PageOne.Height.Pixels - 80;
            prehardmodeList.PaddingTop = 5;

            scrollOne = new FixedUIScrollbar();
            scrollOne.SetView(100f, 1000f);
            scrollOne.Top.Pixels = 32f;
            scrollOne.Left.Pixels = -18;
            scrollOne.Height.Set(-6f, 0.75f);
            scrollOne.HAlign = 1f;

            scrollTwo = new FixedUIScrollbar();
            scrollTwo.SetView(100f, 1000f);
            scrollTwo.Top.Pixels = 32f;
            scrollTwo.Left.Pixels = -28;
            scrollTwo.Height.Set(-6f, 0.75f);
            scrollTwo.HAlign = 1f;

            bossLogPanel.Append(PageOne);

            PageTwo = new BossLogPanel { Id = "PageTwo" };
            PageTwo.Width.Pixels = 375;
            PageTwo.Height.Pixels = 480;
            PageTwo.Left.Pixels = bossLogPanel.Width.Pixels - PageTwo.Width.Pixels;
            PageTwo.Top.Pixels = 32;

            loottableList = new UIList();
            loottableList.Left.Pixels = 0;
            loottableList.Top.Pixels = 125;
            loottableList.Width.Pixels = PageTwo.Width.Pixels - 25;
            loottableList.Height.Pixels = PageTwo.Height.Pixels - 125 - 80;

            loottableScroll = new FixedUIScrollbar();
            loottableScroll.SetView(10f, 1000f);
            loottableScroll.Top.Pixels = 125;
            loottableScroll.Left.Pixels = -18;
            loottableScroll.Height.Set(-88f, 0.75f);
            loottableScroll.HAlign = 1f;

            TextBossName = new UIText(GetBookText(0, 1));
            TextBossName.Top.Pixels = 10;
            TextBossName.Left.Pixels = 10;
            PageOne.Append(TextBossName);

            TextMod = new UIText(GetBookText(6, 1));
            TextMod.Top.Pixels = 60;
            TextMod.Left.Pixels = 10;
            PageOne.Append(TextMod);

            TextDowned = new UIText(GetBookText(1, 1));
            TextDowned.Top.Pixels = 35;
            TextDowned.Left.Pixels = 10;
            PageOne.Append(TextDowned);

            TextRecord = new UIText(GetBookText(2, 1));
            TextRecord.Top.Pixels = 260;
            TextRecord.Left.Pixels = 75;

            TextKills = new UIText(GetBookText(3, 1));
            TextKills.Top.Pixels = 110;
            TextKills.Left.Pixels = 75;

            TextDeaths = new UIText(GetBookText(4, 1));
            TextDeaths.Top.Pixels = 185;
            TextDeaths.Left.Pixels = 75;

            TextBrink = new UIText(GetBookText(5, 1));
            TextBrink.Top.Pixels = 335;
            TextBrink.Left.Pixels = 75;

            bossIcon = new UIImage(BossAssist.instance.GetTexture("Resources/Button_BossLog"));
            bossIcon.Width.Pixels = 20;
            bossIcon.Height.Pixels = 20;
            bossIcon.Left.Pixels = PageOne.Width.Pixels - bossIcon.Width.Pixels - 15;
            bossIcon.Top.Pixels = 5;
            bossIcon.OnClick += new MouseEvent(ResetStats);
            PageOne.Append(bossIcon);

            spazHead = new UIImage(ModLoader.GetTexture("Terraria/NPC_Head_Boss_16"));
            spazHead.Left.Pixels = PageOne.Width.Pixels - 80;
            spazHead.Top = bossIcon.Top;
            spazHead.Width.Pixels = 20;
            spazHead.Height.Pixels = 20;

            itemIngredients = new UIImage[15];
            for (int i = 0; i < 15; i++)
            {
                itemIngredients[i] = new UIImage(Main.itemTexture[0]);
                itemIngredients[i].Top.Pixels = PageTwo.Height.Pixels / 2 + itemIngredients[i].Height.Pixels;
                itemIngredients[i].Left.Pixels = itemIngredients[i].Width.Pixels + (35 * i);
                PageTwo.Append(itemIngredients[i]);
            }

            Texture2D nextTexture = BossAssist.instance.GetTexture("Resources/Next");
            NextPage = new BossAssistButton(nextTexture, "");
            NextPage.Width.Pixels = 14;
            NextPage.Height.Pixels = 20;
            NextPage.Left.Pixels = PageTwo.Width.Pixels - (int)(NextPage.Width.Pixels * 2.5);
            NextPage.Top.Pixels = 415;
            NextPage.OnClick += new MouseEvent(NextPageClicked);
            PageTwo.Append(NextPage);

            hardmodeList = new UIList();
            hardmodeList.Left.Pixels = 4;
            hardmodeList.Top.Pixels = 4;
            hardmodeList.Width.Pixels = PageOne.Width.Pixels - 25;
            hardmodeList.Height.Pixels = PageOne.Height.Pixels - 80;
            hardmodeList.PaddingTop = 5;
            
            recordButton = new SubpageButton("Boss Records");
            recordButton.Width.Pixels = PageTwo.Width.Pixels / 2 - 24;
            recordButton.Height.Pixels = 25;
            recordButton.Left.Pixels = 0;
            recordButton.Top.Pixels = 15;
            recordButton.OnClick += new MouseEvent(OpenRecord);
            PageTwo.Append(recordButton);

            spawnButton = new SubpageButton("Spawn Item");
            spawnButton.Width.Pixels = PageTwo.Width.Pixels / 2 - 24;
            spawnButton.Height.Pixels = 25;
            spawnButton.Left.Pixels = PageTwo.Width.Pixels / 2 - 8;
            spawnButton.Top.Pixels = 15;
            spawnButton.OnClick += new MouseEvent(OpenSpawn);
            PageTwo.Append(spawnButton);

            lootButton = new SubpageButton("Loot Table");
            lootButton.Width.Pixels = PageTwo.Width.Pixels / 2 - 24;
            lootButton.Height.Pixels = 25;
            lootButton.Left.Pixels = 0;
            lootButton.Top.Pixels = 50;
            lootButton.OnClick += new MouseEvent(OpenLoot);
            PageTwo.Append(lootButton);

            collectButton = new SubpageButton("Collectibles");
            collectButton.Width.Pixels = PageTwo.Width.Pixels / 2 - 24;
            collectButton.Height.Pixels = 25;
            collectButton.Left.Pixels = PageTwo.Width.Pixels / 2 - 8;
            collectButton.Top.Pixels = 50;
            collectButton.OnClick += new MouseEvent(OpenCollect);
            PageTwo.Append(collectButton);

            bossLogPanel.Append(PageTwo);

            Append(bossLogPanel);
        }

        public override void Update(GameTime gameTime)
        {
            visible = Main.playerInventory;
            if (!visible)
            {
                RemoveChild(bosslogbutton);
                BossLogPanel.visible = false;
                BookUI.visible = false;
            }
            else Append(bosslogbutton);
            
            bosslogbutton.Left.Set(Main.PendingResolutionWidth - bosslogbutton.Width.Pixels - 190, 0f);
            bosslogbutton.Top.Pixels = Main.PendingResolutionHeight - bosslogbutton.Height.Pixels - 8;

            if (PageNum == BossAssist.instance.setup.SortedBosses.Count - 1) PageTwo.RemoveChild(NextPage);
            else if (!PageTwo.HasChild(NextPage)) PageTwo.Append(NextPage);

            if (PageNum == -1) PageOne.RemoveChild(PrevPage);
            else if (!PageOne.HasChild(PrevPage)) PageOne.Append(PrevPage);

            if (PageNum == -1) // Checklist Pages
            {
                PageOne.RemoveAllChildren();
                PageOne.Append(scrollOne);
                PageOne.Append(prehardmodeList);
                prehardmodeList.SetScrollbar(scrollOne);
                PageTwo.RemoveAllChildren();
                PageTwo.Append(NextPage);
                PageTwo.Append(scrollTwo);
                PageTwo.Append(hardmodeList);
                hardmodeList.SetScrollbar(scrollTwo);
            }
            else
            {
                if (PageOne.HasChild(prehardmodeList)) PageOne.RemoveChild(prehardmodeList);
                if (PageTwo.HasChild(hardmodeList)) PageTwo.RemoveChild(hardmodeList);
                if (PageOne.HasChild(scrollOne)) PageOne.RemoveChild(scrollOne);
                if (PageTwo.HasChild(scrollTwo)) PageTwo.RemoveChild(scrollTwo);
                PageOne.Append(TextBossName);
                PageOne.Append(TextMod);
                PageOne.Append(TextDowned);
                PageOne.Append(bossIcon);
                
                Main.instance.LoadNPC(BossAssist.instance.setup.SortedBosses[PageNum].id);
                Texture2D head;
                if (BossAssist.instance.setup.SortedBosses[PageNum].id < NPCID.Count)
                {
                    int type = BossAssist.instance.setup.SortedBosses[PageNum].id;
                    if (type == NPCID.KingSlime) head = ModLoader.GetTexture("Terraria/NPC_Head_Boss_7");
                    else if (type == NPCID.EyeofCthulhu) head = ModLoader.GetTexture("Terraria/NPC_Head_Boss_0");
                    else if (type == NPCID.EaterofWorldsHead) head = ModLoader.GetTexture("Terraria/NPC_Head_Boss_2");
                    else if (type == NPCID.BrainofCthulhu) head = ModLoader.GetTexture("Terraria/NPC_Head_Boss_23");
                    else if (type == NPCID.QueenBee) head = ModLoader.GetTexture("Terraria/NPC_Head_Boss_14");
                    else if (type == NPCID.SkeletronHead) head = ModLoader.GetTexture("Terraria/NPC_Head_Boss_19");
                    else if (type == NPCID.WallofFlesh) head = ModLoader.GetTexture("Terraria/NPC_Head_Boss_22");
                    else if (type == NPCID.Retinazer) head = ModLoader.GetTexture("Terraria/NPC_Head_Boss_21");
                    else if (type == NPCID.TheDestroyer) head = ModLoader.GetTexture("Terraria/NPC_Head_Boss_25");
                    else if (type == NPCID.SkeletronPrime) head = ModLoader.GetTexture("Terraria/NPC_Head_Boss_18");
                    else if (type == NPCID.Plantera) head = ModLoader.GetTexture("Terraria/NPC_Head_Boss_11");
                    else if (type == NPCID.Golem) head = ModLoader.GetTexture("Terraria/NPC_Head_Boss_5");
                    else if (type == NPCID.DD2Betsy) head = ModLoader.GetTexture("Terraria/NPC_Head_Boss_34");
                    else if (type == NPCID.DukeFishron) head = ModLoader.GetTexture("Terraria/NPC_Head_Boss_4");
                    else if (type == NPCID.CultistBoss) head = ModLoader.GetTexture("Terraria/NPC_Head_Boss_24");
                    else if (type == NPCID.MoonLordHead) head = ModLoader.GetTexture("Terraria/NPC_Head_Boss_8");
                    else head = ModLoader.GetTexture("Terraria/NPC_Head_0");
                }
                else
                {
                    head = ModLoader.GetTexture(NPCLoader.GetNPC(BossAssist.instance.setup.SortedBosses[PageNum].id).BossHeadTexture);
                }
                bossIcon.SetImage(head);
                
                if (PageNum == BossAssist.instance.setup.SortedBosses.FindIndex(x => x.id == NPCID.Retinazer))
                {
                    if (!PageOne.HasChild(spazHead)) PageOne.Append(spazHead);
                }
                else
                {
                    if (PageOne.HasChild(spazHead)) PageOne.RemoveChild(spazHead);
                }
                if (PageNum == BossAssist.instance.setup.SortedBosses.FindIndex(x => x.id == NPCID.DD2Betsy))
                {
                    bossIcon.Left.Pixels = PageOne.Width.Pixels - bossIcon.Width.Pixels - 10;
                }
                else
                {
                    bossIcon.Left.Pixels = PageOne.Width.Pixels - bossIcon.Width.Pixels - 15;
                }
                TextBossName.SetText(GetBookText(0, PageNum));
                TextMod.SetText(GetBookText(6, PageNum));
                TextDowned.SetText(GetBookText(1, PageNum));
            }
            base.Update(gameTime);
        }

        private void OpenBossLog(UIMouseEvent evt, UIElement listeningElement)
        {
            if (BossLogPanel.visible)
            {
                BossLogPanel.visible = false;
                BookUI.visible = false;
            }
            else
            {
                PageNum = -1;
                SubPageNum = 0;
                BossLogPanel.visible = true;
                BookUI.visible = true;
                UpdateTableofContents();
            }
        }

        private void OpenNextBoss(UIMouseEvent evt, UIElement listeningElement)
        {
            for (int b = 0; b < BossAssist.instance.setup.SortedBosses.Count; b++)
            {
                if (!BossAssist.instance.setup.SortedBosses[b].downed())
                {
                    if (PageNum != b)
                    {
                        PageNum = b;
                        SubPageNum = 1;
                        OpenSpawn(evt, listeningElement);
                    }
                    else if (BossLogPanel.visible) PageNum = -1;
                    break;
                }
                // If the final boss page is downed, just redirect to the Table of Contents
                if (b == BossAssist.instance.setup.SortedBosses.Count - 1) PageNum = -1;
            }
            if (!BossLogPanel.visible)
            {
                BossLogPanel.visible = true;
                BookUI.visible = true;
                if (PageNum != -1)
                {
                    SubPageNum = 1;
                    OpenSpawn(evt, listeningElement);
                }
            }
        }

        private void ResetStats(UIMouseEvent evt, UIElement listeningElement)
        {
            if (PageNum != -1)
            {
                Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[PageNum].stat.fightTime = 0;
                Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[PageNum].stat.kills = 0;
                Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[PageNum].stat.deaths = 0;
                Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[PageNum].stat.brink = 0;
                Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[PageNum].stat.brinkPercent = 0;
            }
            OpenRecord(evt, listeningElement);
        }

        private void NextPageClicked(UIMouseEvent evt, UIElement listeningElement)
        {
            if (PageNum < BossAssist.instance.setup.SortedBosses.Count - 1) PageNum++;
            if (SubPageNum == 0) OpenRecord(evt, listeningElement);
            else if (SubPageNum == 1) OpenSpawn(evt, listeningElement);
            else if (SubPageNum == 2) OpenLoot(evt, listeningElement);
            else if (SubPageNum == 3) OpenCollect(evt, listeningElement);
        }

        private void PrevPageClicked(UIMouseEvent evt, UIElement listeningElement)
        {
            if (PageNum > -1) PageNum--;
            if (PageNum == -1) UpdateTableofContents();
            else
            {
                if (SubPageNum == 0) OpenRecord(evt, listeningElement);
                else if (SubPageNum == 1) OpenSpawn(evt, listeningElement);
                else if (SubPageNum == 2) OpenLoot(evt, listeningElement);
                else if (SubPageNum == 3) OpenCollect(evt, listeningElement);
            }
        }

        private void OpenRecord(UIMouseEvent evt, UIElement listeningElement)
        {
            SubPageNum = 0;
            ResetPageTwo();

            TextKills.SetText(GetBookText(2, PageNum));
            TextDeaths.SetText(GetBookText(3, PageNum));
            TextRecord.SetText(GetBookText(4, PageNum));
            TextBrink.SetText(GetBookText(5, PageNum));

            PageTwo.Append(TextKills);
            PageTwo.Append(TextDeaths);
            PageTwo.Append(TextRecord);
            PageTwo.Append(TextBrink);
        }

        private void OpenSpawn(UIMouseEvent evt, UIElement listeningElement)
        {
            SubPageNum = 1;
            Mod thorium = ModLoader.GetMod("ThoriumMod");
            if (thorium != null)
            {
                BossInfo spot = BossAssist.instance.setup.SortedBosses.Find(x => x.name == "Plantera");
                spot.spawnItem = thorium.ItemType("PlantBulb");
            }
            ResetPageTwo();
            List<Item> ingredients = new List<Item>();
            List<int> requiredTiles = new List<int>();
            List<Recipe> recipes = Main.recipe.ToList();
            Item spawn = new Item();
            spawn.SetDefaults(BossAssist.instance.setup.SortedBosses[PageNum].spawnItem);
            if (spawn.type != 0)
            {
                LogItemSlot spawnItemSlot = new LogItemSlot(spawn, spawn.HoverName, ItemSlot.Context.EquipDye);
                spawnItemSlot.Height.Pixels = 50;
                spawnItemSlot.Width.Pixels = 50;
                spawnItemSlot.Top.Pixels = 105;
                spawnItemSlot.Left.Pixels = 0;
                PageTwo.Append(spawnItemSlot);

                for (int i = 0; i < recipes.Count; i++)
                {
                    if (recipes[i].createItem.type != BossAssist.instance.setup.SortedBosses[PageNum].spawnItem) continue;
                    for (int j = 0; j < recipes[i].requiredItem.Length; j++)
                    {
                        ingredients.Add(recipes[i].requiredItem[j]);
                    }
                    for (int k = 0; k < recipes[i].requiredTile.Length; k++)
                    {
                        if (recipes[i].requiredTile[k] == -1) break;
                        requiredTiles.Add(recipes[i].requiredTile[k]);
                    }
                    break;
                }
                int row = 0;
                int col = 0;
                for (int k = 0; k < ingredients.Count; k++)
                {
                    LogItemSlot ingList = new LogItemSlot(ingredients[k], ingredients[k].HoverName);
                    ingList.Height.Pixels = 50;
                    ingList.Width.Pixels = 50;
                    ingList.Top.Pixels = 105 + (56 * (row + 1));
                    ingList.Left.Pixels = (56 * col);
                    PageTwo.Append(ingList);
                    col++;
                    if (k == 4 || k == 9)
                    {
                        col = 0;
                        row++;
                    }
                }

                Item craft = new Item();
                // Testing for Tile Requirements in ItemSlots
                if (ingredients.Count > 0 && requiredTiles.Count == 0 && spawn.type != 0)
                {
                    craft.SetDefaults(ItemID.PowerGlove);

                    LogItemSlot craftItem = new LogItemSlot(craft, "By Hand", ItemSlot.Context.EquipArmorVanity);
                    craftItem.Height.Pixels = 50;
                    craftItem.Width.Pixels = 50;
                    craftItem.Top.Pixels = 105;
                    craftItem.Left.Pixels = 56;
                    PageTwo.Append(craftItem);
                }
                else if (requiredTiles.Count > 0 && spawn.type != 0)
                {
                    for (int l = 0; l < requiredTiles.Count; l++)
                    {
                        for (int m = 0; m < ItemLoader.ItemCount; m++)
                        {
                            craft.SetDefaults(m);
                            if (craft.createTile == requiredTiles[l]) break;
                        }

                        if (craft.type == -1) break;
                        LogItemSlot tileList;
                        if (requiredTiles[l] == 26)
                        {
                            craft.SetDefaults(0);
                            string altarType;
                            if (WorldGen.crimson) altarType = "Crimson Altar";
                            else altarType = "Demon Altar";
                            tileList = new LogItemSlot(craft, altarType, ItemSlot.Context.EquipArmorVanity);
                        }
                        else
                        {
                            tileList = new LogItemSlot(craft, craft.HoverName, ItemSlot.Context.EquipArmorVanity);
                        }
                        tileList.Height.Pixels = 50;
                        tileList.Width.Pixels = 50;
                        tileList.Top.Pixels = 105;
                        tileList.Left.Pixels = 56 * (l + 1);
                        PageTwo.Append(tileList);
                        if (requiredTiles[l] == 26)
                        {
                            Texture2D altarTexture = BossAssist.instance.GetTexture("Resources/Demon_Altar");
                            if (WorldGen.crimson) altarTexture = BossAssist.instance.GetTexture("Resources/Crimson_Altar");
                            UIImage altar = new UIImage(altarTexture);
                            altar.Height.Pixels = 50;
                            altar.Width.Pixels = 50;
                            altar.ImageScale = 0.75f;
                            altar.Top.Pixels = tileList.Top.Pixels + tileList.Height.Pixels / 3 * 0.45f;
                            altar.Left.Pixels = tileList.Left.Pixels + tileList.Width.Pixels / 3 * 0.15f;
                            PageTwo.Append(altar);
                        }
                    }
                }
            }
        }

        private void OpenLoot(UIMouseEvent evt, UIElement listeningElement)
        {
            SubPageNum = 2;
            ResetPageTwo();
            int row = 0;
            int col = 0;

            loottableList.Clear();
            List<BossInfo> shortcut = BossAssist.instance.setup.SortedBosses;
            LootRow newRow = new LootRow(0);
            for (int i = 0; i < shortcut[PageNum].loot.Count; i++)
            {
                Item expertItem = new Item();
                expertItem.SetDefaults(shortcut[PageNum].loot[i]);
                if (!expertItem.expert || expertItem.Name.Contains("Treasure Bag")) continue;
                else
                {
                    LogItemSlot lootTable = new LogItemSlot(expertItem, expertItem.Name, ItemSlot.Context.ShopItem);
                    lootTable.Height.Pixels = 50;
                    lootTable.Width.Pixels = 50;
                    lootTable.Left.Pixels = (col * 56);
                    newRow.Append(lootTable);
                    col++;
                    if (col == 6)
                    {
                        col = 0;
                        row++;
                        loottableList.Add(newRow);
                        newRow = new LootRow(row);
                    }
                }
            }
            for (int i = 0; i < shortcut[PageNum].loot.Count; i++)
            {
                Item loot = new Item();
                loot.SetDefaults(shortcut[PageNum].loot[i]);

                if (loot.expert || loot.Name.Contains("Treasure Bag")) continue;
                else
                {
                    LogItemSlot lootTable = new LogItemSlot(loot, loot.Name, ItemSlot.Context.GuideItem);
                    lootTable.Height.Pixels = 50;
                    lootTable.Width.Pixels = 50;
                    lootTable.Left.Pixels = (col * 56);
                    newRow.Append(lootTable);
                    col++;
                    if (col == 6 || i == shortcut[PageNum].loot.Count - 1)
                    {
                        col = 0;
                        row++;
                        loottableList.Add(newRow);
                        newRow = new LootRow(row);
                    }
                }
            }
            if (row > 4) PageTwo.Append(loottableScroll);
            PageTwo.Append(loottableList);
            loottableList.SetScrollbar(loottableScroll);
        }

        private void OpenCollect(UIMouseEvent evt, UIElement listeningElement)
        {
            SubPageNum = 3;
            ResetPageTwo();
            int row = 0;
            int col = 0;
            List<BossInfo> shortcut = BossAssist.instance.setup.SortedBosses;
            for (int i = 0; i < shortcut[PageNum].collection.Count; i++)
            {
                Item collectible = new Item();
                collectible.SetDefaults(shortcut[PageNum].collection[i]);

                LogItemSlot collectionTable = new LogItemSlot(collectible, collectible.Name, ItemSlot.Context.GuideItem);
                collectionTable.Height.Pixels = 50;
                collectionTable.Width.Pixels = 50;
                collectionTable.Id = "collect" + i;
                collectionTable.Top.Pixels = 190 + (56 * (row + 1));
                collectionTable.Left.Pixels = (56 * (col));
                PageTwo.Append(collectionTable);
                col++;
                if (col == 6)
                {
                    col = 0;
                    row++;
                }
            }
        }

        public string GetBookText(int type, int page)
        {
            BossInfo shortcut = BossAssist.instance.setup.SortedBosses[page];
            int bossCount = BossAssist.instance.setup.SortedBosses.Count;
            if (!Main.gameMenu)
            {
                if (type == 0) return "[c/daa520:" + shortcut.name + "]";
                else if (type == 1)
                {
                    if (shortcut.downed()) return "[c/d3ffb5:Defeated in " + Main.worldName + "]";
                    else return "[c/ffccc8:Undefeated in " + Main.worldName + "]";
                }
                else if (type == 2)
                {
                    int killTimes = Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[page].stat.kills;
                    return "Times boss was killed:\n" + killTimes;
                }
                else if (type == 3)
                {
                    int deathTimes = Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[page].stat.deaths;
                    return "Times died to boss:\n" + deathTimes;
                }
                else if (type == 4)
                {
                    if (Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[page].stat.fightTime != 0)
                    {
                        double record = (double)Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[page].stat.fightTime / 60;
                        int recordMin = (int)record / 60;
                        int recordSec = (int)record % 60;
                        if (recordMin > 0) return "Quickest Defeat:\n" + recordMin + "m " + recordSec + "s";
                        else return "Quickest victory:\n" + record.ToString("0.##") + "s";
                    }
                    else return "Quickest victory:\nNo record!";
                }
                else if (type == 5)
                {
                    if (Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[page].stat.brink != 0)
                    {
                        return "Lowest health triumph:\n" + Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[page].stat.brink +
                               " (" + Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[page].stat.brinkPercent + "%)";
                    }
                    else return "Lowest health triumph:\nNo record!";
                }
                else if (type == 6)
                {
                    if (shortcut.source == "Vanilla") return "[c/9696ff:" + shortcut.source + "]";
                    else return "[c/9696ff:" + ModLoader.GetMod(shortcut.source).DisplayName + "]";
                }
            }
            return "It failed... oops!";
        }

        public void UpdateTableofContents()
        {
            int nextCheck = 0;
            bool nextCheckBool = false;
            prehardmodeList.Clear();
            hardmodeList.Clear();
            TableOfContents Header = new TableOfContents(-1f, "Pre-Hardmode", true, 1.4f)
            {
                Id = "Header1",
                PaddingTop = 2,
                PaddingLeft = 15,
                TextColor = Colors.RarityAmber
            };
            TableOfContents Header2 = new TableOfContents(-1f, "Hardmode", true, 1.4f)
            {
                Id = "Header2",
                PaddingTop = 2,
                PaddingLeft = 15,
                TextColor = Colors.RarityAmber
            };
            prehardmodeList.Add(Header);
            hardmodeList.Add(Header2);
            List<BossInfo> copiedList = new List<BossInfo>(BossAssist.instance.setup.SortedBosses.Count);
            foreach (BossInfo boss in BossAssist.instance.setup.SortedBosses)
            {
                copiedList.Add(boss);
            }
            copiedList.Sort((x, y) => x.progression.CompareTo(y.progression));

            if (WorldGen.crimson) copiedList.Remove(copiedList.Find(x => x.name == "Eater of Worlds"));
            else copiedList.Remove(copiedList.Find(x => x.name == "Brain of Cthulhu"));

            for (int i = 0; i < copiedList.Count; i++)
            {
                if (!copiedList[i].downed()) nextCheck++;
                if (nextCheck == 1) nextCheckBool = true;

                TableOfContents next = new TableOfContents(copiedList[i].progression, copiedList[i].name, nextCheckBool);
                nextCheckBool = false;
                
                if (copiedList[i].progression <= 6f)
                {
                    if (copiedList[i].downed())
                    {
                        next.PaddingTop = 5;
                        next.PaddingLeft = 22;
                        next.TextColor = Color.LawnGreen;
                        if (copiedList[i].progression > 3f || copiedList[i].id == NPCID.BrainofCthulhu) next.Id = (i + 1).ToString();
                        else next.Id = i.ToString();
                        next.OnClick += new MouseEvent(UpdatePage);
                        prehardmodeList.Add(next);
                    }
                    else if (!copiedList[i].downed())
                    {
                        nextCheck++;
                        next.PaddingTop = 5;
                        next.PaddingLeft = 22;
                        next.TextColor = Color.IndianRed;
                        if (copiedList[i].progression > 3f || copiedList[i].id == NPCID.BrainofCthulhu) next.Id = (i + 1).ToString();
                        else next.Id = i.ToString();
                        next.OnClick += new MouseEvent(UpdatePage);
                        prehardmodeList.Add(next);
                    }
                }
                else
                {
                    if (copiedList[i].downed())
                    {
                        next.PaddingTop = 5;
                        next.PaddingLeft = 22;
                        next.TextColor = Color.LawnGreen;
                        if (copiedList[i].progression > 3f || copiedList[i].id == NPCID.BrainofCthulhu) next.Id = (i + 1).ToString();
                        else next.Id = i.ToString();
                        next.OnClick += new MouseEvent(UpdatePage);
                        hardmodeList.Add(next);
                    }
                    else if (!copiedList[i].downed())
                    {
                        nextCheck++;
                        next.PaddingTop = 5;
                        next.PaddingLeft = 22;
                        next.TextColor = Color.IndianRed;
                        if (copiedList[i].progression > 3f || copiedList[i].id == NPCID.BrainofCthulhu) next.Id = (i + 1).ToString();
                        else next.Id = i.ToString();
                        next.OnClick += new MouseEvent(UpdatePage);
                        hardmodeList.Add(next);
                    }
                }
            }
        }

        private void UpdatePage(UIMouseEvent evt, UIElement listeningElement)
        {
            if (!BossAssist.instance.setup.SortedBosses[Convert.ToInt32(listeningElement.Id)].downed())
            {
                PageNum = Convert.ToInt32(listeningElement.Id);
                SubPageNum = 1;
                OpenSpawn(evt, listeningElement);
            }
            else
            {
                PageNum = Convert.ToInt32(listeningElement.Id);
                SubPageNum = 0;
                OpenRecord(evt, listeningElement);
            }
        }

        private void ResetPageTwo()
        {
            PageTwo.RemoveAllChildren();
            PageTwo.Append(spawnButton);
            PageTwo.Append(lootButton);
            PageTwo.Append(collectButton);
            PageTwo.Append(recordButton);
        }
    }
}