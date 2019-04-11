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

// Change the Credits to opted mods to a UIList so it can scroll if it gets too long
// BUG: If UI overlaps with inventory, hovering over the overlap crashes the game

// Boss Log UI has been changed to close the inventory when visible and opens the inventory when clsoing the UI

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

            Rectangle pageRect = GetInnerDimensions().ToRectangle();

            if (Id == "PageOne" && BossLogUI.PageNum >= 0)
            {
                BossInfo shortcut = BossAssist.instance.setup.SortedBosses[BossLogUI.PageNum];

                Texture2D bossTexture = ModLoader.GetTexture(BossAssist.instance.setup.SortedBosses[BossLogUI.PageNum].pageTexture);
                Rectangle posRect = new Rectangle(pageRect.X + (pageRect.Width / 2) - (bossTexture.Width / 2), pageRect.Y + (pageRect.Height / 2) - (bossTexture.Height / 2), bossTexture.Width, bossTexture.Height);
                Rectangle cutRect = new Rectangle(0, 0, bossTexture.Width, bossTexture.Height);
                spriteBatch.Draw(bossTexture, posRect, cutRect, new Color(255, 255, 255));
                
                string sourceDisplayName = "";
                string isDefeated = "";

                if (shortcut.source == "Vanilla") sourceDisplayName = "[c/9696ff:" + shortcut.source + "]";
                else sourceDisplayName = "[c/9696ff:" + ModLoader.GetMod(shortcut.source).DisplayName + "]";

                if (shortcut.downed()) isDefeated = "[c/d3ffb5:Defeated in " + Main.worldName + "]";
                else isDefeated = "[c/ffccc8:Undefeated in " + Main.worldName + "]";

                Vector2 pos = new Vector2(GetInnerDimensions().X + 5, GetInnerDimensions().Y + 5);
                Utils.DrawBorderString(spriteBatch, shortcut.name, pos, Color.Goldenrod);

                pos = new Vector2(GetInnerDimensions().X + 5, GetInnerDimensions().Y + 30);
                Utils.DrawBorderString(spriteBatch, isDefeated, pos, Color.White);

                pos = new Vector2(GetInnerDimensions().X + 5, GetInnerDimensions().Y + 55);
                Utils.DrawBorderString(spriteBatch, sourceDisplayName, pos, Color.White);
            }

            if (Id == "PageOne" && BossLogUI.PageNum == -2)
            {
                // Credits Page
                Vector2 pos = new Vector2(GetInnerDimensions().X + 5, GetInnerDimensions().Y + 5);
                Utils.DrawBorderString(spriteBatch, "Special thanks to:", pos, Color.IndianRed);

                pos = new Vector2(GetInnerDimensions().X + 15, GetInnerDimensions().Y + 35);
                Utils.DrawBorderString(spriteBatch, "Orian34 - Beta Testing", pos, new Color(49, 210, 162));

                pos = new Vector2(GetInnerDimensions().X + 15, GetInnerDimensions().Y + 65);
                Utils.DrawBorderString(spriteBatch, "direwolf420 - Boss Radar Code", pos, Color.Goldenrod);

                pos = new Vector2(GetInnerDimensions().X + 15, GetInnerDimensions().Y + 95);
                Utils.DrawBorderString(spriteBatch, "RiverOaken - Boss Log UI Sprite", pos, Color.LightPink);

                pos = new Vector2(GetInnerDimensions().X + 15, GetInnerDimensions().Y + 125);
                Utils.DrawBorderString(spriteBatch, "Corinna - Boss Placeholder Sprite", pos, Color.MediumPurple);

                pos = new Vector2(GetInnerDimensions().X + 5, GetInnerDimensions().Y + 200);
                Utils.DrawBorderString(spriteBatch, "To add your own bosses to the boss log, just \nfollow the instructions on the homepage.\n\nSuggest adding boss pages, drop additions, \nand collectible additions to other modders. \nThe more this mod expands the better!!", pos, Color.LightCoral);
            }

            if (Id == "PageTwo" && BossLogUI.PageNum == -2)
            {
                // Credits Page

                List<string> optedMods = new List<string>();
                foreach (BossInfo boss in BossAssist.instance.setup.SortedBosses)
                {
                    if (boss.source != "Vanilla")
                    {
                        string sourceDisplayName = ModLoader.GetMod(boss.source).DisplayName;
                        if (!optedMods.Contains(sourceDisplayName))
                        {
                            optedMods.Add(sourceDisplayName);
                        }
                    }
                }

                int adjustment = 0;

                if (optedMods.Count != 0)
                {
                    Vector2 pos = new Vector2(GetInnerDimensions().X + 5, GetInnerDimensions().Y + 5);
                    Utils.DrawBorderString(spriteBatch, "Thanks to all the mods who opted in!*", pos, Color.LightSkyBlue);

                    //TODO: Change this drawing to a UIList of UITexts
                    foreach (string mod in optedMods)
                    {
                        adjustment += 35;
                        Vector2 listPos = new Vector2(GetInnerDimensions().X + 15, GetInnerDimensions().Y + adjustment);
                        Utils.DrawBorderString(spriteBatch, mod, listPos, Color.White);

                        if (adjustment / 35 == optedMods.Count)
                        {
                            adjustment += 35;
                            listPos = new Vector2(GetInnerDimensions().X + 5, GetInnerDimensions().Y + adjustment);
                            Utils.DrawBorderString(spriteBatch, "*The list only contains loaded mods", listPos, Color.LightBlue);
                        }
                    }
                }
                else
                {
                    Vector2 pos = new Vector2(GetInnerDimensions().X + 5, GetInnerDimensions().Y + 5);
                    Utils.DrawBorderString(spriteBatch, "None of your loaded mods have added \npages to the Boss Log. If you want your \nfavorite mods to be included, suggest \nadding their own boss pages to the mod's \ndiscord or forums page!", pos, Color.LightBlue);
                }
            }

            if (Id == "PageTwo" && BossLogUI.PageNum >= 0 && BossLogUI.SubPageNum == 0)
            {
                // Boss Records Subpage
                Texture2D achievements = ModLoader.GetTexture("Terraria/UI/Achievements");
                BossStats record = Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat;

                string recordType = "";
                string recordNumbers = "";
                int achX = 0;
                int achY = 0;
                int achX2 = 0;
                int achY2 = 0;

                for (int i = 0; i < 4; i++)
                {
                    if (i == 0)
                    {
                        recordType = "Kill Death Ratio";
                        achX = 4;
                        achY = 10;
                        achX2 = 4;
                        achY2 = 8;
                        
                        int killTimes = Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.kills;
                        int deathTimes = Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.deaths;
                        if (killTimes == 0 && deathTimes == 0) recordNumbers = "Unchallenged!";
                        else recordNumbers = killTimes + " kills / " + deathTimes + " deaths";
                    }
                    else if (i == 1)
                    {
                        recordType = "Quickest Victory";
                        achX = 4;
                        achY = 9;
                        achX2 = 7;
                        achY2 = 5;

                        string finalResult = "";
                        if (Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.fightTime != 0)
                        {
                            double recordOrg = (double)Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.fightTime / 60;
                            int recordMin = (int)recordOrg / 60;
                            int recordSec = (int)recordOrg % 60;

                            double record2 = (double)Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.fightTime2 / 60;
                            int recordMin2 = (int)recordOrg / 60;
                            int recordSec2 = (int)recordOrg % 60;

                            string rec1 = recordOrg.ToString("0.##");
                            string recSec1 = recordSec.ToString("0.##");
                            string rec2 = record2.ToString("0.##");
                            string recSec2 = recordSec2.ToString("0.##");

                            if (rec1.Length > 2 && rec1.Substring(rec1.Length - 2).Contains(".")) rec1 += "0";
                            if (recSec1.Length > 2 && recSec1.Substring(recSec1.Length - 2).Contains(".")) recSec1 += "0";
                            if (rec2.Length > 2 && rec2.Substring(rec2.Length - 2).Contains(".")) rec2 += "0";
                            if (recSec2.Length > 2 && recSec2.Substring(recSec2.Length - 2).Contains(".")) recSec2 += "0";

                            if (recordMin > 0) finalResult += recordMin + "m " + recSec1 + "s";
                            else finalResult += rec1 + "s";

                            if (Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.fightTime2 != 0)
                            {
                                if (recordMin2 > 0) finalResult += " / " + recordMin2 + "m " + recSec2 + "s";
                                else finalResult += " / " + rec2 + "s";
                            }

                            recordNumbers = finalResult;
                        }
                        else recordNumbers = "No record!";
                    }
                    else if (i == 2)
                    {
                        recordType = "Vitality";
                        achX = 3;
                        achY = 0;
                        achX2 = 6;
                        achY2 = 7;

                        string finalResult = "";
                        if (Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.brink != 0)
                        {
                            finalResult += Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.brink +
                                   " (" + Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.brinkPercent + "%)";
                            if (Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.brink2 != 0)
                            {
                                finalResult += " / " + Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.brink2 +
                                   " (" + Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.brinkPercent2 + "%)";
                            }
                            recordNumbers = finalResult;
                        }
                        else recordNumbers = "No record!";
                    }
                    else if (i == 3)
                    {
                        recordType = "Evasiveness (WIP)";
                        achX = 0;
                        achY = 7;
                        achX2 = 4;
                        achY2 = 2;
                        
                        int timer = Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.dodgeTime;
                        int low = Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.totalDodges;
                        int high = Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.totalDodges2;
                        if (timer == 0 && low == 0 && high == 0) recordNumbers = "No record!";
                        else recordNumbers = timer + " / " + low + " / " + high;
                    }

                    Rectangle posRect = new Rectangle(pageRect.X, pageRect.Y + 100 + (75 * i), 64, 64);
                    Rectangle cutRect = new Rectangle(66 * achX, 66 * achY, 64, 64);
                    spriteBatch.Draw(achievements, posRect, cutRect, new Color(255, 255, 255));

                    posRect = new Rectangle((int)pageRect.TopRight().X - 100, pageRect.Y + 100 + (75 * i), 64, 64);
                    cutRect = new Rectangle(66 * achX2, 66 * achY2, 64, 64);
                    spriteBatch.Draw(achievements, posRect, cutRect, new Color(255, 255, 255));
                    
                    Vector2 stringAdjust = Main.fontMouseText.MeasureString(recordType);
                    Vector2 pos = new Vector2(GetInnerDimensions().X + (GetInnerDimensions().Width / 2 - 45) - (stringAdjust.X / 3), GetInnerDimensions().Y + 110 + i * 75);
                    Utils.DrawBorderString(spriteBatch, recordType, pos, Color.Goldenrod);

                    stringAdjust = Main.fontMouseText.MeasureString(recordNumbers);
                    pos = new Vector2(GetInnerDimensions().X + (GetInnerDimensions().Width / 2 - 45) - (stringAdjust.X / 3), GetInnerDimensions().Y + 135 + i * 75);
                    Utils.DrawBorderString(spriteBatch, recordNumbers, pos, Color.White);
                }
            }

            if (Id == "PageTwo" && BossLogUI.PageNum >= 0 && BossLogUI.SubPageNum == 1)
            {
                // Spawn Item Subpage
            }

            if (Id == "PageTwo" && BossLogUI.PageNum >= 0 && BossLogUI.SubPageNum == 2)
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

            if (Id == "PageTwo" && BossLogUI.PageNum >= 0 && BossLogUI.SubPageNum == 3)
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
                
                spriteBatch.Draw(template, new Rectangle(pageRect.X + (pageRect.Width / 2) - (template.Width / 2) - 20, pageRect.Y + 84, template.Width, template.Height), new Color(255, 255, 255));
                
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
                            Rectangle posRect = new Rectangle(pageRect.X + (pageRect.Width / 2) - (mask.Width / 2) - 8, pageRect.Y + (pageRect.Height / 2) - (frameCut / 2) - 86, mask.Width, frameCut);
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
                                    Rectangle posRect = new Rectangle(pageRect.X + 98 + (offsetX * 16) - (backupX * 16), pageRect.Y + 126 + (offsetY * 16) - (backupY * 16), 16, 16);
                                    Rectangle cutRect = new Rectangle(offsetX * 18, offsetY * 18, 16, 16);

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
                                    Rectangle posRect = new Rectangle(pageRect.X + 98 + (offsetX * 16), pageRect.Y + 126 + (offsetY * 16), 16, 16);
                                    Rectangle cutRect = new Rectangle(offsetX * 18, offsetY * 18, 16, 16);

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
                                    Rectangle posRect = new Rectangle(pageRect.X + 210 + (offsetX * 16) - (backupX * 16), pageRect.Y + 158 + (offsetY * 16) - (backupY * 16), 16, 16);
                                    Rectangle cutRect = new Rectangle(offsetX * 18, (offsetY * 18) - 2, 16, 16);

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
                                    Rectangle posRect = new Rectangle(pageRect.X + 210 + (offsetX * 16), pageRect.Y + 158 + (offsetY * 16), 16, 16);
                                    Rectangle cutRect = new Rectangle(offsetX * 18, (offsetY * 18) - 2, 16, 16);

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
                                    Rectangle posRect = new Rectangle(pageRect.X + 98 + (offsetX * 16) - (backupX * 16), pageRect.Y + 126 + (offsetY * 16) - (backupY * 16), 18, 18);
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
                            Rectangle posRect = new Rectangle(pageRect.X + (pageRect.Width / 2) - (mask.Width / 2) - 8, pageRect.Y + (pageRect.Height / 2) - (frameCut / 2) - 86, mask.Width, frameCut);
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
                                Rectangle posRect = new Rectangle(pageRect.X + 210 + (offsetX * 16) - (backupX * 16), pageRect.Y + 158 + (offsetY * 16) - (backupY * 16), 18, 18);
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
            Main.playerInventory = false;
            
            if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface)
            {
                // Needed to remove mousetext from outside sources when using the Boss Log
                Main.player[Main.myPlayer].mouseInterface = true;
                Main.LocalPlayer.showItemIcon = false;
                Main.LocalPlayer.showItemIcon2 = -1;
                Main.ItemIconCacheUpdate(0);
                Main.mouseText = false;
                Item newItem = new Item();
                newItem.SetDefaults(ItemID.None);
                Main.HoverItem = newItem;
                Main.hoverItemName = "";
                Main.HoveringOverAnNPC = false;
                Main.LocalPlayer.talkNPC = -1;
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

                if (IsMouseHovering && !BA.setup.SortedBosses[Convert.ToInt32(Id)].downed() && nextCheck) TextColor = new Color(189, 180, 64);
                else if (!IsMouseHovering && !BA.setup.SortedBosses[Convert.ToInt32(Id)].downed() && nextCheck) TextColor = new Color(248, 235, 91);
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

        public UIImageButton NextPage;
        public UIImageButton PrevPage;
        public UIImageButton TOCPage;
        public UIImageButton CredPage;

        public UIList prehardmodeList;
        public UIList hardmodeList;
        public FixedUIScrollbar scrollOne;
        public FixedUIScrollbar scrollTwo;

        public UIList pageTwoItemList;
        public FixedUIScrollbar pageTwoScroll;

        public static int PageNum = 0; // Selected Boss Page
        public static int SubPageNum = 0; // Selected Topic Tab (Loot, Stats, etc.)
        public static bool visible = false;

        public override void OnInitialize()
        {            
            Texture2D bookTexture = BossAssist.instance.GetTexture("Resources/Button_BossLog");
            bosslogbutton = new BossAssistButton(bookTexture, "Boss Log");
            bosslogbutton.Width.Set(34, 0f);
            bosslogbutton.Height.Set(38, 0f);
            bosslogbutton.Left.Set(Main.screenWidth - bosslogbutton.Width.Pixels - 190, 0f);
            bosslogbutton.Top.Pixels = Main.screenHeight - bosslogbutton.Height.Pixels - 8;
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
            PrevPage = new BossAssistButton(prevTexture, "") { Id = "Previous" };
            PrevPage.Width.Pixels = 14;
            PrevPage.Height.Pixels = 20;
            PrevPage.Left.Pixels = 30;
            PrevPage.Top.Pixels = 410;
            PrevPage.OnClick += new MouseEvent(PageChangerClicked);
            PageOne.Append(PrevPage);

            Texture2D tocTexture = BossAssist.instance.GetTexture("Resources/ToC");
            TOCPage = new BossAssistButton(tocTexture, "") { Id = "TableOfContents" };
            TOCPage.Width.Pixels = 22;
            TOCPage.Height.Pixels = 22;
            TOCPage.Left.Pixels = 0;
            TOCPage.Top.Pixels = 410;
            TOCPage.OnClick += new MouseEvent(PageChangerClicked);
            PageOne.Append(TOCPage);

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

            pageTwoItemList = new UIList();

            pageTwoScroll = new FixedUIScrollbar();

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
            NextPage = new BossAssistButton(nextTexture, "") { Id = "Next" };
            NextPage.Width.Pixels = 14;
            NextPage.Height.Pixels = 20;
            NextPage.Left.Pixels = PageTwo.Width.Pixels - (int)(NextPage.Width.Pixels * 4.5);
            NextPage.Top.Pixels = 410;
            NextPage.OnClick += new MouseEvent(PageChangerClicked);
            PageTwo.Append(NextPage);

            Texture2D credTexture = BossAssist.instance.GetTexture("Resources/Credits");
            CredPage = new BossAssistButton(credTexture, "") { Id = "Credits" };
            CredPage.Width.Pixels = 22;
            CredPage.Height.Pixels = 22;
            CredPage.Left.Pixels = PageTwo.Width.Pixels - (int)(NextPage.Width.Pixels * 3);
            CredPage.Top.Pixels = 410;
            CredPage.OnClick += new MouseEvent(PageChangerClicked);
            PageTwo.Append(CredPage);

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
            }
            else Append(bosslogbutton);

            if (BossLogPanel.visible && BookUI.visible)
            {
                if (Main.LocalPlayer.controlInv)
                {
                    BossLogPanel.visible = false;
                    BookUI.visible = false;
                    Main.playerInventory = true;
                }
            }
            
            // We rewrite the position of the button to make sure it updates with the screen res
            bosslogbutton.Left.Pixels = Main.screenWidth - bosslogbutton.Width.Pixels - 190;
            bosslogbutton.Top.Pixels = Main.screenHeight - bosslogbutton.Height.Pixels - 8;
            bossLogPanel.Left.Pixels = (Main.screenWidth / 2) - (bossLogPanel.Width.Pixels / 2);
            bossLogPanel.Top.Pixels = (Main.screenHeight / 2) - (bossLogPanel.Height.Pixels / 2);

            if (PageNum == -1) // Checklist Pages
            {
                PageOne.RemoveAllChildren();
                PageOne.Append(TOCPage);
                PageOne.Append(scrollOne);
                PageOne.Append(prehardmodeList);
                prehardmodeList.SetScrollbar(scrollOne);
                PageTwo.RemoveAllChildren();
                PageTwo.Append(CredPage);
                PageTwo.Append(NextPage);
                PageTwo.Append(scrollTwo);
                PageTwo.Append(hardmodeList);
                hardmodeList.SetScrollbar(scrollTwo);
            }
            else if (PageNum == -2)
            {
                PageOne.RemoveAllChildren();
                PageOne.Append(TOCPage);

                PageTwo.RemoveAllChildren();
                PageTwo.Append(CredPage);
                // Setup Credits Page
            }
            else
            {
                if (PageOne.HasChild(prehardmodeList)) PageOne.RemoveChild(prehardmodeList);
                if (PageTwo.HasChild(hardmodeList)) PageTwo.RemoveChild(hardmodeList);
                if (PageOne.HasChild(scrollOne)) PageOne.RemoveChild(scrollOne);
                if (PageTwo.HasChild(scrollTwo)) PageTwo.RemoveChild(scrollTwo);
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
                else head = ModLoader.GetTexture(NPCLoader.GetNPC(BossAssist.instance.setup.SortedBosses[PageNum].id).BossHeadTexture);
                bossIcon.SetImage(head);
                
                if (PageNum == BossAssist.instance.setup.SortedBosses.FindIndex(x => x.id == NPCID.Retinazer)) PageOne.Append(spazHead);
                else PageOne.RemoveChild(spazHead);

                if (PageNum == BossAssist.instance.setup.SortedBosses.FindIndex(x => x.id == NPCID.DD2Betsy)) bossIcon.Left.Pixels = PageOne.Width.Pixels - bossIcon.Width.Pixels - 10;
                else bossIcon.Left.Pixels = PageOne.Width.Pixels - bossIcon.Width.Pixels - 15;
            }

            if (PageNum == -2) PageTwo.RemoveChild(NextPage);
            else PageTwo.Append(NextPage);
            if (PageNum == -1) PageOne.RemoveChild(PrevPage);
            else PageOne.Append(PrevPage);

            base.Update(gameTime);
        }

        private void OpenBossLog(UIMouseEvent evt, UIElement listeningElement)
        {
            if (BossLogPanel.visible && BookUI.visible)
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
            // Since it only applies to Boss Icons, the page check is unnecessary
            if (Main.LocalPlayer.name.Contains("Debugger"))
            {
                Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[PageNum].stat.fightTime = 0;
                Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[PageNum].stat.fightTime2 = 0;
                Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[PageNum].stat.dodgeTime = 0;
                Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[PageNum].stat.totalDodges = 0;
                Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[PageNum].stat.totalDodges2 = 0;
                Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[PageNum].stat.kills = 0;
                Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[PageNum].stat.deaths = 0;
                Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[PageNum].stat.brink = 0;
                Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[PageNum].stat.brinkPercent = 0;
                Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[PageNum].stat.brink2 = 0;
                Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[PageNum].stat.brinkPercent2 = 0;
            }
            OpenRecord(evt, listeningElement);
        }

        private void PageChangerClicked(UIMouseEvent evt, UIElement listeningElement)
        {
            if (listeningElement.Id == "Previous")
            {
                if (PageNum > -1) PageNum--;
                else if (PageNum == -2) PageNum = BossAssist.instance.setup.SortedBosses.Count - 1;
                if (PageNum == -1) UpdateTableofContents();
                else
                {
                    if (SubPageNum == 0) OpenRecord(evt, listeningElement);
                    else if (SubPageNum == 1) OpenSpawn(evt, listeningElement);
                    else if (SubPageNum == 2) OpenLoot(evt, listeningElement);
                    else if (SubPageNum == 3) OpenCollect(evt, listeningElement);
                }
            }
            else if (listeningElement.Id == "Next")
            {
                if (PageNum != BossAssist.instance.setup.SortedBosses.Count - 1) PageNum++;
                else PageNum = -2;

                if (PageNum != -2)
                {
                    if (SubPageNum == 0) OpenRecord(evt, listeningElement);
                    else if (SubPageNum == 1) OpenSpawn(evt, listeningElement);
                    else if (SubPageNum == 2) OpenLoot(evt, listeningElement);
                    else if (SubPageNum == 3) OpenCollect(evt, listeningElement);
                }
            }
            else if (listeningElement.Id == "TableOfContents")
            {
                if (PageNum != -1)
                {
                    PageNum = -1;
                    UpdateTableofContents();
                }
            }
            else if (listeningElement.Id == "Credits")
            {
                PageNum = -2;
                // UpdateCredits();
            }
        }

        private void OpenRecord(UIMouseEvent evt, UIElement listeningElement)
        {
            if (PageNum < 0) return;
            SubPageNum = 0;
            ResetPageTwo();
        }

        private void OpenSpawn(UIMouseEvent evt, UIElement listeningElement)
        {
            if (PageNum < 0) return;
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
            if (PageNum < 0) return;
            SubPageNum = 2;
            ResetPageTwo();
            int row = 0;
            int col = 0;
            
            pageTwoItemList.Left.Pixels = 0;
            pageTwoItemList.Top.Pixels = 125;
            pageTwoItemList.Width.Pixels = PageTwo.Width.Pixels - 25;
            pageTwoItemList.Height.Pixels = PageTwo.Height.Pixels - 125 - 80;

            pageTwoScroll.SetView(10f, 1000f);
            pageTwoScroll.Top.Pixels = 125;
            pageTwoScroll.Left.Pixels = -18;
            pageTwoScroll.Height.Set(-88f, 0.75f);
            pageTwoScroll.HAlign = 1f;

            pageTwoItemList.Clear();
            BossInfo shortcut = BossAssist.instance.setup.SortedBosses[PageNum];
            LootRow newRow = new LootRow(0) { Id = "Loot0" };
            for (int i = 0; i < shortcut.loot.Count; i++)
            {
                Item expertItem = new Item();
                expertItem.SetDefaults(shortcut.loot[i]);
                if (!expertItem.expert || expertItem.Name.Contains("Treasure Bag")) continue;
                else
                {
                    LogItemSlot lootTable = new LogItemSlot(expertItem, expertItem.Name, ItemSlot.Context.ShopItem);
                    lootTable.Height.Pixels = 50;
                    lootTable.Width.Pixels = 50;
                    lootTable.Left.Pixels = (col * 56);
                    newRow.Append(lootTable);
                    col++;
                    if (col == 6 || i == shortcut.loot.Count - 1)
                    {
                        col = 0;
                        row++;
                        pageTwoItemList.Add(newRow);
                        newRow = new LootRow(row) { Id = "Loot" + row };
                    }
                }
            }
            for (int i = 0; i < shortcut.loot.Count; i++)
            {
                Item loot = new Item();
                loot.SetDefaults(shortcut.loot[i]);

                if (loot.expert || loot.Name.Contains("Treasure Bag")) continue;
                else
                {
                    LogItemSlot lootTable = new LogItemSlot(loot, loot.Name, ItemSlot.Context.GuideItem);
                    lootTable.Height.Pixels = 50;
                    lootTable.Width.Pixels = 50;
                    lootTable.Left.Pixels = (col * 56);
                    newRow.Append(lootTable);
                    col++;
                    if (col == 6 || i == shortcut.loot.Count - 1)
                    {
                        col = 0;
                        row++;
                        pageTwoItemList.Add(newRow);
                        newRow = new LootRow(row) { Id = "Loot" + row };
                    }
                }
            }
            if (row > 4) PageTwo.Append(pageTwoScroll);
            PageTwo.Append(pageTwoItemList);
            pageTwoItemList.SetScrollbar(pageTwoScroll);
        }

        private void OpenCollect(UIMouseEvent evt, UIElement listeningElement)
        {
            if (PageNum < 0) return;
            SubPageNum = 3;
            ResetPageTwo();
            int row = 0;
            int col = 0;

            pageTwoItemList.Left.Pixels = 0;
            pageTwoItemList.Top.Pixels = 234;
            pageTwoItemList.Width.Pixels = PageTwo.Width.Pixels - 25;
            pageTwoItemList.Height.Pixels = PageTwo.Height.Pixels - 240 - 75;

            pageTwoScroll.SetView(10f, 1000f);
            pageTwoScroll.Top.Pixels = 250;
            pageTwoScroll.Left.Pixels = -18;
            pageTwoScroll.Height.Set(-220f, 0.75f);
            pageTwoScroll.HAlign = 1f;

            pageTwoItemList.Clear();
            LootRow newRow = new LootRow(0) { Id = "Collect0"};
            BossInfo shortcut = BossAssist.instance.setup.SortedBosses[PageNum];
            for (int i = 0; i < shortcut.collection.Count; i++)
            {
                Item collectible = new Item();
                collectible.SetDefaults(shortcut.collection[i]);

                LogItemSlot collectionTable = new LogItemSlot(collectible, collectible.Name, ItemSlot.Context.GuideItem);
                collectionTable.Height.Pixels = 50;
                collectionTable.Width.Pixels = 50;
                collectionTable.Id = "collect" + i;
                collectionTable.Left.Pixels = (56 * (col));
                newRow.Append(collectionTable);
                col++;
                if (col == 6 || i == shortcut.collection.Count - 1)
                {
                    col = 0;
                    row++;
                    pageTwoItemList.Add(newRow);
                    newRow = new LootRow(row) { Id = "Collect" + row };
                }
            }
            if (row > 3) PageTwo.Append(pageTwoScroll);
            PageTwo.Append(pageTwoItemList);
            pageTwoItemList.SetScrollbar(pageTwoScroll);
        }

        public void UpdateTableofContents()
        {
            if (PageNum != -1) return;
            int nextCheck = 0;
            bool nextCheckBool = false;
            prehardmodeList.Clear();
            hardmodeList.Clear();
            TableOfContents Header = new TableOfContents(-1f, "Pre-Hardmode", true, 0.6f, true)
            {
                Id = "Header1",
                PaddingTop = 12,
                PaddingLeft = 15,
                TextColor = Colors.RarityAmber
            };
            TableOfContents Header2 = new TableOfContents(-1f, "Hardmode", true, 0.6f, true)
            {
                Id = "Header2",
                PaddingTop = 12,
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
            PageNum = Convert.ToInt32(listeningElement.Id);
            if (SubPageNum == 0) OpenRecord(evt, listeningElement);
            else if (SubPageNum == 1) OpenSpawn(evt, listeningElement);
            else if (SubPageNum == 2) OpenLoot(evt, listeningElement);
            else if (SubPageNum == 3) OpenCollect(evt, listeningElement);
        }

        private void ResetPageTwo()
        {
            PageTwo.RemoveAllChildren();
            PageTwo.Append(CredPage);
            PageTwo.Append(spawnButton);
            PageTwo.Append(lootButton);
            PageTwo.Append(collectButton);
            PageTwo.Append(recordButton);
        }
    }
}