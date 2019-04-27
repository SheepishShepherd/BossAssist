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
            if (IsMouseHovering)
            {
                BossLogPanel.headNum = -1; // Fixes PageTwo head drawing when clicking on ToC boss and going back to ToC
                DynamicSpriteFontExtensionMethods.DrawString(spriteBatch, Main.fontMouseText, buttonType, pos, Color.White);
            }
            if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) Main.player[Main.myPlayer].mouseInterface = true;
        }
    }

    internal class LogItemSlot : UIElement
    {
        internal string hoverText;
        internal Item item;
        private readonly int context;
        private readonly float scale;

        public LogItemSlot(Item item, string hoverText = "", int context = ItemSlot.Context.TrashItem, float scale = 1f)
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
            var backup2 = Main.inventoryBack7Texture;

            if (Main.expertMode) Main.inventoryBack6Texture = Main.inventoryBack15Texture;
            else Main.inventoryBack6Texture = BossAssist.instance.GetTexture("Resources/ExpertOnly");

            BossCollection Collection = Main.LocalPlayer.GetModPlayer<PlayerAssist>().BossTrophies[BossLogUI.PageNum];

            if (Collection.itemList.FindIndex(x => x.type == item.type) != -1)
            {
                if (Id.Contains("collect") && Collection.checkList[Collection.itemList.FindIndex(x => x.type == item.type)])
                {
                    Main.inventoryBack7Texture = Main.inventoryBack3Texture;
                }
            }

            if (Collection.lootList.FindIndex(x => x.type == item.type) != -1)
            {
                if (Id.Contains("loot_") && Collection.lootCheck[Collection.lootList.FindIndex(x => x.type == item.type)])
                {
                    Main.inventoryBack7Texture = Main.inventoryBack3Texture;
                    Main.inventoryBack6Texture = BossAssist.instance.GetTexture("Resources/ExpertCollected");
                }
            }

            ItemSlot.Draw(spriteBatch, ref item, context, rectangle.TopLeft());

            Main.inventoryBack6Texture = backup;
            Main.inventoryBack7Texture = backup2;

            Texture2D checkMark = ModLoader.GetTexture("BossAssist/Resources/Checkbox_Check");
            if (Collection.itemList.FindIndex(x => x.type == item.type) != -1)
            {
                if (Id.Contains("collect") && Collection.checkList[Collection.itemList.FindIndex(x => x.type == item.type)])
                {
                    Rectangle rect = new Rectangle(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2, checkMark.Width, checkMark.Height);
                    spriteBatch.Draw(checkMark, rect, new Color(255, 255, 255));
                }
            }

            if (Main.LocalPlayer.GetModPlayer<PlayerAssist>().BossTrophies[BossLogUI.PageNum].lootList.FindIndex(x => x.type == item.type) != -1)
            {
                if (Id.Contains("loot_") && Collection.lootCheck[Collection.lootList.FindIndex(x => x.type == item.type)])
                {
                    Rectangle rect = new Rectangle(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2, checkMark.Width, checkMark.Height);
                    spriteBatch.Draw(checkMark, rect, new Color(255, 255, 255));
                }
            }

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
        public static int headNum = -1;

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!visible) return;
            base.Draw(spriteBatch);

            if (timerTrophy > 0) timerTrophy--;
            else timerTrophy = 480;

            Rectangle pageRect = GetInnerDimensions().ToRectangle();

            if (BossLogUI.PageNum == -1)
            {
                Vector2 pos = new Vector2(GetInnerDimensions().X + 19, GetInnerDimensions().Y + 15);
                if (Id == "PageOne") Utils.DrawBorderStringBig(spriteBatch, "Pre-Hardmode", pos, Colors.RarityAmber, 0.6f);
                else if (Id == "PageTwo") Utils.DrawBorderStringBig(spriteBatch, "Hardmode", pos, Colors.RarityAmber, 0.6f);

                if (!IsMouseHovering) headNum = -1;

                if (headNum != -1)
                {
                    Texture2D head = BossLogUI.GetBossHead(headNum);
                    spriteBatch.Draw(head, new Rectangle(Main.mouseX + 15, Main.mouseY + 15, head.Width, head.Height), new Color(255, 255, 255));
                    if (headNum == BossAssist.instance.setup.SortedBosses.FindIndex(x => x.id == NPCID.Retinazer))
                    {
                        Rectangle rect = new Rectangle(Main.mouseX + head.Width + 15, Main.mouseY + 15, head.Width, head.Height);
                        spriteBatch.Draw(ModLoader.GetTexture("Terraria/NPC_Head_Boss_16"), rect, new Color(255, 255, 255));
                    }
                }
            }

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
                Utils.DrawBorderString(spriteBatch, "direwolf420 - Boss Radar Code", pos, Color.Goldenrod);

                pos = new Vector2(GetInnerDimensions().X + 15, GetInnerDimensions().Y + 65);
                Utils.DrawBorderString(spriteBatch, "Orian34 - Beta Testing", pos, new Color(49, 210, 162));

                pos = new Vector2(GetInnerDimensions().X + 15, GetInnerDimensions().Y + 95);
                Utils.DrawBorderString(spriteBatch, "Panini - Multiplayer/Server Testing", pos, Color.LightPink);

                pos = new Vector2(GetInnerDimensions().X + 15, GetInnerDimensions().Y + 125);
                Utils.DrawBorderString(spriteBatch, "RiverOaken - Boss Log UI Sprite", pos, Color.YellowGreen);

                pos = new Vector2(GetInnerDimensions().X + 15, GetInnerDimensions().Y + 155);
                Utils.DrawBorderString(spriteBatch, "Corinna - Boss Placeholder Sprite", pos, Color.MediumPurple);
                
                pos = new Vector2(GetInnerDimensions().X + 5, GetInnerDimensions().Y + 270);
                Utils.DrawBorderString(spriteBatch, "To add your own bosses to the boss log, \nfollow the instructions on the homepage.\nAdvise other modders to do the same. \nThe more this mod expands the better!!", pos, Color.LightCoral);
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
                    Utils.DrawBorderString(spriteBatch, "Thanks to all the mods who opted in!*", pos, Color.LightSkyBlue); adjustment += 35;

                    pos = new Vector2(GetInnerDimensions().X + 5, GetInnerDimensions().Y + adjustment);
                    Utils.DrawBorderString(spriteBatch, "*The list only contains loaded mods", pos, Color.LightBlue);
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

                for (int i = 0; i < 4; i++) // 4 Records total
                {
                    if (i == 0)
                    {
                        recordType = "Kill Death Ratio";

                        int killTimes = Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.kills;
                        int deathTimes = Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.deaths;

                        if (killTimes >= deathTimes)
                        {
                            achX = 4;
                            achY = 10;
                        }
                        else
                        {
                            achX = 4;
                            achY = 8;
                        }
                        
                        if (killTimes == 0 && deathTimes == 0) recordNumbers = "Unchallenged!";
                        else recordNumbers = killTimes + " kills / " + deathTimes + " deaths";
                    }
                    else if (i == 1)
                    {
                        recordType = "Quickest Victory";
                        string finalResult = "";

                        int BestRecord = Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.fightTime;
                        int WorstRecord = Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.fightTime2;
                        int LastRecord = Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.fightTimeL;

                        if (!BossLogUI.AltRecords)
                        {
                            achX = 4;
                            achY = 9;

                            if (LastRecord == BestRecord && LastRecord != -1)
                            {
                                Texture2D text = ModLoader.GetTexture("Terraria/UI/UI_quickicon1");
                                Rectangle exclam = new Rectangle((int)GetInnerDimensions().X + 232, (int)GetInnerDimensions().Y + 180, text.Width, text.Height);
                                spriteBatch.Draw(text, exclam, new Color(255, 255, 255));
                            }

                            if (BestRecord != 0)
                            {
                                double recordOrg = (double)BestRecord / 60;
                                int recordMin = (int)recordOrg / 60;
                                int recordSec = (int)recordOrg % 60;

                                double record2 = (double)LastRecord / 60;
                                int recordMin2 = (int)record2 / 60;
                                int recordSec2 = (int)record2 % 60;

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

                                if (LastRecord > BestRecord)
                                {
                                    if (recordMin2 > 0) finalResult += " [" + recordMin2 + "m " + recSec2 + "s]";
                                    else finalResult += " [" + rec2 + "s]";
                                }

                                recordNumbers = finalResult;
                            }
                            else recordNumbers = "No record!";
                        }
                        else
                        {
                            achX = 7;
                            achY = 5;

                            if (LastRecord == WorstRecord && LastRecord != -1)
                            {
                                Texture2D text = ModLoader.GetTexture("Terraria/UI/UI_quickicon1");
                                Rectangle exclam = new Rectangle((int)GetInnerDimensions().X + 232, (int)GetInnerDimensions().Y + 180, text.Width, text.Height);
                                spriteBatch.Draw(text, exclam, new Color(255, 255, 255));
                            }

                            if (WorstRecord != 0)
                            {
                                double recordOrg = (double)WorstRecord / 60;
                                int recordMin = (int)recordOrg / 60;
                                int recordSec = (int)recordOrg % 60;

                                double record2 = (double)LastRecord / 60;
                                int recordMin2 = (int)record2 / 60;
                                int recordSec2 = (int)record2 % 60;

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
                                
                                if (LastRecord != -1 && LastRecord != WorstRecord)
                                {
                                    if (recordMin2 > 0) finalResult += " [" + recordMin2 + "m " + recSec2 + "s]";
                                    else finalResult += " [" + rec2 + "s]";
                                }

                                recordNumbers = finalResult;
                            }
                            else recordNumbers = "No record!";
                        }
                    }
                    else if (i == 2)
                    {
                        recordType = "Vitality";

                        int BestRecord = Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.brink2;
                        int BestPercent = Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.brinkPercent2;
                        int WorstRecord = Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.brink;
                        int WorstPercent = Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.brinkPercent;
                        int LastRecord = Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.brinkL;
                        int LastPercent = Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.brinkPercentL;

                        if (!BossLogUI.AltRecords)
                        {
                            achX = 3;
                            achY = 0;
                            if (LastRecord == BestRecord && LastRecord != -1)
                            {
                                Texture2D text = ModLoader.GetTexture("Terraria/UI/UI_quickicon1");
                                Rectangle exclam = new Rectangle((int)GetInnerDimensions().X + 182, (int)GetInnerDimensions().Y + 255, text.Width, text.Height);
                                spriteBatch.Draw(text, exclam, new Color(255, 255, 255));
                            }

                            string finalResult = "";
                            if (BestRecord != 0)
                            {
                                finalResult += BestRecord + " (" + BestPercent + "%)";
                                if (LastRecord != BestRecord && LastRecord != -1)
                                {
                                    finalResult += " [" + LastRecord + " (" + LastPercent + "%)]";
                                }
                                recordNumbers = finalResult;
                            }
                            else recordNumbers = "No record!";
                        }
                        else
                        {
                            achX = 6;
                            achY = 7;

                            if (LastRecord == WorstRecord && LastRecord != -1)
                            {
                                Texture2D text = ModLoader.GetTexture("Terraria/UI/UI_quickicon1");
                                Rectangle exclam = new Rectangle((int)GetInnerDimensions().X + 182, (int)GetInnerDimensions().Y + 255, text.Width, text.Height);
                                spriteBatch.Draw(text, exclam, new Color(255, 255, 255));
                            }

                            string finalResult = "";
                            if (WorstRecord != 0)
                            {
                                finalResult += WorstRecord + " (" + WorstPercent + "%)";
                                if (LastRecord != WorstRecord && LastRecord != -1)
                                {
                                    finalResult += " [" + LastRecord + " (" + LastPercent + "%)]";
                                }
                                recordNumbers = finalResult;
                            }
                            else recordNumbers = "No record!";
                        }
                    }
                    else if (i == 3)
                    {
                        recordType = "Artful Dodging";

                        int timer = Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.dodgeTime;
                        int low = Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.totalDodges;
                        int high = Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.totalDodges2;
                        int last = Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.totalDodgesL;

                        double timer2 = (double)Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[BossLogUI.PageNum].stat.dodgeTime / 60;
                        string timerOutput = timer2.ToString("0.##");

                        if (!BossLogUI.AltRecords)
                        {
                            achX = 0;
                            achY = 7;

                            if (last == low && last != -1)
                            {
                                Texture2D text = ModLoader.GetTexture("Terraria/UI/UI_quickicon1");
                                Rectangle exclam = new Rectangle((int)GetInnerDimensions().X + 225, (int)GetInnerDimensions().Y + 332, text.Width, text.Height);
                                spriteBatch.Draw(text, exclam, new Color(255, 255, 255));
                            }

                            if (timer == 0 && low == 0 && high == 0) recordNumbers = "No record!";
                            else if (low != last && last != -1) recordNumbers = low + " (" + timerOutput + "s)" + " [" + last + "]";
                            else recordNumbers = low + " (" + timerOutput + "s)";
                        }
                        else
                        {
                            achX = 4;
                            achY = 2;

                            if (last == high && last != -1)
                            {
                                Texture2D text = ModLoader.GetTexture("Terraria/UI/UI_quickicon1");
                                Rectangle exclam = new Rectangle((int)GetInnerDimensions().X + 225, (int)GetInnerDimensions().Y + 332, text.Width, text.Height);
                                spriteBatch.Draw(text, exclam, new Color(255, 255, 255));
                            }

                            if (high == 0) recordNumbers = "No record!";
                            else if (high != last && last != -1) recordNumbers = high + " (" + timerOutput + "s)" + " [" + last + "]";
                            else recordNumbers = high + " (" + timerOutput + "s)";
                        }
                    }

                    Rectangle posRect = new Rectangle(pageRect.X, pageRect.Y + 100 + (75 * i), 64, 64);
                    Rectangle cutRect = new Rectangle(66 * achX, 66 * achY, 64, 64);
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

                // Draw Mask
                if (Collections.itemList.FindIndex(x => x.Name.Contains("Mask") && x.vanity) != -1)
                {
                    if (Collections.checkList[Collections.itemList.FindIndex(x => x.Name.Contains("Mask") && x.vanity)])
                    {
                        Texture2D mask;
                        if (BossPage.collection[0] < ItemID.Count)
                        {
                            Item newItem = new Item();
                            if (BossPage.id == NPCID.Retinazer) newItem.SetDefaults(ItemID.TwinMask);
                            else newItem.SetDefaults(BossPage.collection[1]);
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
                if (BossPage.id == NPCID.Retinazer)
                {
                    // Draw both Twins trophies
                    if (Collections.itemList.FindIndex(x => x.type == ItemID.RetinazerTrophy) != -1 && Collections.itemList.FindIndex(x => x.type == ItemID.SpazmatismTrophy) != -1)
                    {
                        if (Collections.checkList[Collections.itemList.FindIndex(x => x.type == ItemID.RetinazerTrophy)] || Collections.checkList[Collections.itemList.FindIndex(x => x.type == ItemID.SpazmatismTrophy)])
                        {
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
                }
                else
                {
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
                Main.mouseText = true;

                // Item icons such as hovering over a bed will not appear
                Main.LocalPlayer.showItemIcon = false;
                Main.LocalPlayer.showItemIcon2 = -1;
                Main.ItemIconCacheUpdate(0);
            }
        }
    }

    internal class TableOfContents : UIText
    {
        float order = 0;
        bool nextCheck;
        string text;

        public TableOfContents(float order, string text, bool nextCheck, float textScale = 1, bool large = false) : base(text, textScale, large)
        {
            this.order = order;
            this.nextCheck = nextCheck;
            Recalculate();
            this.text = text;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            //TODO: fix up the collection star marker

            Texture2D progressBox = BossAssist.instance.GetTexture("Resources/Checkbox_Empty");
            CalculatedStyle innerDimensions = GetInnerDimensions();
            Vector2 pos = new Vector2(innerDimensions.X - 20, innerDimensions.Y - 5);
            spriteBatch.Draw(progressBox, pos, Color.White);
            Vector2 pos2 = new Vector2(innerDimensions.X + Main.fontMouseText.MeasureString(text).X + 6, innerDimensions.Y - 2);
            int index = BossAssist.instance.setup.SortedBosses.FindIndex(x => x.progression == order);
            if (Main.LocalPlayer.GetModPlayer<PlayerAssist>().BossTrophies[index].lootCheck.All(x => x == true))
            {
                spriteBatch.Draw(BossAssist.instance.GetTexture("Resources/CheckBox_Chest"), pos2, Color.White);
                pos2 = new Vector2(innerDimensions.X + Main.fontMouseText.MeasureString(text).X + 32, innerDimensions.Y - 2);
            }
            if (Main.LocalPlayer.GetModPlayer<PlayerAssist>().BossTrophies[index].checkList.All(x => x == true))
            {
                spriteBatch.Draw(BossAssist.instance.GetTexture("Resources/CheckBox_Star"), pos2, Color.White);
            }
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

                if (IsMouseHovering) BossLogPanel.headNum = Convert.ToInt32(Id);
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
            if (buttonString != "Disclaimer" && buttonString != "recordAlts")
            {
                DynamicSpriteFontExtensionMethods.DrawString(spriteBatch, Main.fontMouseText, buttonString, pos, Color.Gold);
            }
            
            Texture2D text = ModLoader.GetTexture("Terraria/UI/Achievement_Categories");
            Rectangle exclamPos = new Rectangle((int)GetInnerDimensions().X - 12, (int)GetInnerDimensions().Y - 12, 32, 32);

            if (buttonString == "")
            {
                if (BossLogUI.SubPageNum == 0)
                {
                    if (!BossLogUI.AltRecords)
                    {
                        Rectangle exclamCut = new Rectangle(34 * 3, 0, 32, 32);
                        spriteBatch.Draw(text, exclamPos, exclamCut, new Color(255, 255, 255));
                        if (IsMouseHovering) Main.hoverItemName = "Click to see your 'Worst' records" +
                                                                "\nRecords are shown as your best compared to your last fight" +
                                                                "\n[c/daa520:Quickest Victory:] Fastest time to kill a boss" +
                                                                "\n[c/daa520:Vitality:] Highest health drop youve been in a fight" +
                                                                "\n[c/daa520:Artful Dodging:] Least amount of hits taken and most time between hits";
                    }
                    else
                    {
                        Rectangle exclamCut = new Rectangle(0, 0, 32, 32);
                        spriteBatch.Draw(text, exclamPos, exclamCut, new Color(255, 255, 255));
                        if (IsMouseHovering) Main.hoverItemName = "Click to see your 'Best' records" +
                                                                "\nRecords are shown as your worst compared to your last fight" +
                                                                "\n[c/daa520:Quickest Victory:] Longest time to kill a boss" +
                                                                "\n[c/daa520:Vitality:] Lowest health you have had in a fight" +
                                                                "\n[c/daa520:Artful Dodging:] Most amount of hits taken and most time between hits";
                    }
                }
                else if (BossLogUI.SubPageNum == 2)
                {
                    Rectangle exclamCut = new Rectangle(34 * 1, 0, 32, 32);
                    spriteBatch.Draw(text, exclamPos, exclamCut, new Color(255, 255, 255));
                    if (IsMouseHovering) Main.hoverItemName = "DISCLAIMER:" +
                                                            "\nLoot Tables only show the original drops provided" +
                                                            "\nand does not include drops added by outside mods!";
                }
                else if (BossLogUI.SubPageNum == 3)
                {
                    Rectangle exclamCut = new Rectangle(34 * 1, 0, 32, 32);
                    spriteBatch.Draw(text, exclamPos, exclamCut, new Color(255, 255, 255));
                    if (IsMouseHovering) Main.hoverItemName = "DISCLAIMER:" +
                                                            "\nCollections only show the original collectibles provided" +
                                                            "\nand does not include collectibles added by outside mods!";
                }
            }
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
        public SubpageButton toolTipButton;

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

        public UIList pageTwoItemList; // Item slot lists that include: Loot tables, spawn item, and collectibles
        public FixedUIScrollbar pageTwoScroll;
        
        public static int PageNum = 0; // Selected Boss Page
        public static int SubPageNum = 0; // Selected Topic Tab (Loot, Stats, etc.)
        public static bool AltRecords = false; // Flip between best and worst
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

            Texture2D tocTexture = BossAssist.instance.GetTexture("Resources/ToC");
            TOCPage = new BossAssistButton(tocTexture, "") { Id = "TableOfContents" };
            TOCPage.Width.Pixels = 22;
            TOCPage.Height.Pixels = 22;
            TOCPage.Left.Pixels = 0;
            TOCPage.Top.Pixels = 410;
            TOCPage.OnClick += new MouseEvent(PageChangerClicked);

            prehardmodeList = new UIList();
            prehardmodeList.Left.Pixels = 4;
            prehardmodeList.Top.Pixels = 44;
            prehardmodeList.Width.Pixels = PageOne.Width.Pixels - 60;
            prehardmodeList.Height.Pixels = PageOne.Height.Pixels - 136;
            prehardmodeList.PaddingTop = 5;

            scrollOne = new FixedUIScrollbar();
            scrollOne.SetView(100f, 1000f);
            scrollOne.Top.Pixels = 50f;
            scrollOne.Left.Pixels = -18;
            scrollOne.Height.Set(-24f, 0.75f);
            scrollOne.HAlign = 1f;

            scrollTwo = new FixedUIScrollbar();
            scrollTwo.SetView(100f, 1000f);
            scrollTwo.Top.Pixels = 50f;
            scrollTwo.Left.Pixels = -28;
            scrollTwo.Height.Set(-24f, 0.75f);
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

            Texture2D credTexture = BossAssist.instance.GetTexture("Resources/Credits");
            CredPage = new BossAssistButton(credTexture, "") { Id = "Credits" };
            CredPage.Width.Pixels = 22;
            CredPage.Height.Pixels = 22;
            CredPage.Left.Pixels = PageTwo.Width.Pixels - (int)(NextPage.Width.Pixels * 3);
            CredPage.Top.Pixels = 410;
            CredPage.OnClick += new MouseEvent(PageChangerClicked);

            hardmodeList = new UIList();
            hardmodeList.Left.Pixels = 4;
            hardmodeList.Top.Pixels = 44;
            hardmodeList.Width.Pixels = PageOne.Width.Pixels - 60;
            hardmodeList.Height.Pixels = PageOne.Height.Pixels - 136;
            hardmodeList.PaddingTop = 5;
            
            recordButton = new SubpageButton("Boss Records");
            recordButton.Width.Pixels = PageTwo.Width.Pixels / 2 - 24;
            recordButton.Height.Pixels = 25;
            recordButton.Left.Pixels = 0;
            recordButton.Top.Pixels = 15;
            recordButton.OnClick += new MouseEvent(OpenRecord);

            spawnButton = new SubpageButton("Spawn Item");
            spawnButton.Width.Pixels = PageTwo.Width.Pixels / 2 - 24;
            spawnButton.Height.Pixels = 25;
            spawnButton.Left.Pixels = PageTwo.Width.Pixels / 2 - 8;
            spawnButton.Top.Pixels = 15;
            spawnButton.OnClick += new MouseEvent(OpenSpawn);

            lootButton = new SubpageButton("Loot Table");
            lootButton.Width.Pixels = PageTwo.Width.Pixels / 2 - 24;
            lootButton.Height.Pixels = 25;
            lootButton.Left.Pixels = 0;
            lootButton.Top.Pixels = 50;
            lootButton.OnClick += new MouseEvent(OpenLoot);

            collectButton = new SubpageButton("Collectibles");
            collectButton.Width.Pixels = PageTwo.Width.Pixels / 2 - 24;
            collectButton.Height.Pixels = 25;
            collectButton.Left.Pixels = PageTwo.Width.Pixels / 2 - 8;
            collectButton.Top.Pixels = 50;
            collectButton.OnClick += new MouseEvent(OpenCollect);

            toolTipButton = new SubpageButton("Disclaimer");
            toolTipButton.Width.Pixels = 32;
            toolTipButton.Height.Pixels = 32;
            toolTipButton.Left.Pixels = PageTwo.Width.Pixels - toolTipButton.Width.Pixels - 30;
            toolTipButton.Top.Pixels = 100;
            toolTipButton.OnClick += new MouseEvent(SwapRecordPage);

            bossLogPanel.Append(PageTwo);

            Append(bossLogPanel);
        }

        public override void Update(GameTime gameTime)
        {
            visible = Main.playerInventory;
            if (!visible) RemoveChild(bosslogbutton);
            else Append(bosslogbutton);

            if (BossAssist.ToggleBossLog.JustPressed)
            {
                if (!BookUI.visible)
                {
                    PageNum = -1;
                    SubPageNum = 0;
                    BossLogPanel.visible = true;
                    bossLogPanel.Append(PageOne);
                    bossLogPanel.Append(PageTwo);
                    BookUI.visible = true;
                    UpdateTableofContents();
                }
                else
                {
                    BossLogPanel.visible = false;
                    RemoveChild(PageOne);
                    RemoveChild(PageTwo);
                    BookUI.visible = false;
                }
            }

            if (BossLogPanel.visible && BookUI.visible)
            {
                if (Main.LocalPlayer.controlInv || Main.mouseItem.type != 0)
                {
                    BossLogPanel.visible = false;
                    RemoveChild(PageOne);
                    RemoveChild(PageTwo);
                    BookUI.visible = false;
                    Main.playerInventory = true;
                }
            }

            if (PageNum >= 0 && SubPageNum != 1 && !PageTwo.HasChild(toolTipButton))
            {
                toolTipButton = new SubpageButton("");
                toolTipButton.Width.Pixels = 32;
                toolTipButton.Height.Pixels = 32;
                toolTipButton.Left.Pixels = PageTwo.Width.Pixels - toolTipButton.Width.Pixels - 30;
                toolTipButton.Top.Pixels = 86;
                toolTipButton.OnClick += new MouseEvent(SwapRecordPage);
                PageTwo.Append(toolTipButton);
            }

            // We rewrite the position of the button to make sure it updates with the screen res
            bosslogbutton.Left.Pixels = Main.screenWidth - bosslogbutton.Width.Pixels - 190;
            bosslogbutton.Top.Pixels = Main.screenHeight - bosslogbutton.Height.Pixels - 8;
            bossLogPanel.Left.Pixels = (Main.screenWidth / 2) - (bossLogPanel.Width.Pixels / 2);
            bossLogPanel.Top.Pixels = (Main.screenHeight / 2) - (bossLogPanel.Height.Pixels / 2);

            if (PageNum >= 0)
            {
                PageOne.Append(bossIcon);
                bossIcon.SetImage(GetBossHead(PageNum));

                if (PageNum == BossAssist.instance.setup.SortedBosses.FindIndex(x => x.id == NPCID.Retinazer)) PageOne.Append(spazHead);
                else PageOne.RemoveChild(spazHead);

                if (PageNum == BossAssist.instance.setup.SortedBosses.FindIndex(x => x.id == NPCID.DD2Betsy)) bossIcon.Left.Pixels = PageOne.Width.Pixels - bossIcon.Width.Pixels - 10;
                else bossIcon.Left.Pixels = PageOne.Width.Pixels - bossIcon.Width.Pixels - 15;
            }
            else
            {
                bossIcon.Left.Pixels = PageOne.Width.Pixels - bossIcon.Width.Pixels - 15;
                PageOne.RemoveChild(bossIcon);
                PageOne.RemoveChild(spazHead);
            }

            if (PageNum == -2) PageTwo.RemoveChild(NextPage);
            else if (!PageTwo.HasChild(NextPage)) PageTwo.Append(NextPage);
            if (PageNum == -1) PageOne.RemoveChild(PrevPage);
            else if (!PageOne.HasChild(PrevPage)) PageOne.Append(PrevPage);
            if (!PageOne.HasChild(TOCPage)) PageOne.Append(TOCPage);

            base.Update(gameTime);
        }

        private void OpenBossLog(UIMouseEvent evt, UIElement listeningElement)
        {
            PageNum = -1;
            SubPageNum = 0;
            BossLogPanel.visible = true;
            bossLogPanel.Append(PageOne);
            bossLogPanel.Append(PageTwo);
            BookUI.visible = true;
            UpdateTableofContents();
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
            BossLogPanel.visible = true;
            bossLogPanel.Append(PageOne);
            bossLogPanel.Append(PageTwo);
            BookUI.visible = true;
            if (PageNum != -1)
            {
                SubPageNum = 1;
                PageOne.RemoveChild(prehardmodeList);
                OpenSpawn(evt, listeningElement);
            }
        }

        private void ResetStats(UIMouseEvent evt, UIElement listeningElement)
        {
            // Since it only applies to Boss Icons, the page check is unnecessary
            if (true)
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
            pageTwoItemList.Clear();
            prehardmodeList.Clear();
            hardmodeList.Clear();
            PageOne.RemoveChild(scrollOne);
            PageTwo.RemoveChild(scrollTwo);
            PageTwo.RemoveChild(pageTwoScroll);
            
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
                else UpdateCredits();

                if (PageNum != -2)
                {
                    if (SubPageNum == 0) OpenRecord(evt, listeningElement);
                    else if (SubPageNum == 1) OpenSpawn(evt, listeningElement);
                    else if (SubPageNum == 2) OpenLoot(evt, listeningElement);
                    else if (SubPageNum == 3) OpenCollect(evt, listeningElement);
                }
            }
            else if (listeningElement.Id == "TableOfContents") UpdateTableofContents();
            else if (listeningElement.Id == "Credits") UpdateCredits();
        }

        private void OpenRecord(UIMouseEvent evt, UIElement listeningElement)
        {
            ResetPageTwo();
            if (PageNum < 0) return;
            SubPageNum = 0;
        }

        private void OpenSpawn(UIMouseEvent evt, UIElement listeningElement)
        {
            ResetPageTwo();
            if (PageNum < 0) return;
            SubPageNum = 1;
            Mod thorium = ModLoader.GetMod("ThoriumMod");
            if (thorium != null)
            {
                BossInfo spot = BossAssist.instance.setup.SortedBosses.Find(x => x.name == "Plantera");
                spot.spawnItem = thorium.ItemType("PlantBulb");
            }
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
                    LogItemSlot ingList = new LogItemSlot(ingredients[k], ingredients[k].HoverName, ItemSlot.Context.GuideItem);
                    ingList.Height.Pixels = 50;
                    ingList.Width.Pixels = 50;
                    ingList.Top.Pixels = 105 + (56 * (row + 1));
                    ingList.Left.Pixels = (56 * col);
                    PageTwo.Append(ingList);
                    col++;
                    if (k == 4 || k == 9)
                    {
                        if (ingList.item.type == 0) break;
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
            ResetPageTwo();
            if (PageNum < 0) return;
            SubPageNum = 2;
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
                    lootTable.Id = "loot_" + i;
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
                    LogItemSlot lootTable = new LogItemSlot(loot, loot.Name, ItemSlot.Context.TrashItem);
                    lootTable.Height.Pixels = 50;
                    lootTable.Width.Pixels = 50;
                    lootTable.Id = "loot_" + i;
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
            ResetPageTwo();
            if (PageNum < 0) return;
            SubPageNum = 3;
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

                LogItemSlot collectionTable = new LogItemSlot(collectible, collectible.Name);
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
            PageNum = -1;
            ResetPageTwo();
            int nextCheck = 0;
            bool nextCheckBool = false;
            prehardmodeList.Clear();
            hardmodeList.Clear();
            
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

            if (prehardmodeList.Count > 14) PageOne.Append(scrollOne);
            PageOne.Append(prehardmodeList);
            prehardmodeList.SetScrollbar(scrollOne);
            if (hardmodeList.Count > 14) PageTwo.Append(scrollTwo);
            PageTwo.Append(hardmodeList);
            hardmodeList.SetScrollbar(scrollTwo);
        }

        private void UpdateCredits()
        {
            PageNum = -2;
            ResetPageTwo();
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

            pageTwoItemList.Left.Pixels = 15;
            pageTwoItemList.Top.Pixels = 75;
            pageTwoItemList.Width.Pixels = PageTwo.Width.Pixels - 66;
            pageTwoItemList.Height.Pixels = PageTwo.Height.Pixels - 75 - 80;

            pageTwoScroll.SetView(10f, 1000f);
            pageTwoScroll.Top.Pixels = 90;
            pageTwoScroll.Left.Pixels = -24;
            pageTwoScroll.Height.Set(-60f, 0.75f);
            pageTwoScroll.HAlign = 1f;

            pageTwoItemList.Clear();

            if (optedMods.Count != 0)
            {
                foreach (string mod in optedMods)
                {
                    UIText modListed = new UIText("●" + mod)
                    {
                        PaddingTop = 8,
                        PaddingLeft = 5
                    };
                    pageTwoItemList.Add(modListed);
                }
            }
            else // No mods are using the Log
            {
                pageTwoItemList.Left.Pixels = 0;
                pageTwoItemList.Top.Pixels = 15;
                pageTwoItemList.Width.Pixels = PageTwo.Width.Pixels;
                pageTwoItemList.Height.Pixels = PageTwo.Height.Pixels - 75 - 80;
                
                string noMods = "None of your loaded mods have added \npages to the Boss Log. If you want your \nfavorite mods to be included, suggest \nadding their own boss pages to the mod's \ndiscord or forums page!";
                UIText noModListed = new UIText(noMods)
                {
                    TextColor = Color.LightBlue,
                    PaddingTop = 8,
                    PaddingLeft = 5
                };
                pageTwoItemList.Add(noModListed);
            }
            if (optedMods.Count > 11) PageTwo.Append(pageTwoScroll);
            PageTwo.Append(pageTwoItemList);
            pageTwoItemList.SetScrollbar(pageTwoScroll);
        }

        private void UpdatePage(UIMouseEvent evt, UIElement listeningElement)
        {
            PageNum = Convert.ToInt32(listeningElement.Id);
            PageOne.RemoveAllChildren();
            PageOne.Append(TOCPage);
            if (SubPageNum == 0) OpenRecord(evt, listeningElement);
            else if (SubPageNum == 1) OpenSpawn(evt, listeningElement);
            else if (SubPageNum == 2) OpenLoot(evt, listeningElement);
            else if (SubPageNum == 3) OpenCollect(evt, listeningElement);
        }

        private void ResetPageTwo()
        {
            PageTwo.RemoveAllChildren();
            PageTwo.Append(CredPage);
            if (PageNum >= 0)
            {
                PageTwo.Append(spawnButton);
                PageTwo.Append(lootButton);
                PageTwo.Append(collectButton);
                PageTwo.Append(recordButton);
            }
        }

        private void SwapRecordPage(UIMouseEvent evt, UIElement listeningElement)
        {
            if (SubPageNum == 0) AltRecords = !AltRecords;
        }

        public static Texture2D GetBossHead(int page)
        {
            Main.instance.LoadNPC(BossAssist.instance.setup.SortedBosses[page].id);
            Texture2D head;
            if (BossAssist.instance.setup.SortedBosses[page].id < NPCID.Count)
            {
                int type = BossAssist.instance.setup.SortedBosses[page].id;
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
            else head = ModLoader.GetTexture(NPCLoader.GetNPC(BossAssist.instance.setup.SortedBosses[page].id).BossHeadTexture);

            return head;
        }
    }
}