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
    static class Utility
    {
        public static Color CA(Color c, float a)
        {
            return c * a;
        }

        public static void Draw(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Color color, float scale, float alpha, float rotation, SpriteEffects flip)
        {
            if (texture == null) return;
            spriteBatch.Draw(texture, position, new Rectangle(0, 0, texture.Width, texture.Height), Utility.CA(color, alpha), rotation, new Vector2(texture.Width / 2f, texture.Height / 2f), scale, flip, 0f);
        }

        public static void Draw(SpriteBatch spriteBatch, string tex, Vector2 position, Color color, float scale, float alpha, float rotation, SpriteEffects flip)
        {
            Texture2D texture = TextureManager.Get(tex);
            spriteBatch.Draw(texture, position, new Rectangle(0, 0, texture.Width, texture.Height), Utility.CA(color, alpha), rotation, new Vector2(texture.Width / 2f, texture.Height / 2f), scale, flip, 0f);
        }

        public static void Draw(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Color color, float scale, float alpha, float rotation, Vector2 origin, SpriteEffects flip)
        {
            if (texture == null) return;
            spriteBatch.Draw(texture, position, new Rectangle(0, 0, texture.Width, texture.Height), Utility.CA(color, alpha), rotation, origin, scale, flip, 0f);
        }

        public static void Draw(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Color color, Vector2 scale, float alpha, float rotation, Vector2 origin, SpriteEffects flip)
        {
            if (texture == null) return;
            spriteBatch.Draw(texture, position, new Rectangle(0, 0, texture.Width, texture.Height), Utility.CA(color, alpha), rotation, origin, scale, flip, 0f);
        }

        public static void DrawText(SpriteBatch spriteBatch, string text, Vector2 position, Color color, float scale, float alpha, float rotation)
        {
            //spriteBatch.DrawString(Game1.spriteFont, text, position, CA(color, alpha), rotation, Game1.spriteFont.MeasureString(text) / 2f, scale, SpriteEffects.None, 0f);
        }       
    }
}
