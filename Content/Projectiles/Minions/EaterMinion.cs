using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CorruptionOverhaul.Content.Buffs;

namespace CorruptionOverhaul.Content.Projectiles.Minions
{
    public class EaterMinion : ModProjectile
    {
        public const int MaxLevel = 5;

        private const float TargetDistance = 700f;
        private const float FollowDistance = 90f;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            Main.projPet[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 52;
            Projectile.height = 24;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.minion = true;
            Projectile.minionSlots = 1f;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.timeLeft = 2;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            if (!player.HasBuff(ModContent.BuffType<EaterMinionBuff>()))
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;

            int level = (int)Projectile.ai[0];

            if (level < 1)
                level = 1;

            if (level > MaxLevel)
                level = MaxLevel;

            Animate();
            FollowPlayer(player);

            NPC target = FindTarget(player);

            if (target != null)
            {
                FaceTarget(target);
                Shoot(target, level);
            }
            else
            {
                Projectile.localAI[1] = 0f;
                UpdateDirection();
            }
        }

        private void Animate()
        {
            Projectile.frameCounter++;

            if (Projectile.frameCounter >= 7)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;

                if (Projectile.frame >= 4)
                    Projectile.frame = 0;
            }
        }

        private void FollowPlayer(Player player)
        {
            Vector2 targetPosition = player.Center;

            targetPosition.X -= player.direction * FollowDistance;
            targetPosition.Y -= 45f;

            Vector2 direction = targetPosition - Projectile.Center;
            float distance = direction.Length();

            if (distance > 1200f)
            {
                Projectile.Center = targetPosition;
                Projectile.velocity = Vector2.Zero;
                Projectile.netUpdate = true;
                return;
            }

            float speed = 8f;

            if (distance > 40f)
            {
                direction.Normalize();

                Projectile.velocity = Vector2.Lerp(
                    Projectile.velocity,
                    direction * speed,
                    0.12f
                );
            }
            else
            {
                Projectile.velocity *= 0.92f;
            }

            Projectile.position.Y +=
                (float)System.Math.Sin(
                    Main.GameUpdateCount * 0.05f + Projectile.whoAmI
                ) * 0.15f;
        }

        private void UpdateDirection()
        {
            if (Projectile.velocity.X < -0.1f)
                Projectile.spriteDirection = 1;
            else if (Projectile.velocity.X > 0.1f)
                Projectile.spriteDirection = -1;
        }

        private void FaceTarget(NPC target)
        {
            float directionX = target.Center.X - Projectile.Center.X;

            if (directionX < 0f)
                Projectile.spriteDirection = 1;
            else if (directionX > 0f)
                Projectile.spriteDirection = -1;
        }

        private NPC FindTarget(Player player)
        {
            if (player.HasMinionAttackTargetNPC)
            {
                NPC target = Main.npc[player.MinionAttackTargetNPC];

                if (target.CanBeChasedBy(Projectile))
                {
                    if (Vector2.Distance(
                        Projectile.Center,
                        target.Center
                    ) <= TargetDistance)
                    {
                        return target;
                    }
                }
            }

            NPC bestTarget = null;
            float bestDistance = TargetDistance;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                if (!npc.CanBeChasedBy(Projectile))
                    continue;

                float distance = Vector2.Distance(
                    Projectile.Center,
                    npc.Center
                );

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestTarget = npc;
                }
            }

            return bestTarget;
        }

        private void Shoot(NPC target, int level)
        {
            Projectile.localAI[1]++;

            int cooldown = 24 - (level - 1) * 2;

            if (cooldown < 16)
                cooldown = 16;

            if (Projectile.localAI[1] < cooldown)
                return;

            Projectile.localAI[1] = 0f;

            if (Projectile.owner != Main.myPlayer)
                return;

            FaceTarget(target);

            Vector2 velocity =
                (target.Center - Projectile.Center)
                .SafeNormalize(Vector2.UnitX);

            float randomSpread = Main.rand.NextFloat(-0.12f, 0.12f);

            velocity = velocity.RotatedBy(randomSpread);
            velocity *= 9f;

            int damage = Projectile.damage;
            damage += (level - 1) * 4;

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                velocity,
                ModContent.ProjectileType<EaterMinionProj>(),
                damage,
                1f,
                Projectile.owner
            );
        }
    }
}