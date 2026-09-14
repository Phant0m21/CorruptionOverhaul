using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CorruptionOverhaul.Content.Items;

namespace CorruptionOverhaul.Common.Players
{
    public class RottenParasitePlayer : ModPlayer
    {
        public bool RottenParasiteEquipped;

        public override void ResetEffects()
        {
            RottenParasiteEquipped = false;
        }

        public override void ModifyHurt(
            ref Player.HurtModifiers modifiers)
        {
            if (!RottenParasiteEquipped)
                return;

            if (Player.statLife >= Player.statLifeMax2)
            {
                modifiers.FinalDamage *= 0.5f;
            }
        }

        public override void OnHurt(
            Player.HurtInfo info)
        {
            if (!RottenParasiteEquipped)
                return;

            if (Main.rand.NextFloat() >= 0.10f)
                return;

            int radius = 420;
            int radiusSquared = radius * radius;

            for (
                int i = 0;
                i < Main.maxNPCs;
                i++
            )
            {
                NPC npc = Main.npc[i];

                if (!npc.active ||
                    npc.friendly ||
                    npc.dontTakeDamage)
                {
                    continue;
                }

                if (npc.DistanceSQ(Player.Center) > radiusSquared)
                    continue;

                npc.AddBuff(
                    BuffID.Confused,
                    180
                );
            }

            CreateConfusionEffect();
        }

        private void CreateConfusionEffect()
        {
            for (
                int i = 0;
                i < 35;
                i++
            )
            {
                Vector2 velocity =
                    Main.rand.NextVector2Circular(
                        5f,
                        5f
                    );

                Dust dust =
                    Dust.NewDustPerfect(
                        Player.Center +
                        Main.rand.NextVector2Circular(
                            18f,
                            18f
                        ),
                        DustID.Poisoned,
                        velocity,
                        100,
                        default,
                        Main.rand.NextFloat(
                            1.1f,
                            1.7f
                        )
                    );

                dust.noGravity = true;
            }

            for (
                int i = 0;
                i < 12;
                i++
            )
            {
                Dust dust =
                    Dust.NewDustPerfect(
                        Player.Center +
                        Main.rand.NextVector2Circular(
                            28f,
                            28f
                        ),
                        DustID.GreenTorch,
                        Main.rand.NextVector2Circular(
                            3f,
                            3f
                        ),
                        100,
                        default,
                        Main.rand.NextFloat(
                            0.8f,
                            1.3f
                        )
                    );

                dust.noGravity = true;
            }
        }
    }
}