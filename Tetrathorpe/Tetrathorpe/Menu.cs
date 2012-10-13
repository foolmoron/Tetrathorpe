using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Tetrathorpe
{
    public class Menu
    {
        public Game1 game;
        public int selectedOption = 0;
        public List<string> menuOptions = new List<string>();
        public bool inGame;

        public Menu(Game1 game)
        {
            this.game = game;
            setMenuOptions(false);
        }

        public void Update(GameTime gameTime)
        {
            GamePadState state = GamePad.GetState(PlayerIndex.One);
            if (game.elapsedDelay > game.inputLag)
            {
                if (state.ThumbSticks.Left.Y < 0 || state.IsButtonDown(Buttons.DPadDown))
                {
                    selectedOption = (selectedOption + 1) % menuOptions.Count();
                    game.elapsedDelay = 0;
                }
                if (state.ThumbSticks.Left.Y > 0 || state.IsButtonDown(Buttons.DPadUp))
                {
                    selectedOption = (selectedOption + menuOptions.Count() - 1) % menuOptions.Count();
                    game.elapsedDelay = 0;
                }
                if (state.IsButtonDown(Buttons.A))
                {
                    parseMenuOptions(menuOptions[selectedOption]);
                    game.elapsedDelay = 0;
                }
                if (state.IsButtonDown(Buttons.Start) && inGame)
                {
                    Game1.inMenu = false;
                    game.elapsedDelay = 0;
                }
            }
            else
            {
                game.elapsedDelay += gameTime.ElapsedGameTime.Milliseconds;
            }
        }

        public void Draw(GameTime gameTime)
        {
            game.GraphicsDevice.Clear(Color.White);
            game.spriteBatch.Draw(TextureManager.Get("tetrathorpelogo"), new Vector2(5, 40), Color.White);
            game.spriteBatch.DrawString(Game1.verdanaBigFont, "Tetrathorpe", new Vector2(game.GraphicsDevice.Viewport.Width / 2, 25), Color.Black);
            for (int i = 0; i < menuOptions.Count; i++)
            {
                if (i == selectedOption)
                    game.spriteBatch.DrawString(Game1.verdanaFont, menuOptions[i], new Vector2(3 * game.GraphicsDevice.Viewport.Width / 5 + 15, i * 25 + 100), Color.Red);
                else
                    game.spriteBatch.DrawString(Game1.verdanaFont, menuOptions[i], new Vector2(3 * game.GraphicsDevice.Viewport.Width / 5, i * 25 + 100), Color.Black);
            }
        }

        public void setMenuOptions(bool _inGame)
        {
            inGame = _inGame;
            menuOptions.Clear();
            selectedOption = 0;
            if (inGame)
            {
                menuOptions.Add("Resume Game");
                menuOptions.Add("Return to Level Select");
                menuOptions.Add("Quit Game");
            }
            else
            {
                StreamReader levelCount = new StreamReader("Content/Levels/levelcount.txt");
                int levels = Int32.Parse(levelCount.ReadLine());
                string op;
                for (int i = 1; i <= levels; i++)
                {
                    op = "Level " + i;
                    menuOptions.Add(op);
                }
                menuOptions.Add("Quit Game");
            }
        }

        public void parseMenuOptions(string selection)
        {
            string[] split = selection.Split();
            switch (split[0])
            {
                case "Level":
                    Game1.inMenu = false;
                    setMenuOptions(true);
                    Game1.levelManager.loadFromFile("level" + split[1]);
                    Game1.characterManager = new CharacterManager();
                    break;
                case "Resume":
                    Game1.inMenu = false;
                    break;
                case "Return":
                    setMenuOptions(false);
                    break;
                case "Quit":
                    game.Exit();
                    break;
            }
        }
    }
}
