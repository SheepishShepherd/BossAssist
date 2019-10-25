using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Localization;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

namespace BossAssist
{
    public class WorldAssist : ModWorld
    {
        public static bool downedBetsy;
        public static List<bool> ActiveBossesList = new List<bool>();
        public static List<int> ModBossTypes = new List<int>();
        public static List<string> ModBossMessages = new List<string>();
        public static List<bool> ActiveSpecialBosses = new List<bool>();
        
        string EventKey = "";
        bool isBloodMoon = false;
        bool isPumpkinMoon = false;
        bool isFrostMoon = false;
        bool isEclipse = false;

        public override void PreUpdate()
        {
            List<BossInfo> BL = BossAssist.instance.setup.SortedBosses;
            PlayerAssist modPlayer = Main.LocalPlayer.GetModPlayer<PlayerAssist>();
            ResetArrays(BL.Count, modPlayer);

            // Bosses listed below are special cases
            ActiveBossesList[BL.FindIndex(x => x.id == NPCID.EaterofWorldsHead)] = Main.npc.Any(npc => (npc.type == 13 || npc.type == 14 || npc.type == 15) && npc.active);
            ActiveBossesList[BL.FindIndex(x => x.id == NPCID.Retinazer)] = Main.npc.Any(npc => (npc.type == NPCID.Spazmatism || npc.type == NPCID.Retinazer) && npc.active);
            ActiveBossesList[BL.FindIndex(x => x.id == NPCID.MoonLordHead)] = Main.npc.Any(npc => (npc.type == NPCID.MoonLordCore || npc.type == NPCID.MoonLordHand || npc.type == NPCID.MoonLordHead) && npc.active);
            
            for (int n = 0; n < Main.maxNPCs; n++)
            {
                NPC b = Main.npc[n];

                if (NPCAssist.GetListNum(b) != -1)
                {
                    if (NPCAssist.GetListNum(b) == BL.FindIndex(x => x.id == NPCID.EaterofWorldsHead)
                    || NPCAssist.GetListNum(b) == BL.FindIndex(x => x.id == NPCID.Retinazer)
                    || NPCAssist.GetListNum(b) == BL.FindIndex(x => x.id == NPCID.MoonLordHead))
                    {
                        if (ActiveBossesList[NPCAssist.GetListNum(b)])
                        {
                            if (Main.LocalPlayer.dead) modPlayer.DeathTracker[NPCAssist.GetListNum(b)] = true;
                            ActiveSpecialBosses[GetSpecialNum(NPCAssist.GetListNum(b))] = true;
                        }
                        else if (ActiveSpecialBosses[GetSpecialNum(NPCAssist.GetListNum(b))])
                        {
							if ((b.type != NPCID.MoonLordHead && b.life >= 0 && CheckRealLife(b.realLife)) || (b.type == NPCID.MoonLordHead && b.life <= 0))
							{
								if (!BossAssist.ClientConfig.ODespawnBool)
								{
									if (Main.netMode == NetmodeID.SinglePlayer) Main.NewText(GetDespawnMessage(b), Colors.RarityPurple);
									else NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(GetDespawnMessage(b)), Colors.RarityPurple);
								}
							}
                            ActiveSpecialBosses[GetSpecialNum(NPCAssist.GetListNum(b))] = false;
                        }
                    }
                    else
                    {
                        if (b.active)
                        {
                            ActiveBossesList[NPCAssist.GetListNum(b)] = true;
                            if (Main.LocalPlayer.dead) modPlayer.DeathTracker[NPCAssist.GetListNum(b)] = true;
                        }
                        else if (!b.active && Main.npc.All(npc => (npc.type == b.type && !npc.active) || npc.type != b.type)) // <INACTIVE NPC>
                        {
                            if (ActiveBossesList[NPCAssist.GetListNum(b)])
                            {
                                if ((b.type != NPCID.MoonLordHead && b.life >= 0) || (b.type == NPCID.MoonLordHead && b.life < 0))
								{
									if (!BossAssist.ClientConfig.ODespawnBool)
									{
										if (Main.netMode == NetmodeID.SinglePlayer) Main.NewText(GetDespawnMessage(b), Colors.RarityPurple);
										else NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(GetDespawnMessage(b)), Colors.RarityPurple);
									}
                                }
                            }
                            ActiveBossesList[NPCAssist.GetListNum(b)] = false;
                        }
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
                        modPlayer.RecordTimers[active]++;
                        modPlayer.DodgeTimer[active]++;
                        modPlayer.MaxHealth[active] = Main.LocalPlayer.statLifeMax2;
                        if (modPlayer.BrinkChecker[active] == 0 || (Main.LocalPlayer.statLife < modPlayer.BrinkChecker[active] && Main.LocalPlayer.statLife > 0))
                        {
                            modPlayer.BrinkChecker[active] = Main.LocalPlayer.statLife;
                        }
                    }
                    else
                    {
                        modPlayer.MaxHealth[active] = Main.LocalPlayer.statLifeMax2;
                        modPlayer.RecordTimers[active] = 0;
                        modPlayer.BrinkChecker[active] = 0;
                        modPlayer.DodgeTimer[active] = 0;
                    }
                }
                else
                {
                    modPlayer.MaxHealth[active] = Main.LocalPlayer.statLifeMax2;
                    modPlayer.RecordTimers[active] = 0;
                    modPlayer.BrinkChecker[active] = 0;
                    modPlayer.DodgeTimer[active] = 0;
                }
            }
        }

        public override void PostUpdate()
        {
            // Loot Collections
            for (int i = 0; i < BossAssist.instance.setup.SortedBosses.Count; i++)
            {
                for (int j = 0; j < BossAssist.instance.setup.SortedBosses[i].loot.Count; j++)
				{
					int item = BossAssist.instance.setup.SortedBosses[i].loot[j];
					if (Main.LocalPlayer.HasItem(item))
					{
						int BossIndex = Main.LocalPlayer.GetModPlayer<PlayerAssist>().BossTrophies.FindIndex(boss => boss.bossName == BossAssist.instance.setup.SortedBosses[i].name && boss.modName == BossAssist.instance.setup.SortedBosses[i].source);
						if (BossIndex == -1) continue;
						if (Main.LocalPlayer.GetModPlayer<PlayerAssist>().BossTrophies[i].loot.FindIndex(x => x.type == item) == -1)
						{
							Item newItem = new Item();
							newItem.SetDefaults(item);
							Main.LocalPlayer.GetModPlayer<PlayerAssist>().BossTrophies[i].loot.Add(newItem);
						}
                    }
                }
            }

			// Boss Collections
			for (int i = 0; i < BossAssist.instance.setup.SortedBosses.Count; i++)
			{
				for (int j = 0; j < BossAssist.instance.setup.SortedBosses[i].collection.Count; j++)
				{
					int item = BossAssist.instance.setup.SortedBosses[i].collection[j];
					if (Main.LocalPlayer.HasItem(item))
					{
						int BossIndex = Main.LocalPlayer.GetModPlayer<PlayerAssist>().BossTrophies.FindIndex(boss => boss.bossName == BossAssist.instance.setup.SortedBosses[i].name && boss.modName == BossAssist.instance.setup.SortedBosses[i].source);
						if (BossIndex == -1) continue;
						if (Main.LocalPlayer.GetModPlayer<PlayerAssist>().BossTrophies[i].collectibles.FindIndex(x => x.type == item) == -1)
						{
							Item newItem = new Item();
							newItem.SetDefaults(item);
							Main.LocalPlayer.GetModPlayer<PlayerAssist>().BossTrophies[i].collectibles.Add(newItem);
						}
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

        public string GetDespawnMessage(NPC boss)
        {
            if (Main.player.Any(playerCheck => playerCheck.active && !playerCheck.dead)) // If any player is active and alive
            {
                if (Main.dayTime && (boss.type == NPCID.EyeofCthulhu || boss.type == NPCID.TheDestroyer || boss.type == NPCID.Retinazer || boss.type == NPCID.Spazmatism))
                {
                    return boss.FullName + " flees as the sun rises...";
                }
                else if (boss.type == NPCID.WallofFlesh) return "Wall of Flesh has managed to cross the underworld...";
                else if (boss.type == NPCID.Retinazer) return "The Twins are no longer after you...";
                else return boss.FullName + " is no longer after you...";
            }
            else if (BossAssist.ClientConfig.CDespawnBool)
			{
				if (boss.type == NPCID.KingSlime) return "King Slime leaves in triumph...";
				else if (boss.type == NPCID.EyeofCthulhu) return "Eye of Cthulhu has disappeared into the night...";
				else if (boss.type == NPCID.EaterofWorldsHead) return "Eater of Worlds burrows back underground...";
				else if (boss.type == NPCID.BrainofCthulhu) return "Brain of Cthulhu vanishes into the pits of the crimson...";
				else if (boss.type == NPCID.QueenBee) return "Queen Bee returns to her colony's nest...";
				else if (boss.type == NPCID.SkeletronHead) return "Skeletron continues to torture the Old Man...";
				else if (boss.type == NPCID.WallofFlesh) return "Wall of Flesh has managed to cross the underworld...";
				else if (boss.type == NPCID.Retinazer) return "The Twins continue their observations...";
				else if (boss.type == NPCID.TheDestroyer) return "The Destroyer seeks for another world to devour...";
				else if (boss.type == NPCID.SkeletronPrime) return "Skeletron Prime begins searching for a new victim...";
				else if (boss.type == NPCID.Plantera) return "Plantera continues its rest within the jungle...";
				else if (boss.type == NPCID.Golem) return "Golem deactivates in the bowels of the temple...";
				else if (boss.type == NPCID.DukeFishron) return "Duke Fishron returns to the ocean depths...";
				else if (boss.type == NPCID.CultistBoss) return "Lunatic Cultist goes back to its devoted worship...";
				else if (boss.type == NPCID.MoonLordCore) return "Moon Lord has left this realm...";
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
			else return boss.FullName + " has killed every player!";
		}

        public void ResetArrays(int shortcut, PlayerAssist player)
        {
            if (ActiveBossesList.Count != shortcut)
            {
                while (ActiveBossesList.Count > shortcut) ActiveBossesList.RemoveAt(ActiveBossesList.Count - 1);
                while (ActiveBossesList.Count < shortcut) ActiveBossesList.Add(false);
            }
            if (player.DeathTracker.Count != shortcut)
            {
                while (player.DeathTracker.Count > shortcut) player.DeathTracker.RemoveAt(player.DeathTracker.Count - 1);
                while (player.DeathTracker.Count < shortcut) player.DeathTracker.Add(false);
            }
            if (player.RecordTimers.Count != shortcut)
            {
                while (player.RecordTimers.Count > shortcut) player.RecordTimers.RemoveAt(player.RecordTimers.Count - 1);
                while (player.RecordTimers.Count < shortcut) player.RecordTimers.Add(0);
            }
            if (player.BrinkChecker.Count != shortcut)
            {
                while (player.BrinkChecker.Count > shortcut) player.BrinkChecker.RemoveAt(player.BrinkChecker.Count - 1);
                while (player.BrinkChecker.Count < shortcut) player.BrinkChecker.Add(0);
            }
            if (player.MaxHealth.Count != shortcut)
            {
                while (player.MaxHealth.Count > shortcut) player.MaxHealth.RemoveAt(player.MaxHealth.Count - 1);
                while (player.MaxHealth.Count < shortcut) player.MaxHealth.Add(0);
            }
            if (player.AttackCounter.Count != shortcut)
            {
                while (player.AttackCounter.Count > shortcut) player.AttackCounter.RemoveAt(player.AttackCounter.Count - 1);
                while (player.AttackCounter.Count < shortcut) player.AttackCounter.Add(0);
            }
            if (player.DodgeTimer.Count != shortcut)
            {
                while (player.DodgeTimer.Count > shortcut) player.DodgeTimer.RemoveAt(player.DodgeTimer.Count - 1);
                while (player.DodgeTimer.Count < shortcut) player.DodgeTimer.Add(0);
            }
            while (ActiveSpecialBosses.Count != 3)
            {
                ActiveSpecialBosses.Add(false);
            }
        }

        public int GetSpecialNum(int specialPos)
        {
            if (specialPos == BossAssist.instance.setup.SortedBosses.FindIndex(x => x.id == NPCID.EaterofWorldsHead)) return 0;
            if (specialPos == BossAssist.instance.setup.SortedBosses.FindIndex(x => x.id == NPCID.Retinazer)) return 1;
            if (specialPos == BossAssist.instance.setup.SortedBosses.FindIndex(x => x.id == NPCID.MoonLordHead)) return 2;
            return -1; // Shouldn't get to this point
        }

        public override void Initialize()
        {
            downedBetsy = false;
        }

		public bool CheckRealLife(int realNPC)
		{
			if (realNPC == -1) return true;
			if (Main.npc[realNPC].life >= 0) return true;
			else return false;
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