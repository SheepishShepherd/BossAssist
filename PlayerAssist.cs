﻿using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.ID;

namespace BossAssist
{
    public class PlayerAssist : ModPlayer
    {
        public List<BossRecord> AllBossRecords;
        public List<BossCollection> BossTrophies;

        public override void Initialize()
        {
            AllBossRecords = new List<BossRecord>();
            foreach (BossInfo boss in BossAssist.instance.setup.SortedBosses)
            {
                AllBossRecords.Add(new BossRecord(boss.source, boss.name));
            }

            BossTrophies = new List<BossCollection>();
            foreach (BossInfo boss in BossAssist.instance.setup.SortedBosses)
            {
                BossTrophies.Add(new BossCollection(boss.source, boss.name));
            }
            
            foreach (BossCollection bossCollection in BossTrophies)
            {
                bossCollection.itemList = new List<Item>();
                bossCollection.checkList = new List<bool>();
            }
        }
        
        public override TagCompound Save()
        {
            TagCompound saveData = new TagCompound
            {
                { "Records", AllBossRecords },
                { "Collection", BossTrophies }
            };
            return saveData;
        }
        
        public override void Load(TagCompound tag)
        {
            List<BossRecord> TempRecordStorage = tag.Get<List<BossRecord>>("Records");
            foreach (BossRecord record in TempRecordStorage)
            {
                int index = AllBossRecords.FindIndex(x => x.modName == record.modName && x.bossName == record.bossName);
                if (index == -1) AllBossRecords.Add(record);
                else AllBossRecords[index] = record;
            }

            List<BossCollection> TempCollectionStorage = tag.Get<List<BossCollection>>("Collection");
            foreach (BossCollection collection in TempCollectionStorage)
            {
                int index = BossTrophies.FindIndex(x => x.modName == collection.modName && x.bossName == collection.bossName);
                if (index == -1) BossTrophies.Add(collection);
                else BossTrophies[index] = collection;
            }

            for (int c = 0; c < BossTrophies.Count; c++)
            {
                int currentC = c;
                List<Item> templist = new List<Item>();
                List<BossInfo> shortcut = BossAssist.instance.setup.SortedBosses;

                foreach (int item in shortcut[shortcut.FindIndex(x => x.source == BossTrophies[currentC].modName && x.name == BossTrophies[currentC].bossName)].collection)
                {
                    // Possibly include "sorting" code that sorts the Mask, Trophy, and Music Box first
                    Item newItem = new Item();
                    newItem.SetDefaults(item);
                    templist.Add(newItem);
                }

                for (int i = 0; i < templist.Count; i++)
                {
                    int currentI = i;
                    int index = BossTrophies[currentC].itemList.FindIndex(x => x == templist[currentI]);
                    if (index == -1)
                    {
                        BossTrophies[currentC].itemList.Add(templist[currentI]);
                        BossTrophies[currentC].checkList.Add(false);
                    }
                    else BossTrophies[currentC].itemList[index] = templist[currentI];
                }
            }
        }
        
        public static PlayerAssist Get(Player player, Mod mod)
        {
            return player.GetModPlayer<PlayerAssist>(mod);
        }

        public override void OnRespawn(Player player)
        {
            for (int i = 0; i < BossAssist.instance.setup.SortedBosses.Count; i++)
            {
                if (WorldAssist.DeathTracker[i])
                {
                    Main.LocalPlayer.GetModPlayer<PlayerAssist>().AllBossRecords[i].stat.deaths++;
                    WorldAssist.DeathTracker[i] = false;
                }
            }
            base.OnRespawn(player);
        }

        public override void OnEnterWorld(Player player)
        {
            CombatText.NewText(player.getRect(), Color.LightGreen, "Thanks for playing with Shepherd's mods!!", true, false);
            WorldAssist.RecordTimers.Clear();
        }

        // Debugging
        public bool testButton = true;

        public override void ResetEffects()
        {
            if (player.controlSmart && testButton)
            {
                testButton = false;
                Main.NewText("> Start of Debug for " + mod.Name, Color.Goldenrod);
                Main.NewText(AllBossRecords[0].bossName + " from " + AllBossRecords[0].modName);
                Main.NewText("[" + AllBossRecords[0].stat.fightTime + ", " + AllBossRecords[0].stat.kills + ", " + AllBossRecords[0].stat.deaths + "]");
                string recordChain = "Record Timers: ";
                foreach (int i in WorldAssist.RecordTimers)
                {
                    recordChain += i + ", ";
                }
                Main.NewText(recordChain);
                Main.NewText(Colors.RarityBlue.Hex3());
                Main.NewText("> End of Debug for " + mod.Name, Color.IndianRed);
            }
            if (player.releaseSmart) testButton = true;
        }
    }
}
