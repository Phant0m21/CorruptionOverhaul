
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CorruptionOverhaul.Content.Projectiles.Melee;

namespace CorruptionOverhaul.Content.Items.Weapons.Melee
{
    public class DefiledSlicingAxe : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 42;
            Item.damage = 78;
            Item.DamageType = DamageClass.Melee;
            Item.knockBack = 6.5f;
            Item.crit = 4;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
            Item.value = Item.buyPrice(gold: 8);
            Item.rare = ItemRarityID.Yellow;
            Item.shoot = ModContent.ProjectileType<DefiledSlicingAxeProjectile>();
            Item.shootSpeed = 17f;
            Item.UseSound = SoundID.Item1;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.AleThrowingGlove, 1)
                .AddIngredient(ItemID.SoulofFright, 12)
                .AddIngredient(ItemID.SoulofNight, 8)
                .AddIngredient(ItemID.CursedFlame, 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
