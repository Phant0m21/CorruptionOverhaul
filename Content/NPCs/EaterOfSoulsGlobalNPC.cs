using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorruptionOverhaul.Content.NPCs
{
    public class EaterOfSoulsGlobalNPC : GlobalNPC
    {
        private int blockedTimer;
        private int phaseTimer;

        public override bool InstancePerEntity => true;

        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            if (npc.type != NPCID.EaterofSouls)
                return;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int sizeRoll = Main.rand.Next(100);

                npc.scale = sizeRoll < 40
                    ? Main.rand.NextFloat(0.7f, 0.9f)
                    : sizeRoll < 60
                        ? Main.rand.NextFloat(0.95f, 1.05f)
                        : Main.rand.NextFloat(1.15f, 1.4f);

                npc.netUpdate = true;
            }
        }

        public override void PostAI(NPC npc)
        {
            if (npc.type != NPCID.EaterofSouls)
                return;

            HandleTilePhasing(npc);
        }

        private void HandleTilePhasing(NPC npc)
        {
            if (phaseTimer > 0)
            {
                phaseTimer--;
                npc.noTileCollide = true;
                return;
            }

            npc.noTileCollide = false;

            if (blockedTimer > 0)
            {
                blockedTimer--;

                if (blockedTimer == 0)
                    phaseTimer = 45;

                return;
            }

            if (npc.velocity.LengthSquared() < 0.04f)
                return;

            Vector2 nextPosition = npc.position + npc.velocity;

            if (Collision.SolidCollision(nextPosition, npc.width, npc.height))
            {
                blockedTimer = 20;
                npc.velocity *= 0.2f;
            }
        }
    }
}