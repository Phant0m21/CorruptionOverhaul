using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorruptionOverhaul.Content.Projectiles.Melee
{
	public class ShadowYoyoProj : ModProjectile
	{
		private const int SpikeCooldown = 32;

		public override string Texture =>
			"CorruptionOverhaul/Content/Projectiles/Melee/ShadowYoyoProj";

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.YoyosLifeTimeMultiplier[Projectile.type] = 12f;
			ProjectileID.Sets.YoyosMaximumRange[Projectile.type] = 28f * 16f;
			ProjectileID.Sets.YoyosTopSpeed[Projectile.type] = 14f;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
		}

		public override void SetDefaults()
		{
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = -1;
			Projectile.aiStyle = ProjAIStyleID.Yoyo;
			Projectile.netImportant = true;
		}

		public override void AI()
		{
			if (Projectile.ai[2] > 0f)
				Projectile.ai[2]--;

			if (Projectile.owner != Main.myPlayer || Projectile.ai[2] > 0f)
				return;

			NPC target = FindTarget(Projectile.Center, 620f);

			if (target == null)
				return;

			Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
			Vector2 spawnPosition = Projectile.Center + direction * 10f;
			Vector2 velocity = direction * 8.5f;
			int spikeDamage = Math.Max(1, (int)(Projectile.damage * 0.55f));

			Projectile.NewProjectile(
				Projectile.GetSource_FromAI(),
				spawnPosition,
				velocity,
				ModContent.ProjectileType<ShadowSpike>(),
				spikeDamage,
				1f,
				Projectile.owner,
				target.whoAmI);

			Projectile.ai[2] = SpikeCooldown;
			Projectile.netUpdate = true;
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
			Vector2 drawPosition = Projectile.Center - Main.screenPosition;
			Vector2 origin = texture.Size() * 0.5f;

			for (int i = 3; i >= 1; i--)
			{
				float glowScale = Projectile.scale * (1f + i * 0.08f);
				Color glowColor = new Color(120, 35, 190, 0) * (0.08f / i);
				Main.EntitySpriteDraw(texture, drawPosition, null, glowColor, Projectile.rotation, origin, glowScale, SpriteEffects.None, 0f);
			}

			for (int i = 0; i < Projectile.oldPos.Length; i++)
			{
				if (Projectile.oldPos[i] == Vector2.Zero)
					continue;

				float opacity = (Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length * 0.18f;
				Color trailColor = new Color(100, 25, 170, 0) * opacity;
				Vector2 trailPosition = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
				Main.EntitySpriteDraw(texture, trailPosition, null, trailColor, Projectile.rotation, origin, Projectile.scale * 0.85f, SpriteEffects.None, 0f);
			}

			Main.EntitySpriteDraw(texture, drawPosition, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}
	}
}
