using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using CorruptionOverhaul.Content.Players;

namespace CorruptionOverhaul.Content.Projectiles.Melee
{
    public class RottenSlicerWave : ModProjectile
    {
        private static readonly Color RottenColor =
            new Color(105, 230, 90);

        private const int FrameCount = 4;
        private const int SpawnDelay = 42;

        public override string Texture =>
            $"Terraria/Images/Projectile_{ProjectileID.TerraBlade2}";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = FrameCount;

            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 72;
            Projectile.height = 72;

            Projectile.friendly = false;
            Projectile.hostile = false;

            Projectile.DamageType = DamageClass.Melee;

            Projectile.penetrate = 3;

            Projectile.timeLeft = 172;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.extraUpdates = 1;

            Projectile.light = 0.35f;
        }

        public override void AI()
        {
            if (Projectile.ai[0] < SpawnDelay)
            {
                Projectile.ai[0]++;

                Projectile.friendly = false;
                Projectile.alpha = 255;
                Projectile.velocity = Vector2.Zero;

                Player player = Main.player[Projectile.owner];

                if (player.active)
                    Projectile.Center = player.Center;

                return;
            }

            if (Projectile.ai[0] == SpawnDelay)
            {
                Projectile.ai[0]++;

                Player player = Main.player[Projectile.owner];

                if (player.active)
                {
                    Vector2 direction =
                        (Main.MouseWorld - player.Center)
                        .SafeNormalize(Vector2.UnitX);

                    Projectile.Center =
                        player.Center + direction * 45f;

                    Projectile.velocity =
                        direction * 12f;
                }

                Projectile.friendly = true;
                Projectile.alpha = 0;

                SoundEngine.PlaySound(
                    SoundID.Item71 with
                    {
                        Volume = 0.9f,
                        Pitch = -0.3f
                    },
                    Projectile.Center
                );

                SoundEngine.PlaySound(
                    SoundID.Item1 with
                    {
                        Volume = 0.65f,
                        Pitch = -0.5f
                    },
                    Projectile.Center
                );

                for (int i = 0; i < 18; i++)
                {
                    Dust dust = Dust.NewDustPerfect(
                        Projectile.Center,
                        DustID.CorruptGibs,
                        Main.rand.NextVector2Circular(3f, 3f),
                        100,
                        RottenColor,
                        Main.rand.NextFloat(0.7f, 1.25f)
                    );

                    dust.noGravity = true;
                }
            }

            Animate();

            Projectile.rotation =
                Projectile.velocity.ToRotation();

            Projectile.velocity *= 0.994f;

            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center +
                    Main.rand.NextVector2Circular(10f, 10f),
                    DustID.CorruptGibs,
                    -Projectile.velocity * 0.08f +
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    100,
                    RottenColor,
                    Main.rand.NextFloat(0.8f, 1.35f)
                );

                dust.noGravity = true;
            }

            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center +
                    Main.rand.NextVector2Circular(18f, 18f),
                    DustID.GreenTorch,
                    Main.rand.NextVector2Circular(0.4f, 0.4f),
                    100,
                    RottenColor,
                    Main.rand.NextFloat(0.5f, 1f)
                );

                dust.noGravity = true;
            }
        }

        private void Animate()
        {
            Projectile.frameCounter++;

            if (Projectile.frameCounter >= 3)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;

                if (Projectile.frame >= FrameCount)
                    Projectile.frame = 0;
            }
        }

        public override void ModifyHitNPC(
            NPC target,
            ref NPC.HitModifiers modifiers)
        {
            modifiers.Knockback *= 3f;
        }

        public override void OnHitNPC(
            NPC target,
            NPC.HitInfo hit,
            int damageDone)
        {
            target.AddBuff(
                BuffID.CursedInferno,
                180
            );

            target.AddBuff(
                BuffID.WitheredArmor,
                120
            );

            Player player = Main.player[Projectile.owner];

            if (player.active)
            {
                player
                    .GetModPlayer<RottenSlicerPlayer>()
                    .AddScreenShake(6f);
            }

            SoundEngine.PlaySound(
                SoundID.Item14 with
                {
                    Volume = 1.15f,
                    Pitch = -0.35f
                },
                target.Center
            );

            SoundEngine.PlaySound(
                SoundID.Item62 with
                {
                    Volume = 0.65f,
                    Pitch = -0.4f
                },
                target.Center
            );

            for (int i = 0; i < 22; i++)
            {
                Vector2 velocity =
                    Main.rand.NextVector2Circular(5.5f, 5.5f);

                Dust dust = Dust.NewDustPerfect(
                    target.Center,
                    DustID.CorruptGibs,
                    velocity,
                    100,
                    RottenColor,
                    Main.rand.NextFloat(0.9f, 1.5f)
                );

                dust.noGravity = true;
            }

            for (int i = 0; i < 10; i++)
            {
                Vector2 velocity =
                    Main.rand.NextVector2Circular(3f, 3f);

                Dust dust = Dust.NewDustPerfect(
                    target.Center,
                    DustID.GreenTorch,
                    velocity,
                    100,
                    RottenColor,
                    Main.rand.NextFloat(0.7f, 1.15f)
                );

                dust.noGravity = true;
            }
        }

        public override bool PreDraw(
            ref Color lightColor)
        {
            if (Projectile.ai[0] < SpawnDelay)
                return false;

            Texture2D texture =
                TextureAssets.Projectile[
                    ProjectileID.TerraBlade2
                ].Value;

            Rectangle frame =
                texture.Frame(
                    1,
                    FrameCount,
                    0,
                    Projectile.frame
                );

            Vector2 origin =
                frame.Size() * 0.5f;

            Vector2 drawPosition =
                Projectile.Center - Main.screenPosition;

            for (int i = 10; i >= 1; i--)
            {
                float progress = i / 10f;

                Vector2 trailPosition =
                    drawPosition -
                    Projectile.velocity *
                    i *
                    0.5f;

                Color trailColor =
                    RottenColor *
                    (0.075f * (1f - progress));

                Main.EntitySpriteDraw(
                    texture,
                    trailPosition,
                    frame,
                    trailColor,
                    Projectile.rotation,
                    origin,
                    1.25f,
                    SpriteEffects.None,
                    0
                );
            }

            Main.EntitySpriteDraw(
                texture,
                drawPosition,
                frame,
                RottenColor,
                Projectile.rotation,
                origin,
                1.25f,
                SpriteEffects.None,
                0
            );

            Main.EntitySpriteDraw(
                texture,
                drawPosition,
                frame,
                Color.White * 0.3f,
                Projectile.rotation,
                origin,
                1.4f,
                SpriteEffects.None,
                0
            );

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            if (Projectile.ai[0] < SpawnDelay)
                return;

            SoundEngine.PlaySound(
                SoundID.Item8 with
                {
                    Volume = 0.55f,
                    Pitch = -0.35f
                },
                Projectile.Center
            );

            for (int i = 0; i < 24; i++)
            {
                Vector2 velocity =
                    Main.rand.NextVector2Circular(4.5f, 4.5f);

                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.CorruptGibs,
                    velocity,
                    100,
                    RottenColor,
                    Main.rand.NextFloat(0.7f, 1.3f)
                );

                dust.noGravity = true;
            }

            for (int i = 0; i < 12; i++)
            {
                Vector2 velocity =
                    Main.rand.NextVector2Circular(3f, 3f);

                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.GreenTorch,
                    velocity,
                    100,
                    RottenColor,
                    Main.rand.NextFloat(0.5f, 1f)
                );

                dust.noGravity = true;
            }
        }
    }
}