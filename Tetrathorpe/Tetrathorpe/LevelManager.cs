using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using System.IO;

namespace Tetrathorpe
{
    public enum LevelMode
    {
        Time, Distance
    }

    public class LevelManager
    {
        public Game1 game;

        public static float SCREENWIDTH_MIN = 800;
        public static float SCREENWIDTH_MAX = 1333;
        public float screenWidth;
        public static float SCREENHEIGHT_MIN = 480;
        public static float SCREENHEIGHT_MAX = 800;
        public float screenHeight;
        public float leftBound;
        public float rightBound;

        public Vector3 position; // middle of the screen
        public Vector3 desiredPos;
        public readonly float speed = 100f;
        //public static float acceleration = 150f;
        //public static float decelerationFactor = 0.95f;
        public readonly bool ScrollUsingVelocity = true;
        public List<string> screens;
        public List<float> screenWidths;
        public bool scrolling;
        public float characterDistance;

        public List<LevelMode> levelEvents;

        public List<int> delayDistances; // distances traveled at which to spawn enemies
        public List<int> distanceTypes; // types of enemies to spawn at those distances
        public List<Vector2> distancePos; // position the enemy appears
       
        public List<double> delayTimes; // times at which to spawn enemies
        public List<int> timeTypes; // type of enemy to appear at the specified times
        public List<Vector2> timePos; // positions for them to appear on the screen

        public double elapsedTime;
        public int elapsedDistance;

        public static float SCROLL_BUFFER_MIN = 600;
        public float scrollBuffer = SCROLL_BUFFER_MIN;
        public static bool ScrollUsingBufferZone = true;
        public static bool ScrollUsingCenterOfAllCharacters = false;

        public List<Enemy> enemies = new List<Enemy>(); // List of enemies in the game world at the moment
        public List<Enemy> enemiesToKill = new List<Enemy>(); // List of enemies to remove from the world
		public Queue<Enemy> enemiesToAdd = new Queue<Enemy>();        
		public List<Enemy> enemiesToAddImmediate = new List<Enemy>(); // List of enemies to add from the world
        public List<Hitbox> hitboxesToDeactivate = new List<Hitbox>(); // List of hitboxes to deactivate for the next frame of collision

        public LevelManager()
        {
            screenWidth = SCREENWIDTH_MIN;
            screenHeight = MathHelper.Clamp(screenWidth / Game1.viewport.AspectRatio, SCREENHEIGHT_MIN, SCREENHEIGHT_MAX);
            screens = new List<string>();
            screenWidths = new List<float>();
            scrolling = true;

            levelEvents = new List<LevelMode>();
            delayDistances = new List<int>();
            distanceTypes = new List<int>();
            distancePos = new List<Vector2>();

            delayTimes = new List<double>();
            timeTypes = new List<int>();
            timePos = new List<Vector2>();

            elapsedTime = 0;
            elapsedDistance = 0;
        }

