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
    public class Enemy : Entity
    {
        public static string BASETEX = "Enemies/";
        public static string[] enemyTypes = { "crane", "stalac", "cabbage", "turtle", "shelf", "pigeon", "bat", "book", "word" };
        public static Hitbox[] enemyHitboxes = { new Hitbox(148, 310), new Hitbox(87, 289), new Hitbox(172, 89), new Hitbox(155, 111), new Hitbox(159, 149), new Hitbox(171, 135), new Hitbox(250, 191), new Hitbox(146, 26), new Hitbox(112, 34) };
        public Character target;
        public List<Hitbox> hitboxesHitBy = new List<Hitbox>();
        public float hitRadius = 5;
        public float moveSpeed = 80f;
        public float fleeSpeed = 50f;
        public static float fleeTime = 1.5f;
        public float disableMovementTime = 0f;
        public float disableMovementDistance = 0f;
        public float alpharate = 5f;

        public string species = enemyTypes[0]; // what kind of enemy it is

        public double hitDamage = 10;
        
        public double maxHealth = 50;
        public double health = 50;
        public double timer = 0;

        public EnemyState currentState;
        public EnemyAI currentAI;
        public EnemyAI oldAI;

        public Vector2 wanderDir;
        public float wanderDist = 200;
        public int wanderStage = 1;

        public int stunTimer = 0;
        public float knockbackTime = 0;

        int attackCooldown = 0;
        public float attackSpeed = 2f;

        bool thrown = false;
        float throwAngle = 0f;
        float throwRadius = 0f;
        int throwDirection = 1;
        Vector2 throwCenter = Vector2.Zero;

        public enum EnemyState
        {
            Standing, Walking, Blocking, Attacking, AttackingAll
        }

        public enum EnemyAI
        {
            NoMove, Flee, NoFlee, Neutral, SwampBoss, DynamiteBoss, SpawnWords, NoMoveKnockback
        }

        public Enemy(int type)
        {
            if (type == -1)
                return;
            species = enemyTypes[type];
            hitbox = new Hitbox((int)(enemyHitboxes[type].width * .8f), (int)(enemyHitboxes[type].height * .8f));
            hitbox.active = true;
            setTexture(new AnimatedTexture(Enemy.BASETEX + species,2, 150));
            target = Game1.characterManager.getRandomLiveCharacter();
            currentState = EnemyState.Attacking;

            switch (type)
            {
                case 0:
                    {
                        scale = .75f;
                        maxHealth = 50;
                        currentAI = EnemyAI.Flee; break;
                    }
                case 1:
                    {
                        maxHealth = 100;
                        currentState = EnemyState.AttackingAll;
                        currentAI = EnemyAI.NoMove; break;
                    }
                case 2:
                    {
                        maxHealth = 100;
                        currentAI = EnemyAI.NoFlee; break;
                    }
                case 3:
                    {
                        maxHealth = 200;
                        hitDamage = 15;
                        currentAI = EnemyAI.Neutral;
                        moveSpeed = 30f;
                        break;
                    }
                case 4:
                    {
                        maxHealth = 300;
                        currentAI = EnemyAI.NoFlee; break;
                    }
                case 5:
                    {
                        maxHealth = 50;
                        currentAI = EnemyAI.Flee; break;
                    }
                case 6:
                    {
                        maxHealth = 150;
                        scale = .6f;
                        currentAI = EnemyAI.NoFlee; break;
                    }
                case 7:
                    {
                        maxHealth = 500;
                        currentAI = EnemyAI.SpawnWords;
                        currentState = EnemyState.Walking;
                        attackSpeed = .5f;
                        break;
                    }
                case 8:
                    {
                        currentAI = EnemyAI.Flee; break;
                    }
            }

            hitbox.width = (int)(hitbox.width * scale);
            hitbox.height = (int)(hitbox.height * scale);

            maxHealth *= 1.5f;
            health = maxHealth;

            oldAI = currentAI;
        }

        public override SpriteEffects getFlip()
        {
            switch (currentState)
            {
                case EnemyState.Walking: return (wanderDir.X > 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                case EnemyState.Attacking: return ((target.position - position).X > 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            }
            return SpriteEffects.None;
        }

        public override void Update(GameTime gameTime)
        {
            if (thrown)
            {
                throwAngle += throwDirection * gameTime.ElapsedGameTime.Milliseconds * .01f;
                position = throwCenter + new Vector2((float)Math.Cos(throwAngle) * throwRadius, -(float)Math.Sin(throwAngle) * throwRadius);
                if (throwDirection == 1 && throwAngle > Math.PI)
                {
                    thrown = false;                    
                }
                if (throwDirection == -1 && throwAngle < 0) thrown = false;
                return;
            }

            float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            for (int i = hitboxesHitBy.Count - 1; i >= 0; i--)
            {
                if (!hitboxesHitBy[i].intersects(hitbox))
                {
                    hitboxesHitBy.RemoveAt(i);
                    continue;
                }
                Game1.levelManager.hitboxesToDeactivate.Add(hitboxesHitBy[i]);
            }
            if (knockbackTime > 0)
            {
                knockbackTime -= seconds;
                //currentAI = EnemyAI.NoMove;
                float nextX = position.X + (velocity.X * seconds);
                float nextY = position.Y + (velocity.Y * seconds);

                if (!canMove(new Vector2(velocity.X * seconds, 0)))
                    nextX = position.X;
                if (!canMove(new Vector2(0, velocity.Y * seconds)))
                    nextY = position.Y;
                position = new Vector2(nextX, nextY);
            }

			if (stunTimer > 0)
            {
                stunTimer -= gameTime.ElapsedGameTime.Milliseconds;
                return;
            }
            Vector2 distance;
            if (currentAI == EnemyAI.SwampBoss || currentAI == EnemyAI.DynamiteBoss)
            {
                base.Update(gameTime);
                return;
            }
            if (currentAI != EnemyAI.NoMove && currentAI != EnemyAI.NoMoveKnockback)
            {
                if (!target.alive)
                    target = Game1.characterManager.getRandomLiveCharacter();
            }
            else
            {
                target = Game1.characterManager.getClosestLiveCharacter(position);
            }

            if (currentAI == EnemyAI.Neutral) currentState = EnemyState.Walking;

            switch(currentState)
            {
                case EnemyState.Walking:
                    {
                         wanderDist += moveSpeed * seconds;
                         if (wanderDist > 200 && wanderStage == 1)
                         {
                             wanderDir = new Vector2((float)Game1.rand.NextDouble(), (float)Game1.rand.NextDouble());
                             wanderDir.Normalize();
                             wanderDist = 0;
                             wanderStage = 0;
                         }
                         if (wanderDist > 200 && wanderStage == 0)
                         {
                             wanderDir = -wanderDir;
                             wanderDist = 0;
                             wanderStage = 1;
                         }
                         position = position + moveSpeed * seconds * wanderDir;

                         if (currentAI == EnemyAI.SpawnWords)
                         {
                             attackCooldown += gameTime.ElapsedGameTime.Milliseconds;
                             if (attackCooldown > 1000 / attackSpeed)
                             {
                                 attackCooldown = 0;
                                 AnimatedTexture oldTexture = texture;
                                 setTexture(new AnimatedTexture(Enemy.BASETEX + species + " attack", 3, 500));
                                 texture.setOnFrameAction(3, delegate()
                                 {
                                     texture = oldTexture;
                                 });
                                 Game1.levelManager.delayAddEnemy(8, position);
                             }
                         }
                         break;
                    }
                case EnemyState.AttackingAll: {
                    attackCooldown += gameTime.ElapsedGameTime.Milliseconds;
                    foreach (Character target in Game1.characterManager.liveCharacters) {
                            distance = target.position - position;
                            if (hitbox.intersects(target.hitbox))
                            {
                                if (attackCooldown > 1000 / attackSpeed)
                                {
                                    target.hitBy(this);
                                    timer = 0;
                                    AnimatedTexture oldTexture = texture;
                                    setTexture(new AnimatedTexture(Enemy.BASETEX + species + " attack", 1, 250));
                                    texture.setOnFrameAction(1, delegate()
                                    {
                                        texture = oldTexture;

                                        if (currentAI == EnemyAI.Flee)
                                        {
                                            currentState = EnemyState.Blocking;
                                            if (species == "word")
                                            {
                                                setTexture(new AnimatedTexture(Enemy.BASETEX + species + " flee", 2, 150));
                                            }
                                        }
                                    });
                                    attackCooldown = 0;
                                }
                            }
                        }
                        break;
                }
                case EnemyState.Attacking:
                    {
                        if (target != null)
                        {
                            distance = target.position - position;
                            attackCooldown += gameTime.ElapsedGameTime.Milliseconds;
                            if (hitbox.intersects(target.hitbox))
                            {
                                if (attackCooldown > 1000 / attackSpeed)
                                {
                                    target.hitBy(this);
                                    timer = 0;
                                    AnimatedTexture oldTexture = texture;
                                    setTexture(new AnimatedTexture(Enemy.BASETEX + species + " attack", 1, 250));
                                    texture.setOnFrameAction(1, delegate()
                                    {
                                        texture = oldTexture;

                                        if (currentAI == EnemyAI.Flee)
                                        {
                                            currentState = EnemyState.Blocking;
                                            if (species == "word")
                                            {
                                                setTexture(new AnimatedTexture(Enemy.BASETEX + species + " flee", 2, 150));
                                            }
                                        }
                                    });
                                    attackCooldown = 0;
                                }
                            }

                            if (hitbox.intersectPercent(target.hitbox) < .25f)
                            {
                                distance.Normalize();
                                if (disableMovementTime > 0f || disableMovementDistance > 0f && currentAI != EnemyAI.NoMove)
                                {
                                    disableMovementTime -= seconds;                                    
                                    velocity -= velocity * 0.80f * seconds;
                                    disableMovementDistance -= velocity.Length() * seconds;
                                    position += velocity * seconds;
                                }
                                else
                                {
                                    if (currentAI != EnemyAI.NoMove) position = position + moveSpeed * seconds * distance; // close in on a target
                                }
                            }
                        }
                        break;
                    }
                case EnemyState.Blocking:
                    {
                        timer += gameTime.ElapsedGameTime.TotalSeconds;
                        distance = position - target.position;
                        distance.Normalize();
                        if (disableMovementTime > 0f || disableMovementDistance > 0f)
                        {
                            disableMovementTime -= seconds;
                            velocity -= velocity * 0.80f * seconds;
                            disableMovementDistance -= velocity.Length() * seconds;
                            position += velocity * seconds;
                        }
                        else
                        {
                            position = position + fleeSpeed * seconds * distance;
                        }
                        alpha -= alpharate * seconds;
                        if (alpha < 0.5f || alpha > 1f)
                            alpharate = -alpharate;
                        if (timer > fleeTime)
                        {
                            hitboxesHitBy.Clear();
                            currentState = EnemyState.Attacking;

                            if (species == "word")
                            {
                                setTexture(new AnimatedTexture(Enemy.BASETEX + species, 2, 150));
                            }
                            alpha = 1f;
                            timer = 0f;
                        }
                        break;
                    }
                default:
                    break;      
            }
            base.Update(gameTime);
        }

        public void arcThrow(float radius, Vector2 dir)
        {
            if (currentAI == EnemyAI.NoMove) return;

            Console.WriteLine("AI=" + currentAI);
            if (currentAI == EnemyAI.SwampBoss || currentAI == EnemyAI.DynamiteBoss)
                return;
            dir.Normalize();
            thrown = true;
            throwRadius = radius;
            throwAngle = 0f;
            throwCenter = position - dir * radius;

            if (dir.X > 0) throwDirection = 1;
            else
            {
                throwDirection = -1;
                throwAngle = (float)Math.PI;
            }
        }

        public void stun(int ms)
        {
            if (currentAI == EnemyAI.NoMove) return;
            stunTimer += ms;
        }

        public virtual void hitBy(Character e, double damage)
        {
            hitBy(e, damage, Vector2.Zero, 0, 0);
        }

        public virtual void hitBy(Character e, double damage, Vector2 knockbackOrigin, float knockbackStrength, float knockbackDuration)
        {
            //if (charactersHitBy.Contains(e))
            //    return;
            //charactersHitBy.Add(e);

            if (Buffer.attBuff) damage *= Buffer.ATTACKBUFFAMOUNT;
            if (e.numVampBats > 0)
            {
                damage *= 1f + (e.numVampBats / 20f);
            }
            health -= damage;
            if (e.numVampBats > 0)
            {
                e.health += damage * (e.numVampBats / 5f);

                Character buffer = Game1.characterManager.playerCharacters[1];
                if (buffer.alive)
                {
                    buffer.health += damage * (e.numVampBats / 5f) * .25f;
                    buffer.health = Math.Min(buffer.health, buffer.maxHealth);
                }

                e.health = Math.Min(e.health, e.maxHealth);
            }
            if (health <= 0)
                Game1.levelManager.killEnemy(this);
            timer = 0;
            if (currentAI == EnemyAI.Flee)
            {
                currentState = EnemyState.Blocking;
                if (species == "word")
                {
                    setTexture(new AnimatedTexture(Enemy.BASETEX + species + " flee", 2, 150));
                }
            }
            if (currentAI == EnemyAI.Neutral)
            {
                currentAI = EnemyAI.NoFlee;
                moveSpeed = 250f;
                currentState = EnemyState.Attacking;
            }
            target = e;

            knockbackStrength *= 2;
            if (currentAI != EnemyAI.NoMove)
            {
                if (knockbackOrigin != Vector2.Zero)
                {
                    Vector2 distance = position - knockbackOrigin;
                    distance.Normalize();
                    velocity = distance * knockbackStrength;
                    knockbackTime = knockbackDuration;
                   // oldAI = currentAI;
                }
            }
        }

        public void DrawDebug(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Game1.pixel, new Rectangle(
                (int)position.X - 30, (int)(position.Y + 0.5 * hitbox.height) + 5, 60, 20), Color.DarkGray);
            spriteBatch.Draw(Game1.pixel, new Rectangle((int)position.X - 30 + 1, (int)(position.Y + 0.5 * hitbox.height) + 5 + 1,
                (int)((health / maxHealth) * (60 - 2)), 20 - 2), Color.Red);
            base.DrawDebug(spriteBatch);
        }
    }
}
