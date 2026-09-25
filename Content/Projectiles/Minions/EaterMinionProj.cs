using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorruptionOverhaul.Content.Projectiles.Minions
{
    public class EaterMinionProj : ModProjectile
    {
        private const int MaxLifeTime = 65;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;

            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = MaxLifeTime;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.light = 0.3f;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            if (Projectile.velocity.LengthSquared() > 0.01f)
            {
                Projectile.rotation =
                    Projectile.velocity.ToRotation() +
                    MathHelper.PiOver2;
            }

                    Lighting.AddLight(Projectile.Center, 0.25f, 0.04f, 0.45f);

            SpawnDust();

            Projectile.velocity *= 1.003f;
        }

        private void SpawnDust()
        {
            if (!Main.rand.NextBool(1))
                return;

            Vector2 direction =
                Projectile.velocity.SafeNormalize(Vector2.Zero);

            Vector2 position =
                Projectile.Center -
                direction * Main.rand.NextFloat(4f, 9f);

            int dustIndex = Dust.NewDust(
                position,
                1,
                1,
                DustID.PurpleTorch,
                -Projectile.velocity.X * 0.15f,
                -Projectile.velocity.Y * 0.15f,
                80,
                new Color(170, 70, 255),
                Main.rand.NextFloat(0.9f, 1.3f)
            );

            Dust dust = Main.dust[dustIndex];
            dust.noGravity = true;
            dust.velocity *= 0.4f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            for (int i = 3; i >= 1; i--)
            {
                float scale = Projectile.scale * (1f + i * 0.08f);
                Color glowColor = new Color(135, 35, 220, 0) * (0.12f / i);
                Main.EntitySpriteDraw(
                    texture,
                    drawPosition,
                    frame,
                    glowColor,
                    Projectile.rotation,
                    origin,
                    scale,
                    SpriteEffects.None,
                    0f);
            }

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero)
                    continue;

                float opacity = (Projectile.oldPos.Length - i) /
                    (float)Projectile.oldPos.Length * 0.32f;
                Vector2 trailPosition = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;

                Main.EntitySpriteDraw(
                    texture,
                    trailPosition,
                    frame,
                    new Color(100, 20, 180, 0) * opacity,
                    Projectile.rotation,
                    origin,
                    Projectile.scale * 0.9f,
                    SpriteEffects.None,
                    0f);
            }

            Main.EntitySpriteDraw(
                texture,
                drawPosition,
                frame,
                Color.White,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0f);

            Main.EntitySpriteDraw(
                texture,
                drawPosition,
                frame,
                new Color(190, 95, 255, 0) * 0.55f,
                Projectile.rotation,
                origin,
                Projectile.scale * 0.82f,
                SpriteEffects.None,
                0f);

            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.ai[0] >= 1f)
                return true;

            if (Projectile.velocity.X != oldVelocity.X)
                Projectile.velocity.X = -oldVelocity.X;

            if (Projectile.velocity.Y != oldVelocity.Y)
                Projectile.velocity.Y = -oldVelocity.Y;

            Projectile.ai[0] = 1f;
            Projectile.netUpdate = true;

            for (int i = 0; i < 4; i++)
            {
                int dustIndex = Dust.NewDust(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.Corruption,
                    0f,
                    0f,
                    100,
                    default,
                    0.8f
                );

                Main.dust[dustIndex].noGravity = true;
            }

            return false;
        }

        public override void OnHitNPC(
            NPC target,
            NPC.HitInfo hit,
            int damageDone)
        {
            Projectile.Kill();
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 3; i++)
            {
                int dustIndex = Dust.NewDust(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.Corruption,
                    0f,
                    0f,
                    120,
                    default,
                    0.7f
                );

                Main.dust[dustIndex].noGravity = true;
            }
        }
    }
}