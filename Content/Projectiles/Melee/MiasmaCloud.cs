using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorruptionOverhaul.Content.Projectiles.Melee
{
    public class MiasmaCloud1 : ModProjectile
    {
        public override string Texture =>
            "CorruptionOverhaul/Content/Projectiles/Melee/MiasmaCloud1";

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 26;

            Projectile.friendly = true;
            Projectile.hostile = false;

            Projectile.DamageType = DamageClass.Melee;

            Projectile.penetrate = -1;

            Projectile.timeLeft = 30;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Projectile.ai[0]++;

            Projectile.velocity *= 0.94f;

            float progress =
                Projectile.ai[0] / 30f;

            if (progress < 0.2f)
            {
                Projectile.scale =
                    MathHelper.Lerp(
                        0.35f,
                        0.9f,
                        progress / 0.2f
                    );
            }
            else
            {
                Projectile.scale =
                    MathHelper.Lerp(
                        0.9f,
                        1.15f,
                        (progress - 0.2f) / 0.8f
                    );
            }

            if (Projectile.ai[0] < 7)
            {
                Projectile.alpha =
                    (int)MathHelper.Lerp(
                        255f,
                        80f,
                        Projectile.ai[0] / 7f
                    );
            }
            else
            {
                Projectile.alpha =
                    (int)MathHelper.Lerp(
                        80f,
                        255f,
                        (Projectile.ai[0] - 7f) / 23f
                    );
            }

            Projectile.rotation += 0.01f;
        }

        public override void OnHitNPC(
            NPC target,
            NPC.HitInfo hit,
            int damageDone)
        {
            target.AddBuff(
                BuffID.CursedInferno,
                120
            );
        }

        public override bool PreDraw(
            ref Color lightColor)
        {
            Texture2D texture =
                ModContent.Request<Texture2D>(
                    "CorruptionOverhaul/Content/Projectiles/Melee/MiasmaCloud1"
                ).Value;

            Color drawColor =
                Color.White *
                (1f - Projectile.alpha / 255f);

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                drawColor,
                Projectile.rotation,
                texture.Size() * 0.5f,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            return false;
        }
    }

    public class MiasmaCloud2 : MiasmaCloud1
    {
        public override string Texture =>
            "CorruptionOverhaul/Content/Projectiles/Melee/MiasmaCloud2";

        public override bool PreDraw(
            ref Color lightColor)
        {
            Texture2D texture =
                ModContent.Request<Texture2D>(
                    "CorruptionOverhaul/Content/Projectiles/Melee/MiasmaCloud2"
                ).Value;

            Color drawColor =
                Color.White *
                (1f - Projectile.alpha / 255f);

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                drawColor,
                Projectile.rotation,
                texture.Size() * 0.5f,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            return false;
        }
    }

    public class MiasmaCloud3 : MiasmaCloud1
    {
        public override string Texture =>
            "CorruptionOverhaul/Content/Projectiles/Melee/MiasmaCloud3";

        public override bool PreDraw(
            ref Color lightColor)
        {
            Texture2D texture =
                ModContent.Request<Texture2D>(
                    "CorruptionOverhaul/Content/Projectiles/Melee/MiasmaCloud3"
                ).Value;

            Color drawColor =
                Color.White *
                (1f - Projectile.alpha / 255f);

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                drawColor,
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