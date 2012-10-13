using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tetrathorpe
{
    class AOE : Character
    {
        public static string BASETEX = "Characters/AOE/";
        public static string cName = "AOE";
        public static Hitbox CHARACTER_HITBOX = new Hitbox(65, 100);

        static bool texturesLoaded;

        List<BalloonBomb> balloonBombs = new List<BalloonBomb>();
        EntityPool<Entity> storedBalloonPool = new EntityPool<Entity>(5, delegate(Entity e) { e.setTexture(BASETEX + "aoe storedballoon"); });
        List<Entity> storedBalloons = new List<Entity>();
        Vector2[] storedBalloonOffsets = { new Vector2(0, -85), new Vector2(-30, -85), new Vector2(30, -85), new Vector2(-15, -115), new Vector2(15, -115) };
        public static int STORED_BALLOON_MAX = 5;
        public static float STORED_BALLOON_RECHARGE = 1.5f;
        public float currentBalloonCharge = 0f;
        public static int BALLOON_BOMBS_MAX = 10;
        public static int BALLOON_BOMBS_SPEED = 300;
        public static int BALLOON_BOMBS_ACCELERATION = -300;
        public static float AREA_PUSH_RANGE = 150f;
        public static float AREA_PUSH_FORCE = 100f;

        float floatBob = 0f;
        int floatBobDir = 1;

        float whirlwindScale = 0f;
        float allWhirlwindScale = 0f;
        int numWhirlWinds = 0;
        bool whirlWinding = false;
        float allWhirlwindRot = 0f;

        int whirlwindCDTime = 0;
        const int whirlCD = 5000;

        public AOE()
            : base(CHARACTER_HITBOX)
        {
            maxHealth = 75;
            health = 75;
            hitbox.active = true;
            characterTextures.Add(CharacterState.Standing, new AnimatedTexture(BASETEX + "aoe stand"));
            characterTextures.Add(CharacterState.Walking, new AnimatedTexture(BASETEX + "aoe walk", 1, 100));
            characterTextures.Add(CharacterState.AttackingLight, new AnimatedTexture(BASETEX + "aoe light", 2, 100));
            characterTextures.Add(CharacterState.AttackingHeavy, new AnimatedTexture(BASETEX + "aoe heavy", 1, 200));
            characterTextures.Add(CharacterState.AttackingSpecial, new AnimatedTexture(BASETEX + "aoe special", 1, 400));
            characterTextures.Add(CharacterState.Blocking, new AnimatedTexture(BASETEX + "aoe block", 1, 100));

            Action stand = delegate()
            {
                setCharacterState(CharacterState.Standing);
            };
            characterTextures[CharacterState.AttackingLight].onStart = delegate() { swingSword(); };
            characterTextures[CharacterState.AttackingLight].onFinish = stand;
            characterTextures[CharacterState.AttackingLight].setOnFrameRangeHitbox(1, 2, new Hitbox(new Vector2(30, -10), 60, 45, true));
            characterTextures[CharacterState.AttackingHeavy].onStart = delegate() { throwBalloon(); };
            characterTextures[CharacterState.AttackingHeavy].onFinish = stand;
            characterTextures[CharacterState.AttackingSpecial].onStart = delegate() { releaseAllBalloons(); };
            characterTextures[CharacterState.AttackingSpecial].onFinish = stand;
            characterTextures[CharacterState.Blocking].onStart = delegate() { areaPush(); };
            characterTextures[CharacterState.Blocking].onFinish = stand;

            TextureManager.Add(BASETEX + cName);
            hudTexture = TextureManager.Get(BASETEX + cName);
            TextureManager.Add(BASETEX + cName + " dead");
            deadTexture = TextureManager.Get(BASETEX + cName + " dead");

            TextureManager.Add(BASETEX + "aoe-whirlwind");

            Initialize(this);
        }

        public void Initialize(Character c)
        {
            if (!texturesLoaded)
            {
                foreach (AnimatedTexture tex in c.characterTextures.Values)
                {
                    tex.LoadTextures();
                }
                TextureManager.Add(BASETEX + "aoe storedballoon");
                TextureManager.Add(BASETEX + "aoe balloonbomb");
                TextureManager.Add(BASETEX + "aoe balloonline");
                TextureManager.Add(BASETEX + "aoe-balloon-explode 1");
                TextureManager.Add(BASETEX + "aoe-balloon-explode 2");
                TextureManager.Add(BASETEX + "aoe-balloon-explode 3");
                TextureManager.Add(BASETEX + "aoe-balloon-explode 4");
                texturesLoaded = true;
            }
            for (int i = 0; i < STORED_BALLOON_MAX; i++)
                storedBalloons.Add(storedBalloonPool.obtain(null));
        }

        public override void setCharacterState(CharacterState state)
        {
            base.setCharacterState(state);
        }

        public void areaPush()
        {
            if (whirlwindCDTime < whirlCD) return;

            whirlwindCDTime = 0;
            float RANGE_STEP = 10f;
            foreach (Enemy e in Game1.levelManager.enemies)
            {
                for (float range = 0; range < AREA_PUSH_RANGE; range += RANGE_STEP)
                {
                    Vector2 distance = e.position - position;
                    distance.Normalize();
                    Vector2 p = position + range * distance;
                    if (e.hitbox.intersects(new Point((int)p.X, (int)p.Y)) && e.disableMovementTime <= 0f)
                    {
                        e.disableMovementDistance = Math.Max(0, AREA_PUSH_RANGE - distance.Length() - 30f);
                        e.velocity += distance * AREA_PUSH_FORCE;
                    }
                }
            }
            whirlWinding = true;
            allWhirlwindScale = 0f;
        }

        public void releaseAllBalloons()
        {
            while (balloonBombs.Count < BALLOON_BOMBS_MAX && storedBalloons.Count > 0)
            {
                balloonBombs.Add(new BalloonBomb(this).applyRandomPosition());
                balloonBombs.Add(new BalloonBomb(this).applyRandomPosition());
                currentBalloonCharge = 0;
                storedBalloons.RemoveAt(storedBalloons.Count - 1);
            }
        }

        public void throwBalloon()
        {
            if (balloonBombs.Count < BALLOON_BOMBS_MAX && storedBalloons.Count > 0)
            {
                balloonBombs.Add(new BalloonBomb(this));
                currentBalloonCharge = 0;
                storedBalloons.RemoveAt(storedBalloons.Count - 1);
            }
        }
        public void swingSword()
        {
            // Does not yet actually do damage or push or anything! Just detonates the balloons.
            foreach (BalloonBomb bomb in balloonBombs)
            {
                bomb.fuse = BalloonBomb.FUSE_SECONDS;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (!alive)
                return;

            whirlwindCDTime += gameTime.ElapsedGameTime.Milliseconds;
 

            float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            hitbox.active = true;
            List<BalloonBomb> deadbombs = new List<BalloonBomb>();
            //position bombs
            foreach (BalloonBomb bomb in balloonBombs)
            {
                bomb.Update(gameTime);
                if (!bomb.alive)
                    deadbombs.Add(bomb);
            }
            //remove bombs
            foreach (BalloonBomb bomb in deadbombs)
                balloonBombs.Remove(bomb);

            //recharge balloons
            if (storedBalloons.Count < STORED_BALLOON_MAX)
            {
                currentBalloonCharge += seconds;
                if (currentBalloonCharge > STORED_BALLOON_RECHARGE)
                {
                    storedBalloons.Add(storedBalloonPool.obtain(null));
                    currentBalloonCharge = 0;
                }
            }
            //position balloons
            for (int i = 0; i < storedBalloons.Count; i++)
            {
                storedBalloons[i].position = position + storedBalloonOffsets[i];
                storedBalloons[i].Update(gameTime);
            }

            //melee collision
            foreach (Enemy e in Game1.levelManager.enemies)
            {
                if (characterTextures[curState].getHitbox() != null && characterTextures[curState].getHitbox().intersects(e.hitbox))
                {
                    if (!e.hitboxesHitBy.Contains(characterTextures[curState].getHitbox()))
                    {
                        Console.WriteLine("hit=" + e.hitboxesHitBy);
                        e.hitboxesHitBy.Add(characterTextures[curState].getHitbox());
                        e.hitBy(this, 10);
                    }
                }
            }

            floatBob += floatBobDir * .02f * gameTime.ElapsedGameTime.Milliseconds;
            if (floatBob > 20f) { floatBobDir = -1; floatBob = 20f; }
            if (floatBob < 0f) { floatBobDir = 1; floatBob = 0f; }

            if (whirlWinding)
            {
                whirlwindScale += gameTime.ElapsedGameTime.Milliseconds * .05f;
                if (whirlwindScale > 1f)
                {
                    numWhirlWinds++;
                    whirlwindScale = 0f;
                }
                allWhirlwindRot += gameTime.ElapsedGameTime.Milliseconds * .01f;
                allWhirlwindRot = MathHelper.WrapAngle(allWhirlwindRot);
                if (numWhirlWinds > 10)
                {
                    allWhirlwindScale -= gameTime.ElapsedGameTime.Milliseconds * .005f;
                    if (allWhirlwindScale <= 0f)
                    {
                        whirlWinding = false;
                        numWhirlWinds = 0;
                        whirlwindScale = 0;
                    }
                }
                else if (allWhirlwindScale < 1f) allWhirlwindScale += gameTime.ElapsedGameTime.Milliseconds * .005f;
            }

            base.Update(gameTime);
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            Vector2 actualPosition = position;
            if (alive)
            {
                position.Y += floatBob;
            }
            base.Draw(spriteBatch);
            if (!alive) return;

            foreach (BalloonBomb bomb in balloonBombs)
                bomb.Draw(spriteBatch);
            foreach (Entity balloon in storedBalloons)
            {
                float balloonBottom = balloon.position.Y + (balloon.getTexture().Height / 4); // stuff to calculate where to draw balloon line
                float width = Math.Abs(position.X - balloon.position.X);
                if (width < 5f) width = 5f;
                SpriteEffects effects = ((position.X - balloon.position.X) < 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                int xPos = ((position.X - balloon.position.X) < 0) ? (int)position.X : (int)balloon.position.X;

                Rectangle lineRectangle = new Rectangle(xPos, (int)balloonBottom, (int)width, (int)(getTop().Y - balloon.position.Y + (balloon.getTexture().Height / 4)));
                //spriteBatch.Draw(TextureManager.Get(BASETEX + "aoe balloonline"), lineRectangle, null, Color.White, 0f, Vector2.Zero, effects, 0);
                balloon.position.Y += floatBob;
                balloon.Draw(spriteBatch);
                balloon.position.Y -= floatBob;
            }
            int radius = (int)AREA_PUSH_RANGE;
            int outerRadius = radius * 2 + 2; // So circle doesn't go out of bounds
            Texture2D areaPushCircle = new Texture2D(Game1.graphics.GraphicsDevice, outerRadius, outerRadius);

            Color[] data = new Color[outerRadius * outerRadius];

            for (int i = 0; i < data.Length; i++)
                data[i] = Color.Transparent;

            double angleStep = 1f / radius;

            for (double angle = 0; angle < Math.PI * 2; angle += angleStep)
            {
                int x = (int)Math.Round(radius + radius * Math.Cos(angle));
                int y = (int)Math.Round(radius + radius * Math.Sin(angle));

                data[y * outerRadius + x + 1] = Color.White;
            }
            areaPushCircle.SetData(data);
            //spriteBatch.Draw(areaPushCircle, position, null, Color.Gold, 0, new Vector2(radius, radius), 1, SpriteEffects.None, 0);

            position = actualPosition;

            if (whirlWinding)
            {
                for (int i = 0; i < numWhirlWinds - 1; i++)
                {                    
                    float rot =  i * (float)Math.PI / 5f;
                    Utility.Draw(spriteBatch, BASETEX + "aoe-whirlwind", position + new Vector2((float)Math.Cos(rot) * (20f + 5f * rot), (float)Math.Sin(rot) * (20f + 5f * rot)), Color.White, allWhirlwindScale, allWhirlwindScale * .5f, rot * allWhirlwindRot, SpriteEffects.None);
                }
                float rot2 = (numWhirlWinds - 1) * (float)Math.PI / 5f;
                Utility.Draw(spriteBatch, BASETEX + "aoe-whirlwind", position + new Vector2((float)Math.Cos(rot2) * (20f + 5f * rot2), (float)Math.Sin(rot2) * (20f + 5f * rot2)), Color.White, allWhirlwindScale * whirlwindScale, allWhirlwindScale * whirlwindScale * .5f, rot2 * allWhirlwindRot, SpriteEffects.None);
            }
        }
    }
}