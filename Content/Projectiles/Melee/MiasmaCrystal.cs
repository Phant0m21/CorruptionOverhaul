using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorruptionOverhaul.Content.Projectiles.Melee
{
    public class MiasmaCrystal : ModProjectile
    {
        private const float HomingRange = 500f;
        private const float HomingStrength = 0.08f;
        private const int SpiralTime = 12;

        public override string Texture =>
            "CorruptionOverhaul/Content/Projectiles/Melee/MiasmaCrystal1";

        public override void SetDefaults()
        {
            Projectile.width = 34;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 40;

                if (Projectile.alpha < 0)
                    Projectile.alpha = 0;
            }

            Projectile.ai[1]++;

            if (Projectile.ai[1] <= SpiralTime)
            {
                float angle =
                    Projectile.ai[1] * 0.32f +
                    Projectile.ai[0] * 2.1f;

                Vector2 perpendicular =
                    new Vector2(
                        (float)System.Math.Cos(angle),
                        (float)System.Math.Sin(angle)
                    );

                Projectile.velocity +=
                    perpendicular * 0.18f;
            }

            NPC target = FindTarget();

            if (target != null)
            {
                Vector2 desiredVelocity =
                    Projectile.DirectionTo(target.Center) *
                    Projectile.velocity.Length();

                if (desiredVelocity.LengthSquared() > 1f)
                {
                    Projectile.velocity =
                        Vector2.Lerp(
                            Projectile.velocity,
                            desiredVelocity,
                            HomingStrength
                        );
                }
            }

            Projectile.rotation +=
                0.28f +
                Projectile.velocity.Length() * 0.025f;

            Projectile.velocity *= 0.992f;

            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center +
                    Main.rand.NextVector2Circular(7f, 7f),
                    DustID.CorruptGibs,
                    -Projectile.velocity * 0.04f +
                    Main.rand.NextVector2Circular(0.25f, 0.25f),
                    120,
                    new Color(105, 230, 90),
                    Main.rand.NextFloat(0.4f, 0.7f)
                );

                dust.noGravity = true;
            }
        }

        private NPC FindTarget()
        {
            NPC closest = null;
            float closestDistance = HomingRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                if (!npc.active ||
                    npc.friendly ||
                    npc.dontTakeDamage ||
                    npc.life <= 0)
                {
                    continue;
                }

                float distance =
                    Vector2.Distance(
                        Projectile.Center,
                        npc.Center
                    );

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = npc;
                }
            }

            return closest;
        }

        public override void ModifyHitNPC(
            NPC target,
            ref NPC.HitModifiers modifiers)
        {
            modifiers.Defense -=
                System.Math.Min(target.defense, 20);
        }

        public override void OnHitNPC(
            NPC target,
            NPC.HitInfo hit,
            int damageDone)
        {
            target.AddBuff(
                BuffID.WitheredArmor,
                60
            );
        }

        public override bool PreDraw(
            ref Color lightColor)
        {
            int crystal =
                (int)Projectile.ai[0];

            string texturePath;

            if (crystal == 0)
            {
                texturePath =
                    "CorruptionOverhaul/Content/Projectiles/Melee/MiasmaCrystal1";
            }
            else if (crystal == 1)
            {
                texturePath =
                    "CorruptionOverhaul/Content/Projectiles/Melee/MiasmaCrystal2";
            }
            else
            {
                texturePath =
                    "CorruptionOverhaul/Content/Projectiles/Melee/MiasmaCrystal3";
            }

            Texture2D texture =
                ModContent.Request<Texture2D>(
                    texturePath
                ).Value;

            float opacity =
                1f - Projectile.alpha / 255f;

            Vector2 drawPosition =
                Projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(
                texture,
                drawPosition,
                null,
                Color.White * opacity * 0.35f,
                Projectile.rotation,
                texture.Size() * 0.5f,
                1.18f,
                SpriteEffects.None,
                0
            );

            Main.EntitySpriteDraw(
                texture,
                drawPosition,
                null,
                Color.White * opacity,
                Projectile.rotation,
                texture.Size() * 0.5f,
                1f,
                SpriteEffects.None,
                0
            );

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.CorruptGibs,
                    Main.rand.NextVector2Circular(2.2f, 2.2f),
                    120,
                    new Color(105, 230, 90),
                    Main.rand.NextFloat(0.4f, 0.8f)
                );

                dust.noGravity = true;
            }
        }
    }
}