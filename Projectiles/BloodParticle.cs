using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ImapoHorrorMod.Projectiles
{
    public class BloodParticle : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        public override string Texture => "ImapoHorrorMod/NPCs/ShadowStalker";

        public override void AI()
        {
            // Получаем ID Сталкера из ai[0]
            int stalkerIndex = (int)Projectile.ai[0];
            
            // Проверяем, жив ли Сталкер
            if (stalkerIndex >= 0 && stalkerIndex < Main.maxNPCs && Main.npc[stalkerIndex].active)
            {
                NPC stalker = Main.npc[stalkerIndex];
                Vector2 toStalker = stalker.Center - Projectile.Center;
                float distance = toStalker.Length();
                
                // Автонаведение: плавно поворачиваем частицу к Сталкеру
                if (distance > 10f)
                {
                    Vector2 targetVelocity = toStalker.SafeNormalize(Vector2.Zero) * 3f;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVelocity, 0.15f);
                }
                
                // Угасание при приближении к Сталкеру
                if (distance < 100f)
                {
                    float fadeFactor = distance / 100f;
                    Projectile.alpha = (int)(255 * (1f - fadeFactor * 0.5f));
                }
                
                // Уничтожение при достижении Сталкера
                if (distance < 20f)
                {
                    Projectile.Kill();
                    return;
                }
            }

            // Плавное появление в начале
            if (Projectile.alpha > 100)
            {
                Projectile.alpha -= 10;
                if (Projectile.alpha < 100) Projectile.alpha = 100;
            }

            // Затухание в конце жизни
            if (Projectile.timeLeft < 60)
            {
                Projectile.alpha += 4;
            }

            // Визуальный эффект: мелкие брызги
            Dust trail = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Smoke, 0, 0, 0, new Color(200, 200, 200), 0.5f);
            trail.scale = 0.3f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}