using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tetrathorpe.Projectiles;

namespace Tetrathorpe
{
    class Buffer : Character
    {
        public static string BASETEX = "Characters/Buffer/";
        public static string cName = "Buffer";
        public static Hitbox CHARACTER_HITBOX = new Hitbox(70, 100);

        public float curInvincBuffDuration = 0;
        public float curAttackBuffDuration = 0;

        public const float ATTACKBUFFAMOUNT = 1.5f;
        public const float ATTACKBUFFDURATION = 10000;        
        public static bool attBuff = false;
        public static float attBuffScale = 0f;
        int attBuffScaleDir = 0;
        public static bool invincBuff = false;
        public const float INVINCBUFFDURATION = 5000;
        public static float invicScale = 0f;
        public const int VAMPBUFFDURATION = 10000;

        const int INVINCOOLDOWN = 25 * 1000;

        int invinCD = INVINCOOLDOWN;
        float invinRot = 0f;
        float invinCDScale = 1f;

        const float LIGHTRANGE = 300;
        const float LIGHTSPEED = .3f;


        static bool texturesLoaded;

        public Buffer()
            : base(CHARACTER_HITBOX)
        {
            maxHealth = 100;
            health = 100;
            hitbox.active = true;
            characterTextures.Add(CharacterState.Standing, new AnimatedTexture(BASETEX + "buffer stand"));
            characterTextures.Add(CharacterState.Walking, new AnimatedTexture(BASETEX + "buffer walk", 3, 100));
            characterTextures.Add(CharacterState.AttackingLight, new AnimatedTexture(BASETEX + "buffer heavy", 2, 100));
            characterTextures.Add(CharacterState.AttackingHeavy, new AnimatedTexture(BASETEX + "buffer light", 2, 100));
            characterTextures.Add(CharacterState.AttackingSpecial, new AnimatedTexture(BASETEX + "buffer special", 1, 100));
            characterTextures.Add(CharacterState.Blocking, new AnimatedTexture(BASETEX + "buffer block", 2, 100));

            Action stand = delegate()
            {
                setCharacterState(CharacterState.Standing);
            };
            characterTextures[CharacterState.AttackingHeavy].onStart = startAttBuff;
            characterTextures[CharacterState.AttackingHeavy].onFinish = stand;
            characterTextures[CharacterState.AttackingLight].setOnFrameAction(2, startVampBuff);
            characterTextures[CharacterState.AttackingLight].onFinish = stand;
            characterTextures[CharacterState.AttackingSpecial].onStart = startInvincBuff;
            characterTextures[CharacterState.AttackingSpecial].onFinish = stand;
            characterTextures[CharacterState.Blocking].onFinish = stand;

            TextureManager.Add(BASETEX + cName);
            hudTexture = TextureManager.Get(BASETEX + cName);
            TextureManager.Add(BASETEX + cName + " dead");
            deadTexture = TextureManager.Get(BASETEX + cName + " dead");

            //Projectiles
            TextureManager.Add(BASETEX + "buffer-protection");
            TextureManager.Add(BASETEX + "buffer-protection-cd");
            TextureManager.Add(BASETEX + "buffer-attack-buff");
            TextureManager.Add(BASETEX + "buffer-vamp");

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
                TextureManager.Add(AOE.BASETEX + "aoe storedballoon");
                TextureManager.Add(AOE.BASETEX + "aoe balloonbomb");
                TextureManager.Add(AOE.BASETEX + "aoe balloonline");
                texturesLoaded = true;
            }
        }

        public override void setCharacterState(CharacterState state)
        {
            base.setCharacterState(state);
        }

        public override void Update(GameTime gameTime)
        {
            float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            invinCD += gameTime.ElapsedGameTime.Milliseconds;
            invinRot += gameTime.ElapsedGameTime.Milliseconds * .001f;
            invinRot = MathHelper.WrapAngle(invinRot);

            if (invinCD < INVINCOOLDOWN) invinCDScale -= gameTime.ElapsedGameTime.Milliseconds * .001f;
            else invinCDScale += gameTime.ElapsedGameTime.Milliseconds * .001f;

            invinCDScale = Math.Max(Math.Min(invinCDScale, 1), 0);

            if (!alive)
            {
                attBuff = false;
                invincBuff = false;
            }
            else
            {
                if (attBuff)
                {
                    curAttackBuffDuration += gameTime.ElapsedGameTime.Milliseconds / 10000;
                    if (curAttackBuffDuration > ATTACKBUFFDURATION)
                    {
                        attBuff = false;
                        curAttackBuffDuration = 0;
                    }
                }
                if (invincBuff)
                {
                    curInvincBuffDuration += gameTime.ElapsedGameTime.Milliseconds;
                    if (invicScale < 1f) invicScale += .01f * gameTime.ElapsedGameTime.Milliseconds;
                    if (curInvincBuffDuration > INVINCBUFFDURATION)
                    {
                        invincBuff = false;
                        curInvincBuffDuration = 0;
                    }
                }
                else if (invicScale > 0f) invicScale -= .01f * gameTime.ElapsedGameTime.Milliseconds;
            }

            if (attBuffScaleDir != 0)
            {
                attBuffScale += gameTime.ElapsedGameTime.Milliseconds * .005f * attBuffScaleDir;
                if (attBuffScale > 1f) attBuffScaleDir = -1;
                if (attBuffScale < 0f) { attBuffScaleDir = 0; attBuffScale = 0; }
            }
            base.Update(gameTime);
        }

        public void startAttBuff()
        {
            attBuff = true;
            curAttackBuffDuration = 0;

            attBuffScale = 0f;
            attBuffScaleDir = 1;
        }

        public void startVampBuff()
        {
            LineProjectile proj = new LineProjectile(this, position, position + fourWayDirection() * LIGHTRANGE, 0, LIGHTSPEED);
            proj.setTexture(new AnimatedTexture(BASETEX + "buffer-vamp"));
            proj.modifiers.Add(delegate(Projectile p, GameTime gameTime)
            {
                p.rotation += .003f * gameTime.ElapsedGameTime.Milliseconds;
            });
            proj.collisions = Projectile.CollisionGroup.Characters;
            proj.characterAction = delegate(Character c)
            {
                c.numVampBats++;
                c.numVampBats = Math.Min(c.numVampBats, 5);
                c.vampBatDuration = 0;
                foreach (Character c2 in Game1.characterManager.liveCharacters)
                {
                    if (c2 != c) c2.numVampBats = 0;
                }
            };
        }

        public void startInvincBuff()
        {
            if (invinCD >= INVINCOOLDOWN)
            {
                invincBuff = true;
                curInvincBuffDuration = 0;
                invicScale = 0f;
                invinCD = 0;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            Utility.Draw(spriteBatch, BASETEX + "buffer-protection-cd", getTop() + new Vector2((float)Math.Cos(invinRot) * 15f, (float)Math.Sin(invinRot) * 5f - 20), Color.White, invinCDScale, invinCDScale, invinRot, SpriteEffects.None);
        }
    }
}