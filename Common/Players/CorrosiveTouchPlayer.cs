using Terraria;
using Terraria.ModLoader;
using CorruptionOverhaul.Content.Buffs;

namespace CorruptionOverhaul.Common.Players
{
    public class CorrosiveTouchPlayer : ModPlayer
    {
        public bool HasCorrosiveTouch
        {
            get
            {
                return Player.HasBuff<CorrosiveTouch>();
            }
        }

        public override void ResetEffects()
        {
            if (Player.ZoneCorrupt)
            {
                Player.AddBuff(
                    ModContent.BuffType<CorrosiveTouch>(),
                    2,
                    true
                );
            }
        }

        public override void UpdateEquips()
        {
            if (!HasCorrosiveTouch)
                return;

            Player.statDefense -= GetDefenseReduction();

            Player.manaRegenDelayBonus +=
                Player.manaRegenDelay * 0.25f;

            Player.blackout = true;
        }

        public override void NaturalLifeRegen(ref float regen)
        {
            if (!HasCorrosiveTouch)
                return;

            regen = 0f;
        }

        private int GetDefenseReduction()
        {
            if (Main.masterMode)
                return 12;

            if (Main.expertMode)
                return 8;

            return 4;
        }
    }
}