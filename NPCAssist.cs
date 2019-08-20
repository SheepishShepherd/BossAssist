using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
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
			if (npc.type == NPCID.DD2Betsy)
            {
                WorldAssist.downedBetsy = true;
                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.WorldData); // Immediately inform clients of new world state.
                }
            }

            string partName = npc.GetFullNetName().ToString();
			if (BossAssist.ClientConfig.PillarMessages)
			{
				if (npc.type == NPCID.LunarTowerSolar || npc.type == NPCID.LunarTowerVortex || npc.type == NPCID.LunarTowerNebula || npc.type == NPCID.LunarTowerStardust)
				{
					if (Main.netMode == 0) Main.NewText("The " + npc.GetFullNetName().ToString() + " has been destroyed", Colors.RarityPurple);
					else NetMessage.BroadcastChatMessage(NetworkText.FromLiteral("The " + npc.GetFullNetName().ToString() + " has been destroyed"), Colors.RarityPurple);
				}
			}
            if (NPCisLimb(npc) && BossAssist.ClientConfig.LimbMessages)
            {
                if (npc.type == NPCID.SkeletronHand) partName = "Skeletron Hand";
                if (Main.netMode == 0) Main.NewText("The " + partName + " is down!", Colors.RarityGreen);
                else NetMessage.BroadcastChatMessage(NetworkText.FromLiteral("The " + npc.FullName + " is down!"), Colors.RarityGreen);
            }
            
            // Setting a record for fastest boss kill, and counting boss kills
            // Twins check makes sure the other is not around before counting towards the record
            if (SpecialBossCheck(npc) != -1) // Requires the player to participate in the boss fight
			{
				if (EaterOfWorldsCheck(npc))
                {
					if (Main.netMode == NetmodeID.SinglePlayer && npc.playerInteraction[Main.myPlayer])
					{
						Player player = Main.player[Main.myPlayer];
						CheckRecords(npc, player, PlayerAssist.Get(player, mod));
					}
					else
					{
						CheckRecordsMultiplayer(npc);
					}
                }
            }
        }

		public void CheckRecords(NPC npc, Player player, PlayerAssist modplayer)
		{
            int recordIndex = SpecialBossCheck(npc);
            int recordAttempt = modplayer.RecordTimers[recordIndex]; // Trying to set a new record
			int currentRecord = modplayer.AllBossRecords[recordIndex].stat.fightTime;
			int worstRecord = modplayer.AllBossRecords[recordIndex].stat.fightTime2;

			modplayer.AllBossRecords[recordIndex].stat.fightTimeL = recordAttempt;

			int brinkAttempt = modplayer.BrinkChecker[recordIndex]; // Trying to set a new record
			int MaxLife = modplayer.MaxHealth[recordIndex];
			int currentBrink = modplayer.AllBossRecords[recordIndex].stat.brink2;
			int worstBrink = modplayer.AllBossRecords[recordIndex].stat.brink;

			modplayer.AllBossRecords[recordIndex].stat.brinkL = brinkAttempt;
			double lastHealth = (double)brinkAttempt / (double)MaxLife;
			modplayer.AllBossRecords[recordIndex].stat.brinkPercentL = (int)(lastHealth * 100);

			int dodgeTimeAttempt = modplayer.DodgeTimer[recordIndex];
			int currentDodgeTime = modplayer.AllBossRecords[recordIndex].stat.dodgeTime;
			int dodgeAttempt = modplayer.AttackCounter[recordIndex];
			int currentDodges = modplayer.AllBossRecords[recordIndex].stat.totalDodges;
			int worstDodges = modplayer.AllBossRecords[recordIndex].stat.totalDodges2;

			modplayer.AllBossRecords[recordIndex].stat.dodgeTimeL = dodgeTimeAttempt;
			modplayer.AllBossRecords[recordIndex].stat.totalDodgesL = dodgeAttempt;

			// Increase kill count
			modplayer.AllBossRecords[recordIndex].stat.kills++;

			if (recordAttempt < currentRecord && currentRecord != 0 && worstRecord <= 0)
			{
				// First make the current record the worst record if no worst record has been made and a new record was made
				modplayer.AllBossRecords[recordIndex].stat.fightTime2 = currentRecord;
			}
			if (recordAttempt < currentRecord || currentRecord <= 0)
			{
				//The player has beaten their best record, so we have to overwrite the old record with the new one
				modplayer.AllBossRecords[recordIndex].stat.fightTime = recordAttempt;
			}
			else if (recordAttempt > worstRecord || worstRecord <= 0)
			{
				//The player has beaten their worst record, so we have to overwrite the old record with the new one
				modplayer.AllBossRecords[recordIndex].stat.fightTime2 = recordAttempt;
			}

			if (brinkAttempt > currentBrink && currentBrink != 0 && worstBrink <= 0)
			{
				modplayer.AllBossRecords[recordIndex].stat.brink = currentBrink;
			}
			if (brinkAttempt > currentBrink || currentBrink <= 0)
			{
				modplayer.AllBossRecords[recordIndex].stat.brink2 = brinkAttempt;
				double newHealth = (double)brinkAttempt / (double)MaxLife; // Casts may be redundant, but this setup doesn't work without them.
				modplayer.AllBossRecords[recordIndex].stat.brinkPercent2 = (int)(newHealth * 100);
			}
			else if (brinkAttempt < worstBrink || worstBrink <= 0)
			{
				modplayer.AllBossRecords[recordIndex].stat.brink = brinkAttempt;
				double newHealth = (double)brinkAttempt / (double)MaxLife; // Casts may be redundant, but this setup doesn't work without them.
				modplayer.AllBossRecords[recordIndex].stat.brinkPercent = (int)(newHealth * 100);
			}

			if (dodgeTimeAttempt > currentDodgeTime || currentDodgeTime < 0)
			{
				// There is no "worse record" for this one so just overwrite any better records made
				modplayer.AllBossRecords[recordIndex].stat.dodgeTime = dodgeTimeAttempt;
			}

			if (dodgeAttempt < currentDodges || currentDodges < 0)
			{
				modplayer.AllBossRecords[recordIndex].stat.totalDodges = dodgeAttempt;
				if (worstDodges == 0) modplayer.AllBossRecords[recordIndex].stat.totalDodges2 = currentDodges;
			}
			else if (dodgeAttempt > worstDodges || worstDodges < 0)
			{
				modplayer.AllBossRecords[recordIndex].stat.totalDodges2 = dodgeAttempt;
			}

			modplayer.DodgeTimer[recordIndex] = 0;
			modplayer.AttackCounter[recordIndex] = 0;

			// If a new record was made, notify the player
			if ((recordAttempt < currentRecord || currentRecord <= 0) || (brinkAttempt > currentBrink || currentBrink <= 0) || (dodgeAttempt < currentDodges || dodgeAttempt <= 0))
			{
				CombatText.NewText(player.getRect(), Color.LightYellow, "New Record!", true);
			}
		}

		public void CheckRecordsMultiplayer(NPC npc)
		{
            int recordIndex = SpecialBossCheck(npc);
            for (int i = 0; i < 255; i++)
			{
				Player player = Main.player[i];
                PlayerAssist modPlayer = player.GetModPlayer<PlayerAssist>();

                if (!player.active || !npc.playerInteraction[i]) continue; // Players must be active AND have interacted with the boss
				if (Main.netMode == NetmodeID.Server)
				{
					Console.WriteLine("<<<<<<<<<<<<<<<<<<< Starting the Server NPCLoot stuff");
					List<BossStats> list = BossAssist.ServerCollectedRecords[i];
					BossStats oldRecord = list[recordIndex];

					// Establish the new records for comparing

					BossStats newRecord = new BossStats()
					{
						fightTimeL = modPlayer.RecordTimers[recordIndex],
						totalDodgesL = modPlayer.AttackCounter[recordIndex],
						dodgeTimeL = modPlayer.DodgeTimer[recordIndex],
						brinkL = modPlayer.BrinkChecker[recordIndex],
							
						brinkPercentL = (int)(((double)modPlayer.BrinkChecker[recordIndex] / modPlayer.MaxHealth[recordIndex]) * 100),
					};

					Console.WriteLine("<<<<<<<<<<<<<<<<<<< Declared new and old records");

					// Compare the records

					RecordID specificRecord = RecordID.None;

					if (newRecord.fightTimeL < oldRecord.fightTime)
					{
						specificRecord |= RecordID.ShortestFightTime;
						BossAssist.ServerCollectedRecords[i][recordIndex].fightTime = newRecord.fightTime;
					}
					if (newRecord.fightTimeL > oldRecord.fightTime2)
					{
						specificRecord |= RecordID.LongestFightTime;
						BossAssist.ServerCollectedRecords[i][recordIndex].fightTime2 = newRecord.fightTime2;
					}
					BossAssist.ServerCollectedRecords[i][recordIndex].fightTimeL = newRecord.fightTimeL;

					if (newRecord.brink2 > oldRecord.brink2)
					{
						specificRecord |= RecordID.BestBrink;
						BossAssist.ServerCollectedRecords[i][recordIndex].brink2 = newRecord.brink;
						BossAssist.ServerCollectedRecords[i][recordIndex].brinkPercent2 = newRecord.brinkPercent2;
					}
					if (newRecord.brink < oldRecord.brink)
					{
						specificRecord |= RecordID.WorstBrink;
						BossAssist.ServerCollectedRecords[i][recordIndex].brink = newRecord.brink;
						BossAssist.ServerCollectedRecords[i][recordIndex].brinkPercent = newRecord.brinkPercent;
					}
					BossAssist.ServerCollectedRecords[i][recordIndex].brinkL = newRecord.brinkL;
					BossAssist.ServerCollectedRecords[i][recordIndex].brinkPercentL = newRecord.brinkPercentL;

					if (newRecord.totalDodges < oldRecord.totalDodges)
					{
						specificRecord |= RecordID.LeastHits;
						BossAssist.ServerCollectedRecords[i][recordIndex].totalDodges = newRecord.totalDodges;
					}
					if (newRecord.totalDodges2 > oldRecord.totalDodges2)
					{
						specificRecord |= RecordID.MostHits;
						BossAssist.ServerCollectedRecords[i][recordIndex].totalDodges2 = newRecord.totalDodges2;
					}
					BossAssist.ServerCollectedRecords[i][recordIndex].totalDodgesL = newRecord.totalDodgesL;

					if (newRecord.dodgeTime > oldRecord.dodgeTime)
					{
						specificRecord |= RecordID.DodgeTime;
						BossAssist.ServerCollectedRecords[i][recordIndex].dodgeTime = newRecord.dodgeTime;
					}
					BossAssist.ServerCollectedRecords[i][recordIndex].dodgeTimeL = newRecord.dodgeTimeL;

					Console.WriteLine("<<<<<<<<<<<<<<<<<<< Updated recrods apporiately");
					// Make the packet

					ModPacket packet = mod.GetPacket();
					packet.Write((byte)MessageType.RecordUpdate);

					packet.Write((int)specificRecord);
					packet.Write((int)recordIndex);
					// Kills update by 1 automatically
					// Deaths have to be sent elsewhere (NPCLoot wont run if the player dies)

					if (specificRecord.HasFlag(RecordID.ShortestFightTime)) packet.Write(newRecord.fightTime);
					if (specificRecord.HasFlag(RecordID.LongestFightTime)) packet.Write(newRecord.fightTime2);
					packet.Write(newRecord.fightTimeL);

					if (specificRecord.HasFlag(RecordID.BestBrink))
					{
						packet.Write(newRecord.brink2);
						packet.Write(newRecord.brinkPercent2);
					}
					if (specificRecord.HasFlag(RecordID.WorstBrink))
					{
						packet.Write(newRecord.brink);
						packet.Write(newRecord.brinkPercent);
					}
					packet.Write(newRecord.brinkL);
					packet.Write(newRecord.brinkPercentL);

					if (specificRecord.HasFlag(RecordID.LeastHits)) packet.Write(newRecord.totalDodges);
					if (specificRecord.HasFlag(RecordID.MostHits)) packet.Write(newRecord.totalDodges2);
					packet.Write(newRecord.totalDodgesL);
					if (specificRecord.HasFlag(RecordID.DodgeTime)) packet.Write(newRecord.dodgeTime);
					packet.Write(newRecord.dodgeTimeL);
					Console.WriteLine("<<<<<<<<<<<<<<<<<<< Making Packet");

					// ORDER MATTERS
					packet.Send(toClient: i);
					Console.WriteLine("<<<<<<<<<<<<<<<<<<< PACKET SENT!");
				}
			}
		}

		public static int GetListNum(NPC boss)
        {
            List<BossInfo> BL = BossAssist.instance.setup.SortedBosses;
            if (boss.type == NPCID.MoonLordCore) return BL.FindIndex(x => x.id == NPCID.MoonLordHead);
            if (boss.type == NPCID.Spazmatism) return BL.FindIndex(x => x.id == NPCID.Retinazer);
            if (boss.type < Main.maxNPCTypes) return BL.FindIndex(x => x.id == boss.type);
            else return BL.FindIndex(x => x.name == boss.FullName && x.source == boss.modNPC.mod.Name);
        }

        public bool NPCisLimb(NPC npcType)
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

        public static int SpecialBossCheck(NPC npc)
        {
            List<BossInfo> BL = BossAssist.instance.setup.SortedBosses;

            if (npc.type == NPCID.MoonLordCore) return BL.FindIndex(x => x.id == NPCID.MoonLordHead);
            else if (TwinsCheck(npc)) return BL.FindIndex(x => x.id == NPCID.Retinazer);
            else return GetListNum(npc);
        }

        public static bool TwinsCheck(NPC npc)
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

		
		public override void OnChatButtonClicked(NPC npc, bool firstButton)
		{
			if (npc.type == NPCID.Dryad && !firstButton)
			{
				MapAssist.LocateNearestEvil();
			}
		}
		
        public void UpdateRecordServerSide(int fromWho, NPC npc, string recordType, int recordValue)
		{
			ModPacket packet = mod.GetPacket(); // Create a packet
			packet.Write(npc.whoAmI);
			packet.Write(recordType);
			packet.Write(recordValue);
			packet.Send(fromWho); // Send it to the record maker's client?

			NetMessage.BroadcastChatMessage(NetworkText.FromLiteral("Packet Sent"), Colors.RarityPurple);
		}
	}
}
