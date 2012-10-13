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
    public class Entity
    {
        public Vector2 position;
        public Vector2 velocity;
        public Vector2 acceleration;
        public bool alive;

        public delegate bool DelayedDeathAction();

        public DelayedDeathAction delayedDeath;

        public Hitbox hitbox;
        protected AnimatedTexture texture;
        public float scale = 1;
        public float alpha = 1;
        public float rotation = 0;
        public static Vector2 INITIAL_POSITION = new Vector2(300, 0);
        public Direction curDirection = Direction.Right;
        public enum Direction
        {
            Left, Right, Forward, Back
        }

        public Vector2 origin;
        public Vector2 dualScale = new Vector2(1f, 1f);
        public bool centered = true;
        public bool singleScale = true;

        public Entity()
        {
            alive = true;
            position = INITIAL_POSITION;
            hitbox = new Hitbox(new Vector2(0, 0), 0, 0);
        }

        public Entity(Hitbox hitbox)
        {
            alive = true;
            position = INITIAL_POSITION;
            this.hitbox = hitbox;
        }

        public void setPosition(Vector2 v)
        {
            position = v;
        }

        public virtual Texture2D getTexture()
        {
            return texture.getTexture();
        }

        public Vector2 getTop()
        {
            return position + new Vector2(0, -hitbox.height / 2f) + hitbox.offset;
        }

        public Vector2 getBottom()
        {
            return position + new Vector2(0, hitbox.height / 2f) + hitbox.offset;
        }

        public Vector2 getLeft()
        {
            return position + new Vector2(-hitbox.width / 2f, 0) + hitbox.offset;
        }

        public Vector2 getRight()
        {
            return position + new Vector2(hitbox.width / 2f, 0) + hitbox.offset;
        }

        public Vector2 getFeet()
        {
            return position + new Vector2(0, hitbox.height / 2f) + hitbox.offset;
        }

        public void setTop(float Y)
        {
            position = new Vector2(position.X, Y + getTexture().Height / 2f);
        }

        public void setFeet(float Y)
        {
            position = new Vector2(position.X, Y + getTexture().Height / 2f);
        }

        public void setLeft(float X)
        {
            position = new Vector2(X + getTexture().Height / 2f, position.Y);
        }

        public void setRight(float X)
        {
            position = new Vector2(X - texture.getTexture().Height / 2f, position.Y);
        }

        public float getTransformedScale()
        {
            return .6f + .4f * (getBottom().Y / Game1.screenSize.Y);
        }

        public void setTexture(string tex)
        {
            texture = new AnimatedTexture(tex);
        }

        public void setTexture(AnimatedTexture tex)
        {
            if (texture != null)
                foreach (Hitbox hitbox in texture.onFrameHitboxes)
                    if (hitbox != null)
                    {
                        hitbox.active = false;
                    }
            texture = tex;
            texture.Update(null, this);
            if (texture.onStart != null)
                texture.onStart();
        }

        public void offsetPosition(Vector2 off)
        {
            position += off;
        }

        public bool canMove(Vector2 movement)
        {
            if (Game1.levelManager.isInView(this, movement))
                return true;
            return Game1.levelManager.attemptScroll(this, movement);
        }

        public virtual SpriteEffects getFlip()
        {
            return SpriteEffects.None;
        }

        public virtual void Update(GameTime gameTime)
        {

            if (alive)
            {
                texture.Update(gameTime, this);
                hitbox.Update(this);
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (alive)
            {
                if (centered) origin = new Vector2(getTexture().Width / 2f, getTexture().Height / 2f);
                if (singleScale) dualScale = new Vector2(scale, scale);
                Utility.Draw(spriteBatch, getTexture(), position, Color.White, dualScale * getTransformedScale(), alpha, rotation, origin, getFlip());
            }
        }
        public virtual void DrawDebug(SpriteBatch spriteBatch)
        {
            if (alive)
            {
                hitbox.Draw(spriteBatch);
                texture.DrawHitbox(spriteBatch);
            }
        }
    }
}
