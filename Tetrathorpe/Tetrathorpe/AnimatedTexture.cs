﻿using System;
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
    public class AnimatedTexture
    {
        string baseTex;

        bool animated = true;
        int frame;
        int framemax;

        int timeStep;
        int curTime;

        public Hitbox[] onFrameHitboxes;

        public Action onStart;
        public Action onFinish;
        private Action[] onFrameActions; // use setOnFrameActions

        public AnimatedTexture(string _base)
        {
            baseTex = _base;
            animated = false;
            onFrameHitboxes = new Hitbox[1];
            onFrameHitboxes[0] = null;
            frame = 1;
        }

        public AnimatedTexture(string _base, int numframes, int _timeStep)
        {
            baseTex = _base;
            framemax = numframes;
            onFrameActions = new Action[numframes];
            onFrameHitboxes = new Hitbox[numframes];
            for (int i = 0; i < onFrameHitboxes.Length; i++)
            {
                onFrameHitboxes[i] = null;
            }
            frame = 1;
            timeStep = _timeStep;
        }

        public Hitbox getHitbox()
        {
            return onFrameHitboxes[frame - 1];
        }

        public string get()
        {
            if (animated) return baseTex + " " + frame;
            else return baseTex;
        }

        public Texture2D getTexture()
        {
            return TextureManager.Get(get());
        }

        public string get(Character.Direction direction)
        {
            if (animated) return baseTex + " " + getSide(direction) + " " + frame;
            else return baseTex + " " + getSide(direction);
        }

        public Texture2D getTexture(Character.Direction direction)
        {
            return TextureManager.Get(get(direction));
        }

        public string getSide(Character.Direction direction)
        {
            switch (direction)
            {
                case Character.Direction.Left: return "side";
                case Character.Direction.Right: return "side";
                case Character.Direction.Forward: return "forward";
                case Character.Direction.Back: return "back";
                default: return "side";
            }
        }

        public void setOnFrameAction(int frame, Action action)
        {
            if (frame < 1 || frame > framemax)
                throw new ArgumentOutOfRangeException("frame", "Frame needs to be between 1 and framemax");
            onFrameActions[frame - 1] = action;
        }

        public void setOnFrameHitbox(int frame, Hitbox hitbox)
        {
            if (frame < 1 || frame > framemax)
                throw new ArgumentOutOfRangeException("frame", "Frame needs to be between 1 and framemax");
            if (hitbox == null)
                onFrameHitboxes[frame - 1] = new Hitbox(0, 0);
            else
                onFrameHitboxes[frame - 1] = hitbox;
        }

        public void setOnFrameRangeHitbox(int startFrame, int endFrame, Hitbox hitbox)
        {
            if (startFrame < 1 || startFrame > framemax || endFrame < 1 || endFrame > framemax)
                throw new ArgumentOutOfRangeException("frame", "Frames need to be between 1 and framemax");
            if (endFrame < startFrame)
                throw new ArgumentOutOfRangeException("frame", "End frame cannot be less than start frame");
            for (int i = startFrame; i <= endFrame; i++)
            {
                if (hitbox == null)
                    onFrameHitboxes[i - 1] = new Hitbox(0, 0);
                else
                    onFrameHitboxes[i - 1] = hitbox;
            }
        }

        public void increment()
        {
            if (frame >= framemax)
            {
                frame = 0;
                if (onFinish != null) onFinish();
            }
            if (onFrameActions != null)
                if (onFrameActions[frame] != null)
                    onFrameActions[frame]();
            frame++;
        }

        public void Update(GameTime gameTime, Entity owner)
        {
            curTime += (gameTime != null) ? gameTime.ElapsedGameTime.Milliseconds : 0;
            if (curTime > timeStep)
            {
                increment();
                curTime = 0; 
            }
            if (frame > 0 && getHitbox() != null)
            {
                getHitbox().active = true;
                getHitbox().Update(owner);
            }
        }

        public void DrawHitbox(SpriteBatch spriteBatch)
        {
            if (frame > 0 && getHitbox() != null)
                getHitbox().Draw(spriteBatch);
        }

        public void LoadTextures()
        {
            if (animated) {
                for (int i = 1; i <= framemax; i++)
                {
                    frame = i;
                    TextureManager.Add(get(Character.Direction.Forward));
                    TextureManager.Add(get(Character.Direction.Back));
                    TextureManager.Add(get(Character.Direction.Left));
                }
            }
            else {
                TextureManager.Add(get(Character.Direction.Forward));
                TextureManager.Add(get(Character.Direction.Back));
                TextureManager.Add(get(Character.Direction.Left));
            }

            frame = 1;
        }
    }
}
