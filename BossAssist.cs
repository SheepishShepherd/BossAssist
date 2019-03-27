using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;

namespace BossAssist
{
    public class BossAssist : Mod
    {
        internal static BossAssist instance;
        
        internal UserInterface BossLogInterface;
        internal BossLogUI BossLog;
        internal SetupBossList setup;

        public BossAssist()
        {

        }

        public override void Load()
        {
            instance = this;

            MapAssist.FullMapInitialize();

            setup = new SetupBossList();

            if (!Main.dedServ)
            {
                BossLog = new BossLogUI();
                BossLog.Activate();
                BossLogInterface = new UserInterface();
                BossLogInterface.SetState(BossLog);
            }
        }

        public override void Unload()
        {
            instance = null;
            setup = null;
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (BossLogInterface != null) BossLogInterface.Update(gameTime);
        }

        public override void PostDrawFullscreenMap(ref string mouseText)
        {
            MapAssist.DrawFullscreenMap(this, ref mouseText);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int MouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (MouseTextIndex != -1)
            {
                layers.Insert(MouseTextIndex, new LegacyGameInterfaceLayer("Boss Log",
                    delegate
                    {
                        if (BossLogUI.visible) BossLogInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
            int InventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Death Text"));
            if (InventoryIndex != -1)
            {
                layers.Insert(InventoryIndex + 1, new LegacyGameInterfaceLayer("Respawn Timer",
                    delegate
                    {
                        string timer;
                        if (Main.LocalPlayer.dead && Main.LocalPlayer.difficulty != 2)
                        {
                            if (Main.LocalPlayer.respawnTimer % 60 == 0 && Main.LocalPlayer.respawnTimer / 60 <= 3) Main.PlaySound(25);
                            timer = (Main.LocalPlayer.respawnTimer / 60 + 1).ToString();
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
                    int BossSpawn = Convert.ToInt32(args[6]);
                    List<int> BossCollect = args[7] as List<int>;
                    List<int> BossLoot = args[8] as List<int>;
                    Texture2D BossTexture = null;
                    if (args.Length > 9) BossTexture = args[9] as Texture2D;

                    setup.AddBoss(BossValue, BossID, ModName, BossName, BossDowned, BossSpawn, BossCollect, BossLoot, BossTexture);
                }
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
                else
                {
                    ErrorLogger.Log("BossAssist Call Error: Type not found");
                }
            }
            catch (Exception e)
            {
                ErrorLogger.Log("BossChecklist Call Error: " + e.StackTrace + e.Message);
            }
            return "Failure";
        }
    }
}