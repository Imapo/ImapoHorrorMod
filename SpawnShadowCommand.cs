using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using ImapoHorrorMod.NPCs;

namespace ImapoHorrorMod
{
    public class SpawnShadowCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "summonshadow";
        public override string Description => "Призывает Теневую Копию прямо перед игроком для теста.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (caller.Player.whoAmI != Main.myPlayer) return;

            // Находим позицию на земле рядом с игроком
            int spawnX = (int)caller.Player.Center.X + (caller.Player.direction * 150);
            int spawnY = (int)caller.Player.Center.Y;
            
            // Ищем землю (спускаемся вниз пока не найдём тайл)
            for (int i = 0; i < 500; i++)
            {
                int tileX = spawnX / 16;
                int tileY = (spawnY + i) / 16;
                
                if (tileY < Main.maxTilesY && Main.tile[tileX, tileY] != null && Main.tile[tileX, tileY].HasTile)
                {
                    spawnY += i - 100; // Чуть выше земли
                    break;
                }
            }

            // Спавним NPC
            int npcIndex = NPC.NewNPC(
                caller.Player.GetSource_FromThis(),
                spawnX,
                spawnY,
                ModContent.NPCType<ShadowStalker>()
            );

            // Проверяем, заспавнился ли NPC
            if (npcIndex < Main.maxNPCs && Main.npc[npcIndex].active)
            {
                Main.NewText("Теневая Копия призвана!", Color.DarkRed);
                Main.npc[npcIndex].timeLeft = 3600; // Живёт 1 минуту
            }
            else
            {
                Main.NewText("Не удалось призвать Теневую Копию (слишком много NPC?)", Color.Red);
            }
        }
    }
}