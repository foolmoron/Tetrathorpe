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
using Tetrathorpe.Projectiles;

namespace Tetrathorpe
{
    class Turtle : Character
    {
        public static string BASETEX = "Characters/Turtle/";
        public static string cName = "Turtle";
        public static Hitbox CHARACTER_HITBOX = new Hitbox(45, 95);

        public static float CHARGE_VELOCITY = 700f;
        public static float SCOOP_VELOCITY = 300f;
        public static float CHARGE_COOLDOWN = 0.1f; //seconds
        public float chargeCooldown = 0f;
        public bool charging = false;
        public bool superArmor = false;
        public bool countering = false;

        public static Dictionary<CharacterState, double> attackDamages = new Dictionary<CharacterState, double>();
        public static Dictionary<CharacterState, float> knockbackStrengths = new Dictionary<CharacterState, float>();
        public static Dictionary<CharacterState, float> knockbackDurations = new Dictionary<CharacterState, float>();

        static bool texturesLoaded;

        public Turtle()
            : base(CHARACTER_HITBOX)
        {
            maxHealth = 125;
            health = 125;

            hitbox.active = true;
            characterTextures.Add(CharacterState.Standing, new AnimatedTexture(BASETEX + "turtle stand"));
            characterTextures.Add(CharacterState.Walking, new AnimatedTexture(BASETEX + "turtle walk", 3, 200));
            characterTextures.Add(CharacterState.AttackingLight, new AnimatedTexture(BASETEX + "turtle light", 2, 100));
            characterTextures.Add(CharacterState.AttackingHeavy, new AnimatedTexture(BASETEX + "turtle heavy", 7, 75));
            characterTextures.Add(CharacterState.AttackingSpecial, new AnimatedTexture(BASETEX + "turtle special", 2, 200));
            characterTextures.Add(CharacterState.Blocking, new AnimatedTexture(BASETEX + "turtle block", 2, 200));

            characterTextures[CharacterState.AttackingLight].onFinish = stand;
            characterTextures[CharacterState.AttackingLight].setOnFrameRangeHitbox(1, 2, new Hitbox(new Vector2(40, 0), 40, 95, true));
            characterTextures[CharacterState.AttackingHeavy].onStart = startScoop;
            characterTextures[CharacterState.AttackingHeavy].setOnFrameAction(4, stopScoop);
            characterTextures[CharacterState.AttackingHeavy].onFinish = delegate() { charging = false; stand(); chargeCooldown = CHARGE_COOLDOWN; };
            characterTextures[CharacterState.AttackingHeavy].setOnFrameRangeHitbox(1, 7, new Hitbox(new Vector2(30, 0), 60, 95, true));
            characterTextures[CharacterState.AttackingSpecial].onStart = startCharge;
            characterTextures[CharacterState.AttackingSpecial].onFinish = endCharge;
            characterTextures[CharacterState.AttackingSpecial].setOnFrameRangeHitbox(1, 2, new Hitbox(new Vector2(40, 0), 40, 95, true));
            characterTextures[CharacterState.Blocking].onStart = delegate() { countering = true; };
            characterTextures[CharacterState.Blocking].setOnFrameAction(2, delegate() { countering = false; }); // counter on first frame only
            characterTextures[CharacterState.Blocking].onFinish = stand;

            attackDamages[CharacterState.AttackingLight] = 5;
            knockbackStrengths[CharacterState.AttackingLight] = 50f;
            knockbackDurations[CharacterState.AttackingLight] = 2f;
            attackDamages[CharacterState.AttackingHeavy] = 10;
            knockbackStrengths[CharacterState.AttackingHeavy] = 100f;
            knockbackDurations[CharacterState.AttackingHeavy] = 1f;
            attackDamages[CharacterState.AttackingSpecial] = 15;
            knockbackStrengths[CharacterState.AttackingSpecial] = 150f;
            knockbackDurations[CharacterState.AttackingSpecial] = 0.5f;

            TextureManager.Add(BASETEX + cName);
            hudTexture = TextureManager.Get(BASETEX + cName);
            TextureManager.Add(BASETEX + cName + " dead");
            deadTexture = TextureManager.Get(BASETEX + cName + " dead");

            Initialize(this);
        }

