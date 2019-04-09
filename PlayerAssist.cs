using Terraria;
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

            // Make a new list of collections
            BossTrophies = new List<BossCollection>();
            // For each boss added...
            foreach (BossInfo boss in BossAssist.instance.setup.SortedBosses)
            {
                // 1.) Add a collection for the boss
                BossTrophies.Add(new BossCollection(boss.source, boss.name));
                // 2.) setup the item list and check off list for the boss
                int index = BossTrophies.FindIndex(x => x.modName == boss.source && x.bossName == boss.name);
                BossTrophies[index].itemList = new List<Item>();
                BossTrophies[index].checkList = new List<bool>();
                // 3.) Add the items setup in the SortedBosses list (checks dealt with in Load)
                foreach (int collectible in BossAssist.instance.setup.SortedBosses[index].collection)
                {
                    Item newItem = new Item();
                    newItem.SetDefaults(collectible);

                    BossTrophies[index].itemList.Add(newItem);
                    BossTrophies[index].checkList.Add(false);
                }
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
            List<BossCollection> TempCollectionStorage2 = tag.Get<List<BossCollection>>("Collection");
            foreach (BossCollection collection in TempCollectionStorage)
            {
                int index = BossTrophies.FindIndex(x => x.modName == collection.modName && x.bossName == collection.bossName);
                if (index == -1) BossTrophies.Add(collection);
                else
                {
                    BossTrophies[index] = collection;
                }
            }

            // Refill the Item and Check lists with the same method as above
            foreach (BossCollection collection in TempCollectionStorage2)
            {
                int index = BossTrophies.FindIndex(x => x.modName == collection.modName && x.bossName == collection.bossName);
                foreach (Item item in collection.itemList)
                {
                    int index2 = collection.itemList.FindIndex(x => x == item);
                    if (index2 == -1)
                    {
                        BossTrophies[index].itemList.Add(item);
                        BossTrophies[index].checkList.Add(false);
                    }
                    else
                    {
                        BossTrophies[index].itemList[index2] = item;
                        BossTrophies[index].checkList[index2] = collection.checkList[index2];
                    }
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

        public override void OnHitByNPC(NPC npc, int damage, bool crit)
        {
            if (WorldAssist.ActiveBossesList.Contains(true))
            {
                for (int i = 0; i < WorldAssist.DodgeTimer.Count; i++)
                {
                    if (WorldAssist.ActiveBossesList[i])
                    {
                        WorldAssist.AttackCounter[i]++;
                    }
                }
                for (int i = 0; i < WorldAssist.DodgeTimer.Count; i++)
                {
                    WorldAssist.DodgeTimer[i] = 0;
                }
            }
        }

        public override void OnHitByProjectile(Projectile proj, int damage, bool crit)
        {
            if (WorldAssist.ActiveBossesList.Contains(true))
            {
                for (int i = 0; i < WorldAssist.DodgeTimer.Count; i++)
                {
                    if (WorldAssist.ActiveBossesList[i])
                    {
                        WorldAssist.AttackCounter[i]++;
                    }
                }
                for (int i = 0; i < WorldAssist.DodgeTimer.Count; i++)
                {
                    WorldAssist.DodgeTimer[i] = 0;
                }
            }
        }

        /* Debugging
        public bool testButton = true;

        public override void ResetEffects()
        {
            if (player.controlSmart && testButton))
            {
                testButton = false;
                Main.NewText("> Start of Debug for " + mod.Name, Color.Goldenrod);
                Main.NewText(WorldAssist.ActiveBossesList[BossAssist.instance.setup.SortedBosses.FindIndex(x => x.id == NPCID.Retinazer)]);
                Main.NewText("> End of Debug for " + mod.Name, Color.IndianRed);
            }
            if (player.releaseSmart) testButton = true;
        }
        */
    }
}