        public void loadFromFile(string name)
        {
            elapsedDistance = 0;
            elapsedTime = 0;
            screens.Clear();
            screenWidths.Clear();
            enemies.Clear();
            enemiesToAdd.Clear();
            enemiesToAddImmediate.Clear();
            enemiesToKill.Clear();
            hitboxesToDeactivate.Clear();
            levelEvents.Clear();
            delayDistances.Clear();
            distanceTypes.Clear();
            distancePos.Clear();
            delayTimes.Clear();
            timeTypes.Clear();
            timePos.Clear();

            StreamReader file = new StreamReader("Content/Levels/" + name + ".txt");
            String[] line;
            String[] firstLine = file.ReadLine().Split();
            String filePrefix = firstLine[0];


            while (!file.EndOfStream)
            {
                line = file.ReadLine().Split();
                if (line[0][0].Equals('#'))
                {
                    continue;
                }
                if (line[0].Equals("D"))
                {
                    levelEvents.Add(LevelMode.Distance);
                    delayDistances.Add(int.Parse(line[1]));
                    distanceTypes.Add(int.Parse(line[2]));
                    distancePos.Add(new Vector2(int.Parse(line[3]), int.Parse(line[4], System.Globalization.NumberStyles.AllowLeadingSign)));
                }
                if (line[0].Equals("T"))
                {
                    levelEvents.Add(LevelMode.Time);
                    delayTimes.Add(float.Parse(line[1]));
                    timeTypes.Add(int.Parse(line[2]));
                    timePos.Add(new Vector2(int.Parse(line[3]), int.Parse(line[4], System.Globalization.NumberStyles.AllowLeadingSign)));
                }
            }
            int numberOfTiles = 1; // represents the number of tiles available for each background; hardcoding because of time constraints.
            switch (filePrefix)
            {
                case "swamp": numberOfTiles = 2; break;
                case "library": numberOfTiles = 3; break;
                case "sky": numberOfTiles = 3; break;
                case "cave": numberOfTiles = 3; break;
            }
            if (delayDistances.Count > 0)
            {
                double numberOfIntermediateScreens = 1 + (delayDistances[delayDistances.Count - 1]) / 582.0;
                if (numberOfIntermediateScreens < 0)
                {
                    addScreen(filePrefix + " block 1");
                    addScreen(filePrefix + " block 2");
                }
                else
                {
                    addScreen(filePrefix + " block 1");
                    for (int i = 0; i < Math.Ceiling(numberOfIntermediateScreens); i++) { addScreen(filePrefix + " block " + (2+(i%numberOfTiles))); }
                }
            }
            else
            {
                addScreen(filePrefix + " block 1");
                addScreen(filePrefix + " block 2");
            }
        }

        public void addScreen(string texture)
        {
            TextureManager.Add("Backgrounds/" + texture);
            screens.Add("Backgrounds/" + texture);
            screenWidths.Add(0);
            float firstScreenWidth = 0;
            float totalLevelWidth = 0;
            float boundOffset = (SCREENWIDTH_MIN - SCROLL_BUFFER_MIN) / 2; // prevent seeing blue background at edges
            for (int i = 0; i < screens.Count; i++)
            {
                if (i == 0)
                {
                    firstScreenWidth = TextureManager.Get(screens[i]).Width;
                    leftBound = boundOffset;
                }
                screenWidths[i] = TextureManager.Get(screens[i]).Width;
                totalLevelWidth += TextureManager.Get(screens[i]).Width;
                Console.WriteLine(screenWidths[i]);
            }
            rightBound = totalLevelWidth - boundOffset;
        }
        public void delayAddEnemy(int type, Vector2 position)
        {
            Enemy e = new Enemy(type);
            e.position = position;
            enemiesToAdd.Enqueue(e);
        }
        public void addEnemy(int type, float xposition, float yposition)
        {
            Enemy e;
            if (type == 98)
                e = new SwampBoss();
            if (type == 99)
                e = new DynaBoss();
            else
                e = new Enemy(type);
            e.position = new Vector2(xposition, yposition);
            enemies.Add(e);
        }
        public void addEnemy(int type, Vector2 position)
        {
            Enemy e;
            if (type == 98)
                e = new SwampBoss();
            else if (type == 99)
                e = new DynaBoss();
            else
                e = new Enemy(type);
            e.position = position;
            enemies.Add(e);
        }
        public void clear()
        {
            screens.Clear();
            screenWidths.Clear();
        }

        public Matrix getTranslation()
        {
            return Matrix.CreateTranslation(new Vector3(-position.X, position.Y, 0));
        }

        public Matrix getScale()
        {
            return Matrix.CreateScale(new Vector3(Game1.viewport.Width / screenWidth, Game1.viewport.Height / screenHeight, 1));
        }

        public Matrix getViewMatrix()
        {
            return getTranslation() * getScale();
        }

