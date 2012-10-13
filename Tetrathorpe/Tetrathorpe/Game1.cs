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
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public static GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        public static SpriteFont verdanaFont;
        public static SpriteFont verdanaBigFont;

        public static Viewport viewport;
        public static CharacterManager characterManager;
        public static EntityManager managedEntities = new EntityManager();
        public static LevelManager levelManager = new LevelManager();
        public static Vector2 screenSize = new Vector2(1024, 768);
        public static Texture2D pixel;
        public static bool gameOver = false, wonLevel = false, inMenu = true;
        public Menu menu;
        public int inputLag = 120, elapsedDelay = 150;

        public static ContentManager musicContent;
        public static Random rand = new Random();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            menu = new Menu(this);
            Content.RootDirectory = "Content"; 
            TextureManager.SetContent(Content);
            //characterManager = new CharacterManager();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            pixel = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            graphics.PreferredBackBufferWidth = (int)screenSize.X;
            graphics.PreferredBackBufferHeight = (int)screenSize.Y;

            viewport = GraphicsDevice.Viewport;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            verdanaFont = Content.Load<SpriteFont>("Verdana");
            verdanaBigFont = Content.Load<SpriteFont>("VerdanaBig");

            TextureManager.Add("diamond-petal");
            TextureManager.Add("tetrathorpelogo");
            foreach (String species in Enemy.enemyTypes)
            {
                TextureManager.Add(Enemy.BASETEX + species + " 1");
                TextureManager.Add(Enemy.BASETEX + species + " 2");
                TextureManager.Add(Enemy.BASETEX + species + " attack 1");
                try
                {
                    TextureManager.Add(Enemy.BASETEX + species + " attack 2");
                    TextureManager.Add(Enemy.BASETEX + species + " attack 3");
                }
                catch { }
            }

            TextureManager.Add(Enemy.BASETEX + "word flee 1");
            TextureManager.Add(Enemy.BASETEX + "word flee 2");

            Sound.LoadContent(Content);
            //Sound.PlaySong("Glass Cannon");

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            Sound.Update(gameTime);

            if (inMenu)
            {
                menu.Update(gameTime);
            }
            else if (gameOver || wonLevel)
            {
                if (elapsedDelay > inputLag)
                {
                    if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Start))
                    {
                        gameOver = false;
                        wonLevel = false;
                        menu.setMenuOptions(false);
                        inMenu = true;
                        elapsedDelay = 0;
                    }
                }
                else
                {
                    elapsedDelay += gameTime.ElapsedGameTime.Milliseconds;
                }
            }
            else
            {
                if (elapsedDelay > inputLag)
                {
                    if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Start))
                    {
                        inMenu = true;
                        elapsedDelay = 0;
                    }
                }
                else
                {
                    elapsedDelay += gameTime.ElapsedGameTime.Milliseconds;
                }

                characterManager.Update(gameTime);
                levelManager.Update(gameTime);
                managedEntities.Update(gameTime);
            }
            
            ControllerManager.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            if (inMenu)
            {
                spriteBatch.Begin();
                menu.Draw(gameTime);
                spriteBatch.End();
            }
            else if (gameOver)
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(verdanaBigFont, "GAME OVER", new Vector2(graphics.GraphicsDevice.Viewport.Width / 4, graphics.GraphicsDevice.Viewport.Height / 3), Color.Red);
                spriteBatch.DrawString(verdanaFont, "Press Start to go to the menu.", new Vector2(graphics.GraphicsDevice.Viewport.Width / 4 + 40, graphics.GraphicsDevice.Viewport.Height / 2), Color.Red);
                spriteBatch.End();
            }
            else if (wonLevel)
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(verdanaBigFont, "YOU'RE WINNER", new Vector2(graphics.GraphicsDevice.Viewport.Width / 4 - 50, graphics.GraphicsDevice.Viewport.Height / 3), Color.Black);
                spriteBatch.DrawString(verdanaFont, "Press Start to go to the menu.", new Vector2(graphics.GraphicsDevice.Viewport.Width / 4 + 40, graphics.GraphicsDevice.Viewport.Height / 2), Color.Black);
                spriteBatch.End();
            }
            else
            {
                // Draw on game area
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullCounterClockwise,
                    null, levelManager.getViewMatrix());
                levelManager.Draw(spriteBatch);
                characterManager.Draw(spriteBatch);
                managedEntities.Draw(spriteBatch);
#if DEBUG
                levelManager.DrawDebug(spriteBatch);
                characterManager.DrawDebug(spriteBatch);
                managedEntities.DrawDebug(spriteBatch);
#endif
                spriteBatch.End();


                // Draw on HUD/UI area

                spriteBatch.Begin();
                characterManager.DrawHud(spriteBatch);
#if DEBUG
                levelManager.debug(spriteBatch);
#endif

                spriteBatch.End();
            }           

            base.Draw(gameTime);
        }
    }
}
