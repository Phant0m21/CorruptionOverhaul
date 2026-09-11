using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CorruptionOverhaul.Content.Players
{
    public class RottenSlicerPlayer : ModPlayer
    {
        public float ScreenShake;

        public override void Initialize()
        {
            ScreenShake = 0f;
        }

        public override void OnRespawn()
        {
            ScreenShake = 0f;
        }

        public void AddScreenShake(float amount)
        {
            if (amount > ScreenShake)
                ScreenShake = amount;
        }

        public override void ModifyScreenPosition()
        {
            if (ScreenShake <= 0f)
                return;

            Main.screenPosition += new Vector2(
                Main.rand.NextFloat(-ScreenShake, ScreenShake),
                Main.rand.NextFloat(-ScreenShake, ScreenShake)
            );

            ScreenShake *= 0.82f;

            if (ScreenShake < 0.1f)
                ScreenShake = 0f;
        }
    }
}