        public bool attemptScroll(Entity entity, Vector2 offset)
        {
            Vector2 topEdge = (entity.getTop() + offset);
            Vector2 bottomEdge = (entity.getBottom() + offset);
            Vector2 leftEdge = entity.getLeft() + offset;
            Vector2 rightEdge = entity.getRight() + offset;
            if (!(topEdge.Y > -screenHeight / 2 && bottomEdge.Y < screenHeight / 2))
                return false;
            int i = getCurrentScreen(entity, offset);
            Texture2D currentScreen = TextureManager.Get(screens[i]);
            if (topEdge.Y < -currentScreen.Height / 2 || bottomEdge.Y > currentScreen.Height / 2)
                return false;
            if (!scrolling)
            {
                if (leftEdge.X < position.X || rightEdge.X > (position.X + screenWidth))
                {
                    return false;
                }
            }
            else
            {
                if (leftEdge.X < position.X)
                {
                    if ((position.X + screenWidth) - leftEdge.X > SCREENWIDTH_MAX)
                        return false;
                }
                else if (rightEdge.X > (position.X + screenWidth))
                {
                    if (rightEdge.X - position.X > SCREENWIDTH_MAX)
                        return false;
                }
            }
            if (leftEdge.X < leftBound || rightEdge.X > rightBound)
                return false;
            return true;
        }

        public bool isInView(Entity entity, Vector2 offset)
        {
            return ((entity.getLeft() + offset).X >= position.X && (entity.getRight() + offset).X <= (position.X + screenWidth)
                && (entity.getTop() + offset).Y >= position.Y && (entity.getBottom() + offset).Y <= (position.Y + screenHeight));
        }

        public int getCurrentScreen(Entity entity, Vector2 offset)
        {
            float xPos = entity.position.X + offset.X;
            for (int screen = 0; screen < screenWidths.Count; screen++)
            {
                xPos -= screenWidths[screen];
                if (xPos < 0)
                    return screen;
            }
            return screenWidths.Count-1; //error, return last screen
        }

        public void Update(GameTime gameTime)
        {
            if (enemies.Count == 0 && delayDistances.Count == 0 && delayTimes.Count == 0)
            {
                Game1.wonLevel = true;
            }
            elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;
            float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float minPosition = float.MaxValue;
            float maxPosition = float.MinValue;
            float averagePosition = 0f;
            foreach (Character c in Game1.characterManager.playerCharacters)
            {
                if (c.alive)
                {
                    if (c.getLeft().X < minPosition) minPosition = c.getLeft().X;
                    if (c.getRight().X > maxPosition) maxPosition = c.getRight().X;
                    averagePosition += c.position.X;
                }
            }
            characterDistance = maxPosition - minPosition;
           // averagePosition -= minPosition * Game1.characterManager.playerCharacters.Count;
            averagePosition = averagePosition / Game1.characterManager.playerCharacters.Count;
            //averagePosition += minPosition;

            if (scrolling)
            {
                screenWidth = MathHelper.Clamp(characterDistance, SCREENWIDTH_MIN, SCREENWIDTH_MAX);
                screenHeight = MathHelper.Clamp(screenWidth / Game1.viewport.AspectRatio, SCREENHEIGHT_MIN, SCREENHEIGHT_MAX);
                if (ScrollUsingBufferZone)
                {
                    scrollBuffer = MathHelper.Clamp(characterDistance, SCROLL_BUFFER_MIN, screenWidth);
                    if (maxPosition > position.X + screenWidth / 2 + scrollBuffer / 2)
                    {
                        desiredPos = new Vector3(maxPosition - (screenWidth / 2 + scrollBuffer / 2), screenHeight / 2, 0);
                    }
                    else if (minPosition < position.X + screenWidth / 2 - scrollBuffer / 2)
                    {
                        desiredPos = new Vector3(minPosition - (screenWidth / 2 - scrollBuffer / 2), screenHeight / 2, 0);
                    }
                    desiredPos.Y = screenHeight / 2;
                }
                else if (ScrollUsingCenterOfAllCharacters)
                    desiredPos = new Vector3(averagePosition - screenWidth / 2, screenHeight / 2, 0);
                else
                    desiredPos = new Vector3(minPosition, screenHeight / 2, 0);

                if (ScrollUsingVelocity)
                {
                    position.Y = desiredPos.Y;
                    if (desiredPos.X > position.X)
                    {
                        //speed += acceleration * seconds;
                        position.X += speed * seconds;
                        //if (position.X >= desiredPos.X)
                        //{
                        //    //speed = 0;
                        //    //acceleration = 0;
                        //    position = desiredPos;
                        //}
                    }
                    else if (desiredPos.X < position.X)
                    {
                        //speed += acceleration * seconds;
                        position.X -= speed * seconds;
                        //if (position.X <= desiredPos.X)
                        //{
                        //    //speed = 0;
                        //    //acceleration = 0;
                        //    position = desiredPos;
                        //}
                    }
                    //else
                    //{
                    //    //speed *= decelerationFactor;
                    //    position = desiredPos;
                    //}
                    position = desiredPos;
                }
                else position = desiredPos;
            }
            elapsedDistance = (int)position.X;
            spawnEnemies();
            if (enemies.Count > 0) scrolling = false;
            else scrolling = true;

            foreach (Enemy e in enemies)
            {
                if (e.alive)
                    e.Update(gameTime);
            }
            foreach (Enemy e in enemiesToKill)
            {
                e.alive = false;
                enemies.Remove(e);
                e.hitbox = null;
            }
            enemiesToKill.Clear();
            foreach (Enemy e in enemiesToAddImmediate)
            {
                enemies.Add(e);
            }
            enemiesToAddImmediate.Clear();
            foreach (Hitbox h in hitboxesToDeactivate)
            {
                h.active = false;
            }
            hitboxesToDeactivate.Clear();
			while (enemiesToAdd.Count > 0) enemies.Add(enemiesToAdd.Dequeue());
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            float offset = 0;
            for (int i = 0; i < screens.Count; i++)
            {
                if (i != 0)
                    offset += screenWidths[i - 1] / 2; //don't add any extra for first drawing
                offset += screenWidths[i] / 2;
                Utility.Draw(spriteBatch, screens[i], new Vector2(offset, 0), Color.White, 1, 1, 0, SpriteEffects.None);
            }

            foreach (Enemy e in enemies)
            {
                if (e.alive)
                    e.Draw(spriteBatch);
            }
        }

