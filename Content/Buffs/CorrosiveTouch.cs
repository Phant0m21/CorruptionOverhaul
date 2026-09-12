using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorruptionOverhaul.Content.Buffs
{
    public class CorrosiveTouch : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.pvpBuff[Type] = true;

            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            BuffID.Sets.TimeLeftDoesNotDecrease[Type] = true;
        }

        public override bool RightClick(int buffIndex)
        {
            return false;
        }

        public override void Update(
            Player player,
            ref int buffIndex)
        {
            if (!player.ZoneCorrupt)
            {
                player.DelBuff(buffIndex);
                buffIndex--;
                return;
            }

            player.buffTime[buffIndex] = 2;
        }
    }
}