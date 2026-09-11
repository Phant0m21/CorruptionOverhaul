using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorruptionOverhaul.Content.Projectiles.Minions
{
    public class EaterMinionProj : ModProjectile
    {
        private const int MaxLifeTime = 65;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;

            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 29;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = MaxLifeTime;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.light = 0.05f;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.frameCounter++;

            if (Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;

                if (Projectile.frame >= 4)
                    Projectile.frame = 0;
            }

            if (Projectile.velocity.LengthSquared() > 0.01f)
            {
                Projectile.rotation =
                    Projectile.velocity.ToRotation() +
                    MathHelper.PiOver2;
            }

            SpawnDust();

            Projectile.velocity *= 1.003f;
        }

        private void SpawnDust()
        {
            if (!Main.rand.NextBool(2))
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
                DustID.Corruption,
                -Projectile.velocity.X * 0.15f,
                -Projectile.velocity.Y * 0.15f,
                120,
                default,
                Main.rand.NextFloat(0.7f, 1.1f)
            );

            Dust dust = Main.dust[dustIndex];
            dust.noGravity = true;
            dust.velocity *= 0.25f;
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