using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace BossAssist
{
    class BossAssist : Mod
    {
        internal static BossAssist instance;

        internal UserInterface TimerUI;
        ModTranslation text;

        public BossAssist()
        {

        }

        public override void Load()
        {
            instance = this;

            MapAssist.FullMapInitialise();

            if (!Main.dedServ)
            {
                TimerUI = new UserInterface();
                TimerUI.SetState(null);
            }

            // Event End Messages
            
            text = CreateTranslation("BMoonEnd");
            text.SetDefault("The blood moon falls past the horizon...");
            AddTranslation(text);

            text = CreateTranslation("EclipseEnd");
            text.SetDefault("The solar eclipse has ended... until next time...");
            AddTranslation(text);
            
            text = CreateTranslation("PMoonEnd");
            text.SetDefault("The pumpkin moon ends its harvest...");
            AddTranslation(text);
            
            text = CreateTranslation("FMoonEnd");
            text.SetDefault("The frost moon melts as the sun rises...");
            AddTranslation(text);

            // Lunar Pillar death messages

            text = CreateTranslation("PillarDestroyed");
            text.SetDefault("The {0} has been destroyed");
            AddTranslation(text);


            // Generic Mod Boss Despawn Messages

            text = CreateTranslation("GenericBossWins");
            text.SetDefault("{0} has killed every player!");
            AddTranslation(text);

            text = CreateTranslation("GenericBossLeft");
            text.SetDefault("{0} is no longer after you...");
            AddTranslation(text);

            text = CreateTranslation("GenericBossSunCondition");
            text.SetDefault("{0} flees as the sun rises...");
            AddTranslation(text);

            // Vanilla Boss Despawn Messages

            // King Slime
            text = CreateTranslation("KingSlimeWins");
            text.SetDefault("King Slime leaves in triumph...");
            AddTranslation(text);

            // Eye of Cthulhu
            text = CreateTranslation("EyeOfCthulhuWins");
            text.SetDefault("Eye of Cthulhu has disappeared into the night...");
            AddTranslation(text);

            // Eater of Worlds
            text = CreateTranslation("EaterOfWorldsWins");
            text.SetDefault("Eater of Worlds burrows back underground...");
            AddTranslation(text);

            // Brain of Cthulhu
            text = CreateTranslation("BrainOfCthulhuWins");
            text.SetDefault("Brain of Cthulhu vanishes into the pits of the crimson...");
            AddTranslation(text);

            // Queen Bee
            text = CreateTranslation("QueenBeeWins");
            text.SetDefault("Queen Bee returns to her colony's nest...");
            AddTranslation(text);

            // Skeletron
            text = CreateTranslation("SkeletronWins");
            text.SetDefault("Skeletron continues to torture the Old Man...");
            AddTranslation(text);

            // Wall of Flesh
            text = CreateTranslation("WallOfFleshWins");
            text.SetDefault("Wall of Flesh has managed to cross the underworld...");
            AddTranslation(text);

            // Retinazer
            text = CreateTranslation("RetinazerWins");
            text.SetDefault("Retinazer continues its observations...");
            AddTranslation(text);

            // Spazmatism
            text = CreateTranslation("SpazmatismWins");
            text.SetDefault("Spazmatism continues its observations...");
            AddTranslation(text);

            // The Destroyer
            text = CreateTranslation("DestroyerWins");
            text.SetDefault("The Destroyer seeks for another world to devour...");
            AddTranslation(text);

            // Skeletron Prime
            text = CreateTranslation("SkeletronPrimeWins");
            text.SetDefault("Skeletron Prime begins searching for a new victim...");
            AddTranslation(text);

            // Plantera
            text = CreateTranslation("PlanteraWins");
            text.SetDefault("Plantera continues its rest within the jungle...");
            AddTranslation(text);

            // Golem
            text = CreateTranslation("GolemWins");
            text.SetDefault("Golem deactivates in the bowels of the temple...");
            AddTranslation(text);

            // Duke Fishron
            text = CreateTranslation("DukeFishronWins");
            text.SetDefault("Duke Fishron returns to the ocean depths...");
            AddTranslation(text);

            // Lunatic Cultist
            text = CreateTranslation("LunaticCultistWins");
            text.SetDefault("Lunatic Cultist goes back to its devoted worship...");
            AddTranslation(text);

            // Moon Lord
            text = CreateTranslation("MoonLordWins");
            text.SetDefault("Moon Lord has left this realm...");
            AddTranslation(text);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (Main.LocalPlayer.dead) TimerUI.SetState(new RespawnTimer());
            else TimerUI.SetState(null);
        }

        public override void Unload()
        {
            instance = null;
        }

        public override void PostDrawFullscreenMap(ref string mouseText)
        {
            MapAssist.DrawFullscreenMap(this, ref mouseText);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int InventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Death Text"));
            if (InventoryIndex != -1)
            {
                layers.Insert(InventoryIndex + 1, new LegacyGameInterfaceLayer(
                    "Respawn Timer",
                    delegate
                    {
                        // If the current UIState of the UserInterface is null, nothing will draw. We don't need to track a separate .visible value.
                        TimerUI.Draw(Main.spriteBatch, new GameTime());
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
                else
                {
                    ErrorLogger.Log("BossAssist Call Error: AddBoss not found");
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