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
    static class ControllerManager
    {
        public static List<Controllable> controllables = new List<Controllable>();
        static bool[] aDown = new bool[4];
        static bool[] bDown = new bool[4];
        static bool[] xDown = new bool[4];
        static bool[] yDown = new bool[4];
        static GamePadState[] prevState = new GamePadState[4];

        public static void Initialize()
        {
            
        }

        public static PlayerIndex indexFromNumber(int i)
        {
            switch (i)
            {
                case 0: return PlayerIndex.One;
                case 1: return PlayerIndex.Two;
                case 2: return PlayerIndex.Three;
                case 3: return PlayerIndex.Four;
                default: return PlayerIndex.One;
            }
        }

        public static void Update(GameTime gameTime)
        {
            for (int i = 0; i < 4; i++)
            {
                GamePadState state = GamePad.GetState(indexFromNumber(i));

                if (state.Buttons.A == ButtonState.Pressed) aDown[i] = true;
                else
                {
                    if (aDown[i]) aButton(i);
                    aDown[i] = false;
                }

                if (state.Buttons.B == ButtonState.Pressed) bDown[i] = true;
                else
                {
                    if (bDown[i]) bButton(i);
                    bDown[i] = false;
                }

                if (state.Buttons.X == ButtonState.Pressed) xDown[i] = true;
                else
                {
                    if (xDown[i]) xButton(i);
                    xDown[i] = false;
                }

                if (state.Buttons.Y == ButtonState.Pressed) yDown[i] = true;
                else
                {
                    if (yDown[i]) yButton(i);
                    yDown[i] = false;
                }

                prevState[i] = state;               
            }
        }

        public static void aButton(int index)
        {
            foreach (Controllable c in controllables) c.aButton(index);
        }
        public static void bButton(int index)
        {
            foreach (Controllable c in controllables) c.bButton(index);
        }
        public static void xButton(int index)
        {
            foreach (Controllable c in controllables) c.xButton(index);
        }
        public static void yButton(int index)
        {
            foreach (Controllable c in controllables) c.yButton(index);
        }
    }
}
