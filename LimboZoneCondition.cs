// Новый файл: LimboZoneCondition.cs
using Terraria;

namespace LimboFilterSystem
{
    public static class LimboZoneCondition
    {
        public static bool IsPlayerInZone(Player player)
        {
            return player.ZoneCorrupt; // единое условие для фильтра и музыки
        }
    }
}