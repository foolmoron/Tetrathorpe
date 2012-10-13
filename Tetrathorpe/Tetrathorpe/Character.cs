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
    public abstract class Character : Entity
    {
        public Texture2D hudTexture;
        public Texture2D deadTexture;
        public double maxHealth = 100;
        public double health = 100; // health is measured out of 100.
        public enum CharacterState
        {
            Standing, Walking, Blocking, AttackingLight, AttackingHeavy, AttackingSpecial
        }

        public Dictionary<CharacterState, AnimatedTexture> characterTextures = new Dictionary<CharacterState, AnimatedTexture>();

        public float moveSpeed = 3;

        bool playerControlled;

        public CharacterState curState;

        public int numVampBats = 0;
        public int vampBatDuration = 0;
        float vampRot = 0;               

        public Character(Hitbox hitbox)
            : base(hitbox)
        {
        }

        public override Texture2D getTexture()
        {
            if (alive) return texture.getTexture(curDirection);
            return deadTexture;
        }

        public override SpriteEffects getFlip()
        {
            if (curDirection == Direction.Left) return SpriteEffects.FlipHorizontally;
            else return SpriteEffects.None;
        }

        public void initiateControl()
        {
            playerControlled = true;
            Game1.characterManager.setSingleControlled(this);
        }

        public void releaseControl()
        {
            playerControlled = false;
            setCharacterState(CharacterState.Standing);
        }

        public bool isControlled()
        {
            return playerControlled;
        }

        public Vector2 fourWayDirection()
        {
            switch (curDirection)
            {
                case Direction.Back: return new Vector2(0, -1);
                case Direction.Forward: return new Vector2(0, 1);
                case Direction.Left: return new Vector2(-1, 0);
                case Direction.Right: return new Vector2(1, 0);
                default: return Vector2.Zero;
            }
        }

        public virtual void attemptMove(Vector2 movement)
        {
            if (!alive) return;
            if (curState != CharacterState.Standing && curState != CharacterState.Walking) return;

            if (movement.LengthSquared() > .05)
            {
                float nextX = position.X + (movement.X * moveSpeed);
                float nextY = position.Y + (movement.Y * moveSpeed);

                if (!canMove(new Vector2(movement.X * moveSpeed, 0)))
                {
                    nextX = position.X;
                }
                if (!canMove(new Vector2(0, movement.Y * moveSpeed)))
                {
                    nextY = position.Y;
                }

                position = new Vector2(nextX, nextY);


                setCharacterState(CharacterState.Walking);

                if (Math.Abs(movement.X) > Math.Abs(movement.Y))
                {
                    if (movement.X > 0) curDirection = Direction.Right;
                    if (movement.X < 0) curDirection = Direction.Left;
                }
                if (Math.Abs(movement.Y) > Math.Abs(movement.X))
                {
                    if (movement.Y > 0) curDirection = Direction.Forward;
                    if (movement.Y < 0) curDirection = Direction.Back;
                }
            }
            else setCharacterState(CharacterState.Standing);
        }

        public virtual void setCharacterState(CharacterState state)
        {
            if (characterTextures.ContainsKey(state))
            {
                setTexture(characterTextures[state]);
                curState = state;
            }
        }

        public void lightAttack()
        {
            setCharacterState(CharacterState.AttackingLight);
        }

        public void heavyAttack()
        {
            setCharacterState(CharacterState.AttackingHeavy);
        }

        public void specialAttack()
        {
            setCharacterState(CharacterState.AttackingSpecial);
        }

        public void block()
        {
            setCharacterState(CharacterState.Blocking);
        }

        internal virtual void hitBy(Enemy enemy)
        {
            if (curState != CharacterState.Blocking && Buffer.invincBuff == false)
            {
                health -= enemy.hitDamage;
                if (health <= 0)
                    Game1.characterManager.killCharacter(this);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            vampRot += gameTime.ElapsedGameTime.Milliseconds * .001f;
            vampRot = MathHelper.WrapAngle(vampRot);

            if (numVampBats > 0)
            {
                vampBatDuration += gameTime.ElapsedGameTime.Milliseconds;
                if (vampBatDuration > Buffer.VAMPBUFFDURATION)
                {
                    vampBatDuration = 0;
                    numVampBats = 0;
                }
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (alive)
            {
                Utility.Draw(spriteBatch, getTexture(), position, Color.White, scale * getTransformedScale(), alpha, rotation, getFlip());
            }
            else
            {
                Utility.Draw(spriteBatch, deadTexture, position, Color.White, scale * getTransformedScale(), alpha, rotation, getFlip());
            }

            if (Buffer.invincBuff || Buffer.invicScale > 0f)
            {
                Utility.Draw(spriteBatch, Buffer.BASETEX + "buffer-protection", position, Color.White, scale * getTransformedScale() * Buffer.invicScale, alpha * Buffer.invicScale, rotation, getFlip());
            }

            if (numVampBats > 0)
            {
                for (int i = 0; i < numVampBats; i++)
                {
                    float rot = vampRot;
                    rot += i * (float)Math.PI * .4f;
                    Utility.Draw(spriteBatch, Buffer.BASETEX + "buffer-vamp", getTop() + new Vector2((float)Math.Cos(rot) * 20f, (float)Math.Sin(rot) * 20f), Color.White, .2f, 1f * (1f - (float)vampBatDuration / Buffer.VAMPBUFFDURATION), rot, SpriteEffects.None);
                }
            }

            if (Buffer.attBuffScale > 0f)
            {
                Utility.Draw(spriteBatch, Buffer.BASETEX + "buffer-attack-buff", getTop() + new Vector2(0, -30), Color.White, Buffer.attBuffScale * .75f, Buffer.attBuffScale, 0f, SpriteEffects.None);
            }
        }
    }
}
