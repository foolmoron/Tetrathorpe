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
    class GlassCannon : Character
    {
        public static string BASETEX = "Characters/Glass Cannon/";
        public static string cName = "Glass Cannon";
        public static Hitbox CHARACTER_HITBOX = new Hitbox(50, 100);

        const float LIGHTRANGE = 300;
        const float LIGHTDAMAGE = 20;
        const float LIGHTSPEED = .4f;

        const float HEAVYRANGE = 350;
        const float HEAVYDAMAGE = 30;
        const float HEAVYSPEED = .3f;

        const float SPECIALRANGE = 1000;
        const float SPECIALDAMAGE = 15;
        const float SPECIALDURATION = 300;

        static bool texturesLoaded;

        public GlassCannon()
            : base(CHARACTER_HITBOX)
        {
            maxHealth = 50;
            health = 50;

            hitbox.active = true;
            characterTextures.Add(CharacterState.Standing, new AnimatedTexture(BASETEX + "glass stand"));
            characterTextures.Add(CharacterState.Walking, new AnimatedTexture(BASETEX + "glass walk", 3, 200));
            characterTextures.Add(CharacterState.AttackingLight, new AnimatedTexture(BASETEX + "glass light", 3, 200));
            characterTextures.Add(CharacterState.AttackingHeavy, new AnimatedTexture(BASETEX + "glass heavy", 3, 300));
            characterTextures.Add(CharacterState.AttackingSpecial, new AnimatedTexture(BASETEX + "glass special", 2, 200));
            characterTextures.Add(CharacterState.Blocking, new AnimatedTexture(BASETEX + "glass block", 2, 200));

            characterTextures[CharacterState.AttackingLight].onFinish = stand;
            characterTextures[CharacterState.AttackingHeavy].onFinish = stand;
            characterTextures[CharacterState.AttackingSpecial].onFinish = stand;
            characterTextures[CharacterState.Blocking].onFinish = stand;

            characterTextures[CharacterState.AttackingLight].setOnFrameAction(2, fireMask);
            characterTextures[CharacterState.AttackingHeavy].setOnFrameAction(3, shootCrystals);
            characterTextures[CharacterState.AttackingSpecial].setOnFrameAction(2, shootLaser);

            TextureManager.Add(BASETEX + cName);
            hudTexture = TextureManager.Get(BASETEX + cName);
            TextureManager.Add(BASETEX + cName + " dead");
            deadTexture = TextureManager.Get(BASETEX + cName + " dead");

            //Projectiles
            TextureManager.Add(BASETEX + "glass-eruption");
            TextureManager.Add(BASETEX + "glass-hat");
            TextureManager.Add(BASETEX + "glass-laser");

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
            base.setCharacterState(state);
        }

        public void stand()
        {
            setCharacterState(CharacterState.Standing);
        }

        public void fireMask()
        {
            LineProjectile proj = new LineProjectile(this, position, position + fourWayDirection() * LIGHTRANGE, LIGHTDAMAGE, LIGHTSPEED);
            proj.setTexture(new AnimatedTexture(BASETEX + "glass-hat"));
            proj.modifiers.Add(delegate(Projectile p, GameTime gameTime)
            {
                p.rotation += .003f * gameTime.ElapsedGameTime.Milliseconds;
            });
        }

        public void shootCrystals()
        {

            Vector2 start = Vector2.Zero;
            switch (curDirection)
            {
                case Direction.Left: start = new Vector2(getLeft().X, getFeet().Y + 20); break;
                case Direction.Right: start = new Vector2(getRight().X, getFeet().Y + 20); break;
                case Direction.Forward: start = new Vector2(getFeet().X, getFeet().Y + 20); break;
                case Direction.Back: start = new Vector2(getFeet().X, getFeet().Y - 20); break;
            }
            TrailProjectile proj = new TrailProjectile(this, start, start + fourWayDirection() * HEAVYRANGE, HEAVYDAMAGE, HEAVYSPEED, new AnimatedTexture(BASETEX + "glass-eruption"));
            proj.setTexture(new AnimatedTexture(BASETEX + "glass-eruption"));
        }

        public void shootLaser()
        {
            Vector2 start = Vector2.Zero;
            switch (curDirection)
            {
                case Direction.Left: start = new Vector2(-38, 0); break;
                case Direction.Right: start = new Vector2(38, 0); break;
                case Direction.Forward: start = new Vector2(0, 10); break;
                case Direction.Back: start = new Vector2(0, -38); break;
            }
            LaserProjectile proj = new LaserProjectile(this, start, position + fourWayDirection() * SPECIALRANGE, SPECIALDAMAGE, SPECIALDURATION);
            proj.setTexture(new AnimatedTexture(BASETEX + "glass-laser"));
        }
    }
}
