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
        private string sound1;
        private string sound2;
        private bool isAudioEnabled = false;
        private float volSound = 20;
        private float defaultVolMusic = 10;

        private readonly Clock dragonClock = new Clock();
        private readonly Clock collisionClock = new Clock();
        private readonly Clock gameClock = new Clock();

        private readonly Random rnd = new Random();

        private List<ObjectCoord> fireballsPos = new List<ObjectCoord>();
        private List<ObjectCoord> spearsPos = new List<ObjectCoord>();
        private int hp = 5;
        private float timeLimitSec = 300;

        private readonly Collider colliderFirebasll = new Collider(7, 7, 55, 50);
        private readonly Collider[] collidersSpear = new[]
        {
            new Collider(7, 16, 175, 9),
            new Collider(14, 5, 72, 30),
        };

        private readonly Collider[] collidersDragon = new[]
        {
            new Collider(158, 32, 37, 16),
            new Collider(85, 54, 87, 37),
        };

        public void Start()
        {
            var delay = 10;

            InitWindow(1280, 720, "Heroes III - Dragon fly");
            LoadAssets();

            var currentDifficulty = DifficultyEnum.VeryLow;
            var difficultyClock = new Clock();
            var isPrepare = true;
            var isExit = false;

            while (!isExit)
            {
                if (isPrepare)
                {
                    isPrepare = DoPrepareCycle(delay, difficultyClock);
                }
                else
                {
                    if (hp > 0 && gameClock.ElapsedTime.AsSeconds() < timeLimitSec)
                    {
                        DoGameCycle(delay, ref currentDifficulty, difficultyClock);
                    }
                    else
                    {
                        isExit = DoEndGameCycle(delay);
                    }
                }
            }
        }

        private bool DoEndGameCycle(int delay)
        {
            DispatchEvents();
            ClearWindow(Color.Transparent);
            DrawBg();
            DrawHelpText2();

            if (gameClock.ElapsedTime.AsSeconds() > timeLimitSec && hp > 0)
                DrawHelpTextSuccess();
            else
                DrawHelpTextFail();

            DrawDragon(MouseX, MouseY, DifficultyEnum.Normal);

            var isQuit = GetKeyUp(Keyboard.Key.Q);

            DisplayWindow();
            Delay(delay);

            return isQuit;
        }

        private bool DoPrepareCycle(int delay, Clock difficultyClock)
        {
            DispatchEvents();
            ClearWindow(Color.Transparent);
            DrawBg();
            DrawHelpText();
            DrawDragon(MouseX, MouseY, DifficultyEnum.Normal);

            var isStart = GetKeyUp(Keyboard.Key.S);
            var isVolUp = GetKeyUp(Keyboard.Key.Up);
            var isVolDown = GetKeyUp(Keyboard.Key.Down);
            var isMusicAndSound = GetKeyUp(Keyboard.Key.M);

            if (isVolUp)
            {
                if (music.Volume < 110)
                    music.Volume += 10f;
                if (volSound < 200)
                    volSound += 10f;
            }

            if (isVolDown)
            {
                if (music.Volume > 10)
                    music.Volume -= 10f;
                if (volSound > 10)
                    volSound -= 10f;
            }

            if (isMusicAndSound)
            {
                isAudioEnabled = true;
                if (music.Status != SoundStatus.Playing)
                    music.Play();
            }

            DisplayWindow();
            Delay(delay);

            if (!isStart)
            {
                gameClock.Restart();
                difficultyClock.Restart();
            }

            return !isStart;
        }

        private void DrawHelpTextSuccess()
        {
            DrawText(50, 100, "You come to safe place!", 20);
        }

        private void DrawHelpTextFail()
        {
            DrawText(50, 100, "Your journey ended up on dangerous skies!", 20);
        }

        private void DrawHelpText()
        {
            DrawText(50, 50, "Press S to start\nPress M to enable sounds and music\nPress ↑ or ↓ to attenuate volume\n\nYou need to fly over dangerous sky for more then 5 minutes!!!", 20);
        }

        private void DrawHelpText2()
        {
            DrawText(50, 50, "Press Q to exit", 20);
        }

        private void DoGameCycle(int delay, ref DifficultyEnum currentDifficulty, Clock difficultyClock)
        {
            DispatchEvents();
            ClearWindow(Color.Transparent);

            currentDifficulty = GetDifficulty(difficultyClock, currentDifficulty);
            spearsPos = FilterProjectile(spearsPos, 190);
            fireballsPos = FilterProjectile(fireballsPos, 115);
            AddProjectile(spearsPos, fireballsPos, currentDifficulty);

            DrawBg();
            var curDragonCoord = DrawDragon(MouseX, MouseY, currentDifficulty);

            foreach (var p in spearsPos)
                DrawSpear(p, currentDifficulty);


            foreach (var p in fireballsPos)
                DrawFireball(p, currentDifficulty);

            if (CheckCollision(fireballsPos, spearsPos, curDragonCoord) && collisionClock.ElapsedTime.AsSeconds() > 0.5f)
            {
                hp--;

                if (isAudioEnabled)
                {
                    if (hp > 1)
                        PlaySound(sound1, volSound);
                    else
                        PlaySound(sound2, volSound);
                }

                collisionClock.Restart();
            }

            DrawHp(hp);
            DrawTimer();

            DisplayWindow();
            Delay(delay);
        }

        private void DrawTimer()
        {
            var secAll = Convert.ToInt32(gameClock.ElapsedTime.AsSeconds());
            var min = secAll / 60;
            var sec = secAll - min * 60;

            var str = string.Format("{0:00}:{1:00}", min, sec);

            DrawText(600, 20, str, 20);
        }

        private bool CheckCollision(List<ObjectCoord> fireballs, List<ObjectCoord> spears, ObjectCoord dragon)
        {
            var dX1 = dragon.X + collidersDragon[0].CornerOffsetX;
            var dY1 = dragon.Y + collidersDragon[0].CornerOffsetY;
            var dW1 = collidersDragon[0].Width;
            var dH1 = collidersDragon[0].Height;

            var dX2 = dragon.X + collidersDragon[1].CornerOffsetX;
            var dY2 = dragon.Y + collidersDragon[1].CornerOffsetY;
            var dW2 = collidersDragon[1].Width;
            var dH2 = collidersDragon[1].Height;

            foreach (var oc in fireballs)
            {
                var ocX = oc.X + colliderFirebasll.CornerOffsetX;
                var ocY = oc.Y + colliderFirebasll.CornerOffsetY;

                if (DetectBoxCollision(dX1, dY1, dW1, dH1, ocX, ocY, colliderFirebasll.Width, colliderFirebasll.Height))
                    return true;
                if (DetectBoxCollision(dX2, dY2, dW2, dH2, ocX, ocY, colliderFirebasll.Width, colliderFirebasll.Height))
                    return true;
            }

            foreach (var oc in spears)
            {
                var ocX1 = oc.X + collidersSpear[0].CornerOffsetX;
                var ocY1 = oc.Y + collidersSpear[0].CornerOffsetY;

                var ocX2 = oc.X + collidersSpear[1].CornerOffsetX;
                var ocY2 = oc.Y + collidersSpear[1].CornerOffsetY;

                if (DetectBoxCollision(dX1, dY1, dW1, dH1, ocX1, ocY1, collidersSpear[0].Width, collidersSpear[0].Height))
                    return true;
                if (DetectBoxCollision(dX1, dY1, dW1, dH1, ocX2, ocY2, collidersSpear[1].Width, collidersSpear[1].Height))
                    return true;
                if (DetectBoxCollision(dX2, dY2, dW2, dH2, ocX1, ocY1, collidersSpear[0].Width, collidersSpear[0].Height))
                    return true;
                if (DetectBoxCollision(dX2, dY2, dW2, dH2, ocX2, ocY2, collidersSpear[1].Width, collidersSpear[1].Height))
                    return true;
            }

            return false;
        }

        private bool DetectBoxCollision(float originX1, float originY1, int width1, int height1, float originX2, float originY2, int width2, int height2)
        {
            var isLeftSide = originX1 < originX2;
            var isTopSide = originY1 < originY2;
            var isXCollision = false;
            var isYCollision = false;

            if (isLeftSide && originX1 + width1 > originX2)
                isXCollision = true;
            else if (!isLeftSide && originX1 < originX2 + width2)
                isXCollision = true;

            if (isTopSide && originY1 + height1 > originY2)
                isYCollision = true;
            else if (!isTopSide && originY1 < originY2 + height2)
                isYCollision = true;

            if (isXCollision && isYCollision)
                return true;

            return false;
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

        private List<ObjectCoord> FilterProjectile(List<ObjectCoord> list, float width)
        {
            return list.Where(x => x.X > -width).ToList();
        }

        private void AddProjectile(List<ObjectCoord> spears, List<ObjectCoord> fireballs, DifficultyEnum difficulty)
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
                spears.Add(new ObjectCoord(1280 * delta, rnd.Next(10, 720 - 40)));
                return;
            }

            if (!fireballs.Any(x => x.X > dx2)
                && ((rnd.Next(0, 100) >= 25 && rnd.Next(0, 100) < 50) || (rnd.Next(0, 100) >= 75 && rnd.Next(0, 100) < 100)))
            {
                fireballs.Add(new ObjectCoord(1280 * delta, rnd.Next(10, 720 - 80)));
                return;
            }
        }

        private void LoadAssets()
        {
            music = new Music("assets/COMBAT04.wav")
            {
                Volume = defaultVolMusic,
                Loop = true,
            };

            sound1 = LoadSound("assets/AZURDFND.wav");
            sound2 = LoadSound("assets/AZURWNCE.wav");

            imgBg = LoadTexture("assets/bg.jpg");
            imgDragon = LoadTexture("assets/dragon.png");
            imgFireball = LoadTexture("assets/fireball.png");
            imgSpear = LoadTexture("assets/spear.png");
            imgHeart = LoadTexture("assets/heart.png");

            SetFont("assets/arial.ttf");
        }

        private void DrawHp(int hp)
        {
            if (hp >= 5)
                DrawSprite(imgHeart, 180, 20);
            if (hp >= 4)
                DrawSprite(imgHeart, 140, 20);
            if (hp >= 3)
                DrawSprite(imgHeart, 100, 20);
            if (hp >= 2)
                DrawSprite(imgHeart, 60, 20);
            if (hp >= 1)
                DrawSprite(imgHeart, 20, 20);
        }

        private void DrawBg()
        {
            DrawSprite(imgBg, 0, 0);
        }

        private ObjectCoord DrawDragon(float x, float y, DifficultyEnum speed)
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

            return new ObjectCoord(x, y);
        }

        private void DrawSpear(ObjectCoord p, DifficultyEnum speed)
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

        private void DrawFireball(ObjectCoord p, DifficultyEnum speed)
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

        private class ObjectCoord
        {
            public float X { get; set; }
            public float Y { get; set; }

            public ObjectCoord(float X, float Y)
            {
                this.X = X;
                this.Y = Y;
            }
        }

        private class Collider
        {
            public float CornerOffsetX { get; set; }
            public float CornerOffsetY { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            public Collider(float cornerOffsetX, float cornerOffsetY, int width, int height)
            {
                CornerOffsetX = cornerOffsetX;
                CornerOffsetY = cornerOffsetY;
                Width = width;
                Height = height;
            }
        }
    }
}
