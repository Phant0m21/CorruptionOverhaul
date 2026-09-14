using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CorruptionOverhaul.Common.Players;

namespace CorruptionOverhaul.Content.Items
{
    public class RottenParasite : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(
                gold: 8
            );

            Item.rare = ItemRarityID.LightPurple;
        }

        public override void UpdateAccessory(
            Player player,
            bool hideVisual)
        {
            RottenParasitePlayer modPlayer =
                player.GetModPlayer<RottenParasitePlayer>();

            modPlayer.RottenParasiteEquipped = true;

            player.AddBuff(
                BuffID.Dangersense,
                2
            );

            player.statDefense += 4;
            player.lifeRegen += 3;

            if (player.statLife >= player.statLifeMax2)
            {
                player.GetDamage(DamageClass.Generic) +=
                    player.endurance * 0.60f;
            }
        }
    }
}