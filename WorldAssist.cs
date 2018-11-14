using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Localization;
using System.Collections.Generic;
using System.Linq;

namespace BossAssist
{
    public class WorldAssist : ModWorld
    {
        public static List<NPC> bossList = new List<NPC>();
        public static List<int> bossListPos = new List<int>();

        public static List<int> ModBossTypes = new List<int>();
        public static List<string> ModBossMessages = new List<string>();
        bool ModBossDetected = false;

        string key;

        string EventKey = "";
        bool isBloodMoon = false;
        bool isPumpkinMoon = false;
        bool isFrostMoon = false;
        bool isEclipse = false;

        public override void PreUpdate()
        {
            for (int n = 0; n < Main.maxNPCs; n++)
            {
                if (Main.npc[n].active && !bossList.Contains(Main.npc[n]) && !bossListPos.Contains(n) && (Main.npc[n].boss || Main.npc[n].type == 13))
                {
                    // Make sure these "bosses" are not counted
                    if (Main.npc[n].type != NPCID.MoonLordHand && Main.npc[n].type != NPCID.MoonLordCore && Main.npc[n].type != NPCID.MartianSaucerCore)
                    {
                        bossList.Add(Main.npc[n]);
                        bossListPos.Add(n);
                    }
                    // NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(Main.npc[n].FullName + " Added!"), Colors.RarityRed);
                }
            }

            for (int b = bossList.Count - 1; b >= 0; b--)
            {
                if (!Main.npc[bossListPos[b]].active)  // If boss is no longer active, check for despawn conditions and remove NPC from the list
                {
                    if ((Main.npc[bossListPos[b]].type != NPCID.MoonLordHead && Main.npc[bossListPos[b]].life >= 0)
                    || (Main.npc[bossListPos[b]].type == NPCID.MoonLordHead && Main.npc[bossListPos[b]].life < 0))
                    {
                        if (Main.player.Any(playerCheck => playerCheck.active && !playerCheck.dead)) // If any player is active and alive
                        {
                            if (Main.dayTime && (bossList[b].type == 4 || bossList[b].type == 125 || bossList[b].type == 126 || bossList[b].type == 134))
                            {
                                // Bosses that despawn upon day time: EoC, Retinazar, Spazmatism, The Destroyer
                                key = "Mods.BossAssist.GenericBossSunCondition";
                            }
                            else if (bossList[b].type == NPCID.WallofFlesh) key = "Mods.BossAssist.WallOfFleshWins";
                            else key = "Mods.BossAssist.GenericBossLeft";
                        }
                        else
                        {
                            if (bossList[b].type == NPCID.KingSlime) key = "Mods.BossAssist.KingSlimeWins";
                            else if (bossList[b].type == NPCID.EyeofCthulhu) key = "Mods.BossAssist.EyeOfCthulhuWins";
                            else if (bossList[b].type == NPCID.EaterofWorldsHead) key = "Mods.BossAssist.EaterOfWorldsWins";
                            else if (bossList[b].type == NPCID.BrainofCthulhu) key = "Mods.BossAssist.BrainOfCthulhuWins";
                            else if (bossList[b].type == NPCID.QueenBee) key = "Mods.BossAssist.QueenBeeWins";
                            else if (bossList[b].type == NPCID.SkeletronHead) key = "Mods.BossAssist.SkeletronWins";
                            else if (bossList[b].type == NPCID.WallofFlesh) key = "Mods.BossAssist.WallOfFleshWins";
                            else if (bossList[b].type == NPCID.Retinazer) key = "Mods.BossAssist.RetinazerWins";
                            else if (bossList[b].type == NPCID.Spazmatism) key = "Mods.BossAssist.SpazmatismWins";
                            else if (bossList[b].type == NPCID.TheDestroyer) key = "Mods.BossAssist.TheDestroyerWins";
                            else if (bossList[b].type == NPCID.SkeletronPrime) key = "Mods.BossAssist.SkeletronPrimeWins";
                            else if (bossList[b].type == NPCID.Plantera) key = "Mods.BossAssist.PlanteraWins";
                            else if (bossList[b].type == NPCID.Golem) key = "Mods.BossAssist.GolemWins";
                            else if (bossList[b].type == NPCID.DukeFishron) key = "Mods.BossAssist.DukeFishronWins";
                            else if (bossList[b].type == NPCID.CultistBoss) key = "Mods.BossAssist.LunaticCultistWins";
                            else if (bossList[b].type == NPCID.MoonLordHead) key = "Mods.BossAssist.MoonLordWins";
                            else
                            {
                                for (int i = 0; i < ModBossTypes.Count; i++)
                                {
                                    if (bossList[b].type == ModBossTypes[i])
                                    {
                                        // Main.NewText("Modded Boss Detected!");
                                        key = ModBossMessages[i];
                                        ModBossDetected = true;
                                        break;
                                    }
                                    else
                                    {
                                        // Main.NewText("Checked all instances inside ModBossTypes, but none were found");
                                        key = "Mods.BossAssist.GenericBossWins";
                                    }
                                }
                            }
                        }
                        if (Main.netMode == 0)
                        {
                            if (ModBossDetected)
                            {
                                ModBossDetected = false;
                                Main.NewText(string.Format(bossList[b].GetFullNetName().ToString() + key), Colors.RarityPurple);
                            }
                            else Main.NewText(string.Format(Language.GetTextValue(key), bossList[b].GetFullNetName().ToString()), Colors.RarityPurple);
                        }
                        else
                        {
                            if (ModBossDetected)
                            {
                                ModBossDetected = false;
                                NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(bossList[b].GetFullNetName() + key), Colors.RarityPurple);
                            }
                            else NetMessage.BroadcastChatMessage(NetworkText.FromKey(key, bossList[b].FullName), Colors.RarityPurple);
                        }
                    }
                    bossList.RemoveAt(b);
                    bossListPos.RemoveAt(b);
                    // NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(bossList[b].FullName + " Removed!"), Colors.RarityRed);
                }
            }
        }

        public override void PostUpdate()
        {
            if (Main.bloodMoon) isBloodMoon = true;
            if (Main.snowMoon) isFrostMoon = true;
            if (Main.pumpkinMoon) isPumpkinMoon = true;
            if (Main.eclipse) isEclipse = true;

            if (Main.dayTime && isBloodMoon)
            {
                isBloodMoon = false;
                EventKey = "Mods.BossAssist.BMoonEnd";
            }
            else if (Main.dayTime && isFrostMoon)
            {
                isFrostMoon = false;
                EventKey = "Mods.BossAssist.FMoonEnd";
            }
            else if (Main.dayTime && isPumpkinMoon)
            {
                isPumpkinMoon = false;
                EventKey = "Mods.BossAssist.PMoonEnd";
            }
            else if (!Main.dayTime && isEclipse)
            {
                isEclipse = false;
                EventKey = "Mods.BossAssist.EclipseEnd";
            }

            if (EventKey != "")
            {
                if (Main.netMode == 0) Main.NewText(Language.GetTextValue(EventKey), Colors.RarityGreen); // Single Player
                else NetMessage.BroadcastChatMessage(NetworkText.FromKey(EventKey), Colors.RarityGreen); // Multiplayer
                EventKey = "";
            }
        }
    }
}