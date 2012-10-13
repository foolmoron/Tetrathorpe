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

namespace Tetrathorpe.Projectiles
{
    class LaserProjectile : Projectile
    {
        Vector2 direction = Vector2.Zero;
        float distance;
        int time = 0;

        public LaserProjectile(Character _character, Vector2 _start, Vector2 _end, float _damage, float _speed)
            : base(_character, _start, _end, _damage, _speed)
        {
            centered = false;
            singleScale = false;
            direction = Vector2.Normalize(end - _character.position + start);
            distance = (end - character.position + start).Length();
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            float width = texture.getTexture().Width * getTransformedScale();
            float lengthDrawn = 0;
            position = character.position + start;
            origin = new Vector2(0, texture.getTexture().Height / 2f);
            dualScale.Y = 1f - 2f * Math.Abs(time / speed - .5f);
            Console.WriteLine("Time: " + time + " Speed: " + speed + " Scale: " + dualScale.Y);
            alpha = dualScale.Y;

            if (direction.Y != 0) rotation = MathHelper.ToRadians(90);

            while (lengthDrawn < distance)
            {
                width = texture.getTexture().Width * getTransformedScale();
                if (direction == new Vector2(1, 0) || direction == new Vector2(0, 1)) base.Draw(spriteBatch);
                lengthDrawn += width;
                position += width * direction;
                if (direction == new Vector2(-1, 0) || direction == new Vector2(0, -1)) base.Draw(spriteBatch);
            }

            position = character.position + start + distance / 2f * direction;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (direction.X != 0)
            {
                hitbox.width = (int)distance;
                hitbox.height = texture.getTexture().Height;
            }
            else
            {
                hitbox.width = texture.getTexture().Height;
                hitbox.height = (int)distance;
            }

            time += gameTime.ElapsedGameTime.Milliseconds;
            if (time > speed) base.endProjectile(null);
        }

        public override void endProjectile(Enemy e)
        {

        }
    }
}
