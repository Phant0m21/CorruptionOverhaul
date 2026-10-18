
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;

namespace CorruptionOverhaul.Content.Projectiles.Melee
{
    public class DefiledSlicingAxeEcho : ModProjectile
    {
        private const int ChargeTime = 32;
        private const float OrbitRadius = 210f;
        private const float LaunchSpeed = 17f;
        private const float HomingSpeed = 18f;
        private const float SearchRadius = 300f;
        private const int FrameCount = 5;
        private const int FrameHeight = 107;
        private const int FrameSpeed = 6;

        public override string Texture =>
            "CorruptionOverhaul/Content/Projectiles/Melee/DefiledSlicingAxeEcho";

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = FrameHeight;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 150;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 18;
        }

        private bool IsValidTarget(NPC target)
        {
            return target.active
                && !target.friendly
                && !target.dontTakeDamage
                && target.lifeMax > 5
                && target.CanBeChasedBy(Projectile);
        }

        private int FindNearbyTarget()
        {
            int bestTarget = -1;
            float bestDistance = SearchRadius * SearchRadius;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC target = Main.npc[i];

                if (!IsValidTarget(target))
                    continue;

                float distance = Vector2.DistanceSquared(
                    Projectile.Center,
                    target.Center
                );

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestTarget = i;
                }
            }

            return bestTarget;
        }

        private int GetValidOrNearbyTarget(int targetIndex)
        {
            if (targetIndex >= 0 && targetIndex < Main.maxNPCs)
            {
                NPC target = Main.npc[targetIndex];

                if (IsValidTarget(target))
                    return targetIndex;
            }

            return FindNearbyTarget();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            int frameY = Projectile.frame * FrameHeight;
            Rectangle frame = new Rectangle(0, frameY, 40, FrameHeight);
            Vector2 origin = new Vector2(frame.Width / 2f, frame.Height / 2f);
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            float timer = Projectile.localAI[0];
            float pulse = (float)System.Math.Sin(timer * 0.45f) * 0.5f + 0.5f;
            float glowStrength = timer < ChargeTime
                ? 0.18f + pulse * 0.28f
                : 0.12f;

            Color glowColor = new Color(80, 255, 45, 0) * glowStrength;

            SpriteEffects effects = Projectile.spriteDirection == -1
                ? SpriteEffects.FlipHorizontally
                : SpriteEffects.None;

            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;

                Vector2 offset = new Vector2(
                    (float)System.Math.Cos(angle),
                    (float)System.Math.Sin(angle)
                ) * (3f + pulse * 2f);

                spriteBatch.Draw(
                    texture,
                    drawPosition + offset,
                    frame,
                    glowColor,
                    Projectile.rotation,
                    origin,
                    Projectile.scale,
                    effects,
                    0f
                );
            }

            spriteBatch.Draw(
                texture,
                drawPosition,
                frame,
                Color.White,
                Projectile.rotation,
                origin,
                Projectile.scale,
                effects,
                0f
            );

            return false;
        }

        public override void AI()
        {
            Projectile.frameCounter++;

            if (Projectile.frameCounter >= FrameSpeed)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;

                if (Projectile.frame >= FrameCount)
                    Projectile.frame = 0;
            }

            int targetIndex = (int)Projectile.ai[0];
            float angle = Projectile.ai[2];

            Projectile.localAI[0]++;

            if (Projectile.localAI[0] <= ChargeTime)
            {
                targetIndex = GetValidOrNearbyTarget(targetIndex);

                if (targetIndex >= 0)
                {
                    Projectile.ai[0] = targetIndex;
                    NPC target = Main.npc[targetIndex];

                    float currentAngle = angle + Projectile.localAI[0] * 0.025f;

                    Vector2 offset = new Vector2(
                        (float)System.Math.Cos(currentAngle),
                        (float)System.Math.Sin(currentAngle)
                    ) * OrbitRadius;

                    Projectile.Center = target.Center + offset;
                    Projectile.velocity = Vector2.Zero;
                    Projectile.rotation += 0.16f;
                }

                if (Projectile.localAI[0] == 1f)
                    SoundEngine.PlaySound(SoundID.Item29, Projectile.Center);

                if (Projectile.localAI[0] == 12f)
                    SoundEngine.PlaySound(SoundID.Item4, Projectile.Center);

                if (Projectile.localAI[0] == 24f)
                    SoundEngine.PlaySound(SoundID.Item8, Projectile.Center);

                if (Main.rand.NextBool(2))
                {
                    Dust dust = Dust.NewDustDirect(
                        Projectile.position,
                        Projectile.width,
                        Projectile.height,
                        DustID.CursedTorch,
                        Main.rand.NextFloat(-1.5f, 1.5f),
                        Main.rand.NextFloat(-1.5f, 1.5f),
                        120,
                        new Color(105, 255, 75),
                        Main.rand.NextFloat(0.7f, 1.2f)
                    );

                    dust.noGravity = true;
                    dust.velocity *= 0.25f;
                }

                Lighting.AddLight(Projectile.Center, 0.12f, 0.5f, 0.06f);
                return;
            }

            if (Projectile.localAI[0] == ChargeTime + 1)
            {
                targetIndex = GetValidOrNearbyTarget(targetIndex);

                if (targetIndex >= 0)
                {
                    Projectile.ai[0] = targetIndex;
                    NPC target = Main.npc[targetIndex];

                    Vector2 direction = target.Center - Projectile.Center;

                    if (direction.LengthSquared() > 0f)
                        direction.Normalize();
                    else
                        direction = Vector2.UnitX;

                    Projectile.velocity = direction * LaunchSpeed;
                }
                else
                {
                    Vector2 direction = new Vector2(
                        (float)System.Math.Cos(angle),
                        (float)System.Math.Sin(angle)
                    );

                    Projectile.velocity = direction * LaunchSpeed;
                }

                Projectile.netUpdate = true;

                for (int i = 0; i < 16; i++)
                {
                    Dust dust = Dust.NewDustDirect(
                        Projectile.position,
                        Projectile.width,
                        Projectile.height,
                        DustID.CursedTorch,
                        Main.rand.NextFloat(-4f, 4f),
                        Main.rand.NextFloat(-4f, 4f),
                        100,
                        new Color(110, 255, 75),
                        Main.rand.NextFloat(1f, 1.6f)
                    );

                    dust.noGravity = true;
                }

                SoundEngine.PlaySound(SoundID.Item74, Projectile.Center);
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, Projectile.Center);
            }

            targetIndex = GetValidOrNearbyTarget(targetIndex);

            if (targetIndex >= 0)
            {
                Projectile.ai[0] = targetIndex;
                NPC target = Main.npc[targetIndex];

                Vector2 toTarget = target.Center - Projectile.Center;

                if (toTarget.LengthSquared() > 0f)
                {
                    toTarget.Normalize();

                    Vector2 desiredVelocity = toTarget * HomingSpeed;

                    Projectile.velocity = Vector2.Lerp(
                        Projectile.velocity,
                        desiredVelocity,
                        0.08f
                    );
                }
            }

            Projectile.rotation += 0.42f;

            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.CursedTorch,
                    -Projectile.velocity.X * 0.08f,
                    -Projectile.velocity.Y * 0.08f,
                    100,
                    new Color(105, 255, 75),
                    Main.rand.NextFloat(0.8f, 1.3f)
                );

                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }

            Lighting.AddLight(Projectile.Center, 0.16f, 0.55f, 0.08f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.BrokenArmor, 240);
            target.AddBuff(BuffID.CursedInferno, 180);

            for (int i = 0; i < 14; i++)
            {
                Dust dust = Dust.NewDustDirect(
                    target.position,
                    target.width,
                    target.height,
                    DustID.CursedTorch,
                    Main.rand.NextFloat(-4f, 4f),
                    Main.rand.NextFloat(-4f, 4f),
                    100,
                    new Color(110, 255, 75),
                    Main.rand.NextFloat(1f, 1.7f)
                );

                dust.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item14, target.Center);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustDirect(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.CursedTorch,
                    Main.rand.NextFloat(-3f, 3f),
                    Main.rand.NextFloat(-3f, 3f),
                    100,
                    new Color(110, 255, 75),
                    Main.rand.NextFloat(0.8f, 1.4f)
                );

                dust.noGravity = true;
            }
        }
    }
}
