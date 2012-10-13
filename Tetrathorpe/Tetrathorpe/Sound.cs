using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.Threading;

namespace Tetrathorpe
{
    public static class Sound
    {
        static AudioEngine audioEngine;
        static WaveBank waveBank;
        static SoundBank soundBank;

        static Dictionary<string, Cue> singles = new Dictionary<string, Cue>();

        public static bool MusicOn = true;
        public delegate bool MusicCallback(MediaState state);
        public static String CurrentSong = "";

        public static string[] MUSIC = { "Glass Cannon" }; //40

        static float MusicVolume
        {
            get { return 1f; }
        }

        public static bool ThreadingSong = false;

        public static bool Loaded = false;

        static bool playing = false;

        public static void Initialize()
        {
            //audioEngine = new AudioEngine("Content\\Audio\\lexiv-audio.xgs");
            //soundBank = new SoundBank(audioEngine, "Content\\Audio\\Sound Bank.xsb");
            //waveBank = new WaveBank(audioEngine, "Content\\Audio\\Wave Bank.xwb");

            MediaPlayer.Volume = .35f;
            MediaPlayer.IsRepeating = true;

            MediaPlayer.MediaStateChanged += new EventHandler<EventArgs>(MediaPlayer_MediaStateChanged);

            //List<string> w4music = new List<string>();
            //w4music.AddRange(WORLD1MUSIC); w4music.AddRange(WORLD2MUSIC); w4music.AddRange(WORLD3MUSIC);
            //WORLD4MUSIC = w4music.ToArray<string>();

       
        }

        static void MediaPlayer_MediaStateChanged(object sender, EventArgs e)
        {
            if (MediaPlayer.State != MediaState.Playing) PlaySong(MUSIC[Game1.rand.Next(0, MUSIC.Length)]);
        }

        public static void LoadContent(ContentManager Content)
        {
            Loaded = true;
            PlaySong(MUSIC[Game1.rand.Next(0, MUSIC.Length)]);
        }

        public static void Update(GameTime gameTime)
        {
            if (!Loaded) return;

            //audioEngine.Update();

            /*if (MainGame.mainmenu.settings.Visible)
            {
                UpdateVolume();
            }*/
        }

        public static void UpdateVolume()
        {
            if (!Loaded) return;
            MediaPlayer.Volume = MusicVolume;
            //audioEngine.GetCategory("Default").SetVolume(SoundVolume);
        }

        public static void Unload()
        {
            if (!Loaded) return;
            soundBank.Dispose();
            waveBank.Dispose();
            audioEngine.Dispose();
        }

        public static void Stop(Cue cue)
        {
            cue.Stop(AudioStopOptions.AsAuthored);
        }

        public static Cue Play(string Name)
        {
            if (!Loaded) return null;
            Cue returnValue = soundBank.GetCue(Name);
            returnValue.Play();
            return returnValue;
        }

        /*public static void PlayDelay(string Name, int delay)
        {
            if (!Loaded) return;
            if (delay == 0) { Play(Name); return; }
            AnimatedVal<float> timer = Animator.floatAnimator.makeAnimVal(new Ref<float>(0f), delay);
            timer.OnFinish = delegate(AnimatedVal<float> anim)
            {
                Play(Name);
            };
            Animator.floatAnimator.Add(timer, true);
        }

        public static Cue PlaySingle(string Name)
        {
            if (!Loaded) return null;
            if (singles.ContainsKey(Name))
            {
                if (!singles[Name].IsPlaying) singles.Remove(Name);
                else return null;
            }
            singles.Add(Name, Play(Name));
            return singles[Name];
        }*/

        public static void PlaySong(string Name)
        {
            Thread t = null;
            ThreadingSong = true;
            MediaPlayer.Volume = MusicVolume;
            ThreadStart job = new ThreadStart(delegate()
            {
                Game1.musicContent = new ContentManager(TextureManager.content.ServiceProvider, TextureManager.content.RootDirectory);
                Console.WriteLine("Threading Load/Play of: " + Name);
                Song s = null;
                s = Game1.musicContent.Load<Song>("Audio\\MP3\\" + Name);
                MediaPlayer.Play(s);
                CurrentSong = Name;
                ThreadingSong = false;
            });
            t = new Thread(job);
            t.Start();
        }


        public static Cue PlayRandom(params string[] Names)
        {
            if (!Loaded) return null;
            Cue ret = soundBank.GetCue(Names[Game1.rand.Next(0, Names.Length)]);
            ret.Play();
            return ret;
        }

        /*public static bool PlayMusic(MediaState state)
        {
            if (MediaPlayer.State != MediaState.Stopped) return false;
            String random = curMusic[Game1.rand.Next(0, curMusic.Length)];
            PlaySong(random, PlayMusic);
            return true;
        }*/
    }
}
