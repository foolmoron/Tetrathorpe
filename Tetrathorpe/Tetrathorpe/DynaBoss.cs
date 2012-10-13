using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tetrathorpe
{
    class DynaBoss : Enemy
    {
        static bool texturesLoaded = false;
        public EntityPool<Dynamite> dynamitePool = new EntityPool<Dynamite>(5, null);
        public List<Dynamite> dynamite = new List<Dynamite>();
        public static float DYNAMITE_THROW_INTERVAL = 3f; //secs
        public float timeSinceLastThrow = 0f;

        public static float STUN_TIME = 5f; //secs
        public float stunTime = 0f;
        public bool stunned = false;
        public static AnimatedTexture stunnedTexture;
        public static AnimatedTexture movingTexture;
        public int shakeAnimationStep = 0;
        public Vector2 originalPosition;
        public int dirMultiplier = 1;
        public bool exploding = false;
        public static float EXPLODING_TIME = 5f;
        public float explodingTime = 0f;
        public List<AnimatedTexture> explosions = new List<AnimatedTexture>();
        public List<AnimatedTexture> finishedExplosions = new List<AnimatedTexture>();
        public Vector2[] explosionOffsets = { new Vector2(30, 30), new Vector2(0, 20), new Vector2(-100, 50), new Vector2(120, -40), new Vector2(-50, 0), new Vector2(0, 0) };
        public bool[] drawExplosion = { false, false, false, false, false, false};
        public int exploded = 0;

        public DynaBoss()
            : base(-1)
        {
            if (!texturesLoaded)
            {
                TextureManager.Add(Enemy.BASETEX + "Cave/cave boss attack 1");
                TextureManager.Add(Enemy.BASETEX + "Cave/cave boss attack 2");
                TextureManager.Add(Enemy.BASETEX + "Cave/cave boss attack 3");
                TextureManager.Add(Enemy.BASETEX + "Cave/cave boss hurt");
                TextureManager.Add(Enemy.BASETEX + "Cave/cave boss walk 1");
                TextureManager.Add(Enemy.BASETEX + "Cave/cave boss walk 2");
                TextureManager.Add(Enemy.BASETEX + "Cave/cave boss walk 3");
                TextureManager.Add(Enemy.BASETEX + "Cave/cave boss walk 4");
                TextureManager.Add(Enemy.BASETEX + "Cave/dynamite");
                TextureManager.Add(Enemy.BASETEX + "Cave/explosion small 1");
                TextureManager.Add(Enemy.BASETEX + "Cave/explosion small 2");
                TextureManager.Add(Enemy.BASETEX + "Cave/explosion small 3");
                TextureManager.Add(Enemy.BASETEX + "Cave/explosion big 1");
                TextureManager.Add(Enemy.BASETEX + "Cave/explosion big 2");
                TextureManager.Add(Enemy.BASETEX + "Cave/explosion big 3");
                TextureManager.Add(Enemy.BASETEX + "Cave/fuse 1");
                TextureManager.Add(Enemy.BASETEX + "Cave/fuse 2");
                TextureManager.Add(Enemy.BASETEX + "Cave/fuse 3");
                texturesLoaded = true;
            }
            scale = 2f;
            hitbox = new Hitbox(360, 125);
            hitbox.active = true;
            moveSpeed = 100f;
            maxHealth = 2500f;
            health = maxHealth;
            movingTexture = new AnimatedTexture(Enemy.BASETEX + "Cave/cave boss walk", 4, 100);
            stunnedTexture = new AnimatedTexture(Enemy.BASETEX + "Cave/cave boss walk 2");
            setTexture(movingTexture);
            position.X = Game1.levelManager.position.X + Game1.levelManager.screenWidth * 2;
            currentAI = EnemyAI.DynamiteBoss;
            oldAI = EnemyAI.DynamiteBoss;
        }

        public override void Update(GameTime gameTime)
        {
            float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            //death exploding animation
            if (exploding)
            {
                setTexture(new AnimatedTexture(Enemy.BASETEX + "Cave/cave boss walk 2"));
                explodingTime += seconds;
                if (explosions.Count == 0 && exploded == 0)
                {
                    AnimatedTexture deathTexture = new AnimatedTexture(Enemy.BASETEX + "Cave/explosion big", 3, 250 + (int)(Game1.rand.NextDouble() * 100));
                    deathTexture.onFinish = delegate() { drawExplosion[0] = false;  };
                    drawExplosion[0] = true;
                    explosions.Add(deathTexture);
                    exploded++;
                }                
                if (explodingTime >= EXPLODING_TIME / 4 && exploded == 1)
                {
                    AnimatedTexture deathTexture = new AnimatedTexture(Enemy.BASETEX + "Cave/explosion big", 3, 250 + (int)(Game1.rand.NextDouble() * 100));
                    deathTexture.onFinish = delegate() { drawExplosion[1] = false; };
                    drawExplosion[1] = true;
                    explosions.Add(deathTexture);
                    exploded++;
                }                
                if (explodingTime >= EXPLODING_TIME / 3 && exploded == 2)
                {
                    AnimatedTexture deathTexture = new AnimatedTexture(Enemy.BASETEX + "Cave/explosion big", 3, 250 + (int)(Game1.rand.NextDouble() * 100));
                    deathTexture.onFinish = delegate() { drawExplosion[2] = false; };
                    drawExplosion[2] = true;
                    explosions.Add(deathTexture);
                    exploded++;
                }                
                if (explodingTime >= EXPLODING_TIME / 2 && exploded == 3)
                {
                    AnimatedTexture deathTexture = new AnimatedTexture(Enemy.BASETEX + "Cave/explosion big", 3, 250 + (int)(Game1.rand.NextDouble() * 100));
                    deathTexture.onFinish = delegate() { drawExplosion[3] = false; };
                    drawExplosion[3] = true;
                    explosions.Add(deathTexture);
                    exploded++;
                }                
                if (explodingTime >= EXPLODING_TIME / 1.5 && exploded == 4)
                {
                    AnimatedTexture deathTexture = new AnimatedTexture(Enemy.BASETEX + "Cave/explosion big", 3, 250 + (int)(Game1.rand.NextDouble() * 100));
                    deathTexture.onFinish = delegate() { drawExplosion[4] = false; };
                    drawExplosion[4] = true;
                    explosions.Add(deathTexture);
                    exploded++;
                }                
                if (explodingTime >= EXPLODING_TIME && exploded == 5)
                {
                    AnimatedTexture deathTexture = new AnimatedTexture(Enemy.BASETEX + "Cave/explosion big", 3, 250 + (int)(Game1.rand.NextDouble() * 100));
                    deathTexture.onFinish = delegate() { Game1.levelManager.killEnemy(this); };
                    drawExplosion[5] = true;
                    explosions.Add(deathTexture);
                    exploded++;
                }                
            }
            //stunned animation
            else if (stunned)
            {
                //shake animation
                float newX = originalPosition.X;
                float newY = originalPosition.Y;
                switch (shakeAnimationStep)
                {
                    case 0: newX += 5; newY += 5; break;
                    case 2: newX += 0; newY -= 5; break;
                    case 4: newX += 5; newY -= 5; break;
                    case 6: newX -= 5; newY -= 5; break;
                    case 8: newX -= 10; newY -= 5; break;
                    case 10: newX += 3; newY -= 2; break;
                }
                shakeAnimationStep = (shakeAnimationStep + 1);
                position = new Vector2(newX, newY);

                stunTime -= seconds;
                if (stunTime < 0f)
                {
                    setTexture(movingTexture);
                    stunned = false;
                    shakeAnimationStep = 0;
                    position = originalPosition;
                }
            }
            else
            {
                if (position.X < (Game1.levelManager.position.X + 150))
                    dirMultiplier = 1;
                else if ((position.X > (Game1.levelManager.position.X + Game1.levelManager.screenWidth - 150)))
                    dirMultiplier = -1;
                position = new Vector2(position.X + (moveSpeed * dirMultiplier * seconds), Game1.levelManager.position.Y - Game1.levelManager.screenHeight + 100);
                //spawn dynamite
                timeSinceLastThrow += seconds;
                if (timeSinceLastThrow >= DYNAMITE_THROW_INTERVAL)
                {
                    Action<Dynamite> dynamiteReset = delegate(Dynamite d)
                    {
                        d.hitbox = new Hitbox(35, 24);
                        d.hitbox.active = true;
                        d.health = d.maxHealth;
                        d.alreadyHit.Clear();
                        d.hitboxesHitBy.Clear();
                        d.fuse = 0f;
                        d.exploding = false;
                        d.position = position;
                        d.velocity.X = (float)(Game1.rand.NextDouble() - 0.5) * Game1.levelManager.screenWidth;
                        d.velocity.Y = ((float)Game1.rand.NextDouble() + 2) / 3 * Game1.levelManager.screenHeight / 3;
                        d.setTexture(new AnimatedTexture(Enemy.BASETEX + "Cave/dynamite"));
                    };
                    Dynamite d1 = dynamitePool.obtain(dynamiteReset);
                    Dynamite d2 = dynamitePool.obtain(dynamiteReset);
                    Dynamite d3 = dynamitePool.obtain(dynamiteReset);
                    dynamite.Add(d1);
                    dynamite.Add(d2);
                    dynamite.Add(d3);
                    Game1.levelManager.enemiesToAddImmediate.Add(d1);
                    Game1.levelManager.enemiesToAddImmediate.Add(d2);
                    Game1.levelManager.enemiesToAddImmediate.Add(d3);
                    timeSinceLastThrow = 0f;
                }
            }
            List<Dynamite> deaddynamite = new List<Dynamite>();
            foreach (Dynamite d in dynamite)
            {
                if (d.alive)
                    d.Update(gameTime);
                else
                    deaddynamite.Add(d);
            }
            foreach (Dynamite d in deaddynamite)
            {
                dynamite.Remove(d);
                Game1.levelManager.killEnemy(d);
            }
            foreach (AnimatedTexture explosion in explosions)
                explosion.Update(gameTime, this);
            Console.WriteLine("x=" + exploded + "ex=" + explosions.Count + " fex=" + finishedExplosions.Count);
            foreach (AnimatedTexture explosion in finishedExplosions)
                explosions.Remove(explosion);
            finishedExplosions.Clear();
            base.Update(gameTime);
        }

        public void stunHit(double damage)
        {
            health -= damage;
            stunTime = STUN_TIME;
            stunned = true;
            shakeAnimationStep = 0;
            setTexture(stunnedTexture);
            originalPosition = position;
            if (health <= 0f)
            {
                foreach (Dynamite d in dynamite)
                    Game1.levelManager.enemiesToKill.Add(d);
                exploding = true;
            }
        }

        public override void hitBy(Character e, double damage, Vector2 knockbackOrigin, float knockbackStrength, float knockbackDuration)
        {
            if (stunned)
            {
                health -= damage;
                shakeAnimationStep = 0;
                if (health <= 0f)
                {
                    foreach (Dynamite d in dynamite)
                        Game1.levelManager.enemiesToKill.Add(d);
                    exploding = true;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            for(int i = 0; i < explosions.Count; i++)
            {
                if (drawExplosion[i])
                    Utility.Draw(spriteBatch, explosions[i].getTexture(), position + explosionOffsets[i], Color.White, 0.5f, 1, 0, SpriteEffects.None);
            }
        }
    }

    class Dynamite : Enemy
    {
        public List<Entity> alreadyHit = new List<Entity>();
        public static float DYNAMITE_FUSE_LENGTH = 10f; //secs
        public float fuse = 0f;
        public AnimatedTexture idleTexture;
        public AnimatedTexture explodingTexture;
        public AnimatedTexture fuseTexture;
        public bool exploding = false;

        public Dynamite()
            : base(-1)
        {
            idleTexture = new AnimatedTexture(Enemy.BASETEX + "Cave/dynamite");
            explodingTexture = new AnimatedTexture(Enemy.BASETEX + "Cave/explosion small", 3, 200);
            fuseTexture = new AnimatedTexture(Enemy.BASETEX + "Cave/fuse", 3, 50);
            explodingTexture.setOnFrameHitbox(1, new Hitbox(20, 20));
            explodingTexture.setOnFrameHitbox(2, new Hitbox(30, 30));
            explodingTexture.setOnFrameHitbox(3, new Hitbox(40, 40));
            explodingTexture.onFinish = kill;
            hitbox = new Hitbox(35, 24);
            hitbox.active = true;
            maxHealth = 1;
            hitDamage = 20;
            currentAI = EnemyAI.NoMoveKnockback;
            setTexture(idleTexture);
        }

        public override void Update(GameTime gameTime)
        {            
            float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            fuse += seconds;
            if (fuse > DYNAMITE_FUSE_LENGTH)
            {
                setTexture(explodingTexture);
                exploding = true;
            }
                if (velocity.LengthSquared() > 0)
                    velocity -= velocity * 0.95f * seconds;
                float nextX = position.X + (velocity.X * seconds);
                float nextY = position.Y + (velocity.Y * seconds);

                if (!canMove(new Vector2(velocity.X * seconds, 0)))
                    nextX = position.X;
                if (!canMove(new Vector2(0, velocity.Y * seconds)))
                    nextY = position.Y;
                position = new Vector2(nextX, nextY);
            
            //blast collision
            foreach (Character c in Game1.characterManager.liveCharacters)
            {
                if (texture.getHitbox() != null && texture.getHitbox().intersects(c.hitbox))
                {
                    if (!alreadyHit.Contains(c))
                    {
                        alreadyHit.Add(c);
                        c.hitBy(this);
                    }
                }
            }
            //stunhits cave boss allowing for damage
            foreach (Enemy e in Game1.levelManager.enemies)
            {
                if (e is DynaBoss)
                {
                    if (texture.getHitbox() != null && texture.getHitbox().intersects(e.hitbox))
                    {
                        if (!alreadyHit.Contains(e))
                        {
                            alreadyHit.Add(e);
                            ((DynaBoss)e).stunHit(hitDamage);
                        }
                    }
                }
            }
            fuseTexture.Update(gameTime, this);
            base.Update(gameTime);
        }

        public override void hitBy(Character e, double damage, Vector2 knockbackOrigin, float knockbackStrength, float knockbackDuration)
        {
            base.hitBy(e, 0, e.position, 20f, 2f);
        }

        public void kill()
        {
            alive = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (!exploding)
                Utility.Draw(spriteBatch, fuseTexture.getTexture(), new Vector2(position.X - 10, position.Y), new Color(255, 255 - (int)(fuse / DYNAMITE_FUSE_LENGTH * 255), 0), 1, 0.6f, 0, SpriteEffects.None);
        }
    }
}
