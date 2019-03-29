using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BossAssist
{
    class NPCAssist : GlobalNPC
    {
        public override void NPCLoot(NPC npc)
        {
            if (npc.type == NPCID.DD2Betsy) WorldAssist.downedBetsy = true;

            string partName = npc.GetFullNetName().ToString();
            if (npc.type == NPCID.LunarTowerSolar || npc.type == NPCID.LunarTowerVortex || npc.type == NPCID.LunarTowerNebula || npc.type == NPCID.LunarTowerStardust)
            {
                if (Main.netMode == 0) Main.NewText("The " + npc.GetFullNetName().ToString() + " has been destroyed", Colors.RarityPurple);
                else NetMessage.BroadcastChatMessage(NetworkText.FromLiteral("The " + npc.GetFullNetName().ToString() + " has been destroyed"), Colors.RarityPurple);
            }
            else if (CheckForNPCType(npc))
            {
                if (npc.type == NPCID.SkeletronHand) partName = "Skeletron Hand";
                if (Main.netMode == 0) Main.NewText("The " + partName + " is down!", Colors.RarityGreen);
                else NetMessage.BroadcastChatMessage(NetworkText.FromLiteral("The " + npc.FullName + " is down!"), Colors.RarityGreen);
            }
            
            // Setting a record for fastest boss kill, and counting boss kills
            // Twins check makes sure the other is not around before counting towards the record
            if (SpecialBossCheck(npc) != -1 && npc.playerInteraction[Main.myPlayer]) // Requires the player to participate in the boss fight
            {
                Player player = Main.LocalPlayer;
                int currentRecord = player.GetModPlayer<PlayerAssist>().AllBossRecords[SpecialBossCheck(npc)].stat.fightTime;
                int recordAttempt = WorldAssist.RecordTimers[SpecialBossCheck(npc)];
                int currentBrink = player.GetModPlayer<PlayerAssist>().AllBossRecords[SpecialBossCheck(npc)].stat.brink;
                int brinkAttempt = WorldAssist.BrinkChecker[SpecialBossCheck(npc)];
                int MaxLife = WorldAssist.MaxHealth[SpecialBossCheck(npc)];

                if (EaterOfWorldsCheck(npc))
                {
                    player.GetModPlayer<PlayerAssist>().AllBossRecords[SpecialBossCheck(npc)].stat.kills++;
                    if (recordAttempt < currentRecord || currentRecord == 0)
                    {
                        player.GetModPlayer<PlayerAssist>().AllBossRecords[SpecialBossCheck(npc)].stat.fightTime = recordAttempt;
                    }
                    if (brinkAttempt < currentBrink || currentBrink == 0)
                    {
                        player.GetModPlayer<PlayerAssist>().AllBossRecords[SpecialBossCheck(npc)].stat.brink = brinkAttempt;
                        double newHealth = (double)brinkAttempt / (double)MaxLife; // Casts may be redundant, but this setup doesn't work without them.
                        player.GetModPlayer<PlayerAssist>().AllBossRecords[SpecialBossCheck(npc)].stat.brinkPercent = (int)(newHealth * 100);
                    }
                    if ((recordAttempt < currentRecord || currentRecord == 0) || (brinkAttempt < currentBrink || currentBrink == 0))
                    {
                        Rectangle rect = new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height);
                        CombatText.NewText(rect, Color.LightYellow, "New Record!", true);
                    }
                }
            }
            base.NPCLoot(npc);
        }

        public static int GetListNum(NPC boss)
        {
            List<BossInfo> BL = BossAssist.instance.setup.SortedBosses;
            if (boss.type < Main.maxNPCTypes) return BL.FindIndex(x => x.name == boss.FullName && x.source == "Vanilla");
            else return BL.FindIndex(x => x.name == boss.FullName && x.source == boss.modNPC.mod.Name);
        }

        public bool CheckForNPCType(NPC npcType)
        {
            return npcType.type == NPCID.PrimeSaw
                || npcType.type == NPCID.PrimeLaser
                || npcType.type == NPCID.PrimeCannon
                || npcType.type == NPCID.PrimeVice
                || npcType.type == NPCID.SkeletronHand
                || npcType.type == NPCID.GolemFistLeft
                || npcType.type == NPCID.GolemFistRight
                || npcType.type == NPCID.GolemHead
                || (npcType.type == NPCID.Retinazer && Main.npc.Any(otherBoss => otherBoss.type == NPCID.Spazmatism && otherBoss.active))
                || (npcType.type == NPCID.Spazmatism && Main.npc.Any(otherBoss => otherBoss.type == NPCID.Retinazer && otherBoss.active));
        }

        public int SpecialBossCheck(NPC npc)
        {
            List<BossInfo> BL = BossAssist.instance.setup.SortedBosses;

            if (npc.type == NPCID.MoonLordCore) return BL.FindIndex(x => x.name == "Moon Lord" && x.source == "Vanilla");
            if (TwinsCheck(npc)) return BL.FindIndex(x => x.name == "The Twins" && x.source == "Vanilla");
            else return GetListNum(npc);
        }

        public bool TwinsCheck(NPC npc)
        {
            if (npc.type == NPCID.Retinazer)
            {
                return (Main.npc.All(otherBoss => otherBoss.type != NPCID.Spazmatism))
                    || (Main.npc.Any(otherBoss => otherBoss.type == NPCID.Spazmatism && (!otherBoss.active || otherBoss.life <= 0)));
            }
            if (npc.type == NPCID.Spazmatism)
            {
                return (Main.npc.All(otherBoss => otherBoss.type != NPCID.Retinazer))
                    || (Main.npc.Any(otherBoss => otherBoss.type == NPCID.Retinazer && (!otherBoss.active || otherBoss.life <= 0)));
            }
            return false; // Neither Boss was selected
        }

        public bool EaterOfWorldsCheck(NPC npc)
        {
            return ((npc.type >= 13 && npc.type <= 15) && npc.boss) || (npc.type != 13 && npc.type != 14 && npc.type != 15);
        }
    }
}
