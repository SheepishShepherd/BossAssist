using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ID;
using System.IO;
using System;

// Bug: ItemLists for loot and collections added in with add loot/collect calls do not get added to saved data

namespace BossAssist
{
    public class PlayerAssist : ModPlayer
    {
        public bool isNewPlayer = true;

        public List<BossRecord> AllBossRecords;
        public List<BossCollection> BossTrophies;

		public List<int> RecordTimers;
        public List<int> BrinkChecker;
        public List<int> MaxHealth;
        public List<bool> DeathTracker;
        public List<int> DodgeTimer; // Track the time in-between hits
        public List<int> AttackCounter;

        public override void Initialize()
        {
            AllBossRecords = new List<BossRecord>();
            foreach (BossInfo boss in BossAssist.instance.setup.SortedBosses)
            {
                AllBossRecords.Add(new BossRecord(boss.source, boss.name));
            }

            // Make a new list of collections
            BossTrophies = new List<BossCollection>();
            // For each boss added...
            foreach (BossInfo boss in BossAssist.instance.setup.SortedBosses)
            {
                // 1.) Add a collection for the boss
                BossTrophies.Add(new BossCollection(boss.source, boss.name));
                // 2.) setup the item list and check off list for the boss
                int index = BossTrophies.FindIndex(x => x.modName == boss.source && x.bossName == boss.name);
                BossTrophies[index].loot = new List<Item>();
                BossTrophies[index].collectibles = new List<Item>();
            }

            // For being able to complete records in Multiplayer
            RecordTimers = new List<int>();
            BrinkChecker = new List<int>(BossAssist.instance.setup.SortedBosses.Count);
            MaxHealth = new List<int>(BossAssist.instance.setup.SortedBosses.Count);
            DeathTracker = new List<bool>(BossAssist.instance.setup.SortedBosses.Count);
            DodgeTimer = new List<int>(BossAssist.instance.setup.SortedBosses.Count);
            AttackCounter = new List<int>(BossAssist.instance.setup.SortedBosses.Count);

			foreach (BossInfo boss in BossAssist.instance.setup.SortedBosses)
			{
				RecordTimers.Add(0);
				BrinkChecker.Add(0);
				MaxHealth.Add(0);
				DeathTracker.Add(false);
				DodgeTimer.Add(0);
				AttackCounter.Add(0);
			}
		}

        public override TagCompound Save()
        {
            TagCompound saveData = new TagCompound
            {
                { "Records", AllBossRecords },
                { "Collection", BossTrophies },
                { "NewPlayer", isNewPlayer }
            };
            return saveData;
        }
        
        public override void Load(TagCompound tag)
        {
            // Add new bosses to the list, and place existing ones accordingly
            List<BossRecord> TempRecordStorage = tag.Get<List<BossRecord>>("Records");
            foreach (BossRecord record in TempRecordStorage)
            {
                int index = AllBossRecords.FindIndex(x => x.modName == record.modName && x.bossName == record.bossName);
                if (index == -1) AllBossRecords.Add(record);
                else AllBossRecords[index] = record;
            }

            // Prepare the collections for the player. Putting unloaded bosses in the back and new/existing ones up front
            List<BossCollection> TempCollectionStorage = tag.Get<List<BossCollection>>("Collection");
			
			List<BossCollection> AddedCollections = new List<BossCollection>();
            foreach (BossCollection collection in TempCollectionStorage)
            {
                int index = BossTrophies.FindIndex(x => x.modName == collection.modName && x.bossName == collection.bossName);
                if (index == -1) BossTrophies.Add(collection);
                else BossTrophies[index] = collection;
			}
            isNewPlayer = tag.Get<bool>("NewPlayer");
        }

        public override void clientClone(ModPlayer clientClone)
        {
            PlayerAssist clone = clientClone as PlayerAssist;
            clone.BossTrophies = BossTrophies;
            clone.AllBossRecords = AllBossRecords;
        }

