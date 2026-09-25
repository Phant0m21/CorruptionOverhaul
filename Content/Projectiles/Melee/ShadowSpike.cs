using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorruptionOverhaul.Content.Projectiles.Melee
{
	public class ShadowSpike : ModProjectile
	{
		private const float HomingRange = 520f;
		private const float HomingStrength = 0.12f;

		public override string Texture =>
			"CorruptionOverhaul/Content/Projectiles/Melee/ShadowSpike";

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
		}

		public override void SetDefaults()
		{
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 90;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.extraUpdates = 1;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Lighting.AddLight(Projectile.Center, 0.2f, 0.03f, 0.35f);

			NPC target = FindTarget(Projectile.Center, HomingRange);

			if (target != null)
			{
				Vector2 desiredVelocity = (target.Center - Projectile.Center).SafeNormalize(Projectile.velocity) * 9f;
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, HomingStrength);
			}

			if (Main.rand.NextBool(2))
			{
				Dust dust = Dust.NewDustPerfect(
					Projectile.Center,
					DustID.PurpleTorch,
					-Projectile.velocity * 0.08f,
					120,
					new Color(175, 100, 255),
					Main.rand.NextFloat(0.7f, 1.1f));

				dust.noGravity = true;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Projectile.owner != Main.myPlayer)
				return;

			Projectile.NewProjectile(
				Projectile.GetSource_OnHit(target),
				target.Center,
				Vector2.Zero,
				ModContent.ProjectileType<ShadowTangle>(),
				0,
				0f,
				Projectile.owner,
				target.whoAmI,
				Main.rand.NextFloat(MathHelper.TwoPi));
		}

		private static NPC FindTarget(Vector2 position, float maxDistance)
		{
			NPC closest = null;
			float closestDistance = maxDistance * maxDistance;

			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];

				if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.life <= 0)
					continue;

				float distance = Vector2.DistanceSquared(position, npc.Center);

				if (distance < closestDistance)
				{
					closest = npc;
					closestDistance = distance;
				}
			}

			return closest;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 origin = texture.Size() * 0.5f;

			for (int i = 0; i < Projectile.oldPos.Length; i++)
			{
				if (Projectile.oldPos[i] == Vector2.Zero)
					continue;

				float opacity = (Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length * 0.28f;
				Color trailColor = new Color(145, 50, 240, 0) * opacity;
				Vector2 trailPosition = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
				Main.EntitySpriteDraw(texture, trailPosition, null, trailColor, Projectile.rotation, origin, Projectile.scale * 0.9f, SpriteEffects.None, 0f);
			}

			Color glowColor = new Color(170, 70, 255, 0) * 0.45f;
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, glowColor, Projectile.rotation, origin, Projectile.scale * 1.2f, SpriteEffects.None, 0f);
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}
	}
}
