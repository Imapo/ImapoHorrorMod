using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace LimboFilterSystem
{
    public class LimboFilterSystem : ModSystem
    {
        public override void Load()
        {
            if (Main.dedServ) return;

            var effectAsset = ModContent.Request<Effect>("LimboFilterSystem/Effects/Filters/LimboFilter", AssetRequestMode.ImmediateLoad);

            var shader = new ScreenShaderData(effectAsset, "LimboFilterPass");
            Filters.Scene["LimboFilterSystem:LimboFilter"] = new Filter(shader, EffectPriority.VeryHigh);
            Filters.Scene["LimboFilterSystem:LimboFilter"].Load();
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (Main.dedServ) return;
            if (Main.LocalPlayer == null || Main.gameMenu) return;

            var filter = Filters.Scene["LimboFilterSystem:LimboFilter"];

            if (!filter.IsActive())
            {
                Filters.Scene.Activate("LimboFilterSystem:LimboFilter");
            }

            var effect = ((ScreenShaderData)filter.GetShader()).Shader;
            effect.Parameters["uContrast"]?.SetValue(1.6f);
            effect.Parameters["uVignetteRadius"]?.SetValue(0.75f);
            effect.Parameters["uVignetteSoftness"]?.SetValue(0.45f);
            effect.Parameters["uGrainAmount"]?.SetValue(0.08f);
            effect.Parameters["uTime"]?.SetValue((float)Main.timeForVisualEffects * 0.02f);
            effect.Parameters["uScreenResolution"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
        }
    }
}