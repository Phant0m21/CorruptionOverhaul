using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using CorruptionOverhaul.Content.Projectiles.Magic;

namespace CorruptionOverhaul.Content.Items.Weapons.Magic
{
    public class EpitaphsOrbit : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 52;

            Item.damage = 180;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 23;

            Item.useTime = 34;
            Item.useAnimation = 34;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.crit = 17;
            Item.noMelee = true;

            Item.knockBack = 6f;

            Item.value = Item.buyPrice(
                gold: 24
            );

            Item.rare = ItemRarityID.Yellow;

            Item.UseSound = SoundID.Item20;

            Item.autoReuse = true;

            Item.shoot =
                ModContent.ProjectileType<EpitaphsOrbitProj>();

            Item.shootSpeed = 10f;
        }

        public override bool Shoot(
            Player player,
            EntitySource_ItemUse_WithAmmo source,
            Vector2 position,
            Vector2 velocity,
            int type,
            int damage,
            float knockback)
        {
            Vector2 direction =
                Main.MouseWorld -
                player.Center;

            if (direction.LengthSquared() < 0.01f)
                direction = Vector2.UnitX;

            direction.Normalize();

            Vector2[] directions =
            {
                direction.RotatedBy(-0.32f),
                direction,
                direction.RotatedBy(0.32f)
            };

            for (int i = 0; i < 3; i++)
            {
                Vector2 spawnPosition =
                    player.Center +
                    directions[i] * 30f;

                Vector2 projectileVelocity =
                    directions[i] *
                    Main.rand.NextFloat(
                        9.5f,
                        10.5f
                    );

                Projectile.NewProjectile(
                    source,
                    spawnPosition,
                    projectileVelocity,
                    type,
                    damage,
                    knockback,
                    player.whoAmI,
                    i - 1
                );
            }

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.RazorbladeTyphoon)
                .AddIngredient(ItemID.CursedFlames)
                .AddIngredient(ItemID.EbonstoneBlock, 100)
                .AddIngredient(ItemID.SoulofNight, 15)
                .AddIngredient(ItemID.FragmentNebula, 12)
                .AddTile(TileID.Bookcases)
                .Register();
        }
    }
}