        public void DrawDebug(SpriteBatch spriteBatch)
        {
            foreach (Enemy e in enemies)
            {
                if (e.alive)
                    e.DrawDebug(spriteBatch);
            }
        }

        public void spawnEnemies()
        {
            Vector2 offset = new Vector2(position.X /*- screenWidth / 2*/, 0);
            while (delayTimes.Count > 0 && elapsedTime >= delayTimes[0])
            {
                addEnemy(timeTypes[0], timePos[0] + offset);
                delayTimes.RemoveAt(0);
                timeTypes.RemoveAt(0);
                timePos.RemoveAt(0);
            }
            while (delayDistances.Count > 0 && elapsedDistance >= delayDistances[0])
            {
                addEnemy(distanceTypes[0], distancePos[0] + offset);
                delayDistances.RemoveAt(0);
                distanceTypes.RemoveAt(0);
                distancePos.RemoveAt(0);
            }
        }

        public void killEnemy(Enemy e)
        {
            enemiesToKill.Add(e);
        }

        public void debug(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Game1.verdanaFont, "width = " + screenWidth, new Vector2(150, 5), Color.White);
            spriteBatch.DrawString(Game1.verdanaFont, "height = " + screenHeight, new Vector2(150, 25), Color.White);
            spriteBatch.DrawString(Game1.verdanaFont, "(" + position.X.ToString("000.00") + ", " + position.Y.ToString("000.00") + ")", new Vector2(150, 45), Color.White);
            spriteBatch.DrawString(Game1.verdanaFont, "scrolling=" + scrolling, new Vector2(150, 65), Color.White);
            spriteBatch.DrawString(Game1.verdanaFont, "distance delays:" + delayDistances.Count, new Vector2(150, 90), Color.White);
            spriteBatch.DrawString(Game1.verdanaFont, "elapsed distance:" + elapsedDistance, new Vector2(150, 110), Color.White);

            int i = 0;
            foreach (Character e in Game1.characterManager.playerCharacters)
            {
                i++;
                spriteBatch.DrawString(Game1.verdanaFont, "P(" + e.position.X.ToString("000") + ", " + e.position.Y.ToString("000") + ")", new Vector2(350, i * 20 + 5), Color.White);
                spriteBatch.DrawString(Game1.verdanaFont, "C(" + ((e is Turtle) ? ((Turtle)e).charging : false) + ")", new Vector2(500, i * 20 + 5), Color.White);
            }
        }
    }
}
