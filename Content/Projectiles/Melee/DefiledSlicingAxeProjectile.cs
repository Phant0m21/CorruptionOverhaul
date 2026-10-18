
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorruptionOverhaul.Content.Projectiles.Melee
{
    public class DefiledSlicingAxeProjectile : ModProjectile
    {
        public override string Texture =>
            "CorruptionOverhaul/Content/Projectiles/Melee/DefiledSlicingAxeProjectile";

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 74;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 360;
            Projectile.localNPCHitCooldown = 18;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            Projectile.rotation += Projectile.direction * 0.48f;

            Lighting.AddLight(Projectile.Center, 0.12f, 0.72f, 0.08f);

            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.CursedTorch,
                    -Projectile.velocity.X * 0.12f,
                    -Projectile.velocity.Y * 0.12f,
                    80,
                    new Color(105, 255, 65),
                    Main.rand.NextFloat(1.0f, 1.8f)
                );

                dust.noGravity = true;
                dust.velocity *= 0.25f;
            }

            if (Projectile.ai[0] < 42f)
            {
                Projectile.ai[0]++;
            }
            else
            {
                Vector2 toPlayer = player.MountedCenter - Projectile.Center;

                if (toPlayer.LengthSquared() < 34f * 34f)
                {
                    Projectile.Kill();
                    return;
                }

                if (toPlayer.LengthSquared() > 0f)
                {
                    toPlayer.Normalize();

                    Projectile.velocity = Vector2.Lerp(
                        Projectile.velocity,
                        toPlayer * 19f,
                        0.14f
                    );

                    if (Projectile.velocity.LengthSquared() > 19f * 19f)
                    {
                        Projectile.velocity =
                            Vector2.Normalize(Projectile.velocity) * 19f;
                    }
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.BrokenArmor, 240);
            target.AddBuff(BuffID.CursedInferno, 180);

            for (int i = 0; i < 22; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(6f, 6f);

                Dust dust = Dust.NewDustPerfect(
                    target.Center,
                    DustID.CursedTorch,
                    velocity,
                    70,
                    new Color(125, 255, 75),
                    Main.rand.NextFloat(1.1f, 2.2f)
                );

                dust.noGravity = true;
            }

            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(
                    target.position,
                    target.width,
                    target.height,
                    DustID.Electric,
                    Main.rand.NextFloat(-4f, 4f),
                    Main.rand.NextFloat(-4f, 4f),
                    80,
                    new Color(160, 255, 100),
                    Main.rand.NextFloat(0.8f, 1.5f)
                );

                dust.noGravity = true;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient &&
                Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;

                int secondaryDamage = (int)(Projectile.damage * 0.3f);

                if (secondaryDamage < 1)
                    secondaryDamage = 1;

                for (int i = 0; i < 3; i++)
                {
                    float angle = MathHelper.TwoPi * i / 3f;

                    Vector2 offset = new Vector2(
                        (float)System.Math.Cos(angle),
                        (float)System.Math.Sin(angle)
                    ) * 210f;

                    Vector2 spawnPosition = target.Center + offset;

                    Projectile.NewProjectile(
                        Projectile.GetSource_OnHit(target),
                        spawnPosition,
                        Vector2.Zero,
                        ModContent.ProjectileType<DefiledSlicingAxeEcho>(),
                        secondaryDamage,
                        Projectile.knockBack * 0.3f,
                        Projectile.owner,
                        target.whoAmI,
                        32f,
                        angle
                    );
                }

                if (Main.netMode != NetmodeID.Server)
                {
                    SoundEngine.PlaySound(SoundID.Item74, target.Center);
                    SoundEngine.PlaySound(SoundID.DD2_EtherianPortalOpen, target.Center);
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 18; i++)
            {
                Dust dust = Dust.NewDustDirect(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.CursedTorch,
                    Main.rand.NextFloat(-4f, 4f),
                    Main.rand.NextFloat(-4f, 4f),
                    80,
                    new Color(105, 255, 65),
                    Main.rand.NextFloat(1f, 1.8f)
                );

                dust.noGravity = true;
            }
        }
    }
}
