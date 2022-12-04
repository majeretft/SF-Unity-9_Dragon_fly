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
        private string imgHeart;

        private Music music;

        private Clock dragonClock = new Clock();

        private Random rnd = new Random();

        public void Start()
        {
            var delay = 10;
            var hp = 3;

            InitWindow(1280, 720, "Heroes III - Dragon fly");
            LoadAssets();

            music.Play();

            var fireballsPos = new List<ProjectileCoord>();
            var spearsPos = new List<ProjectileCoord>();
            var currentDifficulty = DifficultyEnum.VeryLow;
            var difficultyClock = new Clock();

            while (true)
            {
                DispatchEvents();
                ClearWindow(Color.Transparent);

                currentDifficulty = GetDifficulty(difficultyClock, currentDifficulty);
                spearsPos = FilterProjectile(spearsPos, 190);
                fireballsPos = FilterProjectile(fireballsPos, 115);
                AddProjectile(spearsPos, fireballsPos, currentDifficulty);

                DrawBg();
                DrawDragon(MouseX, MouseY, currentDifficulty);

                foreach (var p in spearsPos)
                    DrawSpear(p, currentDifficulty);


                foreach (var p in fireballsPos)
                    DrawFireball(p, currentDifficulty);

                DrawHp(hp);

                DisplayWindow();
                Delay(delay);
            }
        }

        private DifficultyEnum GetDifficulty(Clock clock, DifficultyEnum prev)
        {
            var duration = clock.ElapsedTime.AsSeconds();

            var rd = rnd.Next(50, 100) / 100f;
            var ri = rnd.Next(1, 100);

            if (duration * rd > 15)
            {
                clock.Restart();

                if (ri > 80)
                    return DifficultyEnum.VeryHigh;
                if (ri > 60)
                    return DifficultyEnum.High;
                if (ri > 30)
                    return DifficultyEnum.Normal;
                if (ri > 15)
                    return DifficultyEnum.Low;
                return DifficultyEnum.VeryLow;
            }

            return prev;
        }

        private List<ProjectileCoord> FilterProjectile(List<ProjectileCoord> list, float width)
        {
            return list.Where(x => x.X > -width).ToList();
        }

        private void AddProjectile(List<ProjectileCoord> spears, List<ProjectileCoord> fireballs, DifficultyEnum difficulty)
        {
            var max = 2;
            var dx1 = 0;
            var dx2 = 0;

            switch (difficulty)
            {
                case DifficultyEnum.VeryLow:
                    dx1 = 1100;
                    dx2 = 1150;
                    max = 6;
                    break;
                case DifficultyEnum.Low:
                    dx1 = 1120;
                    dx2 = 1170;
                    max = 8;
                    break;
                case DifficultyEnum.Normal:
                    dx1 = 1150;
                    dx2 = 1200;
                    max = 11;
                    break;
                case DifficultyEnum.High:
                    dx1 = 1180;
                    dx2 = 1250;
                    max = 16;
                    break;
                case DifficultyEnum.VeryHigh:
                    dx1 = 1220;
                    dx2 = 1280;
                    max = 20;
                    break;
            }

            if (spears.Count + fireballs.Count >= max)
                return;

            var delta = rnd.Next(101, 140) / 100f;

            if (!spears.Any(x => x.X > dx1)
                && ((rnd.Next(0, 100) >= 0 && rnd.Next(0, 100) < 25) || (rnd.Next(0, 100) >= 50 && rnd.Next(0, 100) < 75)))
            {
                spears.Add(new ProjectileCoord(1280 * delta, rnd.Next(10, 720 - 40)));
                return;
            }

            if (!fireballs.Any(x => x.X > dx2)
                && ((rnd.Next(0, 100) >= 25 && rnd.Next(0, 100) < 50) || (rnd.Next(0, 100) >= 75 && rnd.Next(0, 100) < 100)))
            {
                fireballs.Add(new ProjectileCoord(1280 * delta, rnd.Next(10, 720 - 80)));
                return;
            }
        }

        private void LoadAssets()
        {
            music = new Music("assets/music.wav")
            {
                Volume = 1,
                Loop = true,
            };

            imgBg = LoadTexture("assets/bg.jpg");
            imgDragon = LoadTexture("assets/dragon.png");
            imgFireball = LoadTexture("assets/fireball.png");
            imgSpear = LoadTexture("assets/spear.png");
            imgHeart = LoadTexture("assets/heart.png");
        }

        private void DrawHp(int hp)
        {
            if (hp >= 3)
                DrawSprite(imgHeart, 100, 20);
            if (hp >= 2)
                DrawSprite(imgHeart, 20, 20);
            if (hp >= 1)
                DrawSprite(imgHeart, 60, 20);
        }

        private void DrawBg()
        {
            DrawSprite(imgBg, 0, 0);
        }

        private void DrawDragon(float x, float y, DifficultyEnum speed)
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
                case DifficultyEnum.VeryLow: msStep = 170; break;
                case DifficultyEnum.Low: msStep = 140; break;
                case DifficultyEnum.Normal: msStep = 120; break;
                case DifficultyEnum.High: msStep = 90; break;
                case DifficultyEnum.VeryHigh: msStep = 60; break;
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

        private void DrawSpear(ProjectileCoord p, DifficultyEnum speed)
        {
            var dX = 1;
            switch (speed)
            {
                case DifficultyEnum.VeryLow: dX = 4; break;
                case DifficultyEnum.Low: dX = 5; break;
                case DifficultyEnum.Normal: dX = 6; break;
                case DifficultyEnum.High: dX = 8; break;
                case DifficultyEnum.VeryHigh: dX = 9; break;
            }

            p.X -= dX;

            DrawSprite(imgSpear, p.X, p.Y);
        }

        private void DrawFireball(ProjectileCoord p, DifficultyEnum speed)
        {
            var dX = 1;
            switch (speed)
            {
                case DifficultyEnum.VeryLow: dX = 5; break;
                case DifficultyEnum.Low: dX = 6; break;
                case DifficultyEnum.Normal: dX = 8; break;
                case DifficultyEnum.High: dX = 10; break;
                case DifficultyEnum.VeryHigh: dX = 12; break;
            }

            p.X -= dX;

            DrawSprite(imgFireball, p.X, p.Y);
        }

        private enum DifficultyEnum
        {
            VeryLow,
            Low,
            Normal,
            High,
            VeryHigh,
        }

        private class ProjectileCoord
        {
            public float X { get; set; }
            public float Y { get; set; }

            public ProjectileCoord(float X, float Y)
            {
                this.X = X;
                this.Y = Y;
            }
        }
    }
}
