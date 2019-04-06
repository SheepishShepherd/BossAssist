using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Localization;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

/*
 * Twins broken despawn messages (of course) 
 * 
 * 
 */


namespace BossAssist
{
    public class WorldAssist : ModWorld
    {
        public static bool downedBetsy;

        public static List<bool> ActiveBossesList = new List<bool>();
        public static List<int> RecordTimers = new List<int>();
        public static List<int> BrinkChecker = new List<int>();
        public static List<int> MaxHealth = new List<int>();
        public static List<bool> DeathTracker = new List<bool>();
        public static List<int> DodgeTimer = new List<int>(); // Turn into List
        public static List<int> AttackCounter = new List<int>(); // Amount of attacks taken

        public static List<int> ModBossTypes = new List<int>();
        public static List<string> ModBossMessages = new List<string>();
        
        string EventKey = "";
        bool isBloodMoon = false;
        bool isPumpkinMoon = false;
        bool isFrostMoon = false;
        bool isEclipse = false;
        
        public override void PreUpdate()
        {
            List<BossInfo> BL = BossAssist.instance.setup.SortedBosses;
            ResetArrays(BL.Count);

            for (int n = 0; n < Main.maxNPCs; n++)
            {
                NPC b = Main.npc[n];

                // Bosses listed below are special cases
                ActiveBossesList[BL.FindIndex(x => x.id == NPCID.EaterofWorldsHead)] = Main.npc.Any(npc => (npc.type == 13 || npc.type == 14 || npc.type == 15) && npc.active);
                ActiveBossesList[BL.FindIndex(x => x.id == NPCID.Retinazer)] = Main.npc.Any(npc => (npc.type == NPCID.Spazmatism || npc.type == NPCID.Retinazer) && npc.active);
                ActiveBossesList[BL.FindIndex(x => x.id == NPCID.MoonLordHead)] = Main.npc.Any(npc => (npc.type == NPCID.MoonLordCore || npc.type == NPCID.MoonLordHand || npc.type == NPCID.MoonLordHead) && npc.active);

                if (NPCAssist.SpecialBossCheck(b) != -1)
                {
                    if (b.active)
                    {
                        ActiveBossesList[NPCAssist.GetListNum(b)] = true;
                        if (Main.LocalPlayer.dead) DeathTracker[NPCAssist.GetListNum(b)] = true;
                    }
                    else if (!b.active && Main.npc.All(npc => (npc.type == b.type && !npc.active) || npc.type != b.type)) // <INACTIVE NPC>
                    {
                        if (ActiveBossesList[NPCAssist.GetListNum(b)])
                        {
                            if ((b.type != NPCID.MoonLordHead && b.life >= 0) || (b.type == NPCID.MoonLordHead && b.life < 0))
                            {
                                if (Main.netMode == NetmodeID.SinglePlayer) Main.NewText(GetDespawnMessage(b), Colors.RarityPurple);
                                else NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(GetDespawnMessage(b)), Colors.RarityPurple);
                            }
                        }
                        ActiveBossesList[NPCAssist.GetListNum(b)] = false;
                    }
                }
            }

         /// Record Timers
            for (int active = 0; active < ActiveBossesList.Count; active++)
            {
                if (!Main.LocalPlayer.dead)
                {
                    if (ActiveBossesList[active])
                    {
                        RecordTimers[active]++;
                        DodgeTimer[active]++;
                        MaxHealth[active] = Main.LocalPlayer.statLifeMax2;
                        if (BrinkChecker[active] == 0 || (Main.LocalPlayer.statLife < BrinkChecker[active] && Main.LocalPlayer.statLife > 0))
                        {
                            BrinkChecker[active] = Main.LocalPlayer.statLife;
                        }
                    }
                    else
                    {
                        MaxHealth[active] = Main.LocalPlayer.statLifeMax2;
                        RecordTimers[active] = 0;
                        BrinkChecker[active] = 0;
                        DodgeTimer[active] = 0;
                    }
                }
                else
                {
                    MaxHealth[active] = Main.LocalPlayer.statLifeMax2;
                    RecordTimers[active] = 0;
                    BrinkChecker[active] = 0;
                    DodgeTimer[active] = 0;
                }
            }
        }

