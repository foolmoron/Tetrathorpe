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
    public class CharacterManager : Controllable
    {
        public List<Character> playerCharacters = new List<Character>();
        public List<Character> liveCharacters = new List<Character>();
        public List<Character> charactersToKill = new List<Character>();
        public static Character singleControlled;

        public int healthBarHeight = 20;
        public int healthBarOffset = 5;
        public int healthBarWidth = 100;
        public int healthBarBorder = 2;
        public float hudHorizontalOffset = 80f;
        public float hudVerticalOffset = 60f;
        public float portraitScale = 0.6f;

        public static bool Multiplayer = true;

        public CharacterManager()
        {
            playerCharacters.Clear();
            liveCharacters.Clear();
            charactersToKill.Clear();

            if (Multiplayer)
            {
                playerCharacters.Add(new Turtle());
                playerCharacters[0].position = new Vector2(180f, -40f);
            }
            else
            {
                playerCharacters.Add(new Turtle());
                playerCharacters[0].position = new Vector2(180f, -40f);
                playerCharacters.Add(new AOE());
                playerCharacters[1].position = new Vector2(260f, -40f);
                playerCharacters.Add(new Buffer());
                playerCharacters[2].position = new Vector2(180f, 40f);
                playerCharacters.Add(new GlassCannon());
                playerCharacters[3].position = new Vector2(260f, 40f);
            }

            foreach (Character c in playerCharacters)
            {
                c.setCharacterState(Character.CharacterState.Standing);
                liveCharacters.Add(c);
            }

            setSingleControlled(playerCharacters[0]);

            ControllerManager.controllables.Add(this);
        }

        public void addPlayer()
        {
            if (playerCharacters.Count == 1) { playerCharacters.Add(new Buffer()); }
            else if (playerCharacters.Count == 2) { playerCharacters.Add(new AOE()); }
            else if (playerCharacters.Count == 3) { playerCharacters.Add(new GlassCannon()); }

            playerCharacters[playerCharacters.Count - 1].setCharacterState(Character.CharacterState.Standing);
            playerCharacters[playerCharacters.Count - 1].position = liveCharacters[0].position;
            liveCharacters.Add(playerCharacters[playerCharacters.Count - 1]);
        }

        public void LoadContent()
        {
            
        }

        public void setSingleControlled(Character c)
        {
            singleControlled = c;
        }

        public void Update(GameTime gameTime)
        {
            foreach (Character c in charactersToKill)
            {
                liveCharacters.Remove(c);
                if (liveCharacters.Count == 0)
                {
                    Game1.gameOver = true; //End game
                }
            }
            if (!Multiplayer)
            {       
                GamePadState gamepad = GamePad.GetState(PlayerIndex.One);
                Vector2 leftStick = gamepad.ThumbSticks.Left;
                Vector2 rightStick = gamepad.ThumbSticks.Right;

                if (gamepad.Buttons.LeftShoulder == ButtonState.Pressed && playerCharacters[0].alive)
                {
                    if (!playerCharacters[0].isControlled()) playerCharacters[0].initiateControl();
                }
                else
                {
                    if (playerCharacters[0].isControlled()) { playerCharacters[0].releaseControl(); }
                }

                if (gamepad.Buttons.RightShoulder == ButtonState.Pressed && playerCharacters[1].alive)
                {
                    if (!playerCharacters[1].isControlled()) playerCharacters[1].initiateControl();
                }
                else
                {
                    if (playerCharacters[1].isControlled()) { playerCharacters[1].releaseControl(); }
                }

                if (gamepad.Triggers.Left > .7 && playerCharacters[2].alive)
                {
                    if (!playerCharacters[2].isControlled()) playerCharacters[2].initiateControl();
                }
                else
                {
                    if (playerCharacters[2].isControlled()) { playerCharacters[2].releaseControl(); }
                }

                if (gamepad.Triggers.Right > .7 && playerCharacters[3].alive)
                {
                    if (!playerCharacters[3].isControlled()) playerCharacters[3].initiateControl();
                }
                else
                {
                    if (playerCharacters[3].isControlled()) { playerCharacters[3].releaseControl(); }
                }

                Vector2 movement = new Vector2(leftStick.X, -leftStick.Y);
                Vector2 rightMovement = new Vector2(rightStick.X, -rightStick.Y);
                if (movement.Length() > 0.001f)
                {
                    applyGlobalCharacterAction(delegate(Character c)
                    {
                        c.attemptMove(Vector2.Zero);
                    });
                    applyCharacterAction(delegate(Character c)
                    {
                        c.attemptMove(movement);
                    });
                }
                else
                {
                    applyGlobalCharacterAction(delegate(Character c)
                    {
                        c.attemptMove(rightMovement);
                    });
                }
            }
            else
            {
                for (int i = 0; i < playerCharacters.Count; i++)
                {
                    GamePadState gamepad = GamePad.GetState(ControllerManager.indexFromNumber(i));
                    Vector2 leftStick = gamepad.ThumbSticks.Left;
                    Vector2 movement = new Vector2(leftStick.X, -leftStick.Y);
                    playerCharacters[i].attemptMove(movement);
                }
            }


            applyGlobalCharacterAction(delegate(Character c)
            {
                c.Update(gameTime);
            });
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Character c in playerCharacters)
            {
                c.Draw(spriteBatch);
            }
        }
        public void DrawDebug(SpriteBatch spriteBatch)
        {
            foreach (Character c in playerCharacters)
            {
                c.DrawDebug(spriteBatch);
            }
            /*applyGlobalCharacterAction(delegate(Character c)
            {
                c.DrawDebug(spriteBatch);
            });*/
        }
        public void DrawHud(SpriteBatch spriteBatch)
        {
            /*int healthBarHeight = 20;
            int healthBarOffset = 5;
            int healthBarWidth = 100;
            int healthBarBorder = 2;*/
            
            Vector2 p = new Vector2(hudHorizontalOffset, hudVerticalOffset);
            Utility.Draw(spriteBatch, playerCharacters[0].hudTexture, p, Color.White, portraitScale, 1.0f, 0.0f, SpriteEffects.None);
            
            // Draws a box underneath the character's portrait for the health bar.
            spriteBatch.Draw(Game1.pixel, new Rectangle(
                (int)p.X - healthBarWidth/2,
                (int)(p.Y + 0.5*portraitScale*playerCharacters[0].hudTexture.Height) + healthBarOffset,
                healthBarWidth, healthBarHeight), Color.DarkGray);
            
            // This is a doozy of a line. Draws a rectangle positioned in the same place as the previous one, but with a width 
            // corresponding to the health of the character. Also, there's a border.
            spriteBatch.Draw(Game1.pixel, new Rectangle(
                (int)p.X - healthBarWidth / 2 + healthBarBorder,
                (int)(p.Y + 0.5 * portraitScale * playerCharacters[0].hudTexture.Height) + healthBarOffset + healthBarBorder,
                (int)((playerCharacters[0].health/playerCharacters[0].maxHealth) * (healthBarWidth - 2*healthBarBorder)), 
                healthBarHeight - 2*healthBarBorder), Color.Red);
            if (Buffer.attBuff) Utility.Draw(spriteBatch, Buffer.BASETEX + "buffer-attack-buff", p + new Vector2(40, -30), Color.White, .35f, 1f, 0f, SpriteEffects.None);

            if (playerCharacters.Count > 1)
            {
                p = new Vector2(Game1.viewport.Width - hudHorizontalOffset, hudVerticalOffset);
                Utility.Draw(spriteBatch, playerCharacters[1].hudTexture, p, Color.White, portraitScale, 1.0f, 0.0f, SpriteEffects.None);
                spriteBatch.Draw(Game1.pixel, new Rectangle(
                    (int)p.X - healthBarWidth / 2,
                    (int)(p.Y + 0.5 * portraitScale * playerCharacters[1].hudTexture.Height) + healthBarOffset,
                    healthBarWidth, healthBarHeight), Color.DarkGray);
                spriteBatch.Draw(Game1.pixel, new Rectangle(
                    (int)p.X - healthBarWidth / 2 + healthBarBorder,
                    (int)(p.Y + 0.5 * portraitScale * playerCharacters[1].hudTexture.Height) + healthBarOffset + healthBarBorder,
                    (int)((playerCharacters[1].health / playerCharacters[1].maxHealth) * (healthBarWidth - 2 * healthBarBorder)),
                    healthBarHeight - 2 * healthBarBorder), Color.Red);

                if (Buffer.attBuff) Utility.Draw(spriteBatch, Buffer.BASETEX + "buffer-attack-buff", p + new Vector2(40, -30), Color.White, .35f, 1f, 0f, SpriteEffects.None);
            }

            if (playerCharacters.Count > 2)
            {
                p = new Vector2(hudHorizontalOffset, Game1.viewport.Height - hudVerticalOffset);
                Utility.Draw(spriteBatch, playerCharacters[2].hudTexture, p, Color.White, portraitScale, 1.0f, 0.0f, SpriteEffects.None);
                spriteBatch.Draw(Game1.pixel, new Rectangle(
                    (int)p.X - healthBarWidth / 2,
                    (int)(p.Y - 0.5 * portraitScale * playerCharacters[2].hudTexture.Height) - healthBarOffset - healthBarHeight,
                    healthBarWidth, healthBarHeight), Color.DarkGray);
                spriteBatch.Draw(Game1.pixel, new Rectangle(
                    (int)p.X - healthBarWidth / 2 + healthBarBorder,
                    (int)(p.Y - 0.5 * portraitScale * playerCharacters[2].hudTexture.Height) - healthBarOffset - healthBarHeight + healthBarBorder,
                    (int)((playerCharacters[2].health / playerCharacters[2].maxHealth) * (healthBarWidth - 2 * healthBarBorder)),
                    healthBarHeight - 2 * healthBarBorder), Color.Red);

                //if (Buffer.attBuff) Utility.Draw(spriteBatch, Buffer.BASETEX + "buffer-attack-buff", p + new Vector2(40, -30), Color.White, .35f, 1f, 0f, SpriteEffects.None);
            }

            if (playerCharacters.Count > 3)
            {
                p = new Vector2(Game1.viewport.Width - hudHorizontalOffset, Game1.viewport.Height - hudVerticalOffset);
                Utility.Draw(spriteBatch, playerCharacters[3].hudTexture, p, Color.White, portraitScale, 1.0f, 0.0f, SpriteEffects.None);
                spriteBatch.Draw(Game1.pixel, new Rectangle(
                    (int)p.X - healthBarWidth / 2,
                    (int)(p.Y - 0.5 * portraitScale * playerCharacters[3].hudTexture.Height) - healthBarOffset - healthBarHeight,
                    healthBarWidth, healthBarHeight), Color.DarkGray);
                spriteBatch.Draw(Game1.pixel, new Rectangle(
                    (int)p.X - healthBarWidth / 2 + healthBarBorder,
                    (int)(p.Y - 0.5 * portraitScale * playerCharacters[3].hudTexture.Height) - healthBarOffset - healthBarHeight + healthBarBorder,
                    (int)((playerCharacters[3].health / playerCharacters[3].maxHealth) * (healthBarWidth - 2 * healthBarBorder)),
                    healthBarHeight - 2 * healthBarBorder), Color.Red);

                if (Buffer.attBuff) Utility.Draw(spriteBatch, Buffer.BASETEX + "buffer-attack-buff", p + new Vector2(40, -30), Color.White, .35f, 1f, 0f, SpriteEffects.None);
            }
        }
        public void applyCharacterAction(Action<Character> action)
        {
            foreach (Character c in playerCharacters)
            {
                if (c.isControlled() || singleControlled == c)
                {
                    action(c);
                }
            }
        }

        public void applyGlobalCharacterAction(Action<Character> action) {
            foreach (Character c in playerCharacters) {
                if (c.alive)
                    action(c);
            }
        }

        public void aButton(int index)
        {
            if (Multiplayer)
            {
                if (playerCharacters.Count - 1 >= index) playerCharacters[index].lightAttack();
                else
                {
                    if (index == playerCharacters.Count) addPlayer();
                }
            }
            else
            {
                applyCharacterAction(delegate(Character c)
                {
                    c.lightAttack();
                });
            }
        }

        public void bButton(int index)
        {
            if (Multiplayer)
            {
                if (playerCharacters.Count - 1 >= index) playerCharacters[index].heavyAttack();
            }
            else
            {
                applyCharacterAction(delegate(Character c)
                {
                    c.heavyAttack();
                });
            }
        }

        public void xButton(int index)
        {
            if (Multiplayer)
            {
                if (playerCharacters.Count - 1 >= index) playerCharacters[index].specialAttack();
            }
            else
            {
                applyCharacterAction(delegate(Character c)
                {
                    c.specialAttack();
                });
            }
        }

        public void yButton(int index)
        {
            if (Multiplayer)
            {
                if (playerCharacters.Count - 1 >= index) playerCharacters[index].block();
            }
            else
            {
                applyCharacterAction(delegate(Character c)
                {
                    c.block();
                });
            }
        }

        public void killCharacter(Character c)
        {
            c.alive = false;
            charactersToKill.Add(c);
            //c.hitbox = null;
            if (singleControlled == c)
                singleControlled = null;
        }

        public Character getClosestCharacter(Vector2 location)
        {
            float distance = 1000f;
            Character closest = liveCharacters[0];
            foreach (Character c in liveCharacters)
            {
                Vector2 sep = c.position - location;
                if (sep.Length() < distance)
                {
                    distance = sep.Length();
                    closest = c;
                }
            }
            return closest;
        }

        public Character getRandomLiveCharacter()
        {
            if (liveCharacters.Count > 0)
                return liveCharacters[Game1.rand.Next(0, liveCharacters.Count)];
            return null;
        }

        public Character getClosestLiveCharacter(Vector2 v)
        {
            float min = float.MaxValue;
            Character close = null;
            foreach (Character c in liveCharacters)
            {
                float dist = Vector2.DistanceSquared(v, c.position);
                if (dist < min) min = dist;
                close = c;
            }
            return close;
        }
    }
}
