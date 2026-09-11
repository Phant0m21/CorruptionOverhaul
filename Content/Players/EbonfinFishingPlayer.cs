using Microsoft.Xna.Framework;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

using CorruptionOverhaul.Content.Items.Weapons.Summon;

namespace CorruptionOverhaul.Content.Players
{
    public class EbonfinFishingPlayer : ModPlayer
    {
        public override void CatchFish(
            FishingAttempt attempt,
            ref int itemDrop,
            ref int npcSpawn,
            ref AdvancedPopupRequest sonar,
            ref Vector2 sonarPosition)
        {
            if (attempt.inLava || attempt.inHoney)
                return;

            if (!Player.ZoneCorrupt)
                return;

            if (attempt.waterTilesCount <= 0)
                return;

            if (!Main.rand.NextBool(10))
                return;

            itemDrop = ModContent.ItemType<EaterStaff>();
            npcSpawn = -1;
        }
    }
}