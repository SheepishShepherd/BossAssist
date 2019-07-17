using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria.Graphics;
using System.IO;
using Terraria.ID;

namespace BossAssist
{
    public class BossAssist : Mod
    {
        internal static BossAssist instance;

        public static ModHotKey ToggleBossLog;

		internal static ClientConfiguration ClientConfig;

		internal UserInterface BossLogInterface;
        internal BossLogUI BossLog;
        internal SetupBossList setup;

        //Zoom level, (for UIs)
        public static Vector2 ZoomFactor; //0f == fully zoomed out, 1f == fully zoomed in

        internal static UserInterface BossRadarUIInterface;
        internal static BossRadarUI BossRadarUI;

        public BossAssist()
        {

        }

        public override void Load()
        {
            instance = this;

            ToggleBossLog = RegisterHotKey("Toggle Boss Log", "L");

            MapAssist.FullMapInitialize();

            setup = new SetupBossList();

            if (!Main.dedServ)
            {
                BossLog = new BossLogUI();
                BossLog.Activate();
                BossLogInterface = new UserInterface();
                BossLogInterface.SetState(BossLog);

                //important, after setup has been initialized
                BossRadarUI = new BossRadarUI();
                BossRadarUI.Activate();
                BossRadarUIInterface = new UserInterface();
                BossRadarUIInterface.SetState(BossRadarUI);
            }
        }

        public override void Unload()
        {
            instance = null;
            ToggleBossLog = null;
            setup = null;
            BossRadarUI.arrowTexture = null;
        }

        public override void ModifyTransformMatrix(ref SpriteViewMatrix Transform)
        {
            //this is needed for Boss Radar, so it takes the range at which to draw the icon properly
            ZoomFactor = Transform.Zoom - (Vector2.UnitX + Vector2.UnitY);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (BossLogInterface != null) BossLogInterface.Update(gameTime);
            BossRadarUI.Update(gameTime);
        }

        public override void PostDrawFullscreenMap(ref string mouseText)
        {
            MapAssist.DrawFullscreenMap();
			if (MapAssist.shouldDraw)
			{
				MapAssist.DrawNearestEvil(MapAssist.tilePos);
			}
        }
        
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int MouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (MouseTextIndex != -1)
            {
                layers.Insert(MouseTextIndex, new LegacyGameInterfaceLayer("BossAssist: Boss Log",
                    delegate
                    {
                        BossLogInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
                layers.Insert(++MouseTextIndex, new LegacyGameInterfaceLayer("BossAssist: Boss Radar",
                    delegate
                    {
                        BossRadarUIInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
            int InventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Death Text"));
            if (InventoryIndex != -1)
            {
                layers.Insert(InventoryIndex, new LegacyGameInterfaceLayer("BossAssist: Respawn Timer",
                    delegate
                    {
                        if (Main.LocalPlayer.dead && Main.LocalPlayer.difficulty != 2)
                        {
                            if (Main.LocalPlayer.respawnTimer % 60 == 0 && Main.LocalPlayer.respawnTimer / 60 <= 3) Main.PlaySound(25);
                            string timer = (Main.LocalPlayer.respawnTimer / 60 + 1).ToString();
                            DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, Main.fontDeathText, timer, new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 - 75), new Color(1f, 0.388f, 0.278f), 0f, default(Vector2), 1, SpriteEffects.None, 0f);
                        }
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }

        public override object Call(params object[] args)
        {
            try
            {
                string AddType = args[0].ToString();
                if (AddType == "AddBoss")
                {
                    int bossID = Convert.ToInt32(args[1]);
                    string bossMessage = args[2] as string;

                    WorldAssist.ModBossTypes.Add(bossID);
                    WorldAssist.ModBossMessages.Add(bossMessage);
                    return "Success";
                }
                else if (AddType == "AddStatPage")
                {
                    float BossValue = Convert.ToSingle(args[1]);
                    int BossID = Convert.ToInt32(args[2]);
                    string ModName = args[3].ToString();
                    string BossName = args[4].ToString();
                    Func<bool> BossDowned = args[5] as Func<bool>;
					
					List<int> BossSpawn;

					if (args[6] is List<int>)
					{
						BossSpawn = args[6] as List<int>;
					}
					else
					{
						BossSpawn = new List<int>() { Convert.ToInt32(args[6]) };
					}


                    List<int> BossCollect = args[7] as List<int>;
                    List<int> BossLoot = args[8] as List<int>;
                    string BossTexture = "";
                    if (args.Length > 9) BossTexture = args[9].ToString();

					setup.AddBoss(BossValue, BossID, ModName, BossName, BossDowned, BossSpawn, BossCollect, BossLoot, BossTexture);
				}
                // Will be added in later once some fixes are made and features are introduced

				//
                else if (AddType == "AddLoot")
                {
                    string ModName = args[1].ToString();
                    int BossID = Convert.ToInt32(args[2]);
                    List<int> BossLoot = args[3] as List<int>;
                    // This list is for adding on to existing bosses loot drops
                    setup.AddToLootTable(BossID, ModName, BossLoot);
                }
                else if (AddType == "AddCollectibles")
                {
                    string ModName = args[1].ToString();
                    int BossID = Convert.ToInt32(args[2]);
                    List<int> BossCollect = args[3] as List<int>;
                    // This list is for adding on to existing bosses loot drops
                    setup.AddToCollection(BossID, ModName, BossCollect);
                }
				//
                else
                {

                }
            }
            catch (Exception e)
            {
				
            }
            return "Failed";
        }

		public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            BossAssistMessageID msgType = (BossAssistMessageID)reader.ReadByte();
            switch (msgType)
            {
                // Trying to update player Records
                case BossAssistMessageID.UpdateRecords:
                    byte plrnum = reader.ReadByte();
                    PlayerAssist playerdata = Main.player[plrnum].GetModPlayer<PlayerAssist>();
                    if (Main.netMode == NetmodeID.Server)
                    {
                        var packet = GetPacket();
                        packet.Write((byte)BossAssistMessageID.UpdateRecords);
                        packet.Write(plrnum);
                        packet.Send(-1, plrnum);
                    }
                    break;
            }
        }

        internal enum BossAssistMessageID : byte
        {
            UpdateRecords
        }
    }
}
 