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


namespace Tetrathorpe.Projectiles
{
    class LineProjectile : Projectile
    {
        public static float HIT_RADIUS = 40f;
        public LineProjectile(Character _character, Vector2 _start, Vector2 _end, float _damage, float _speed) : base (_character, _start, _end, _damage, _speed)
        {
            hitbox.width = hitbox.height = (int)HIT_RADIUS;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            position += Vector2.Normalize(end - start) * speed * gameTime.ElapsedGameTime.Milliseconds;

            if ((position - end).LengthSquared() > (end - start).LengthSquared())
            {
                alive = false;
            }
        }
    }
}
