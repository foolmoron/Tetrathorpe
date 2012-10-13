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
    class TrailProjectile : LineProjectile
    {
        public float trailDistance = 60;
        public int trailNum = 4;

        bool drawingEnd;
        AnimatedTexture endTexture;
        float endProgress;


        public TrailProjectile(Character _character, Vector2 _start, Vector2 _end, float _damage, float _speed, AnimatedTexture _endTexture) : base (_character, _start, _end, _damage, _speed)
        {
            endTexture = _endTexture;
            centered = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (drawingEnd)
            {
                scale = Math.Min(endProgress, 1f);
                alpha = scale;
                base.Draw(spriteBatch);
            }
            else
            {
                Vector2 curPos = position;
                float curScale = scale;
                float curAlpha = alpha;

                Vector2 distance = position - start;
                float length = distance.Length();
                int segment = (int)Math.Floor(length / trailDistance);
                float segmentStart = trailDistance * segment;
                float segmentAmount = length - segmentStart;
                Vector2 direction = Vector2.Normalize(end - start);

                for (int i = 1; i < trailNum + 1; i++)
                {
                    position = start + segmentStart * direction + (float)i / (trailNum + 1) * direction * trailDistance;
                    position = position - new Vector2(0, hitbox.height / 2f);
                    scale = (float)i / (trailNum + 1) * curScale * (1f - 2f * Math.Abs(segmentAmount / trailDistance  - .5f));
 
                    base.Draw(spriteBatch);
                }

                position = curPos;
                scale = curScale;
                alpha = curAlpha;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            origin = new Vector2(texture.getTexture().Width / 2f, texture.getTexture().Height);
            hitbox.offset.Y = -hitbox.height;

            if (drawingEnd)
            {
                if (endProgress >= 10f)
                {
                    base.endProjectile(null);
                }
                else endProgress += .005f * gameTime.ElapsedGameTime.Milliseconds;
            }
        }

        public override void endProjectile(Enemy e)
        {
            speed = 0;

            if (e != null)
            {
                drawingEnd = true;

                hitbox = new Hitbox(endTexture.getTexture().Width, endTexture.getTexture().Height);

                texture = endTexture;
                position = e.getFeet();
                e.stun(500);
            }
        }
    }
}
