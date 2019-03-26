using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader.IO;

namespace BossAssist
{
    public class BossRecord : TagSerializable
    {
        internal string bossName;
        internal string modName;
        internal BossStats stat = new BossStats();

        public static Func<TagCompound, BossRecord> DESERIALIZER = tag => new BossRecord(tag);

        private BossRecord(TagCompound tag)
        {
            modName = tag.Get<string>(nameof(modName));
            bossName = tag.Get<string>(nameof(bossName));
            stat = tag.Get<BossStats>(nameof(stat));
        }

        public BossRecord(string mod, string boss)
        {
            modName = mod;
            bossName = boss;
        }

        public TagCompound SerializeData()
        {
            return new TagCompound
            {
                { nameof(bossName), bossName },
                { nameof(modName), modName },
                { nameof(stat), stat }
            };
        }
    }

    public class BossStats : TagSerializable
    {
        public int fightTime;
        public int kills;
        public int deaths;
        public int brink;
        public int brinkPercent;

        public static Func<TagCompound, BossStats> DESERIALIZER = tag => new BossStats(tag);

        public BossStats() { }
        private BossStats(TagCompound tag)
        {
            fightTime = tag.Get<int>(nameof(fightTime));
            deaths = tag.Get<int>(nameof(deaths));
            kills = tag.Get<int>(nameof(kills));
            brink = tag.Get<int>(nameof(brink));
            brinkPercent = tag.Get<int>(nameof(brinkPercent));
        }

        public TagCompound SerializeData()
        {
            return new TagCompound
            {
                { nameof(fightTime), fightTime },
                { nameof(kills), kills },
                { nameof(deaths), deaths },
                { nameof(brink), brink },
                { nameof(brinkPercent), brinkPercent }
            };
        }
    }

    public class BossCollection : TagSerializable
    {
        internal string modName;
        internal string bossName;
        internal List<Item> itemList;
        internal List<bool> checkList;

        public static Func<TagCompound, BossCollection> DESERIALIZER = tag => new BossCollection(tag);

        private BossCollection(TagCompound tag)
        {
            modName = tag.Get<string>(nameof(modName));
            bossName = tag.Get<string>(nameof(bossName));
            itemList = tag.Get<List<Item>>(nameof(itemList));
            checkList = tag.Get<List<bool>>(nameof(checkList));
        }

        public BossCollection(string mod, string boss)
        {
            modName = mod;
            bossName = boss;
        }

        public TagCompound SerializeData()
        {
            return new TagCompound
            {
                { nameof(bossName), bossName },
                { nameof(modName), modName },
                { nameof(itemList), itemList },
                { nameof(checkList), checkList }
            };
        }
    }
}