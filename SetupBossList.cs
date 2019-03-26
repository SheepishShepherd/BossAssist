using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace BossAssist
{
    public class SetupBossList
    {
        internal List<BossInfo> SortedBosses;

        public SetupBossList()
        {
            InitList();
        }

        private void InitList()
        {
            SortedBosses = new List<BossInfo>
            {
                new BossInfo(1f, NPCID.KingSlime, "Vanilla", "King Slime", (() => NPC.downedSlimeKing), ItemID.SlimeCrown, SetupCollect(50), SetupLoot(50), BossAssist.instance.GetTexture("Resources/BossTextures/Boss50")),
                new BossInfo(2f, NPCID.EyeofCthulhu, "Vanilla", "Eye of Cthulhu", (() => NPC.downedBoss1), ItemID.SuspiciousLookingEye,SetupCollect(4), SetupLoot(4), BossAssist.instance.GetTexture("Resources/BossTextures/Boss4")),
                new BossInfo(3f, NPCID.EaterofWorldsHead, "Vanilla", "Eater of Worlds", (() => NPC.downedBoss2), ItemID.WormFood, SetupCollect(13), SetupLoot(13), BossAssist.instance.GetTexture("Resources/BossTextures/Boss13")),
                new BossInfo(3f, NPCID.BrainofCthulhu, "Vanilla", "Brain of Cthulhu", (() => NPC.downedBoss2), ItemID.BloodySpine, SetupCollect(266), SetupLoot(266), BossAssist.instance.GetTexture("Resources/BossTextures/Boss266")),
                new BossInfo(4f, NPCID.QueenBee, "Vanilla", "Queen Bee", (() => NPC.downedQueenBee), ItemID.Abeemination, SetupCollect(222), SetupLoot(222), BossAssist.instance.GetTexture("Resources/BossTextures/Boss222")),
                new BossInfo(5f, NPCID.SkeletronHead, "Vanilla", "Skeletron", (() => NPC.downedBoss3), ItemID.ClothierVoodooDoll, SetupCollect(35), SetupLoot(35), BossAssist.instance.GetTexture("Resources/BossTextures/Boss35")),
                new BossInfo(6f, NPCID.WallofFlesh, "Vanilla", "Wall of Flesh", (() => Main.hardMode), ItemID.GuideVoodooDoll, SetupCollect(113), SetupLoot(113), BossAssist.instance.GetTexture("Resources/BossTextures/Boss113")),
                new BossInfo(7f, NPCID.Retinazer, "Vanilla", "The Twins", (() => NPC.downedMechBoss2), ItemID.MechanicalEye, SetupCollect(125), SetupLoot(125), BossAssist.instance.GetTexture("Resources/BossTextures/Boss125")),
                new BossInfo(8f, NPCID.TheDestroyer, "Vanilla", "The Destroyer", (() => NPC.downedMechBoss1), ItemID.MechanicalWorm, SetupCollect(134), SetupLoot(134), BossAssist.instance.GetTexture("Resources/BossTextures/Boss134")),
                new BossInfo(9f, NPCID.SkeletronPrime, "Vanilla", "Skeletron Prime", (() => NPC.downedMechBoss3), ItemID.MechanicalSkull, SetupCollect(127), SetupLoot(127), BossAssist.instance.GetTexture("Resources/BossTextures/Boss127")),
                new BossInfo(10f, NPCID.Plantera, "Vanilla", "Plantera", (() => NPC.downedPlantBoss), 0, SetupCollect(262), SetupLoot(262), BossAssist.instance.GetTexture("Resources/BossTextures/Boss262")),
                new BossInfo(11f, NPCID.Golem, "Vanilla", "Golem", (() => NPC.downedGolemBoss), ItemID.LihzahrdPowerCell,  SetupCollect(245), SetupLoot(245), BossAssist.instance.GetTexture("Resources/BossTextures/Boss245")),
                new BossInfo(11.5f, NPCID.DD2Betsy, "Vanilla", "Betsy", (() => WorldAssist.downedBetsy), ItemID.DD2ElderCrystal, SetupCollect(551), SetupLoot(551), BossAssist.instance.GetTexture("Resources/BossTextures/Boss551")),
                new BossInfo(12f, NPCID.DukeFishron, "Vanilla", "Duke Fishron", (() => NPC.downedFishron), ItemID.TruffleWorm, SetupCollect(370), SetupLoot(370), BossAssist.instance.GetTexture("Resources/BossTextures/Boss370")),
                new BossInfo(13f, NPCID.CultistBoss, "Vanilla", "Lunatic Cultist", (() => NPC.downedAncientCultist), 0, SetupCollect(439), SetupLoot(439), BossAssist.instance.GetTexture("Resources/BossTextures/Boss439")),
                new BossInfo(14f, NPCID.MoonLordHead, "Vanilla", "Moon Lord", (() => NPC.downedMoonlord), ItemID.CelestialSigil, SetupCollect(396), SetupLoot(396), BossAssist.instance.GetTexture("Resources/BossTextures/Boss396"))
            };
        }
        
        internal void AddBoss(float val, int id, string source, string name, Func<bool> down, int spawn, List<int> collect, List<int> loot, Texture2D texture = null)
        {
            if (texture == null) texture = BossAssist.instance.GetTexture("Resources/tehBoss");
            
            SortedBosses.Add(new BossInfo(val, id, source, name, down, spawn, collect, loot, texture));
            SortedBosses.Sort((x, y) => x.progression.CompareTo(y.progression));
        }

        internal void AddToLootTable(int bType, string bSource, List<int> bLoot)
        {
            foreach (int item in bLoot)
            {
                SortedBosses[SortedBosses.FindIndex(x => x.id == bType && x.source == bSource)].loot.Add(item);
            }
        }

        internal void AddToCollection(int bType, string bSource, List<int> bCollect)
        {
            foreach (int item in bCollect)
            {
                SortedBosses[SortedBosses.FindIndex(x => x.id == bType && x.source == bSource)].collection.Add(item);
            }
        }

        internal protected List<int> SetupLoot(int bossNum)
        {
            if (bossNum == NPCID.KingSlime)
            {
                return new List<int>()
                {
                    ItemID.KingSlimeBossBag,
                    ItemID.RoyalGel,
                    ItemID.Solidifier,
                    ItemID.SlimySaddle,
                    ItemID.NinjaHood,
                    ItemID.NinjaShirt,
                    ItemID.NinjaPants,
                    ItemID.SlimeHook,
                    ItemID.SlimeGun,
                    ItemID.LesserHealingPotion,
                    ItemID.KingSlimeMask,
                    ItemID.KingSlimeTrophy
                };
            }
            if (bossNum == NPCID.EyeofCthulhu)
            {
                return new List<int>()
                {
                    ItemID.EyeOfCthulhuBossBag,
                    ItemID.EoCShield,
                    ItemID.DemoniteOre,
                    ItemID.UnholyArrow,
                    ItemID.CorruptSeeds,
                    ItemID.Binoculars,
                    ItemID.LesserHealingPotion,
                    ItemID.EyeMask,
                    ItemID.EyeofCthulhuTrophy
                };
            }
            if (bossNum == NPCID.EaterofWorldsHead)
            {
                return new List<int>()
                {
                    ItemID.EaterOfWorldsBossBag,
                    ItemID.WormScarf,
                    ItemID.ShadowScale,
                    ItemID.DemoniteOre,
                    ItemID.EatersBone,
                    ItemID.LesserHealingPotion,
                    ItemID.EaterMask,
                    ItemID.EaterofWorldsTrophy
                };
            }
            if (bossNum == NPCID.BrainofCthulhu)
            {
                return new List<int>()
                {
                    ItemID.BrainOfCthulhuBossBag,
                    ItemID.BrainOfConfusion,
                    ItemID.CrimtaneOre,
                    ItemID.TissueSample,
                    ItemID.BoneRattle,
                    ItemID.LesserHealingPotion,
                    ItemID.BrainMask,
                    ItemID.BrainofCthulhuTrophy
                };
            }
            if (bossNum == NPCID.QueenBee)
            {
                return new List<int>()
                {
                    ItemID.QueenBeeBossBag,
                    ItemID.HiveBackpack,
                    ItemID.BeeGun,
                    ItemID.BeeKeeper,
                    ItemID.BeesKnees,
                    ItemID.HiveWand,
                    ItemID.BeeHat,
                    ItemID.BeeShirt,
                    ItemID.BeePants,
                    ItemID.HoneyComb,
                    ItemID.Nectar,
                    ItemID.HoneyedGoggles,
                    ItemID.Beenade,
                    ItemID.BottledHoney,
                    ItemID.BeeMask,
                    ItemID.QueenBeeTrophy
                };
            }
            if (bossNum == NPCID.SkeletronHead)
            {
                return new List<int>()
                {
                    ItemID.SkeletronBossBag,
                    ItemID.BoneGlove,
                    ItemID.SkeletronHand,
                    ItemID.BookofSkulls,
                    ItemID.LesserHealingPotion,
                    ItemID.SkeletronMask,
                    ItemID.SkeletronTrophy
                };
            }
            if (bossNum == NPCID.WallofFlesh)
            {
                return new List<int>()
                {
                    ItemID.WallOfFleshBossBag,
                    ItemID.DemonHeart,
                    ItemID.Pwnhammer,
                    ItemID.BreakerBlade,
                    ItemID.ClockworkAssaultRifle,
                    ItemID.LaserRifle,
                    ItemID.WarriorEmblem,
                    ItemID.SorcererEmblem,
                    ItemID.RangerEmblem,
                    ItemID.SummonerEmblem,
                    ItemID.HealingPotion,
                    ItemID.FleshMask,
                    ItemID.WallofFleshTrophy
                };
            }
            if (bossNum == NPCID.Retinazer)
            {
                return new List<int>()
                {
                    ItemID.TwinsBossBag,
                    ItemID.MechanicalWheelPiece,
                    ItemID.SoulofSight,
                    ItemID.HallowedBar,
                    ItemID.GreaterHealingPotion,
                    ItemID.TwinMask,
                    ItemID.RetinazerTrophy,
                    ItemID.SpazmatismTrophy
                };
            }
            if (bossNum == NPCID.TheDestroyer)
            {
                return new List<int>()
                {
                    ItemID.DestroyerBossBag,
                    ItemID.MechanicalWagonPiece,
                    ItemID.SoulofMight,
                    ItemID.HallowedBar,
                    ItemID.GreaterHealingPotion,
                    ItemID.DestroyerMask,
                    ItemID.DestroyerTrophy
                };
            }
            if (bossNum == NPCID.SkeletronPrime)
            {
                return new List<int>()
                {
                    ItemID.SkeletronPrimeBossBag,
                    ItemID.MechanicalBatteryPiece,
                    ItemID.SoulofFright,
                    ItemID.HallowedBar,
                    ItemID.GreaterHealingPotion,
                    ItemID.SkeletronPrimeMask,
                    ItemID.SkeletronPrimeTrophy
                };
            }
            if (bossNum == NPCID.Plantera)
            {
                return new List<int>()
                {
                    ItemID.PlanteraBossBag,
                    ItemID.SporeSac,
                    ItemID.TempleKey,
                    ItemID.Seedling,
                    ItemID.TheAxe,
                    ItemID.PygmyStaff,
                    ItemID.GrenadeLauncher,
                    ItemID.VenusMagnum,
                    ItemID.NettleBurst,
                    ItemID.LeafBlower,
                    ItemID.FlowerPow,
                    ItemID.WaspGun,
                    ItemID.Seedler,
                    ItemID.ThornHook,
                    ItemID.GreaterHealingPotion,
                    ItemID.PlanteraMask,
                    ItemID.PlanteraTrophy
                };
            }
            if (bossNum == NPCID.Golem)
            {
                return new List<int>()
                {
                    ItemID.GolemBossBag,
                    ItemID.ShinyStone,
                    ItemID.Stynger,
                    ItemID.PossessedHatchet,
                    ItemID.SunStone,
                    ItemID.EyeoftheGolem,
                    ItemID.Picksaw,
                    ItemID.HeatRay,
                    ItemID.StaffofEarth,
                    ItemID.GolemFist,
                    ItemID.BeetleHusk,
                    ItemID.GreaterHealingPotion,
                    ItemID.GolemMask,
                    ItemID.GolemTrophy
                };
            }
            if (bossNum == NPCID.DD2Betsy)
            {
                return new List<int>()
                {
                    ItemID.BossBagBetsy,
                    ItemID.BetsyWings,
                    ItemID.DD2BetsyBow, // Aerial Bane
                    ItemID.MonkStaffT3, // Sky Dragon's Fury
                    ItemID.ApprenticeStaffT3, // Betsy's Wrath
                    ItemID.DD2SquireBetsySword, // Flying Dragon
                    ItemID.BossMaskBetsy,
                    ItemID.BossTrophyBetsy
                };
            }
            if (bossNum == NPCID.DukeFishron)
            {
                return new List<int>()
                {
                    ItemID.FishronBossBag,
                    ItemID.ShrimpyTruffle,
                    ItemID.FishronWings,
                    ItemID.BubbleGun,
                    ItemID.Flairon,
                    ItemID.RazorbladeTyphoon,
                    ItemID.TempestStaff,
                    ItemID.Tsunami,
                    ItemID.GreaterHealingPotion,
                    ItemID.DukeFishronMask,
                    ItemID.DukeFishronTrophy
                };
            }
            if (bossNum == NPCID.CultistBoss)
            {
                return new List<int>()
                {
                    ItemID.CultistBossBag,
                    ItemID.LunarCraftingStation,
                    ItemID.GreaterHealingPotion,
                    ItemID.BossMaskCultist,
                    ItemID.AncientCultistTrophy
                };
            }
            if (bossNum == NPCID.MoonLordHead)
            {
                return new List<int>()
                {
                    ItemID.MoonLordBossBag,
                    ItemID.GravityGlobe,
                    ItemID.PortalGun,
                    ItemID.LunarOre,
                    ItemID.Meowmere,
                    ItemID.Terrarian,
                    ItemID.StarWrath,
                    ItemID.SDMG,
                    ItemID.FireworksLauncher, // The Celebration
                    ItemID.LastPrism,
                    ItemID.LunarFlareBook,
                    ItemID.RainbowCrystalStaff,
                    ItemID.MoonlordTurretStaff, // Lunar Portal Staff
                    ItemID.SuspiciousLookingTentacle,
                    ItemID.GreaterHealingPotion,
                    ItemID.BossMaskMoonlord,
                    ItemID.MoonLordTrophy
                };
            }
            return new List<int>();
        }

        internal protected List<int> SetupCollect(int bossNum)
        {
            if (bossNum == NPCID.KingSlime)
            {
                return new List<int>()
                {
                    ItemID.KingSlimeTrophy,
                    ItemID.KingSlimeMask,
                    ItemID.MusicBoxBoss1
                };
            }
            if (bossNum == NPCID.EyeofCthulhu)
            {
                return new List<int>()
                {
                    ItemID.EyeofCthulhuTrophy,
                    ItemID.EyeMask,
                    ItemID.MusicBoxBoss1
                };
            }
            if (bossNum == NPCID.EaterofWorldsHead)
            {
                return new List<int>()
                {
                    ItemID.EaterofWorldsTrophy,
                    ItemID.EaterMask,
                    ItemID.MusicBoxBoss1
                };
            }
            if (bossNum == NPCID.BrainofCthulhu)
            {
                return new List<int>()
                {
                    ItemID.BrainofCthulhuTrophy,
                    ItemID.BrainMask,
                    ItemID.MusicBoxBoss3
                };
            }
            if (bossNum == NPCID.QueenBee)
            {
                return new List<int>()
                {
                    ItemID.QueenBeeTrophy,
                    ItemID.BeeMask,
                    ItemID.MusicBoxBoss4
                };
            }
            if (bossNum == NPCID.SkeletronHead)
            {
                return new List<int>()
                {
                    ItemID.SkeletronTrophy,
                    ItemID.SkeletronMask,
                    ItemID.MusicBoxBoss1
                };
            }
            if (bossNum == NPCID.WallofFlesh)
            {
                return new List<int>()
                {
                    ItemID.WallofFleshTrophy,
                    ItemID.FleshMask,
                    ItemID.MusicBoxBoss2
                };
            }
            if (bossNum == NPCID.Retinazer)
            {
                return new List<int>()
                {
                    ItemID.RetinazerTrophy,
                    ItemID.SpazmatismTrophy,
                    ItemID.TwinMask,
                    ItemID.MusicBoxBoss2
                };
            }
            if (bossNum == NPCID.TheDestroyer)
            {
                return new List<int>()
                {
                    ItemID.DestroyerTrophy,
                    ItemID.DestroyerMask,
                    ItemID.MusicBoxBoss3
                };
            }
            if (bossNum == NPCID.SkeletronPrime)
            {
                return new List<int>()
                {
                    ItemID.SkeletronPrimeTrophy,
                    ItemID.SkeletronPrimeMask,
                    ItemID.MusicBoxBoss1
                };
            }
            if (bossNum == NPCID.Plantera)
            {
                return new List<int>()
                {
                    ItemID.PlanteraTrophy,
                    ItemID.PlanteraMask,
                    ItemID.MusicBoxPlantera
                };
            }
            if (bossNum == NPCID.Golem)
            {
                return new List<int>()
                {
                    ItemID.GolemTrophy,
                    ItemID.GolemMask,
                    ItemID.MusicBoxBoss5
                };
            }
            if (bossNum == NPCID.DD2Betsy)
            {
                return new List<int>()
                {
                    ItemID.BossTrophyBetsy,
                    ItemID.BossMaskBetsy,
                    ItemID.MusicBoxDD2
                };
            }
            if (bossNum == NPCID.DukeFishron)
            {
                return new List<int>()
                {
                    ItemID.DukeFishronTrophy,
                    ItemID.DukeFishronMask,
                    ItemID.MusicBoxBoss1
                };
            }
            if (bossNum == NPCID.CultistBoss)
            {
                return new List<int>()
                {
                    ItemID.AncientCultistTrophy,
                    ItemID.BossMaskCultist,
                    ItemID.MusicBoxBoss5
                };
            }
            if (bossNum == NPCID.MoonLordHead)
            {
                return new List<int>()
                {
                    ItemID.MoonLordTrophy,
                    ItemID.BossMaskMoonlord,
                    ItemID.MusicBoxLunarBoss
                };
            }
            return new List<int>();
        }
    }

    public class BossInfo
    {
        internal float progression;
        internal int id;
        internal string source;
        internal string name;
        internal Func<bool> downed;

        internal int spawnItem;
        internal List<int> loot;
        internal List<int> collection;

        internal Texture2D pageTexture;



        internal BossInfo(float progression, int id, string source, string name, Func<bool> downed, int spawnItem, List<int> collection, List<int> loot, Texture2D pageTexture)
        {
            this.progression = progression;
            this.id = id;
            this.source = source;
            this.name = name;
            this.downed = downed;
            this.spawnItem = spawnItem;
            this.collection = collection;
            this.loot = loot;

            if (pageTexture == null) this.pageTexture = BossAssist.instance.GetTexture("Resources/tehBoss");
            this.pageTexture = pageTexture;
        }
    }
}