using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using CorruptionOverhaul.Content.Projectiles.Melee;

namespace CorruptionOverhaul.Content.Items.Weapons.Melee
{
    public class RottenSlicer : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 136;
            Item.DamageType = DamageClass.Melee;
            Item.knockBack = 6.5f;
            Item.crit = 24;
            Item.width = 52;
            Item.height = 52;

            Item.useTime = 52;
            Item.useAnimation = 52;
            Item.useStyle = ItemUseStyleID.Swing;

            Item.autoReuse = true;
            Item.UseSound = null;

            Item.rare = ItemRarityID.Pink;
            Item.value = Item.buyPrice(gold: 8);

            Item.shoot = ModContent.ProjectileType<RottenSlicerWave>();
            Item.shootSpeed = 12f;

            Item.noMelee = false;
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
            Projectile.NewProjectile(
                source,
                player.Center,
                Vector2.Zero,
                ModContent.ProjectileType<RottenSlicerWave>(),
                78,
                16f,
                player.whoAmI
            );

            return false;
        }

        public override void HoldItem(Player player)
        {
            if (Main.rand.NextBool(7))
            {
                Dust dust = Dust.NewDustPerfect(
                    player.Center + Main.rand.NextVector2Circular(25f, 30f),
                    DustID.CorruptGibs,
                    Main.rand.NextVector2Circular(0.6f, 0.6f),
                    100,
                    new Color(105, 230, 90),
                    Main.rand.NextFloat(0.5f, 0.9f)
                );

                dust.noGravity = true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.LightsBane, 1)
                .AddIngredient(ItemID.RottenChunk, 15)
                .AddIngredient(ItemID.SoulofNight, 8)
                .AddIngredient(ItemID.DemoniteBar, 8)
                .AddTile(TileID.DemonAltar)
                .Register();
        }
    }
}