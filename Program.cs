using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Audio;
using SFML.Graphics;
using SFML.Learning;
using SFML.System;
using SFML.Window;

namespace _9_Dragon_fly
{
    public class Program
    {
        public static void Main()
        {
            var game = new MyGame();
            game.Start();
        }
    }

    public class MyGame : Game
    {
        private string imgBg;
        private string imgDragon;
        private string imgFireball;
        private string imgSpear;

        private Music music;

        private Clock dragonClock = new Clock();

        public void Start()
        {
            var delay = 10;

            InitWindow(1280, 720, "Heroes III - Dragon fly");
            LoadAssets();

            music.Play();

            while (true)
            {
                DispatchEvents();
                ClearWindow(Color.Transparent);

                DrawBg();
                DrawDragon(MouseX, MouseY, FlySpeedEnum.VerySlow);

                DisplayWindow();
                Delay(delay);
            }
        }

        private void LoadAssets()
        {
            music = new Music("assets/music.wav")
            {
                Volume = 0.75f,
                Loop = true,
            };

            imgBg = LoadTexture("assets/bg.jpg");
            imgDragon = LoadTexture("assets/dragon.png");
            imgFireball = LoadTexture("assets/fireball.png");
            imgSpear = LoadTexture("assets/spear.png");
        }

        private void DrawBg()
        {
            DrawSprite(imgBg, 0, 0);
        }

        private void DrawDragon(float x, float y, FlySpeedEnum speed)
        {
            var ms = dragonClock.ElapsedTime.AsMilliseconds();
            var needRestart = false;
            int spriteOffsetY = 0;
            int dragonSpriteHeight = 142;
            int dragonSpriteScreenHeight = 122;
            int dragonSpriteWidth = 283;
            int dragonSpriteScreenWidth = 210;

            var msStep = 100;
            switch (speed)
            {
                case FlySpeedEnum.VerySlow: msStep = 150; break;
                case FlySpeedEnum.Slow: msStep = 125; break;
                case FlySpeedEnum.Normal: msStep = 100; break;
                case FlySpeedEnum.Fast: msStep = 85; break;
                case FlySpeedEnum.VeryFast: msStep = 70; break;
            }

            y -= dragonSpriteHeight / 2;

            if (x < 0)
                x = 0;
            if (y < 0)
                y = 0;
            if (x > 1280 - dragonSpriteScreenWidth)
                x = 1280 - dragonSpriteScreenWidth;
            if (y > 720 - dragonSpriteScreenHeight)
                y = 720 - dragonSpriteScreenHeight;

            if (ms > 6 * msStep)
                needRestart = true;

            if (ms > 5 * msStep)
                spriteOffsetY = dragonSpriteHeight * 5;
            else if (ms > 4 * msStep)
                spriteOffsetY = dragonSpriteHeight * 4;
            else if (ms > 3 * msStep)
                spriteOffsetY = dragonSpriteHeight * 3;
            else if (ms > 2 * msStep)
                spriteOffsetY = dragonSpriteHeight * 2;
            else if (ms > 1 * msStep)
                spriteOffsetY = dragonSpriteHeight;

            if (needRestart)
                dragonClock.Restart();

            DrawSprite(imgDragon, x, y, 0, spriteOffsetY, dragonSpriteWidth, dragonSpriteHeight);
        }

        private enum FlySpeedEnum
        {
            VerySlow,
            Slow,
            Normal,
            Fast,
            VeryFast,
        }
    }
}
