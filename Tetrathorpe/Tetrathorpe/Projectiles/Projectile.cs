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
    public class Projectile : Entity
    {
        protected Character character;
        protected Vector2 start;
        protected Vector2 end;
        protected float damage;
        protected float speed;
        public CollisionGroup collisions = CollisionGroup.Enemies;

        public List<Action<Projectile, GameTime>> modifiers = new List<Action<Projectile, GameTime>>();

        public enum CollisionGroup { Characters, Enemies };

        public Action<Character> characterAction = null;

        public Projectile(Character _character, Vector2 _start, Vector2 _end, float _damage, float _speed)
        {
            character = _character;
            start = _start;
            position = _start;
            end = _end;
            speed = _speed;
            damage = _damage;
            hitbox.active = true;

            Game1.managedEntities.AddEntity(this);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            foreach (Action<Projectile, GameTime> mod in modifiers)
            {
                mod(this, gameTime);
            }

            if (alive)
            {
                if (collisions == CollisionGroup.Enemies)
                {
                    foreach (Enemy e in Game1.levelManager.enemies)
                    {
                        if (hitbox.intersects(e.hitbox))
                        {
                            if (!e.hitboxesHitBy.Contains(hitbox))
                            {
                                e.hitboxesHitBy.Add(hitbox);
                                e.hitBy(character, damage);
                            }
                            endProjectile(e);
                        }
                    }
                }
                else
                {
                    foreach (Character c in Game1.characterManager.liveCharacters)
                    {
                        if (c == character) continue;
                        if (hitbox.intersects(c.hitbox))
                        {
                            if (characterAction != null) characterAction(c);
                            endProjectile(null);
                        }
                    }
                }
            }
        }

        public virtual void endProjectile(Enemy e)
        {
            alive = false;
        }
    }
}
