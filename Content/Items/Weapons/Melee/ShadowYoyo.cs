using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using CorruptionOverhaul.Content.Projectiles.Melee;

namespace CorruptionOverhaul.Content.Items.Weapons.Melee
{
	public class ShadowYoyo : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 28;
			Item.DamageType = DamageClass.Melee;
			Item.knockBack = 3.5f;
			Item.crit = 4;
			Item.width = 32;
			Item.height = 32;

			Item.useTime = 25;
			Item.useAnimation = 25;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.channel = true;
			Item.noUseGraphic = true;
			Item.noMelee = true;

			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(silver: 75);
			Item.UseSound = SoundID.Item1;

			Item.shoot = ModContent.ProjectileType<ShadowYoyoProj>();
			Item.shootSpeed = 14f;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.DemoniteBar, 10)
				.AddIngredient(ItemID.ShadowScale, 15)
				.AddIngredient(ItemID.WhiteString, 1)
				.AddTile(TileID.Loom)
				.Register();
		}
	}
}