        public override void OnRespawn(Player player)
        {
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				for (int i = 0; i < DeathTracker.Count; i++)
				{
					if (DeathTracker[i]) Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[i].stat.deaths++;
					DeathTracker[i] = false;
				}
			}
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				ModPacket packet = mod.GetPacket();
				packet.Write((byte)MessageType.DeathCount);
				int npcsToCount = 0;
				for (int i = 0; i < DeathTracker.Count; i++)
				{
					if (DeathTracker[i]) npcsToCount++;
				}
				packet.Write(npcsToCount);
				for (int i = 0; i < DeathTracker.Count; i++)
				{
					if (DeathTracker[i])
					{
						packet.Write(i);
					}
					DeathTracker[i] = false;
				}
				packet.Send();
			}
        }

        public override void OnEnterWorld(Player player)
		{
			RecordTimers = new List<int>(BossAssist.instance.setup.SortedBosses.Count);
			BrinkChecker = new List<int>(BossAssist.instance.setup.SortedBosses.Count);
			MaxHealth = new List<int>(BossAssist.instance.setup.SortedBosses.Count);
			DeathTracker = new List<bool>(BossAssist.instance.setup.SortedBosses.Count);
			DodgeTimer = new List<int>(BossAssist.instance.setup.SortedBosses.Count);
			AttackCounter = new List<int>(BossAssist.instance.setup.SortedBosses.Count);

			foreach (BossInfo boss in BossAssist.instance.setup.SortedBosses)
			{
				RecordTimers.Add(0);
				BrinkChecker.Add(0);
				MaxHealth.Add(0);
				DeathTracker.Add(false);
				DodgeTimer.Add(0);
				AttackCounter.Add(0);
			}

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				// Essentially to get "BossAssist.ServerCollectedRecords[player.whoAmI] = AllBossRecords;"
				ModPacket packet = mod.GetPacket();
				packet.Write((byte)MessageType.SendRecordsToServer);
				for (int i = 0; i < BossAssist.instance.setup.SortedBosses.Count; i++)
				{
					packet.Write(AllBossRecords[i].stat.kills);
					packet.Write(AllBossRecords[i].stat.deaths);
					packet.Write(AllBossRecords[i].stat.fightTime);
					packet.Write(AllBossRecords[i].stat.fightTime2);
					packet.Write(AllBossRecords[i].stat.brink2);
					packet.Write(AllBossRecords[i].stat.brink);
					packet.Write(AllBossRecords[i].stat.totalDodges);
					packet.Write(AllBossRecords[i].stat.totalDodges2);
					packet.Write(AllBossRecords[i].stat.dodgeTime);
				}
				packet.Send();
			}
			/*
            if (isNewPlayer)
            {
                // This wont work in MP, but ill fix that later
                CombatText.NewText(player.getRect(), Color.LightGreen, "Thanks for playing with Shepherd's mods!!", true);
                isNewPlayer = false;
            }
			*/
			//MapAssist.shouldDraw = false;
			//MapAssist.tilePos = new Vector2(0, 0);
        }

        public override void OnHitByNPC(NPC npc, int damage, bool crit)
        {
            if (WorldAssist.ActiveBossesList.Contains(true))
            {
                for (int i = 0; i < DodgeTimer.Count; i++)
                {
                    if (WorldAssist.ActiveBossesList[i])
                    {
                        AttackCounter[i]++;
                    }
                }
                for (int i = 0; i < DodgeTimer.Count; i++)
                {
                    DodgeTimer[i] = 0;
                }
            }
        }

        public override void OnHitByProjectile(Projectile proj, int damage, bool crit)
        {
            if (WorldAssist.ActiveBossesList.Contains(true))
            {
                for (int i = 0; i < DodgeTimer.Count; i++)
                {
                    if (WorldAssist.ActiveBossesList[i])
                    {
                        AttackCounter[i]++;
                    }
                }
                for (int i = 0; i < DodgeTimer.Count; i++)
                {
                    DodgeTimer[i] = 0;
                }
            }
        }
	}
}