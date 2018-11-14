using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BossAssist
{
    class NPCAssist : GlobalNPC
    {
        public static bool isDead = false;
        public override void NPCLoot(NPC npc)
        {
            string key = "Mods.BossAssist.PillarDestroyed";
            string partName = npc.FullName;
            if (npc.type == NPCID.LunarTowerSolar || npc.type == NPCID.LunarTowerVortex || npc.type == NPCID.LunarTowerNebula || npc.type == NPCID.LunarTowerStardust)
            {
                if (Main.netMode == 0) Main.NewText(string.Format(Language.GetTextValue(key), npc.GetFullNetName().ToString()), Colors.RarityPurple);
                else NetMessage.BroadcastChatMessage(NetworkText.FromKey(key, npc.GetTypeNetName()), Colors.RarityPurple);
            }
            else if (CheckForNPCType(npc))
            {
                if (npc.type == NPCID.SkeletronHand) partName = "Skeletron Hand";
                else partName = npc.FullName;

                if (Main.netMode == 0) Main.NewText("The " + partName + " is down!", Colors.RarityGreen);
                else NetMessage.BroadcastChatMessage(NetworkText.FromLiteral("The " + npc.FullName + " is down!"), Colors.RarityGreen);
            }
            base.NPCLoot(npc);
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
                || (npcType.type == NPCID.Retinazer && Main.npc.Any(otherBoss => otherBoss.type == NPCID.Spazmatism && otherBoss.life > 0 && otherBoss.active))
                || (npcType.type == NPCID.Spazmatism && Main.npc.Any(otherBoss => otherBoss.type == NPCID.Retinazer && otherBoss.life > 0 && otherBoss.active));
        }
    }
}