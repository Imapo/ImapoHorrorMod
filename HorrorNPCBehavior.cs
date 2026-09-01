using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ImapoHorrorMod
{
    public class HorrorNPCBehavior : GlobalNPC
    {
        public const float SafeRadius = 400f;

        public override bool InstancePerEntity => true;

        private bool isPassive;

        public override bool PreAI(NPC npc)
        {
            // Игнорируем дружественных NPC, городских жителей и боссов
            if (npc.friendly || npc.townNPC || npc.boss)
            {
                return true; // Разрешаем обычный AI
            }

            // Ищем ближайшего живого игрока
            Player closestPlayer = null;
            float shortestDistance = SafeRadius;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player.active && !player.dead && !player.ghost)
                {
                    float distance = Vector2.Distance(npc.Center, player.Center);
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        closestPlayer = player;
                    }
                }
            }

            if (closestPlayer != null)
            {
                // Игрок в радиусе агрессии: "Пробуждаем" NPC
                if (isPassive)
                {
                    npc.damage = npc.defDamage;
                    npc.target = 255;
                    isPassive = false;
                }
            }
            else
            {
                // Игрок далеко: "призрачный" режим
                if (!isPassive)
                {
                    npc.damage = 0;
                    isPassive = true;
                }

                npc.target = 255;
                npc.velocity.X *= 0.2f;
            }

            // ВСЕГДА возвращаем true, чтобы оригинальный AI врага продолжал работать
            // (гравитация, анимации, столкновения и т.д.)
            return true;
        }
    }
}