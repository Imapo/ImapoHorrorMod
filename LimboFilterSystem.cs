using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace ImapoHorrorMod
{
    public class LimboFilterSystem : ModSystem
    {
        public override void Load()
        {
            if (Main.dedServ) return;

            var combinedEffect = ModContent.Request<Effect>("ImapoHorrorMod/Effects/Filters/LimboRippleCombined", AssetRequestMode.ImmediateLoad);
            var combinedShader = new ScreenShaderData(combinedEffect, "LimboRipplePass");
            Filters.Scene["ImapoHorrorMod:LimboRipple"] = new Filter(combinedShader, EffectPriority.VeryHigh);
            Filters.Scene["ImapoHorrorMod:LimboRipple"].Load();
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (Main.dedServ) return;

            var filter = Filters.Scene["ImapoHorrorMod:LimboRipple"];
            if (filter != null && filter.IsActive())
            {
                var shader = filter.GetShader() as ScreenShaderData;
                if (shader != null && shader.Shader != null)
                {
                    shader.Shader.Parameters["uTime"]?.SetValue((float)gameTime.TotalGameTime.TotalSeconds);
                    shader.Shader.Parameters["uScreenResolution"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
                }
            }
        }
    }
}