using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace BossAssist
{
    public class PlayerAssist : ModPlayer
    {
        public static PlayerAssist Get(Player player, Mod mod)
        {
            return player.GetModPlayer<PlayerAssist>(mod);
        }

        public override void UpdateDead()
        {
            if (Main.LocalPlayer.dead)
            {
                BossAssist.instance.TimerUI.SetState(new RespawnTimer());
            }
        }

        public override void OnEnterWorld(Player player)
        {
            WorldAssist.bossList.Clear();
        }
    }
}
