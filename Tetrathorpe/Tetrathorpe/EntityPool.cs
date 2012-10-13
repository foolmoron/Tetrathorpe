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
    public class EntityPool<T> where T : Entity, new()
    {
        public T[] entities { get; private set; }
        public int active { get; private set; }
        public Action<T> initializer;
    
        // initializer action allows you to specialize the entities in the pool without making dedicated subclasses
        public EntityPool(int budget, Action<T> initializer)
        {
            this.initializer = initializer;
            entities = new T[budget];
            for (int i = 0; i < budget; i++)
            {
                entities[i] = new T();
                if (initializer != null)
                    initializer(entities[i]);
                entities[i].alive = false;
            }
        }

        // reset action is applied to all obtained entities before returning them
        public T obtain(Action<T> reset)
        {
            // expand pool size if necessary
            if (active >= entities.Length)
            {
                T[] newEntities = new T[2 * entities.Length];
                int i = 0;
                for (; i < entities.Length; i++)
                    newEntities[i] = entities[i];
                for (; i < newEntities.Length; i++)
                {
                    newEntities[i] = new T();
                    if (initializer != null)
                      initializer(newEntities[i]);
                    newEntities[i].alive = false;
                }
                entities = newEntities;
            }
            entities[active].alive = true;
            if (reset != null)
                reset(entities[active]);
            return entities[active++];
        }
        
        // automatically detects and recycles dead entities
        public void Update(GameTime time)
        {
            for (int i = 0; i < entities.Length && i < active; i++)
            {
                if (!entities[i].alive)
                {
                    T swap = entities[i];
                    entities[i] = entities[active - 1];
                    entities[active - 1] = swap;
                    active--;
                }
            }
        }
    }
}
