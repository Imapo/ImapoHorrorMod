using Terraria;
using Terraria.ModLoader;

namespace ImapoHorrorMod
{
    [Autoload(Side = ModSide.Client)]
    public class LimboSceneEffect : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/LimboAmbient");
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

        public override bool IsSceneEffectActive(Player player)
        {
            return LimboZoneCondition.IsPlayerInZone(player);
        }

        public override void SpecialVisuals(Player player, bool isActive)
        {
            player.ManageSpecialBiomeVisuals("ImapoHorrorMod:LimboRipple", isActive, player.Center);
        }
    }
}