using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorruptionOverhaul.Content.Projectiles.Melee
{
    public class MiasmaShatterProj : ModProjectile
    {
        private const int CrystalCooldown = 10;

        public override string Texture =>
            "CorruptionOverhaul/Content/Projectiles/Melee/MiasmaShatterProj";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.YoyosLifeTimeMultiplier[Projectile.type] = 11f;
            ProjectileID.Sets.YoyosMaximumRange[Projectile.type] = 28f * 16f;
            ProjectileID.Sets.YoyosTopSpeed[Projectile.type] = 16f;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.aiStyle = ProjAIStyleID.Yoyo;
            Projectile.scale = 1f;
            Projectile.netImportant = true;
        }

        public override void AI()
        {
            if (Main.rand.NextBool(2))
            {
                int cloudType;

                switch (Main.rand.Next(3))
                {
                    case 0:
                        cloudType =
                            ModContent.ProjectileType<MiasmaCloud1>();
                        break;

                    case 1:
                        cloudType =
                            ModContent.ProjectileType<MiasmaCloud2>();
                        break;

                    default:
                        cloudType =
                            ModContent.ProjectileType<MiasmaCloud3>();
                        break;
                }

                IEntitySource source =
                    Projectile.GetSource_FromAI();

                Projectile.NewProjectile(
                    source,
                    Projectile.Center +
                    Main.rand.NextVector2Circular(8f, 8f),
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    cloudType,
                    (int)(Projectile.damage * 0.15f),
                    0f,
                    Projectile.owner
                );
            }

            if (Projectile.ai[2] > 0)
                Projectile.ai[2]--;

            if (Projectile.ai[2] <= 0 &&
                Projectile.owner == Main.myPlayer)
            {
                NPC hitTarget = FindContactTarget();

                if (hitTarget != null)
                {
                    SpawnCrystals();
                    Projectile.ai[2] = CrystalCooldown;
                }
            }
        }

        private NPC FindContactTarget()
        {
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

                if (Projectile.Hitbox.Intersects(npc.Hitbox))
                    return npc;
            }

            return null;
        }

        private void SpawnCrystals()
        {
            Player player =
                Main.player[Projectile.owner];

            if (!player.active)
                return;

            int amount =
                Main.rand.Next(2, 4);

            IEntitySource source =
                Projectile.GetSource_FromAI();

            NPC closestTarget =
                FindCrystalTarget(player.Center);

            for (int i = 0; i < amount; i++)
            {
                float angle =
                    MathHelper.TwoPi /
                    amount *
                    i +
                    Main.rand.NextFloat(-0.35f, 0.35f);

                Vector2 direction =
                    angle.ToRotationVector2();

                Vector2 spawnPosition =
                    player.Center +
                    direction *
                    Main.rand.NextFloat(45f, 75f);

                Vector2 velocity;

                if (closestTarget != null)
                {
                    Vector2 targetDirection =
                        closestTarget.Center -
                        spawnPosition;

                    targetDirection =
                        targetDirection.SafeNormalize(
                            direction
                        );

                    targetDirection =
                        targetDirection.RotatedBy(
                            Main.rand.NextFloat(
                                -0.35f,
                                0.35f
                            )
                        );

                    velocity =
                        targetDirection *
                        Main.rand.NextFloat(7f, 9f);
                }
                else
                {
                    velocity =
                        direction *
                        Main.rand.NextFloat(5f, 7f);
                }

                int crystalDamage =
                    Projectile.damage;

                Projectile.NewProjectile(
                    source,
                    spawnPosition,
                    velocity,
                    ModContent.ProjectileType<MiasmaCrystal>(),
                    crystalDamage,
                    0f,
                    Projectile.owner,
                    Main.rand.Next(3)
                );
            }
        }

        private NPC FindCrystalTarget(
            Vector2 position)
        {
            NPC closest = null;
            float closestDistance = 500f;

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
                        position,
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

        public override bool PreDraw(
            ref Color lightColor)
        {
            Texture2D texture =
                ModContent.Request<Texture2D>(
                    "CorruptionOverhaul/Content/Projectiles/Melee/MiasmaShatterProj"
                ).Value;

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation,
                texture.Size() * 0.5f,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            return false;
        }
    }
}