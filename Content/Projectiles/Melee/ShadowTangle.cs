using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorruptionOverhaul.Content.Projectiles.Melee
{
    public class ShadowTangle : ModProjectile
    {
        private const int Duration = 36;
        private const float OuterRadius = 30f;
        private const float InnerRadius = 18f;

        public override string Texture =>
            "CorruptionOverhaul/Content/Projectiles/Melee/ShadowSpike";

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.timeLeft = Duration;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
        }

        public override void AI()
        {
            if (Projectile.ai[0] < 0f || Projectile.ai[0] >= Main.maxNPCs)
            {
                Projectile.Kill();
                return;
            }

            NPC target = Main.npc[(int)Projectile.ai[0]];

            if (!target.active || target.life <= 0)
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = target.Center;
            Projectile.ai[1] += 0.34f;
            Lighting.AddLight(target.Center, 0.16f, 0.02f, 0.3f);

            if (Main.rand.NextBool(2))
            {
                Vector2 dustOffset = Main.rand.NextVector2Circular(OuterRadius, OuterRadius);
                Dust dust = Dust.NewDustPerfect(
                    target.Center + dustOffset,
                    DustID.PurpleTorch,
                    dustOffset.SafeNormalize(Vector2.Zero) * 0.5f,
                    140,
                    new Color(170, 70, 255),
                    Main.rand.NextFloat(0.8f, 1.3f));

                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<ShadowSpike>()].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 center = Projectile.Center - Main.screenPosition;
            float rotation = Projectile.ai[1];
            float fade = Projectile.timeLeft < 10 ? Projectile.timeLeft / 10f : 1f;

            for (int i = 0; i < 8; i++)
            {
                float angle = rotation + MathHelper.TwoPi * i / 8f;
                Vector2 position = center + angle.ToRotationVector2() * OuterRadius;
                float scale = 1.45f + 0.12f * (float)System.Math.Sin(rotation * 2f + i);
                Color glowColor = new Color(145, 35, 240, 0) * (0.45f * fade);

                Main.EntitySpriteDraw(texture, position, null, glowColor, angle + MathHelper.PiOver2, origin, scale * 1.25f, SpriteEffects.None, 0f);
                Main.EntitySpriteDraw(texture, position, null, lightColor * fade, angle + MathHelper.PiOver2, origin, scale, SpriteEffects.None, 0f);
            }

            for (int i = 0; i < 5; i++)
            {
                float angle = -rotation * 1.35f + MathHelper.TwoPi * i / 5f;
                Vector2 position = center + angle.ToRotationVector2() * InnerRadius;
                Color glowColor = new Color(190, 75, 255, 0) * (0.3f * fade);

                Main.EntitySpriteDraw(texture, position, null, glowColor, angle + MathHelper.PiOver2, origin, 0.9f, SpriteEffects.None, 0f);
            }

            return false;
        }
    }
}