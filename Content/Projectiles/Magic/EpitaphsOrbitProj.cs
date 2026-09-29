using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorruptionOverhaul.Content.Projectiles.Magic
{
    public class EpitaphsOrbitProj : ModProjectile
    {
        private static readonly Color AcidColor =
            new Color(150, 255, 35);

        private static readonly Color AcidBright =
            new Color(215, 255, 70);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 18;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;

            Projectile.friendly = true;
            Projectile.hostile = false;

            Projectile.DamageType = DamageClass.Magic;

            Projectile.penetrate = 1;

            Projectile.timeLeft = 150;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.extraUpdates = 1;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            if (Projectile.ai[1] == 1f)
            {
                ExplosionAI();
                return;
            }

            Player player =
                Main.player[Projectile.owner];

            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            Projectile.localAI[1] += 1f;

            NPC target = FindTarget();

            if (target != null)
            {
                HomeToTarget(target);
            }
            else
            {
                CurveTowardsCursor();
            }

            Projectile.rotation =
                Projectile.velocity.ToRotation() +
                MathHelper.PiOver2;

            SpawnTrail();

            if (Projectile.localAI[1] > 8f &&
                Main.rand.NextBool(2))
            {
                SpawnAcidSpark();
            }

            if (Main.rand.NextBool(10))
            {
                SpawnSmallBurst();
            }

            Lighting.AddLight(
                Projectile.Center,
                0.45f,
                0.85f,
                0.08f
            );
        }

        private NPC FindTarget()
        {
            NPC closest = null;

            float maxDistance = 1050f;
            float closestDistance = maxDistance;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                if (!npc.active ||
                    npc.friendly ||
                    npc.dontTakeDamage ||
                    !npc.CanBeChasedBy())
                {
                    continue;
                }

                float distance =
                    Vector2.Distance(
                        Projectile.Center,
                        npc.Center
                    );

                if (distance >= closestDistance)
                    continue;

                closestDistance = distance;
                closest = npc;
            }

            return closest;
        }

        private void HomeToTarget(NPC target)
        {
            Vector2 direction =
                target.Center -
                Projectile.Center;

            if (direction.LengthSquared() < 1f)
                return;

            direction.Normalize();

            Vector2 currentDirection =
                Projectile.velocity.SafeNormalize(
                    Vector2.UnitX
                );

            float age =
                Projectile.localAI[1];

            float turnStrength;

            if (age < 18f)
            {
                turnStrength = 0.025f;
            }
            else if (age < 35f)
            {
                turnStrength = 0.075f;
            }
            else
            {
                turnStrength = 0.14f;
            }

            currentDirection =
                Vector2.Lerp(
                    currentDirection,
                    direction,
                    turnStrength
                );

            currentDirection.Normalize();

            float speed =
                Projectile.velocity.Length();

            speed =
                MathHelper.Clamp(
                    speed + 0.045f,
                    9f,
                    18f
                );

            Projectile.velocity =
                currentDirection * speed;

            Projectile.netUpdate = true;
        }

        private void CurveTowardsCursor()
        {
            Vector2 direction =
                Main.MouseWorld -
                Projectile.Center;

            if (direction.LengthSquared() < 1f)
                return;

            direction.Normalize();

            Vector2 currentDirection =
                Projectile.velocity.SafeNormalize(
                    Vector2.UnitX
                );

            float age =
                Projectile.localAI[1];

            float turnStrength;

            if (age < 10f)
            {
                turnStrength = 0.01f;
            }
            else if (age < 25f)
            {
                turnStrength = 0.035f;
            }
            else
            {
                turnStrength = 0.07f;
            }

            currentDirection =
                Vector2.Lerp(
                    currentDirection,
                    direction,
                    turnStrength
                );

            currentDirection.Normalize();

            float speed =
                Projectile.velocity.Length();

            speed =
                MathHelper.Clamp(
                    speed + 0.025f,
                    9f,
                    16f
                );

            Projectile.velocity =
                currentDirection * speed;
        }

        private void SpawnTrail()
        {
            Vector2 direction =
                Projectile.velocity.SafeNormalize(
                    Vector2.UnitX
                );

            Vector2 backwards =
                -direction;

            for (int i = 0; i < 2; i++)
            {
                Vector2 position =
                    Projectile.Center +
                    backwards *
                    Main.rand.NextFloat(
                        5f,
                        14f
                    );

                Vector2 velocity =
                    backwards *
                    Main.rand.NextFloat(
                        0.5f,
                        2.2f
                    );

                velocity +=
                    Main.rand.NextVector2Circular(
                        1.2f,
                        1.2f
                    );

                Dust dust =
                    Dust.NewDustPerfect(
                        position,
                        DustID.TintableDustLighted,
                        velocity,
                        0,
                        AcidColor,
                        Main.rand.NextFloat(
                            0.65f,
                            1.15f
                        )
                    );

                dust.noGravity = true;
                dust.fadeIn =
                    dust.scale + 0.25f;
            }
        }

        private void SpawnAcidSpark()
        {
            Vector2 velocity =
                Main.rand.NextVector2Circular(
                    2.5f,
                    2.5f
                );

            Dust dust =
                Dust.NewDustPerfect(
                    Projectile.Center +
                    Main.rand.NextVector2Circular(
                        8f,
                        8f
                    ),
                    DustID.TintableDustLighted,
                    velocity,
                    0,
                    AcidBright,
                    Main.rand.NextFloat(
                        0.5f,
                        0.9f
                    )
                );

            dust.noGravity = true;
        }

        private void SpawnSmallBurst()
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 velocity =
                    Main.rand.NextVector2Circular(
                        3f,
                        3f
                    );

                Dust dust =
                    Dust.NewDustPerfect(
                        Projectile.Center,
                        DustID.TintableDustLighted,
                        velocity,
                        0,
                        AcidColor,
                        Main.rand.NextFloat(
                            0.45f,
                            0.75f
                        )
                    );

                dust.noGravity = true;
            }
        }

        public override bool OnTileCollide(
            Vector2 oldVelocity)
        {
            return false;
        }

        public override void OnHitNPC(
            NPC target,
            NPC.HitInfo hit,
            int damageDone)
        {
            if (Projectile.ai[1] == 1f)
                return;

            target.AddBuff(
                BuffID.CursedInferno,
                300
            );

            Projectile.Kill();
        }

        public override void OnKill(
            int timeLeft)
        {
            if (Projectile.ai[1] == 1f)
                return;

            if (Main.netMode != NetmodeID.MultiplayerClient ||
                Projectile.owner == Main.myPlayer)
            {
                int explosionDamage =
                    (int)(Projectile.damage * 0.65f);

                if (explosionDamage < 1)
                    explosionDamage = 1;

                int explosion =
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        Vector2.Zero,
                        Type,
                        explosionDamage,
                        Projectile.knockBack,
                        Projectile.owner,
                        0f,
                        1f
                    );

                if (explosion >= 0 &&
                    explosion < Main.maxProjectiles)
                {
                    Main.projectile[explosion].DamageType =
                        DamageClass.Magic;
                }
            }

            CreateExplosionEffect();

            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(
                    SoundID.Item74,
                    Projectile.Center
                );
            }
        }

        private void CreateExplosionEffect()
        {
            for (int i = 0; i < 55; i++)
            {
                float angle =
                    MathHelper.TwoPi *
                    i /
                    55f;

                Vector2 velocity =
                    new Vector2(
                        (float)Math.Cos(angle),
                        (float)Math.Sin(angle)
                    );

                velocity *=
                    Main.rand.NextFloat(
                        2.5f,
                        8.5f
                    );

                Dust dust =
                    Dust.NewDustPerfect(
                        Projectile.Center +
                        Main.rand.NextVector2Circular(
                            8f,
                            8f
                        ),
                        DustID.TintableDustLighted,
                        velocity,
                        0,
                        i % 3 == 0
                            ? AcidBright
                            : AcidColor,
                        Main.rand.NextFloat(
                            0.8f,
                            1.6f
                        )
                    );

                dust.noGravity = true;
            }

            for (int i = 0; i < 25; i++)
            {
                Vector2 velocity =
                    Main.rand.NextVector2Circular(
                        5f,
                        5f
                    );

                Dust dust =
                    Dust.NewDustPerfect(
                        Projectile.Center,
                        DustID.TintableDustLighted,
                        velocity,
                        0,
                        AcidBright,
                        Main.rand.NextFloat(
                            1f,
                            2f
                        )
                    );

                dust.noGravity = true;
            }

            for (int i = 0; i < 12; i++)
            {
                Vector2 velocity =
                    Main.rand.NextVector2Circular(
                        2f,
                        2f
                    );

                Dust dust =
                    Dust.NewDustPerfect(
                        Projectile.Center,
                        DustID.Poisoned,
                        velocity,
                        100,
                        AcidColor,
                        Main.rand.NextFloat(
                            1.2f,
                            2f
                        )
                    );

                dust.noGravity = true;
            }

            Lighting.AddLight(
                Projectile.Center,
                1.2f,
                1.4f,
                0.1f
            );
        }

        private void ExplosionAI()
        {
            Projectile.velocity = Vector2.Zero;

            Projectile.alpha += 85;

            if (Projectile.timeLeft == 2)
            {
                Projectile.width = 132;
                Projectile.height = 132;
            }

            if (Projectile.timeLeft == 1)
            {
                Projectile.alpha = 255;
            }

            if (Main.rand.NextBool(2))
            {
                Vector2 velocity =
                    Main.rand.NextVector2Circular(
                        4f,
                        4f
                    );

                Dust dust =
                    Dust.NewDustPerfect(
                        Projectile.Center +
                        Main.rand.NextVector2Circular(
                            55f,
                            55f
                        ),
                        DustID.TintableDustLighted,
                        velocity,
                        0,
                        AcidBright,
                        Main.rand.NextFloat(
                            0.8f,
                            1.6f
                        )
                    );

                dust.noGravity = true;
            }

            Lighting.AddLight(
                Projectile.Center,
                1.3f,
                1.5f,
                0.1f
            );
        }
    }
}