using Terraria;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BossAssist
{
    public class RespawnTimer : UIState
    {
        UIText timer;
        static float alpha = 0f;

        public override void OnInitialize()
        {
            timer = new UIText("", 1f, true);
            timer.Left.Set(0, 0.5f);
            timer.Top.Set(0, 0.44f);
            Append(timer);
        }
        
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            if (Main.LocalPlayer.dead)
            {
                if (Main.LocalPlayer.respawnTimer % 60 == 0 && Main.LocalPlayer.respawnTimer / 60 <= 3) Main.PlaySound(25);
                if (alpha < 1f) alpha += 0.05f;
                timer.SetText(((int)(Main.LocalPlayer.respawnTimer / 60) + 1).ToString());
                timer.TextColor = new Color(1f, 0.388f, 0.278f, alpha);
            }
            else alpha = 0f;
        }
    }
}