        public static void Initialize(Character c)
        {
            if (!texturesLoaded)
            {
                foreach (AnimatedTexture tex in c.characterTextures.Values)
                {
                    tex.LoadTextures();
                }
                texturesLoaded = true;
            }
        }

        public override void setCharacterState(CharacterState state)
        {
            if (!charging)
            {
                if (state == CharacterState.AttackingHeavy || state == CharacterState.AttackingSpecial)
                    if (chargeCooldown > 0f)
                        return;
                base.setCharacterState(state);
            }
        }

        public override void attemptMove(Vector2 movement)
        {
            if (!charging)
                base.attemptMove(movement);
        }

        internal override void hitBy(Enemy enemy)
        {
            if (countering)
                enemy.hitBy(this, enemy.hitDamage);
            else if (superArmor)
                return;
            else
                base.hitBy(enemy);
        }

        public void stand()
        {
            moveSpeed = 3;
            setCharacterState(CharacterState.Standing);
        }

        public void startScoop()
        {
            switch (curDirection)
            {
                case Direction.Right:
                    velocity = new Vector2(SCOOP_VELOCITY, 0);
                    break;
                case Direction.Left:
                    velocity = new Vector2(-SCOOP_VELOCITY, 0);
                    break;
                case Direction.Back:
                    velocity = new Vector2(0, -SCOOP_VELOCITY);
                    break;
                case Direction.Forward:
                    velocity = new Vector2(0, SCOOP_VELOCITY);
                    break;
            }
            charging = true;
        }

        public void stopScoop()
        {
            velocity = Vector2.Zero;
        }

        public void startCharge()
        {
            switch (curDirection)
            {
                case Direction.Right:
                    velocity = new Vector2(CHARGE_VELOCITY, 0);
                    break;
                case Direction.Left:
                    velocity = new Vector2(-CHARGE_VELOCITY, 0);
                    break;
                case Direction.Back:
                    velocity = new Vector2(0, -CHARGE_VELOCITY);
                    break;
                case Direction.Forward:
                    velocity = new Vector2(0, CHARGE_VELOCITY);
                    break;
            }
            charging = true;
            superArmor = true;
        }

        public void endCharge()
        {
            velocity = Vector2.Zero;
            charging = false;
            superArmor = false;
            stand();
            chargeCooldown = CHARGE_COOLDOWN;
        }

        public override void Update(GameTime gameTime)
        {
            if (!alive)
                return;
            float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            chargeCooldown -= seconds;
            if (charging)
            {
                float nextX = position.X + (velocity.X * seconds);
                float nextY = position.Y + (velocity.Y * seconds);

                if (!canMove(new Vector2(velocity.X * seconds, 0)))
                    nextX = position.X;
                if (!canMove(new Vector2(0, velocity.Y * seconds)))
                    nextY = position.Y;

                position = new Vector2(nextX, nextY);
            }

            //melee collision
            bool hasThrown = false;
            foreach (Enemy e in Game1.levelManager.enemies)
            {
                if (characterTextures[curState].getHitbox() != null && characterTextures[curState].getHitbox().intersects(e.hitbox))
                {
                    if (!e.hitboxesHitBy.Contains(characterTextures[curState].getHitbox()))
                    {
                        e.hitboxesHitBy.Add(characterTextures[curState].getHitbox());
                        e.hitBy(this, attackDamages[curState], position, knockbackStrengths[curState], knockbackDurations[curState]);

                        if (!hasThrown && curState == CharacterState.AttackingHeavy)
                        {
                            e.arcThrow(50f, e.position - position);
                            hasThrown = true;
                        }
                    }
                }
            }

            base.Update(gameTime);
        }
    }
}
