using Terraria;
using Terraria.ModLoader;

namespace LimboFilterSystem
{
    [Autoload(Side = ModSide.Client)]
    public class LimboMusicSceneEffect : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/LimboAmbient");

        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

        public override bool IsSceneEffectActive(Player player)
        {
            return true; // играет везде и всегда
        }
    }
}