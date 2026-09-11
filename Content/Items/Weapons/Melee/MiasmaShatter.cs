using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CorruptionOverhaul.Content.Projectiles.Melee;

namespace CorruptionOverhaul.Content.Items.Weapons.Melee
{
    public class MiasmaShatter : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 98;
            Item.DamageType = DamageClass.Melee;
            Item.knockBack = 3.5f;
            Item.crit = 4;
            Item.width = 52;
            Item.height = 36;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.autoReuse = false;
            Item.rare = ItemRarityID.Lime;
            Item.shoot = ModContent.ProjectileType<MiasmaShatterProj>();
            Item.shootSpeed = 16f;
            Item.UseSound = SoundID.Item1;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.JungleYoyo, 1)
                .AddIngredient(ItemID.CorruptYoyo, 1)
                .AddIngredient(ItemID.ChlorophyteBar, 8)
                .AddIngredient(ItemID.CursedFlame, 15)
                .AddIngredient(ItemID.SoulofNight, 10)
                .AddTile(TileID.DemonAltar)
                .Register();
        }
    }
}