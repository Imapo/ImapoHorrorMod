using Terraria;
using Terraria.ID; // <-- ДОБАВЛЕНО: для NetmodeID
using Terraria.ModLoader;

namespace ImapoHorrorMod.Systems
{
    public class HorrorSpawnSystem : ModSystem
    {
        private int spawnTimer = 0;

        public override void PostUpdateEverything()
        {
            // Работаем только в одиночной игре или на сервере (не на клиенте), и только ночью
            if (Main.netMode == NetmodeID.MultiplayerClient || Main.dayTime)
            {
                spawnTimer = 0;
                return;
            }

            // 1. Проверяем, жив ли уже хотя бы один Сталкер в мире
            bool stalkerExists = false;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && npc.type == ModContent.NPCType<NPCs.ShadowStalker>())
                {
                    stalkerExists = true;
                    break;
                }
            }

            // 2. Если Сталкера нет, запускаем таймер и проверку шанса
            if (!stalkerExists)
            {
                spawnTimer++;
                if (spawnTimer >= 60) // Проверяем ровно 1 раз в секунду (60 тиков)
                {
                    spawnTimer = 0;
                    
                    // Шанс 0.05 (5%)
                    if (Main.rand.NextFloat() < 0.05f)
                    {
                        Player player = Main.LocalPlayer;
                        if (player != null && player.active && !player.dead)
                        {
                            // Спавним за пределами экрана (на 800 пикселей в сторону от направления игрока)
                            int spawnX = (int)player.Center.X + (player.direction * 800);
                            int spawnY = (int)player.Center.Y - 200; // Чуть выше земли

                            // ИСПРАВЛЕНО: используем EntitySource_Misc для строкового контекста
                            NPC.NewNPC(
                                new Terraria.DataStructures.EntitySource_Misc("ShadowStalker"),
                                spawnX, spawnY,
                                ModContent.NPCType<NPCs.ShadowStalker>()
                            );
                        }
                    }
                }
            }
            else
            {
                // ВАЖНО: сбрасываем таймер, пока сталкер жив. 
                // Это гарантирует отсутствие "очереди" на спавн после его смерти.
                spawnTimer = 0;
            }
        }
    }
}