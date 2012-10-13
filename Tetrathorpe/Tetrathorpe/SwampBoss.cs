using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tetrathorpe
{
    class SwampBoss : Enemy
    {
        static bool texturesLoaded = false;
        public EntityPool<SwampFlower> flowerPool = new EntityPool<SwampFlower>(5, null);
        public List<SwampFlower> flowers = new List<SwampFlower>();
        public static int FLOWERS_MAX = 10;
        public static float FLOWER_SPAWN_INTERVAL = 1f; //secs
        public float timeSinceLastFlower = 0f;

        public static float BLEED_TIME = 0.25f; //secs
        public float bleedingTime = 0f;
        public bool bleeding = false;
        public static AnimatedTexture bleedingTexture;
        public static AnimatedTexture pulsatingTexture;
        public int shakeAnimationStep = 0;
        public Vector2 originalPosition;

        public SwampBoss()
            : base(-1)
        {
            species = "Swamp/boss";
            hitbox = new Hitbox(200, 200);
            hitbox.active = true;
            maxHealth = 2500;
            health = maxHealth;
            target = Game1.characterManager.getRandomLiveCharacter();
            currentState = EnemyState.Attacking;
            currentAI = EnemyAI.SwampBoss;
            oldAI = EnemyAI.SwampBoss;
            if (!texturesLoaded)
            {
                for (int i = 1; i <= 20; i++)
                {
                    TextureManager.Add(Enemy.BASETEX + "Swamp/boss back " + i);
                    TextureManager.Add(Enemy.BASETEX + "Swamp/boss side " + i);
                    TextureManager.Add(Enemy.BASETEX + "Swamp/boss front " + i);
                }
                TextureManager.Add(Enemy.BASETEX + "Swamp/boss center 1");
                TextureManager.Add(Enemy.BASETEX + "Swamp/boss center 2");
                TextureManager.Add(Enemy.BASETEX + "Swamp/boss center bleed");
                texturesLoaded = true;
            }
            bleedingTexture = new AnimatedTexture(Enemy.BASETEX + "Swamp/boss center bleed");
            pulsatingTexture = new AnimatedTexture(Enemy.BASETEX + species + " center", 2, 250);
            setTexture(pulsatingTexture);
        }

        public override void Update(GameTime gameTime)
        {
            float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            position = new Vector2(MathHelper.Clamp(position.X - moveSpeed * seconds, Game1.levelManager.position.X + Game1.levelManager.screenWidth - 175, float.PositiveInfinity), Game1.levelManager.position.Y - Game1.levelManager.screenHeight / 2);
            //bleeding animation
            if (bleeding)
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
                shakeAnimationStep = (shakeAnimationStep + 1) % 12;
                position = new Vector2(newX, newY);
                
                bleedingTime -= seconds;
                if (bleedingTime < 0f)
                {
                    setTexture(pulsatingTexture);
                    bleeding = false;
                    position = originalPosition;
                }
            }
            //spawn flowers
            timeSinceLastFlower += seconds;
            if (flowers.Count < FLOWERS_MAX && timeSinceLastFlower >= FLOWER_SPAWN_INTERVAL)
            {
                SwampFlower newFlower = flowerPool.obtain(delegate(SwampFlower f)
                {
                    Character target = Game1.characterManager.getRandomLiveCharacter();
                    switch (Game1.rand.Next(4))
                    {
                        case 0: f.position = target.position + new Vector2(0, -80); f.setDirection(Direction.Forward); break;
                        case 1: f.position = target.position + new Vector2(0, 80); f.setDirection(Direction.Back); break;
                        case 2: f.position = target.position + new Vector2(80, 0); f.setDirection(Direction.Left); break;
                        case 3: f.position = target.position + new Vector2(-80, 0); f.setDirection(Direction.Right); break;
                    }
                    f.hitbox.active = true;
                    f.health = f.maxHealth;
                    f.alreadyHit.Clear();
                    f.hitboxesHitBy.Clear();
                });
                flowers.Add(newFlower);
                Game1.levelManager.enemiesToAddImmediate.Add(newFlower);
                timeSinceLastFlower = 0f;
            }
            List<SwampFlower> deadflowers = new List<SwampFlower>();
            foreach (SwampFlower flower in flowers)
            {
                if (flower.alive)
                    flower.Update(gameTime);
                else
                    deadflowers.Add(flower);
            }
            foreach (SwampFlower flower in deadflowers)
            {
                flowers.Remove(flower);
                Game1.levelManager.killEnemy(flower);
            }
            base.Update(gameTime);
        }
        
        public override void hitBy(Character e, double damage, Vector2 knockbackOrigin, float knockbackStrength, float knockbackDuration)
        {
            base.hitBy(e, damage, Vector2.Zero, 0, 0);
            setTexture(bleedingTexture);
            bleedingTime = BLEED_TIME;
            bleeding = true;
            originalPosition = position;
            if (health <= 0)
            {
                foreach (SwampFlower f in flowers)
                    Game1.levelManager.enemiesToKill.Add(f);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (!alive) return;
        }
    }

    class SwampFlower : Enemy
    {
        public SpriteEffects flip = SpriteEffects.None;
        public List<Character> alreadyHit = new List<Character>();

        public SwampFlower()
            : base(-1)
        {
            scale = 0.75f;
            hitbox = new Hitbox(80, 80);
            maxHealth = 25;
            hitDamage = 10;
            currentAI = EnemyAI.NoMove;
        }

        public override SpriteEffects getFlip()
        {
            return flip;
        }

        public void setDirection(Direction dir)
        {
            curDirection = dir;
            AnimatedTexture tex = null;
            switch (dir)
            {
                case Direction.Back: tex = new AnimatedTexture(Enemy.BASETEX + "Swamp/boss back", 20, 150); break;
                case Direction.Forward: tex = new AnimatedTexture(Enemy.BASETEX + "Swamp/boss front", 20, 150); break;
                case Direction.Left: tex = new AnimatedTexture(Enemy.BASETEX + "Swamp/boss side", 20, 150); flip = SpriteEffects.FlipHorizontally; break;
                case Direction.Right: tex = new AnimatedTexture(Enemy.BASETEX + "Swamp/boss side", 20, 150); break;
            }
            tex.onFinish = kill;
            tex.setOnFrameRangeHitbox(16, 18, new Hitbox(new Vector2(30, 0), 50, 40, true));
            setTexture(tex);
        }

        public override void Update(GameTime gameTime)
        {
            //melee collision
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
            base.Update(gameTime);
        }
        
        public override void hitBy(Character e, double damage, Vector2 knockbackOrigin, float knockbackStrength, float knockbackDuration)
        {
            base.hitBy(e, damage, Vector2.Zero, 0, 0);
        }

        public void kill()
        {
            alive = false;
            hitbox.active = false;
        }
    }
}
