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
    public class EntityManager
    {
        List<Entity> entities = new List<Entity>();

        public void AddEntity(Entity e)
        {
            entities.Add(e);
        }

        public void RemoveEntity(Entity e)
        {
            entities.Remove(e);
        }

        public void Update(GameTime gameTime)
        {
            Queue<Entity> removeQ = new Queue<Entity>();
            foreach (Entity e in entities)
            {
                e.Update(gameTime);
                if (!e.alive && (e.delayedDeath == null || e.delayedDeath()))
                {
                    removeQ.Enqueue(e);
                }
            }
            while (removeQ.Count > 0) entities.Remove(removeQ.Dequeue());
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Entity e in entities)
            {
                e.Draw(spriteBatch);
            }
        }
        public void DrawDebug(SpriteBatch spriteBatch)
        {
            foreach (Entity e in entities)
            {
                e.DrawDebug(spriteBatch);
            }
        }

    }
}
