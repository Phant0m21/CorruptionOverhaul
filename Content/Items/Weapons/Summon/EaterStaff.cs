using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using CorruptionOverhaul.Content.Buffs;
using CorruptionOverhaul.Content.Projectiles.Minions;

namespace CorruptionOverhaul.Content.Items.Weapons.Summon
{
    public class EaterStaff : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;

            Item.damage = 31;
            Item.knockBack = 2f;

            Item.DamageType = DamageClass.Summon;
            Item.mana = 11;

            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.useStyle = ItemUseStyleID.Swing;

            Item.noMelee = true;

            Item.UseSound = SoundID.Item44;

            Item.rare = ItemRarityID.Green;
            Item.value = Item.buyPrice(gold: 2);

            Item.shoot = ModContent.ProjectileType<EaterMinion>();
            Item.buffType = ModContent.BuffType<EaterMinionBuff>();
        }

        public override bool Shoot(
            Player player,
            EntitySource_ItemUse_WithAmmo source,
            Microsoft.Xna.Framework.Vector2 position,
            Microsoft.Xna.Framework.Vector2 velocity,
            int type,
            int damage,
            float knockback)
        {
            int minionType = ModContent.ProjectileType<EaterMinion>();

            player.AddBuff(Item.buffType, 2);

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];

                if (!projectile.active)
                    continue;

                if (projectile.owner != player.whoAmI)
                    continue;

                if (projectile.type != minionType)
                    continue;

                if (projectile.ai[0] < EaterMinion.MaxLevel)
                    projectile.ai[0]++;

                projectile.netUpdate = true;

                return false;
            }

            Projectile.NewProjectile(
                source,
                player.Center,
                Microsoft.Xna.Framework.Vector2.Zero,
                minionType,
                damage,
                knockback,
                player.whoAmI
            );

            return false;
        }
    }
}