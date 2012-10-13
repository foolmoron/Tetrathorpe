using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Tetrathorpe
{
    class BalloonBomb : Entity
    {
        public static double damage = 20f;
        public static float knockbackStrength = 50f;
        public static float knockbackDuration = 2f;
        public static float FUSE_SECONDS = 3f;
        public static float EXPLOSION_TIME = .5f;
        public static float EXPLOSION_RADIUS = 100f;
        public static float DROP_HEIGHT = 200f;
        public static float FALL_SPEED_INITIAL = 350f;
        public static float FALL_SPEED_MINIMUM = 20f;
        public static float FALL_SPEED_DECAY_PER_SECOND = 0.95f;
        public float fallSpeed = FALL_SPEED_INITIAL;
        public float height = 0f;
        public float radius
        {
            get { return rad - getTexture().Width; }
            set
            {
                rad = value + getTexture().Width;
            }
        }
        private float rad;
        public float fuse = 0f;

        public float explosionTime = 0f;
        public bool exploding = false;

        public Character parent;

        AnimatedTexture explosion = null;

        public BalloonBomb(AOE p, bool thrown=true)
            : base()
        {
            parent = p;
            if (!thrown)
            {
                setTexture(new AnimatedTexture(AOE.BASETEX + "aoe storedballoon"));
                radius = 0f;
            }
            else
            {
                setTexture(new AnimatedTexture(AOE.BASETEX + "aoe balloonbomb"));
                radius = 0f;
                switch (parent.curDirection)
                {
                    case Character.Direction.Left: position = parent.getLeft(); break;
                    case Character.Direction.Right: position = parent.getRight(); break;
                    case Character.Direction.Forward: position = parent.getBottom(); break;
                    case Character.Direction.Back: position = parent.getTop(); break;
                }
                velocity = AOE.BALLOON_BOMBS_SPEED * parent.fourWayDirection();
                acceleration = AOE.BALLOON_BOMBS_ACCELERATION * parent.fourWayDirection();
            }

            explosion = new AnimatedTexture(AOE.BASETEX + "aoe-balloon-explode", 4, 100);
        }

        public BalloonBomb applyRandomPosition()
        {
            velocity = acceleration = Vector2.Zero;
            height = DROP_HEIGHT + ((float)Game1.rand.NextDouble() * (DROP_HEIGHT / 1.5f));
            position.X = Game1.levelManager.position.X + ((float)Game1.rand.NextDouble() * LevelManager.SCREENWIDTH_MIN * 0.8f);
            position.Y = ((float)Game1.rand.NextDouble()-0.5f) * LevelManager.SCREENHEIGHT_MIN * 0.8f;
            return this;
        }

        public override void Update(GameTime gameTime)
        {
            if (!alive)
                return;

            float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            velocity += acceleration * seconds;
            position += velocity * seconds;
            if (fallSpeed > FALL_SPEED_MINIMUM)
                fallSpeed -= fallSpeed * FALL_SPEED_DECAY_PER_SECOND * seconds;
            if (height > 0f)
                height -= fallSpeed * seconds;
            else height = 0f;

            if (Math.Abs(velocity.X) < 0.001f && Math.Abs(velocity.Y) < 0.001f)
            {
                acceleration = Vector2.Zero;
                velocity = Vector2.Zero;
            }

            if (exploding)
            {
                explosion.Update(gameTime, this);
                hitbox.active = true;
                explosionTime += seconds;
                float timeRatio = explosionTime / BalloonBomb.EXPLOSION_TIME;
                radius = BalloonBomb.EXPLOSION_RADIUS * timeRatio;
                if (explosionTime > BalloonBomb.EXPLOSION_TIME)
                {
                    hitbox.active = false;
                    alive = false;
                }
                else
                {
                    foreach (Enemy e in Game1.levelManager.enemies)
                    {
                        if (hitbox.intersects(e.hitbox) && !e.hitboxesHitBy.Contains(hitbox))
                        {
                            e.hitboxesHitBy.Add(hitbox);
                            e.hitBy(parent, damage, position, knockbackStrength, knockbackDuration);
                        }
                    }
                }
            }
            else
            {
                fuse += seconds;
                if (fuse > BalloonBomb.FUSE_SECONDS && height == 0f)
                    exploding = true;
            }
            hitbox.width = hitbox.height = (int)rad-getTexture().Width/2;

            alpha = 1 - (rad - getTexture().Width) / EXPLOSION_RADIUS;
            scale = (rad / getTexture().Width);
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (alive)
            {
               Vector2 drawPos = position;
                drawPos.Y = position.Y - height;
                if (exploding) Utility.Draw(spriteBatch, explosion.getTexture(), drawPos, Color.White, .25f * scale * getTransformedScale(), 1f, rotation, SpriteEffects.None);
                Utility.Draw(spriteBatch, getTexture(), drawPos, Color.White, scale * getTransformedScale(), alpha, rotation, getFlip());
                //hitbox.Draw(spriteBatch);
                //texture.DrawHitbox(spriteBatch);
            }
        }
    }
}