        public override void PostUpdate()
        {
            // Boss Collections
            for (int i = 0; i < Main.LocalPlayer.GetModPlayer<PlayerAssist>().BossTrophies.Count; i++)
            {
                for (int j = 0; j < Main.LocalPlayer.GetModPlayer<PlayerAssist>().BossTrophies[i].itemList.Count; j++)
                {
                    if (Main.LocalPlayer.HasItem(Main.LocalPlayer.GetModPlayer<PlayerAssist>().BossTrophies[i].itemList[j].type))
                    {
                        Main.LocalPlayer.GetModPlayer<PlayerAssist>().BossTrophies[i].checkList[j] = true;
                    }
                }
            }

            // Event Ending Messages
            if (Main.bloodMoon) isBloodMoon = true;
            if (Main.snowMoon) isFrostMoon = true;
            if (Main.pumpkinMoon) isPumpkinMoon = true;
            if (Main.eclipse) isEclipse = true;

            if (!Main.bloodMoon && isBloodMoon)
            {
                isBloodMoon = false;
                EventKey = "The Blood Moon falls past the horizon...";
            }
            else if (!Main.snowMoon && isFrostMoon)
            {
                isFrostMoon = false;
                EventKey = "The Frost Moon melts as the sun rises...";
            }
            else if (!Main.pumpkinMoon && isPumpkinMoon)
            {
                isPumpkinMoon = false;
                EventKey = "The Pumpkin Moon ends its harvest...";
            }
            else if (!Main.eclipse && isEclipse)
            {
                isEclipse = false;
                EventKey = "The solar eclipse has ended... until next time...";
            }

            if (EventKey != "")
            {
                if (Main.netMode == NetmodeID.SinglePlayer) Main.NewText(EventKey, Colors.RarityGreen);
                else NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(EventKey), Colors.RarityGreen);
                EventKey = "";
            }
        }

        public void ResetArrays(int shortcut)
        {
            if (ActiveBossesList.Count != shortcut)
            {
                while (ActiveBossesList.Count > shortcut) ActiveBossesList.RemoveAt(ActiveBossesList.Count - 1);
                while (ActiveBossesList.Count < shortcut) ActiveBossesList.Add(false);
            }
            if (DeathTracker.Count != shortcut)
            {
                while (DeathTracker.Count > shortcut) DeathTracker.RemoveAt(DeathTracker.Count - 1);
                while (DeathTracker.Count < shortcut) DeathTracker.Add(false);
            }
            if (RecordTimers.Count != shortcut)
            {
                while (RecordTimers.Count > shortcut) RecordTimers.RemoveAt(RecordTimers.Count - 1);
                while (RecordTimers.Count < shortcut) RecordTimers.Add(0);
            }
            if (BrinkChecker.Count != shortcut)
            {
                while (BrinkChecker.Count > shortcut) BrinkChecker.RemoveAt(BrinkChecker.Count - 1);
                while (BrinkChecker.Count < shortcut) BrinkChecker.Add(0);
            }
            if (MaxHealth.Count != shortcut)
            {
                while (MaxHealth.Count > shortcut) MaxHealth.RemoveAt(MaxHealth.Count - 1);
                while (MaxHealth.Count < shortcut) MaxHealth.Add(0);
            }
            if (AttackCounter.Count != shortcut)
            {
                while (AttackCounter.Count > shortcut) AttackCounter.RemoveAt(MaxHealth.Count - 1);
                while (AttackCounter.Count < shortcut) AttackCounter.Add(0);
            }
            if (DodgeTimer.Count != shortcut)
            {
                while (DodgeTimer.Count > shortcut) DodgeTimer.RemoveAt(MaxHealth.Count - 1);
                while (DodgeTimer.Count < shortcut) DodgeTimer.Add(0);
            }
        }

        public string GetDespawnMessage(NPC boss)
        {
            if (Main.player.Any(playerCheck => playerCheck.active && !playerCheck.dead)) // If any player is active and alive
            {
                if (Main.dayTime && (boss.type == NPCID.EyeofCthulhu || boss.type == NPCID.TheDestroyer || boss.type == NPCID.Retinazer || boss.type == NPCID.Spazmatism))
                {
                    return boss.FullName + " flees as the sun rises...";
                }
                else if (boss.type == NPCID.WallofFlesh) return "Wall of Flesh has managed to cross the underworld...";
                else return boss.FullName + " is no longer after you...";
            }
            else
            {
                if (boss.type == NPCID.KingSlime) return "King Slime leaves in triumph...";
                else if (boss.type == NPCID.EyeofCthulhu) return "Eye of Cthulhu has disappeared into the night...";
                else if (boss.type == NPCID.EaterofWorldsHead) return "Eater of Worlds burrows back underground...";
                else if (boss.type == NPCID.BrainofCthulhu) return "Brain of Cthulhu vanishes into the pits of the crimson...";
                else if (boss.type == NPCID.QueenBee) return "Queen Bee returns to her colony's nest...";
                else if (boss.type == NPCID.SkeletronHead) return "Skeletron continues to torture the Old Man...";
                else if (boss.type == NPCID.WallofFlesh) return "Wall of Flesh has managed to cross the underworld...";
                else if (boss.type == NPCID.Retinazer) return "Retinazer continues its observations...";
                else if (boss.type == NPCID.Spazmatism) return "Spazmatism continues its observations...";
                else if (boss.type == NPCID.TheDestroyer) return "The Destroyer seeks for another world to devour...";
                else if (boss.type == NPCID.SkeletronPrime) return "Skeletron Prime begins searching for a new victim...";
                else if (boss.type == NPCID.Plantera) return "Plantera continues its rest within the jungle...";
                else if (boss.type == NPCID.Golem) return "Golem deactivates in the bowels of the temple...";
                else if (boss.type == NPCID.DukeFishron) return "Duke Fishron returns to the ocean depths...";
                else if (boss.type == NPCID.CultistBoss) return "Lunatic Cultist goes back to its devoted worship...";
                else if (boss.type == NPCID.MoonLordHead) return "Moon Lord has left this realm...";
                else
                {
                    for (int i = 0; i < ModBossTypes.Count; i++)
                    {
                        if (boss.type == ModBossTypes[i]) return ModBossMessages[i];
                        // If a mod has submitted a custom despawn message, it will display here
                    }
                    return boss.FullName + " has killed every player!";
                    // Otherwise it defaults to this
                }
            }
        }

        public override void Initialize()
        {
            downedBetsy = false;
        }

        public override TagCompound Save()
        {
            var downed = new List<string>();
            if (downedBetsy)
            {
                downed.Add("betsy");
            }

            return new TagCompound
            {
                {"downed", downed}
            };
        }

        public override void Load(TagCompound tag)
        {
            var downed = tag.GetList<string>("downed");
            downedBetsy = downed.Contains("betsy");
        }
    }
}