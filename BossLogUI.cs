﻿using Microsoft.Xna.Framework;
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
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

/* Patch Notes:
 *   + Added hidden mask feature. Bosses dont show what they look like until defeated
 *   + Upgraded the spawn item tab to contain multiple items and all their recipes (You do not have to change your call, it still works with a singular int)
 *   + Added the ability to display records in chat (Hopefully along with Multiplayer support)
 */

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
				if (!Id.Contains("CycleItem")) DynamicSpriteFontExtensionMethods.DrawString(spriteBatch, Main.fontMouseText, buttonType, pos, Color.White);
				else
				{
					pos = new Vector2(innerDimensions.X - stringAdjust.X + 20, innerDimensions.Y + 36);
					Utils.DrawBorderString(spriteBatch, buttonType, pos, Color.White);
				}
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
		internal bool hasItem;

        public LogItemSlot(Item item, bool hasItem, string hoverText = "", int context = ItemSlot.Context.TrashItem, float scale = 1f)
        {
            this.context = context;
            this.scale = scale;
            this.item = item;
            this.hoverText = hoverText;
			this.hasItem = hasItem;

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
			
			if (Id.Contains("loot_") && hasItem)
            {
                Main.inventoryBack7Texture = Main.inventoryBack3Texture;
				Main.inventoryBack6Texture = BossAssist.instance.GetTexture("Resources/ExpertCollected");
			}
			
            if (Id.Contains("collect_") && hasItem)
            {
                Main.inventoryBack7Texture = Main.inventoryBack3Texture;
            }

			// Prevents empty collectible slots from being drawn
			if (!Id.Contains("collect_") || item.type != 0)
			{
				ItemSlot.Draw(spriteBatch, ref item, context, rectangle.TopLeft());
			}

            Main.inventoryBack6Texture = backup;
            Main.inventoryBack7Texture = backup2;

            Texture2D checkMark = ModContent.GetTexture("BossAssist/Resources/Checkbox_Check");
            if (Id.Contains("loot_") && hasItem)
            {
                Rectangle rect = new Rectangle(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2, checkMark.Width, checkMark.Height);
                spriteBatch.Draw(checkMark, rect, Color.White);
            }
			
            if (Id.Contains("collect_") && hasItem)
            {
                Rectangle rect = new Rectangle(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2, checkMark.Width, checkMark.Height);
                spriteBatch.Draw(checkMark, rect, Color.White);
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
                    Color maskedHead;
                    if (!BossAssist.instance.setup.SortedBosses[headNum].downed() && BossAssist.ClientConfig.BossSilhouettes) maskedHead = Color.Black;
                    else maskedHead = Color.White;

                    Texture2D head = BossLogUI.GetBossHead(headNum);
                    spriteBatch.Draw(head, new Rectangle(Main.mouseX + 15, Main.mouseY + 15, head.Width, head.Height), maskedHead);
                    if (headNum == BossAssist.instance.setup.SortedBosses.FindIndex(x => x.id == NPCID.Retinazer))
                    {
                        Rectangle rect = new Rectangle(Main.mouseX + head.Width + 15, Main.mouseY + 15, head.Width, head.Height);
                        spriteBatch.Draw(ModContent.GetTexture("Terraria/NPC_Head_Boss_16"), rect, maskedHead);
                    }
                }
            }

            if (Id == "PageOne" && BossLogUI.PageNum >= 0)
            {
                BossInfo shortcut = BossAssist.instance.setup.SortedBosses[BossLogUI.PageNum];

                Texture2D bossTexture = ModContent.GetTexture(BossAssist.instance.setup.SortedBosses[BossLogUI.PageNum].pageTexture);
                Rectangle posRect = new Rectangle(pageRect.X + (pageRect.Width / 2) - (bossTexture.Width / 2), pageRect.Y + (pageRect.Height / 2) - (bossTexture.Height / 2), bossTexture.Width, bossTexture.Height);
                Rectangle cutRect = new Rectangle(0, 0, bossTexture.Width, bossTexture.Height);
                Color masked;
                if (!shortcut.downed() && BossAssist.ClientConfig.BossSilhouettes) masked = Color.Black;
				else masked = Color.White;
                spriteBatch.Draw(bossTexture, posRect, cutRect, masked);
                
                Texture2D head = BossLogUI.GetBossHead(BossLogUI.PageNum);
                Rectangle headPos = new Rectangle(pageRect.X + pageRect.Width - 40, pageRect.Y + 5, head.Width, head.Height);
                if (BossLogUI.PageNum == BossAssist.instance.setup.SortedBosses.FindIndex(x => x.id == NPCID.DD2Betsy))
                {
                    headPos = new Rectangle(pageRect.X + pageRect.Width - 50, pageRect.Y + 5, head.Width, head.Height);
                }
                spriteBatch.Draw(head, headPos, masked);

                if (BossLogUI.PageNum == BossAssist.instance.setup.SortedBosses.FindIndex(x => x.id == NPCID.Retinazer))
                {
                    Rectangle spazPos = new Rectangle(pageRect.X + pageRect.Width - 72, pageRect.Y + 5, head.Width, head.Height);
                    spriteBatch.Draw(ModContent.GetTexture("Terraria/NPC_Head_Boss_16"), spazPos, masked);
                }

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
                Texture2D achievements = ModContent.GetTexture("Terraria/UI/Achievements");
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

						SubpageButton.displayArray[i] = recordType + ": " + recordNumbers;
						SubpageButton.displayLastArray[i] = "Victories: " + killTimes;
					}
                    else if (i == 1)
                    {
                        recordType = "Quickest Victory";
						if (BossLogUI.AltRecords) recordType = "Slowest Victory";
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
                                Texture2D text = ModContent.GetTexture("Terraria/UI/UI_quickicon1");
                                Rectangle exclam = new Rectangle((int)GetInnerDimensions().X + 232, (int)GetInnerDimensions().Y + 180, text.Width, text.Height);
                                spriteBatch.Draw(text, exclam, Color.White);
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

								SubpageButton.displayArray[i] = recordType + ": " + finalResult;

								if (LastRecord > BestRecord)
								{
									string lastFight = "";
									if (recordMin2 > 0) lastFight += recordMin2 + "m " + recSec2 + "s";
									else lastFight += rec2 + "s";
									SubpageButton.displayLastArray[i] = "Fight Time: " + lastFight;

									finalResult += " [" + lastFight + "]";
								}
								else
								{
									SubpageButton.displayLastArray[i] = "";
								}

								recordNumbers = finalResult;
							}
							else
							{
								recordNumbers = "No record!";
								SubpageButton.displayArray[i] = recordType + ": " + recordNumbers;
							}
						}
                        else
                        {
                            achX = 7;
                            achY = 5;

                            if (LastRecord == WorstRecord && LastRecord != -1)
                            {
                                Texture2D text = ModContent.GetTexture("Terraria/UI/UI_quickicon1");
                                Rectangle exclam = new Rectangle((int)GetInnerDimensions().X + 232, (int)GetInnerDimensions().Y + 180, text.Width, text.Height);
                                spriteBatch.Draw(text, exclam, Color.White);
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

								SubpageButton.displayArray[i] = recordType + ": " + finalResult;

								string lastFight = "";

								if (LastRecord != -1)
								{
									if (recordMin2 > 0)
									{
										lastFight = "Fight Time: " + recordMin2 + "m " + recSec2 + "s";
										SubpageButton.displayLastArray[i] = "";
										if (LastRecord != WorstRecord) finalResult += " [" + recordMin2 + "m " + recSec2 + "s]";
									}
									else
									{
										lastFight = "Fight Time: " + rec2 + "s";
										if (LastRecord != WorstRecord) finalResult += " [" + rec2 + "s]";
									}
								}

								recordNumbers = finalResult;
							}
							else
							{
								recordNumbers = "No record!";
								SubpageButton.displayArray[i] = recordType + ": " + recordNumbers;
							}
						}
                    }
                    else if (i == 2)
                    {
                        recordType = "Vitality";
						if (BossLogUI.AltRecords) recordType = "Brink of Death";

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
                                Texture2D text = ModContent.GetTexture("Terraria/UI/UI_quickicon1");
                                Rectangle exclam = new Rectangle((int)GetInnerDimensions().X + 182, (int)GetInnerDimensions().Y + 255, text.Width, text.Height);
                                spriteBatch.Draw(text, exclam, Color.White);
                            }

                            string finalResult = "";
							string lastFight = "";
							if (BestRecord != 0)
							{
								finalResult += BestRecord + " (" + BestPercent + "%)";
								SubpageButton.displayArray[i] = recordType + ": " + finalResult;
								if (LastRecord != BestRecord && LastRecord != -1)
								{
									lastFight = "Lowest Health: " + LastRecord + " (" + LastPercent + "%)";
									if (LastRecord != BestRecord) finalResult += " [" + LastRecord + " (" + LastPercent + "%)]";
								}
								recordNumbers = finalResult;
							}
							else
							{
								recordNumbers = "No record!";
								SubpageButton.displayArray[i] = recordType + ": " + recordNumbers;
							}

							SubpageButton.displayLastArray[i] = lastFight;
						}
                        else
                        {
                            achX = 6;
                            achY = 7;

                            if (LastRecord == WorstRecord && LastRecord != -1)
                            {
                                Texture2D text = ModContent.GetTexture("Terraria/UI/UI_quickicon1");
                                Rectangle exclam = new Rectangle((int)GetInnerDimensions().X + 182, (int)GetInnerDimensions().Y + 255, text.Width, text.Height);
                                spriteBatch.Draw(text, exclam, Color.White);
                            }

                            string finalResult = "";
							string lastFight = "";
							if (WorstRecord != 0)
							{
								finalResult += WorstRecord + " (" + WorstPercent + "%)";
								SubpageButton.displayArray[i] = recordType + ": " + finalResult;
								if (LastRecord != -1)
								{
									lastFight = "Lowest Health: " + LastRecord + " (" + LastPercent + "%)";
									if (LastRecord != WorstRecord) finalResult += " [" + LastRecord + " (" + LastPercent + "%)" + "]";
								}
								recordNumbers = finalResult;
							}
							else
							{
								recordNumbers = "No record!";
								SubpageButton.displayArray[i] = recordType + ": " + recordNumbers;
							}

							SubpageButton.displayLastArray[i] = lastFight;
						}
                    }
                    else if (i == 3)
                    {
                        recordType = "Ninja Reflexes";
						if (BossLogUI.AltRecords) recordType = "Clumsy Fool";

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
                                Texture2D text = ModContent.GetTexture("Terraria/UI/UI_quickicon1");
                                Rectangle exclam = new Rectangle((int)GetInnerDimensions().X + 225, (int)GetInnerDimensions().Y + 332, text.Width, text.Height);
                                spriteBatch.Draw(text, exclam, Color.White);
                            }
							
                            if (timer == 0 && low == 0 && high == 0) recordNumbers = "No record!";
							else recordNumbers = low + " (" + timerOutput + "s)";
							SubpageButton.displayArray[i] = recordType + ": " + recordNumbers;

							if (last != -1)
							{
								if (low != last) recordNumbers += " [" + last + "]";
								SubpageButton.displayLastArray[i] = "Times Hit: " + last;
							}
							else SubpageButton.displayLastArray[i] = "";
						}
                        else
                        {
                            achX = 4;
                            achY = 2;

                            if (last == high && last != -1)
                            {
                                Texture2D text = ModContent.GetTexture("Terraria/UI/UI_quickicon1");
                                Rectangle exclam = new Rectangle((int)GetInnerDimensions().X + 225, (int)GetInnerDimensions().Y + 332, text.Width, text.Height);
                                spriteBatch.Draw(text, exclam, Color.White);
                            }

							if (high == 0) recordNumbers = "No record!";
							else recordNumbers = high + " (" + timerOutput + "s)";
							SubpageButton.displayArray[i] = recordType + ": " + recordNumbers;

							if (last != -1)
							{
								if (high != last) recordNumbers = high + " (" + timerOutput + "s)" + " [" + last + "]";
								SubpageButton.displayLastArray[i] = "Times Hit: " + last;
							}
							else SubpageButton.displayLastArray[i] = "";
						}
                    }

                    Rectangle posRect = new Rectangle(pageRect.X, pageRect.Y + 100 + (75 * i), 64, 64);
                    Rectangle cutRect = new Rectangle(66 * achX, 66 * achY, 64, 64);
                    spriteBatch.Draw(achievements, posRect, cutRect, Color.White);
					
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
                Texture2D bag = ModContent.GetTexture("BossAssist/Resources/treasureBag");
                for (int i = 0; i < BossAssist.instance.setup.SortedBosses[BossLogUI.PageNum].loot.Count; i++)
                {
                    Item bagItem = new Item();
                    bagItem.SetDefaults(BossAssist.instance.setup.SortedBosses[BossLogUI.PageNum].loot[i]);
                    if (bagItem.expert && bagItem.Name.Contains("Treasure Bag"))
                    {
                        if (bagItem.type < ItemID.Count)
                        {
                            bag = ModContent.GetTexture("Terraria/Item_" + bagItem.type);
                        }
                        else
                        {
                            bag = ModContent.GetTexture(ItemLoader.GetItem(bagItem.type).Texture);
                            break;
                        }
                    }
                }

                for (int i = 0; i < 7; i++)
                {
                    Rectangle posRect = new Rectangle(pageRect.X + (pageRect.Width / 2) - 20 - (bag.Width / 2), pageRect.Y + 88, bag.Width, bag.Height);
                    spriteBatch.Draw(bag, posRect, Color.White);
                }
            }

            if (Id == "PageTwo" && BossLogUI.PageNum >= 0 && BossLogUI.SubPageNum == 3)
            {
                // Collectibles Subpage
                BossInfo BossPage = BossAssist.instance.setup.SortedBosses[BossLogUI.PageNum];
                BossCollection Collections = Main.LocalPlayer.GetModPlayer<PlayerAssist>().BossTrophies[BossLogUI.PageNum];
				// PageNum already corresponds with the index of the saved player data

				Texture2D template = ModContent.GetTexture("BossAssist/Resources/CollectionTemplate");
                if (Collections.collectibles.Any(x => x.Name.Contains("Music Box") && x.createTile > 0))
                {
                    template = ModContent.GetTexture("BossAssist/Resources/CollectionTemplate_NoMusicBox");
                }
                
                spriteBatch.Draw(template, new Rectangle(pageRect.X + (pageRect.Width / 2) - (template.Width / 2) - 20, pageRect.Y + 84, template.Width, template.Height), Color.White);

                // Draw Mask
                if (Collections.collectibles.Any(x => x.Name.Contains("Mask") && x.vanity) && BossPage.collection[1] != 0)
                {
                    Texture2D mask;
					if (BossPage.collection[1] < ItemID.Count)
					{
                        Item newItem = new Item();
                        if (BossPage.id == NPCID.Retinazer) newItem.SetDefaults(ItemID.TwinMask);
                        else newItem.SetDefaults(BossPage.collection[1]);
                        mask = ModContent.GetTexture("Terraria/Armor_Head_" + newItem.headSlot);
                    }
                    else mask = ModContent.GetTexture(ItemLoader.GetItem(BossPage.collection[1]).Texture + "_Head");

                    int frameCut = mask.Height / 24;
                    Rectangle posRect = new Rectangle(pageRect.X + (pageRect.Width / 2) - (mask.Width / 2) - 8, pageRect.Y + (pageRect.Height / 2) - (frameCut / 2) - 86, mask.Width, frameCut);
                    Rectangle cutRect = new Rectangle(0, 0, mask.Width, frameCut);
                    spriteBatch.Draw(mask, posRect, cutRect, Color.White);
                }

                // Draw Trophy
                if (BossPage.id == NPCID.Retinazer)
                {
                    // Draw both Twins trophies
                    if ((Collections.loot.Any(x => x.type == ItemID.RetinazerTrophy) || Collections.loot.Any(x => x.type == ItemID.SpazmatismTrophy)) && BossPage.collection[0] != 0)
                    {
                        Main.instance.LoadTiles(240);
                        Texture2D trophy = Main.tileTexture[240];
                        int offsetX = 0;
                        int offsetY = 0;

                        bool drawTrophy = true;

                        if (timerTrophy >= 240 && Collections.loot.Any(x => x.type == ItemID.RetinazerTrophy))
                        {
                            offsetX = 24;
                            offsetY = 0;
                            drawTrophy = true;
                        }
                        else if (timerTrophy >= 240 && !Collections.loot.All(x => x.type == ItemID.RetinazerTrophy))
                        {
                            drawTrophy = false;
                        }

                        if (timerTrophy < 240 && Collections.loot.Any(x => x.type == ItemID.SpazmatismTrophy))
                        {
                            offsetX = 27;
                            offsetY = 0;
                            drawTrophy = true;
                        }
                        else if (timerTrophy < 240 && !Collections.loot.All(x => x.type == ItemID.SpazmatismTrophy))
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

                                spriteBatch.Draw(trophy, posRect, cutRect, Color.White);

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
                else
                {
                    if (Collections.collectibles.Any(x => x.Name.Contains("Trophy") && x.createTile > 0) && BossPage.collection[0] != 0)
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

                                spriteBatch.Draw(trophy, posRect, cutRect, Color.White);

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
                            Main.instance.LoadTiles(ItemLoader.GetItem(BossPage.collection[0]).item.createTile);
                            trophy = Main.tileTexture[ItemLoader.GetItem(BossPage.collection[0]).item.createTile];

                            offsetX = 0;
                            offsetY = 0;

                            for (int i = 0; i < 9; i++)
                            {
                                Rectangle posRect = new Rectangle(pageRect.X + 98 + (offsetX * 16), pageRect.Y + 126 + (offsetY * 16), 16, 16);
                                Rectangle cutRect = new Rectangle(offsetX * 18, offsetY * 18, 16, 16);

                                spriteBatch.Draw(trophy, posRect, cutRect, Color.White);

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
                if (Collections.collectibles.Any(x => x.Name.Contains("Music Box") && x.createTile > 0))
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

                            spriteBatch.Draw(musicBox, posRect, cutRect, Color.White);

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

                            spriteBatch.Draw(musicBox, posRect, cutRect, Color.White);

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
			
            Texture2D progressBox = BossAssist.instance.GetTexture("Resources/Checkbox_Empty");
            CalculatedStyle innerDimensions = GetInnerDimensions();
            Vector2 pos = new Vector2(innerDimensions.X - 20, innerDimensions.Y - 5);
			if (BossAssist.ClientConfig.SelectedCheckmarkType != "Strike-through")
			{
				spriteBatch.Draw(progressBox, pos, Color.White);
			}
			
			Vector2 pos2 = new Vector2(innerDimensions.X + Main.fontMouseText.MeasureString(text).X + 6, innerDimensions.Y - 2);
			int index = BossAssist.instance.setup.SortedBosses.FindIndex(x => x.progression == order);

			bool allLoot = false;
			bool allCollect = false;

			foreach (int loot in BossAssist.instance.setup.SortedBosses[index].loot)
			{
				if (Main.LocalPlayer.GetModPlayer<PlayerAssist>().BossTrophies[index].loot.Any(x => x.type == loot))
				{
					allLoot = true;
				}
				else if (loot != BossAssist.instance.setup.SortedBosses[index].loot[0])
				{
					allLoot = false;
					break;
				}
			}
			foreach (int collectible in BossAssist.instance.setup.SortedBosses[index].collection)
			{
				if (Main.LocalPlayer.GetModPlayer<PlayerAssist>().BossTrophies[index].collectibles.Any(x => x.type == collectible))
				{
					allCollect = true;
				}
				else if (collectible != -1 && collectible != 0)
				{
					allCollect = false;
					break;
				}
			}
			
			if (allLoot && allCollect)
			{
				spriteBatch.Draw(BossAssist.instance.GetTexture("Resources/CheckBox_GoldChest"), pos2, Color.White);
			}
			else
			{
				if (allLoot) spriteBatch.Draw(BossAssist.instance.GetTexture("Resources/CheckBox_Chest"), pos2, Color.White);
				if (allCollect) spriteBatch.Draw(BossAssist.instance.GetTexture("Resources/CheckBox_Star"), pos2, Color.White);
			}

			if (order != -1f)
            {
                BossAssist BA = BossAssist.instance;
				
				Texture2D checkType = BA.GetTexture("Resources/Checkbox_Check");
				Texture2D strikeThrough = BA.GetTexture("Resources/Checkbox_Strike");

				if (BossAssist.ClientConfig.SelectedCheckmarkType == "Strike-through")
				{
					if (BA.setup.SortedBosses[Convert.ToInt32(Id)].downed())
					{
						int textWidth = (int)Main.fontMouseText.MeasureString(text).X;

						Rectangle cutRect = new Rectangle(0, 4, 4, 3);
						Rectangle strikePos = new Rectangle((int)innerDimensions.X, (int)innerDimensions.Y + 6, textWidth, 3);
						spriteBatch.Draw(strikeThrough, strikePos, cutRect, Color.White);

						// Draw left end
						cutRect = new Rectangle(0, 0, 4, 3);
						strikePos = new Rectangle((int)innerDimensions.X - 4, (int)innerDimensions.Y + 6, 4, 3);
						spriteBatch.Draw(strikeThrough, strikePos, cutRect, Color.White);

						// Draw right end
						cutRect = new Rectangle(0, 8, 4, 3);
						strikePos = new Rectangle((int)innerDimensions.X + textWidth, (int)innerDimensions.Y + 6, 4, 3);
						spriteBatch.Draw(strikeThrough, strikePos, cutRect, Color.White);
					}
				}
				else
				{
					if (BossAssist.ClientConfig.SelectedCheckmarkType == "X and  ☐")
						checkType = BA.GetTexture("Resources/Checkbox_CheckAlt");
					if (BA.setup.SortedBosses[Convert.ToInt32(Id)].downed())
						spriteBatch.Draw(checkType, pos, Color.White);
					else if (!BA.setup.SortedBosses[Convert.ToInt32(Id)].downed() && nextCheck && BossAssist.ClientConfig.DrawNextMark)
						spriteBatch.Draw(BA.GetTexture("Resources/Checkbox_Next"), pos, Color.White);
					else if (!BA.setup.SortedBosses[Convert.ToInt32(Id)].downed() && BossAssist.ClientConfig.SelectedCheckmarkType == "✓ and  X")
						spriteBatch.Draw(BA.GetTexture("Resources/Checkbox_CheckAlt"), pos, Color.White);
				}
				
				if (BossAssist.ClientConfig.ColoredBossText)
				{
					if (IsMouseHovering && BA.setup.SortedBosses[Convert.ToInt32(Id)].downed()) TextColor = Color.DarkSeaGreen;
					else if (IsMouseHovering && !BA.setup.SortedBosses[Convert.ToInt32(Id)].downed()) TextColor = Color.IndianRed;
					else if (!IsMouseHovering && BA.setup.SortedBosses[Convert.ToInt32(Id)].downed()) TextColor = Colors.RarityGreen;
					else if (!IsMouseHovering && !BA.setup.SortedBosses[Convert.ToInt32(Id)].downed()) TextColor = Colors.RarityRed;
					if (IsMouseHovering && !BA.setup.SortedBosses[Convert.ToInt32(Id)].downed() && nextCheck && BossAssist.ClientConfig.DrawNextMark) TextColor = new Color(189, 180, 64);
					else if (!IsMouseHovering && !BA.setup.SortedBosses[Convert.ToInt32(Id)].downed() && nextCheck && BossAssist.ClientConfig.DrawNextMark) TextColor = new Color(248, 235, 91);
				}
				else
				{
					if (IsMouseHovering) TextColor = new Color(80, 85, 100);
					else TextColor = new Color(140, 145, 160);
				}

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

		public static string[] displayArray = new string[4];
		public static string[] displayLastArray = new string[4];
		bool displayRecord = true; // To prevent multiple messages from accuring by holding down click
		int recordCooldown = 0; // Allow only one display every 5 seconds

		public SubpageButton(string type)
        {
            buttonString = type;
        }

        public override void Draw(SpriteBatch spriteBatch)
		{
			if (recordCooldown > 0) recordCooldown--;

			BackgroundColor = Color.Brown;
            base.DrawSelf(spriteBatch);

            CalculatedStyle innerDimensions = GetInnerDimensions();
            Vector2 stringAdjust = Main.fontMouseText.MeasureString(buttonString);
            Vector2 pos = new Vector2(innerDimensions.X - (stringAdjust.X / 3) + Width.Pixels / 3, innerDimensions.Y - 10);
            if (buttonString != "Disclaimer" && buttonString != "recordAlts")
            {
                DynamicSpriteFontExtensionMethods.DrawString(spriteBatch, Main.fontMouseText, buttonString, pos, Color.Gold);
            }
            
            Texture2D text = ModContent.GetTexture("Terraria/UI/Achievement_Categories");
            Rectangle exclamPos = new Rectangle((int)GetInnerDimensions().X - 12, (int)GetInnerDimensions().Y - 12, 32, 32);

            if (buttonString == "")
            {
                if (BossLogUI.SubPageNum == 0)
                {
					if (Id == "Display Records")
					{
						Rectangle exclamCut = new Rectangle(34 * 2, 0, 32, 32);
						spriteBatch.Draw(text, exclamPos, exclamCut, Color.White);
						if (IsMouseHovering)
						{
							Main.hoverItemName = "Left-click to display your current records";
							if (displayLastArray[3] != "") Main.hoverItemName += "\nRight-click to display the records of your last fight";
							if (displayRecord && recordCooldown == 0)
							{
								if (Main.mouseLeft)
								{
									recordCooldown = 600;
									displayRecord = false;
									/*if (Main.dedServ)
									{
										NetMessage.BroadcastChatMessage(NetworkText.FromLiteral("[" + Main.LocalPlayer.name + "'s current records with " + BossAssist.instance.setup.SortedBosses[BossLogUI.PageNum].name + "]"), new Color(82, 175, 82));
										if (displayArray[0] != "") NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(displayArray[0]), new Color(138, 210, 137));
										if (displayArray[1] != "") NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(displayArray[1]), new Color(138, 210, 137));
										if (displayArray[2] != "") NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(displayArray[2]), new Color(138, 210, 137));
										if (displayArray[3] != "") NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(displayArray[3]), new Color(138, 210, 137));
									}
									else*/
									{
										Main.NewText("[" + Main.LocalPlayer.name + "'s current records with " + BossAssist.instance.setup.SortedBosses[BossLogUI.PageNum].name + "]", new Color(82, 175, 82));
										if (displayArray[0] != "") Main.NewText(displayArray[0], new Color(138, 210, 137));
										if (displayArray[1] != "") Main.NewText(displayArray[1], new Color(138, 210, 137));
										if (displayArray[2] != "") Main.NewText(displayArray[2], new Color(138, 210, 137));
										if (displayArray[3] != "") Main.NewText(displayArray[3], new Color(138, 210, 137));
									}
								}
								else if (Main.mouseRight && displayLastArray[3] != "")
								{
									recordCooldown = 600;
									displayRecord = false;
									/*if (Main.dedServ)
									{
										NetMessage.BroadcastChatMessage(NetworkText.FromLiteral("[" + Main.LocalPlayer.name + "'s last fight stats with " + BossAssist.instance.setup.SortedBosses[BossLogUI.PageNum].name + "]"), new Color(82, 175, 82));
										if (displayLastArray[0] != "") NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(displayLastArray[0]), new Color(138, 210, 137));
										if (displayLastArray[1] != "") NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(displayLastArray[1]), new Color(138, 210, 137));
										if (displayLastArray[2] != "") NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(displayLastArray[2]), new Color(138, 210, 137));
										if (displayLastArray[3] != "") NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(displayLastArray[3]), new Color(138, 210, 137));
									}
									else*/
									{
										Main.NewText("[" + Main.LocalPlayer.name + "'s last fight stats with " + BossAssist.instance.setup.SortedBosses[BossLogUI.PageNum].name + "]", new Color(82, 175, 82));
										Main.NewText(displayLastArray[0], new Color(138, 210, 137));
										Main.NewText(displayLastArray[1], new Color(138, 210, 137));
										Main.NewText(displayLastArray[2], new Color(138, 210, 137));
										Main.NewText(displayLastArray[3], new Color(138, 210, 137));
									}
								}
							}
							else if (Main.mouseLeftRelease && Main.mouseRightRelease)
							{
								displayRecord = true;
							}
						}
					}
					else
					{
						if (!BossLogUI.AltRecords)
						{
							Rectangle exclamCut = new Rectangle(34 * 3, 0, 32, 32);
							spriteBatch.Draw(text, exclamPos, exclamCut, Color.White);
							if (IsMouseHovering) Main.hoverItemName = "Click to see your 'Worst' records" +
																	"\nRecords are shown as your best compared to your last fight";
						}
						else
						{
							Rectangle exclamCut = new Rectangle(0, 0, 32, 32);
							spriteBatch.Draw(text, exclamPos, exclamCut, Color.White);
							if (IsMouseHovering) Main.hoverItemName = "Click to see your 'Best' records" +
																	"\nRecords are shown as your worst compared to your last fight";
						}
					}
                }
                else if (BossLogUI.SubPageNum == 2)
                {
                    Rectangle exclamCut = new Rectangle(34 * 1, 0, 32, 32);
                    spriteBatch.Draw(text, exclamPos, exclamCut, Color.White);
                    if (IsMouseHovering) Main.hoverItemName = "DISCLAIMER:" +
															"\nLoot Tables may not be showing all possible drops" +
															"\nItems added by other mods may not be on the list!";
                }
                else if (BossLogUI.SubPageNum == 3)
                {
                    Rectangle exclamCut = new Rectangle(34 * 1, 0, 32, 32);
                    spriteBatch.Draw(text, exclamPos, exclamCut, Color.White);
                    if (IsMouseHovering) Main.hoverItemName = "DISCLAIMER:" +
                                                            "\nCollections may not be showing all collectibles" +
                                                            "\nItems added by other mods may not be on the list!";
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
		public SubpageButton displayRecordButton;
		public SubpageButton toolTipButton;
        
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
		public static int RecipePageNum = 0;
		public static int RecipeShown = 0;
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
            PageOne.Top.Pixels = 12;

            Texture2D prevTexture = BossAssist.instance.GetTexture("Resources/Prev");
            PrevPage = new BossAssistButton(prevTexture, "") { Id = "Previous" };
            PrevPage.Width.Pixels = 14;
            PrevPage.Height.Pixels = 20;
            PrevPage.Left.Pixels = 30;
            PrevPage.Top.Pixels = 416;
            PrevPage.OnClick += new MouseEvent(PageChangerClicked);

            Texture2D tocTexture = BossAssist.instance.GetTexture("Resources/ToC");
            TOCPage = new BossAssistButton(tocTexture, "") { Id = "TableOfContents" };
            TOCPage.Width.Pixels = 22;
            TOCPage.Height.Pixels = 22;
            TOCPage.Left.Pixels = 0;
            TOCPage.Top.Pixels = 416;
            TOCPage.OnClick += new MouseEvent(PageChangerClicked);
            PageOne.Append(TOCPage);

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
            PageTwo.Top.Pixels = 12;

            pageTwoItemList = new UIList();

            pageTwoScroll = new FixedUIScrollbar();

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
            NextPage.Top.Pixels = 416;
            NextPage.OnClick += new MouseEvent(PageChangerClicked);
            PageTwo.Append(NextPage);

            Texture2D credTexture = BossAssist.instance.GetTexture("Resources/Credits");
            CredPage = new BossAssistButton(credTexture, "") { Id = "Credits" };
            CredPage.Width.Pixels = 22;
            CredPage.Height.Pixels = 22;
            CredPage.Left.Pixels = PageTwo.Width.Pixels - (int)(NextPage.Width.Pixels * 3);
            CredPage.Top.Pixels = 416;
            CredPage.OnClick += new MouseEvent(PageChangerClicked);
            PageTwo.Append(CredPage);

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

			displayRecordButton = new SubpageButton("Display Records");
			displayRecordButton.Width.Pixels = 32;
			displayRecordButton.Height.Pixels = 32;
			displayRecordButton.Left.Pixels = PageTwo.Width.Pixels - displayRecordButton.Width.Pixels - 30;
			displayRecordButton.Top.Pixels = 128;

			bossLogPanel.Append(PageTwo);

            Append(bossLogPanel);
        }

        public override void Update(GameTime gameTime)
        {
            visible = Main.playerInventory;
            if (!visible) RemoveChild(bosslogbutton);
            else if (!HasChild(bosslogbutton)) Append(bosslogbutton);

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
                    bossLogPanel.RemoveChild(PageOne);
                    bossLogPanel.RemoveChild(PageTwo);
                    BookUI.visible = false;
                }
            }
            else if (BossLogPanel.visible && BookUI.visible)
            {
                if (Main.LocalPlayer.controlInv || Main.mouseItem.type != 0)
                {
                    BossLogPanel.visible = false;
                    bossLogPanel.RemoveChild(PageOne);
                    bossLogPanel.RemoveChild(PageTwo);
                    BookUI.visible = false;
                    Main.playerInventory = true;
                }
            }

            // We rewrite the position of the button to make sure it updates with the screen res
            bosslogbutton.Left.Pixels = Main.screenWidth - bosslogbutton.Width.Pixels - 190;
            bosslogbutton.Top.Pixels = Main.screenHeight - bosslogbutton.Height.Pixels - 8;
            bossLogPanel.Left.Pixels = (Main.screenWidth / 2) - (bossLogPanel.Width.Pixels / 2);
            bossLogPanel.Top.Pixels = (Main.screenHeight / 2) - (bossLogPanel.Height.Pixels / 2);

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
			if (BossAssist.ClientConfig.ResetRecordsBool)
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

		private void ChangeSpawnItem(UIMouseEvent evt, UIElement listeningElement)
		{
			if (listeningElement.Id == "NextItem")
			{
				RecipePageNum++;
				RecipeShown = 0;
			}
			else if (listeningElement.Id == "PrevItem")
			{
				RecipePageNum--;
				RecipeShown = 0;
			}
			else if (listeningElement.Id.Contains("CycleItem"))
			{
				int index = listeningElement.Id.IndexOf('_');
				// if (index != -1) Main.NewText(listeningElement.Id.Substring(index + 1));
				if (RecipeShown == Convert.ToInt32(listeningElement.Id.Substring(index + 1)) - 1) RecipeShown = 0;
				else RecipeShown++;
			}
			OpenSpawn(evt, listeningElement);
		}

		private void PageChangerClicked(UIMouseEvent evt, UIElement listeningElement)
        {
            pageTwoItemList.Clear();
            prehardmodeList.Clear();
            hardmodeList.Clear();
            PageOne.RemoveChild(scrollOne);
            PageTwo.RemoveChild(scrollTwo);
            PageTwo.RemoveChild(pageTwoScroll);
			RecipeShown = 0;
			RecipePageNum = 0;
            
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
            ResetPageButtons();
        }

        private void OpenRecord(UIMouseEvent evt, UIElement listeningElement)
        {
            SubPageNum = 0;
            ResetPageTwo();
            if (PageNum < 0) return;
        }

        private void OpenSpawn(UIMouseEvent evt, UIElement listeningElement)
        {
            SubPageNum = 1;
			int TotalRecipes = 0;
			ResetPageTwo();
            if (PageNum < 0) return;
			if (BossAssist.instance.setup.SortedBosses[PageNum].spawnItem.Count < 1) return;
			
            List<Item> ingredients = new List<Item>();
            List<int> requiredTiles = new List<int>();
			string recipeMod = "Vanilla";
            //List<Recipe> recipes = Main.recipe.ToList();
            Item spawn = new Item();
            if (BossAssist.instance.setup.SortedBosses[PageNum].spawnItem[RecipePageNum] != 0)
			{
				RecipeFinder finder = new RecipeFinder();
				finder.SetResult(BossAssist.instance.setup.SortedBosses[PageNum].spawnItem[RecipePageNum]);

				foreach (Recipe recipe in finder.SearchRecipes())
				{
					if (TotalRecipes == RecipeShown)
					{
						foreach (Item item in recipe.requiredItem) ingredients.Add(item);
						foreach (int tile in recipe.requiredTile)
						{
							if (tile != -1 && tile != 0) requiredTiles.Add(tile);
						}
						if (recipe is ModRecipe modRecipe)
						{
							recipeMod = modRecipe.mod.DisplayName;
						}
					}
					TotalRecipes++;
				}
				spawn.SetDefaults(BossAssist.instance.setup.SortedBosses[PageNum].spawnItem[RecipePageNum]);

				LogItemSlot spawnItemSlot = new LogItemSlot(spawn, false, spawn.HoverName, ItemSlot.Context.EquipDye);
                spawnItemSlot.Height.Pixels = 50;
                spawnItemSlot.Width.Pixels = 50;
                spawnItemSlot.Top.Pixels = 125;
                spawnItemSlot.Left.Pixels = 33 + (56 * 2);
                PageTwo.Append(spawnItemSlot);
				
                int row = 0;
                int col = 0;
                for (int k = 0; k < ingredients.Count; k++)
                {
                    LogItemSlot ingList = new LogItemSlot(ingredients[k], false, ingredients[k].HoverName, ItemSlot.Context.GuideItem);
                    ingList.Height.Pixels = 50;
                    ingList.Width.Pixels = 50;
                    ingList.Top.Pixels = 125 + (56 * (row + 1));
                    ingList.Left.Pixels = 33 + (56 * col);
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
                if (ingredients.Count > 0 && requiredTiles.Count == 0)
                {
                    craft.SetDefaults(ItemID.PowerGlove);

                    LogItemSlot craftItem = new LogItemSlot(craft, false, "By Hand", ItemSlot.Context.EquipArmorVanity);
                    craftItem.Height.Pixels = 50;
                    craftItem.Width.Pixels = 50;
                    craftItem.Top.Pixels = 125 + (56 * (row + 2));
                    craftItem.Left.Pixels = 33;
                    PageTwo.Append(craftItem);
                }
                else if (requiredTiles.Count > 0)
                {
                    for (int l = 0; l < requiredTiles.Count; l++)
                    {
                        if (requiredTiles[l] == -1) break; // Prevents extra empty slots from being created
                        LogItemSlot tileList;
                        if (requiredTiles[l] == 26)
                        {
                            craft.SetDefaults(0);
                            string altarType;
                            if (WorldGen.crimson) altarType = "Crimson Altar";
                            else altarType = "Demon Altar";
                            tileList = new LogItemSlot(craft, false, altarType, ItemSlot.Context.EquipArmorVanity);
                        }
                        else
                        {
							for (int m = 0; m < ItemLoader.ItemCount; m++)
							{
								craft.SetDefaults(m);
								if (craft.createTile == requiredTiles[l]) break;
							}
							tileList = new LogItemSlot(craft, false, craft.HoverName, ItemSlot.Context.EquipArmorVanity);
                        }
                        tileList.Height.Pixels = 50;
                        tileList.Width.Pixels = 50;
						tileList.Top.Pixels = 125 + (56 * (row + 2));
                        tileList.Left.Pixels = 33 + (56 * l);
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

				if (RecipePageNum > 0)
				{
					BossAssistButton PrevItem = new BossAssistButton(BossAssist.instance.GetTexture("Resources/Prev"), "");
					PrevItem.Id = "PrevItem";
					PrevItem.Top.Pixels = 140;
					PrevItem.Left.Pixels = 125;
					PrevItem.Width.Pixels = 14;
					PrevItem.Height.Pixels = 20;
					PrevItem.OnClick += new MouseEvent(ChangeSpawnItem);
					PageTwo.Append(PrevItem);
				}

				if (RecipePageNum < BossAssist.instance.setup.SortedBosses[PageNum].spawnItem.Count - 1)
				{
					BossAssistButton NextItem = new BossAssistButton(BossAssist.instance.GetTexture("Resources/Next"), "");
					NextItem.Id = "NextItem";
					NextItem.Top.Pixels = 140;
					NextItem.Left.Pixels = 203;
					NextItem.Width.Pixels = 14;
					NextItem.Height.Pixels = 20;
					NextItem.OnClick += new MouseEvent(ChangeSpawnItem);
					PageTwo.Append(NextItem);
				}

				if (TotalRecipes > 1)
				{
					BossAssistButton CycleItem = new BossAssistButton(BossAssist.instance.GetTexture("Resources/Credits"), "Cycle Alt Recipes");
					CycleItem.Id = "CycleItem_" + TotalRecipes;
					CycleItem.Top.Pixels = 254;
					CycleItem.Left.Pixels = 274;
					CycleItem.Width.Pixels = 22;
					CycleItem.Height.Pixels = 22;
					CycleItem.OnClick += new MouseEvent(ChangeSpawnItem);
					PageTwo.Append(CycleItem);
				}

				string recipeMessage = "This item is not craftable.";
				if (TotalRecipes > 0)
				{
					recipeMessage = "Provided by: " + recipeMod;
				}

				UIText ModdedRecipe = new UIText(recipeMessage, 0.8f);
				ModdedRecipe.Left.Pixels = 0;
				ModdedRecipe.Top.Pixels = 90;
				PageTwo.Append(ModdedRecipe);
			}
        }

        private void OpenLoot(UIMouseEvent evt, UIElement listeningElement)
        {
            SubPageNum = 2;
            ResetPageTwo();
            if (PageNum < 0) return;
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
					BossCollection Collection = Main.LocalPlayer.GetModPlayer<PlayerAssist>().BossTrophies[BossLogUI.PageNum];
					LogItemSlot lootTable = new LogItemSlot(expertItem, Collection.loot.Any(x => x.type == expertItem.type), expertItem.Name, ItemSlot.Context.ShopItem);
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
					BossCollection Collection = Main.LocalPlayer.GetModPlayer<PlayerAssist>().BossTrophies[BossLogUI.PageNum];
					LogItemSlot lootTable = new LogItemSlot(loot, Collection.loot.Any(x => x.type == loot.type), loot.Name, ItemSlot.Context.TrashItem);
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
            SubPageNum = 3;
            ResetPageTwo();
            if (PageNum < 0) return;
            int row = 0;
            int col = 0;

            pageTwoItemList.Left.Pixels = 0;
            pageTwoItemList.Top.Pixels = 235;
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
				if (shortcut.collection[i] == -1) continue;
				Item collectible = new Item();
                collectible.SetDefaults(shortcut.collection[i]);

				BossCollection Collection = Main.LocalPlayer.GetModPlayer<PlayerAssist>().BossTrophies[BossLogUI.PageNum];
				LogItemSlot collectionTable = new LogItemSlot(collectible, Collection.collectibles.Any(x => x.type == collectible.type), collectible.Name);
                collectionTable.Height.Pixels = 50;
                collectionTable.Width.Pixels = 50;
                collectionTable.Id = "collect_" + i;
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
                        next.Id = (i + 1).ToString();
                        next.OnClick += new MouseEvent(UpdatePage);
                        hardmodeList.Add(next);
                    }
                    else if (!copiedList[i].downed())
                    {
                        nextCheck++;
                        next.PaddingTop = 5;
                        next.PaddingLeft = 22;
                        next.TextColor = Color.IndianRed;
                        next.Id = (i + 1).ToString();
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
                    UIText modListed = new UIText("●" + mod, 0.85f)
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
            ResetPageButtons();
            if (SubPageNum == 0) OpenRecord(evt, listeningElement);
            else if (SubPageNum == 1) OpenSpawn(evt, listeningElement);
            else if (SubPageNum == 2) OpenLoot(evt, listeningElement);
            else if (SubPageNum == 3) OpenCollect(evt, listeningElement);
        }

        private void ResetPageTwo()
		{
			PageOne.RemoveAllChildren();
			PageTwo.RemoveAllChildren();
            ResetPageButtons();
            if (PageNum >= 0)
            {
                PageTwo.Append(spawnButton);
                PageTwo.Append(lootButton);
                PageTwo.Append(collectButton);
                PageTwo.Append(recordButton);
			}
        }

        private void ResetPageButtons()
        {
            PageOne.RemoveChild(TOCPage);
            PageOne.RemoveChild(PrevPage);
            PageTwo.RemoveChild(NextPage);
            PageTwo.RemoveChild(CredPage);
            PageTwo.RemoveChild(toolTipButton);
			PageTwo.RemoveChild(displayRecordButton);

			if (PageNum == -2) PageOne.Append(PrevPage);
            else if (PageNum == -1) PageTwo.Append(NextPage);
            else
            {
                if (SubPageNum != 1)
                {
                    toolTipButton = new SubpageButton("");
                    toolTipButton.Width.Pixels = 32;
                    toolTipButton.Height.Pixels = 32;
                    toolTipButton.Left.Pixels = PageTwo.Width.Pixels - toolTipButton.Width.Pixels - 30;
                    toolTipButton.Top.Pixels = 86;
                    toolTipButton.OnClick += new MouseEvent(SwapRecordPage);
                    PageTwo.Append(toolTipButton);
					if (SubPageNum == 0)
					{
						displayRecordButton = new SubpageButton("");
						displayRecordButton.Width.Pixels = 32;
						displayRecordButton.Height.Pixels = 32;
						displayRecordButton.Left.Pixels = PageTwo.Width.Pixels - displayRecordButton.Width.Pixels - 30;
						displayRecordButton.Top.Pixels = 128;
						displayRecordButton.Id = "Display Records";
						PageTwo.Append(displayRecordButton);
					}
				}

                PageTwo.Append(NextPage);
                PageOne.Append(PrevPage);
            }
            PageOne.Append(TOCPage);
            PageTwo.Append(CredPage);
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
                if (type == NPCID.KingSlime) head = ModContent.GetTexture("Terraria/NPC_Head_Boss_7");
                else if (type == NPCID.EyeofCthulhu) head = ModContent.GetTexture("Terraria/NPC_Head_Boss_0");
                else if (type == NPCID.EaterofWorldsHead) head = ModContent.GetTexture("Terraria/NPC_Head_Boss_2");
                else if (type == NPCID.BrainofCthulhu) head = ModContent.GetTexture("Terraria/NPC_Head_Boss_23");
                else if (type == NPCID.QueenBee) head = ModContent.GetTexture("Terraria/NPC_Head_Boss_14");
                else if (type == NPCID.SkeletronHead) head = ModContent.GetTexture("Terraria/NPC_Head_Boss_19");
                else if (type == NPCID.WallofFlesh) head = ModContent.GetTexture("Terraria/NPC_Head_Boss_22");
                else if (type == NPCID.Retinazer) head = ModContent.GetTexture("Terraria/NPC_Head_Boss_21");
                else if (type == NPCID.TheDestroyer) head = ModContent.GetTexture("Terraria/NPC_Head_Boss_25");
                else if (type == NPCID.SkeletronPrime) head = ModContent.GetTexture("Terraria/NPC_Head_Boss_18");
                else if (type == NPCID.Plantera) head = ModContent.GetTexture("Terraria/NPC_Head_Boss_11");
                else if (type == NPCID.Golem) head = ModContent.GetTexture("Terraria/NPC_Head_Boss_5");
                else if (type == NPCID.DD2Betsy) head = ModContent.GetTexture("Terraria/NPC_Head_Boss_34");
                else if (type == NPCID.DukeFishron) head = ModContent.GetTexture("Terraria/NPC_Head_Boss_4");
                else if (type == NPCID.CultistBoss) head = ModContent.GetTexture("Terraria/NPC_Head_Boss_24");
                else if (type == NPCID.MoonLordHead) head = ModContent.GetTexture("Terraria/NPC_Head_Boss_8");
                else head = ModContent.GetTexture("Terraria/NPC_Head_0");
            }
            else head = ModContent.GetTexture(NPCLoader.GetNPC(BossAssist.instance.setup.SortedBosses[page].id).BossHeadTexture);

            return head;
        }
    }
}
