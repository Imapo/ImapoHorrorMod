using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace ImapoHorrorMod.NPCs
{
    public class ShadowStalker : ModNPC
    {
        private bool hasSpawnedMessage = false;
        private float chaseDelay;
        private const float RIPPLE_DISTANCE = 300f;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;
        }

        public override void SetDefaults()
        {
            NPC.width = 30;
            NPC.height = 56;
            NPC.damage = 30;
            NPC.defense = 5;
            NPC.lifeMax = 150;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = 0f;
            NPC.knockBackResist = 0.3f;
            NPC.noGravity = false;
            NPC.aiStyle = -1;
            
            NPC.alpha = 0;
            NPC.color = Color.White;
            NPC.hide = false;
            NPC.timeLeft = 3600;
        }

        public override string Texture => "ImapoHorrorMod/NPCs/ShadowStalker";

        public override void AI()
        {
            if (NPC.timeLeft < 300) NPC.timeLeft = 300;

            if (!hasSpawnedMessage)
            {
                Main.NewText("Ты чувствуешь чьё-то присутствие...", Color.DarkRed);
                hasSpawnedMessage = true;
                chaseDelay = 120f;
            }

            Texture2D currentTexture = ModContent.Request<Texture2D>(Texture).Value;

            // Анимация
            NPC.frameCounter++;
            if (NPC.frameCounter >= 10)
            {
                NPC.frameCounter = 0;
                int frameHeight = currentTexture.Height / Main.npcFrameCount[NPC.type];
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y >= currentTexture.Height) NPC.frame.Y = 0;
            }

            // Поиск игрока
            Player target = null;
            float shortestDistance = 1000f;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player != null && player.active && !player.dead && !player.ghost)
                {
                    float distance = Vector2.Distance(NPC.Center, player.Center);
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        target = player;
                    }
                }
            }

            // Безопасное управление эффектом ряби
            ManageRippleEffect(shortestDistance);

            // ==========================================================
            // ДОБАВЛЕНО: Вампиризм и частицы крови (как у Иссушителя жизни)
            // ==========================================================
            if (target != null && shortestDistance < 300f)
            {
                // Таймер создания частиц: 1 частица каждые 10 тиков = 6 частиц в секунду
                NPC.ai[2]++;
                if (NPC.ai[2] >= 10)
                {
                    NPC.ai[2] = 0;
                    
                    // Создаём умную частицу-снаряд
                    Vector2 initialVelocity = (NPC.Center - target.Center).SafeNormalize(Vector2.Zero) * 2.5f;
                    initialVelocity.X += Main.rand.NextFloat(-0.3f, 0.3f);
                    initialVelocity.Y += Main.rand.NextFloat(-0.3f, 0.3f);
                    
                    Projectile.NewProjectile(
                        Projectile.GetSource_None(),
                        target.Center,
                        initialVelocity,
                        ModContent.ProjectileType<Projectiles.BloodParticle>(),
                        0,
                        0f,
                        Main.myPlayer,
                        ai0: NPC.whoAmI // Передаём ID Сталкера для автонаведения
                    );
                }

                // Механика урона: раз в секунду
                NPC.ai[1]++;
                if (NPC.ai[1] >= 60)
                {
                    NPC.ai[1] = 0;
                    int drainAmount = 5;
                    target.statLife -= drainAmount;
                    CombatText.NewText(target.Hitbox, Color.Crimson, drainAmount, true);
                    
                    if (target.statLife <= 0)
                    {
                        target.KillMe(Terraria.DataStructures.PlayerDeathReason.ByCustomReason($"{target.name} был полностью высасан тенями"), 9999, 0);
                    }
                }
            }
            // ==========================================================

            if (target == null)
            {
                NPC.velocity.X *= 0.9f;
                NPC.velocity.Y += 0.5f;
                return;
            }

            // Поворот к игроку
            if (target.Center.X > NPC.Center.X)
                NPC.direction = 1;
            else
                NPC.direction = -1;

            bool playerIsLooking = IsPlayerLookingAtNPC(target);

            if (playerIsLooking)
            {
                NPC.velocity *= 0.3f;
                chaseDelay = 60f;
                
                if (NPC.ai[0] > 240)
                {
                    TeleportBehindPlayer(target);
                    NPC.ai[0] = 0;
                }
                NPC.ai[0]++;
            }
            else
            {
                if (chaseDelay > 0)
                {
                    chaseDelay--;
                    NPC.velocity *= 0.5f;
                }
                else
                {
                    NPC.ai[0] = 0;
                    Vector2 direction = target.Center - NPC.Center;
                    direction.Normalize();
                    
                    float speed = 2.5f;
                    NPC.velocity = direction * speed;
                }
            }

            // Гравитация
            NPC.velocity.Y += 0.5f;
            NPC.velocity.Y *= 0.9f;
        }

        private void ManageRippleEffect(float distanceToPlayer)
        {
            var filter = Filters.Scene["ImapoHorrorMod:LimboRipple"];
            if (filter == null) return;

            var shader = filter.GetShader() as ScreenShaderData;
            if (shader == null || shader.Shader == null) return;

            if (distanceToPlayer < RIPPLE_DISTANCE)
            {
                float intensity = 1f - (distanceToPlayer / RIPPLE_DISTANCE);
                shader.Shader.Parameters["uRippleIntensity"]?.SetValue(intensity * 0.5f);
                shader.Shader.Parameters["uRippleCenter"]?.SetValue(NPC.Center);
            }
            else
            {
                shader.Shader.Parameters["uRippleIntensity"]?.SetValue(0f);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D currentTexture = ModContent.Request<Texture2D>(Texture).Value;

            int frameHeight = currentTexture.Height / Main.npcFrameCount[NPC.type];
            Rectangle sourceRect = new Rectangle(0, NPC.frame.Y, currentTexture.Width, frameHeight);
            SpriteEffects effects = NPC.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            
            spriteBatch.Draw(currentTexture, NPC.Center - screenPos, sourceRect, drawColor, NPC.rotation, 
                new Vector2(currentTexture.Width / 2f, frameHeight / 2f), NPC.scale, effects, 0f);

            return false;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (Main.dayTime) return 0f;
            return 0.005f;
        }

        private bool IsPlayerLookingAtNPC(Player player)
        {
            Vector2 toNPC = NPC.Center - player.Center;
            float angleToNPC = (float)Math.Atan2(toNPC.Y, toNPC.X);
            float playerLookAngle = player.direction == 1 ? 0 : (float)Math.PI;
            float angleDiff = Math.Abs(angleToNPC - playerLookAngle);
            if (angleDiff > Math.PI) angleDiff = 2 * (float)Math.PI - angleDiff;
            return angleDiff < Math.PI / 2;
        }

        private void TeleportBehindPlayer(Player player)
        {
            float distance = 250f;
            Vector2 behindPlayer = player.Center + new Vector2(-player.direction * distance, -50);
            NPC.position = behindPlayer;
            NPC.velocity = Vector2.Zero;
            
            for (int i = 0; i < 15; i++)
            {
                int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Shadowflame);
                Main.dust[dust].velocity *= 2f;
                Main.dust[dust].noGravity = true;
                Main.dust[dust].color = new Color(100, 0, 0);
            }
            Main.NewText("Оно переместилось...", Color.DarkRed);
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 30; i++)
                {
                    int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Smoke);
                    Main.dust[dust].velocity *= 3f;
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].color = Color.Black;
                }
            }
        }
    }
}