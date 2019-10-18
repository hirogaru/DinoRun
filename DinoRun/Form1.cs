using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DinoRun
{
    public partial class Form1 : Form
    {
        static int DisplayWidth = 800,  //Ширина
            DisplayHeight = 600;  //Высота
        Image Background,  //Задний фон
            Health_img;  //иконка жизней   
        List<Cactus> CactusArray;  //массив с кактусами
        GameEntity Stone, Cloud;  //камни, облака
        PrivateFontCollection PrivateFonts = new PrivateFontCollection();  //игровые шрифты
        bool RunGame = false;  //работает ли игра

        //игрок
        static class User 
        {
            static int imgCounter = 0;  //счётчик кадров

            public static Image[] DinoImages;  //текстуры игрока
            public static int Health,  //жизни
                ImmortalMode = 0,  //режим бессмертия
                MaxAboveCactus = 0,
                Scores,
                MaxScores,
                JumpCounter = 30,
                Width = 70,
                Height = 100,
                X = DisplayWidth / 5,
                Y = DisplayHeight - Height - 100;
            public static bool MakeJump = false;  //во время прыжка

            //прыжок игрока
            public static void Jump()
            {
                if (JumpCounter >= -30)
                {
                    Y -= JumpCounter;
                    JumpCounter -= 3;
                }
                else
                {
                    JumpCounter = 30;
                    MakeJump = false;
                }
            }

            //начальные значения
            public static void SetDefs()
            {
                Scores = 0;
                Health = 3;
                MakeJump = false;
                JumpCounter = 30;
                Y = DisplayHeight - Height - 100;
            }

            //прорисовка игрока
            public static void DrawDino(ref Graphics g)
            {
                if (imgCounter == 15) imgCounter = 0;

                int imgNo = imgCounter / 3;

                if (ImmortalMode > 0 && (imgNo % 2 == 0)) ImmortalMode--;
                else g.DrawImage(DinoImages[imgNo], new Rectangle(X, Y, Width, Height));

                imgCounter++;
            }

            //проверяем жизни
            public static bool CheckHealth()
            {
                if (ImmortalMode > 0) return true;

                Health--;
                if (Health == 0) return false;
                ImmortalMode = 8;
                return true;
            }
        }

        //объект
        class GameEntity 
        {
            protected int y, width, height, speed;
            protected Image imgObj;  //текущая текстура
            public int X;
            public static Random Rand = new Random();
            public static Image[] StoneImages;  //текстуры камня
            public static Image[] CloudImages;  //текстуры облаков

            public GameEntity(int _x, int _y, Image _img ,int _speed)
            {
                X = _x;
                y = _y;
                imgObj = _img;
                speed = _speed;
                width = _img.Width;
                height = _img.Height;
            }
            
            //прорисовка
            public bool Move(ref Graphics g)
            {
                if (X >= -width)
                {
                    g.DrawImage(imgObj, new Rectangle(X, y, width, height));
                    X -= speed;
                    return true;
                }
                else
                {
                    return false;  //если за экраном
                }
            }

            //возвращаем за экран
            public void returnSelf(int _x, int _y, Image _img)
            {
                height = _img.Height;
                width = _img.Width;
                y = _y;
                X = _x;
                imgObj = _img;
            }
        }

        //кактус
        class Cactus : GameEntity
        {
            public static Image[] CactusImages;  //текстуры кактусов

            public Cactus(int _x, int _y, Image _img,int _speed)
                : base (_x, _y, _img, _speed)
            {

            }

            //возвращаем кактус за экран
            public void returnSelf(int radius)
            {
                int choice = Rand.Next(CactusImages.Length);

                height = CactusImages[choice].Height;
                width = CactusImages[choice].Width;
                y = DisplayHeight - 100 - height;
                X = radius;
                imgObj = CactusImages[choice];
            }

            //было ли столкновение с игроком
            public bool IsCollision { get
                {
                    if (height < 55)  //малый cactus
                    {
                        if (!User.MakeJump)  //не в прыжке
                        {
                            if ((X <= User.X + User.Width - 30) && (User.X + User.Width - 30 <= X + width)) return true;
                        }
                        else
                        {
                            if (User.JumpCounter >= 0)  //на взлёте
                            {
                                if (User.Y + User.Height - 5 >= y)
                                    if ((X <= User.X + User.Width - 30) && (User.X + User.Width - 30 <= X + width)) return true;
                            }
                            else  //при падении
                                if (User.Y + User.Height - 10 >= y)
                                    if ((X <= User.X) && (User.X <= X + width)) return true;
                        }
                    }
                    else  //обычный cactus
                    {
                        if (!User.MakeJump)  //не в прыжке
                        {
                            if ((X <= User.X + User.Width - 5) && (User.X + User.Width - 5 <= X + width)) return true;
                        }
                        else
                        {
                            //if (User.JumpCounter == 10)
                            //{
                            //    if (User.Y + User.Height - 5 >= y)
                            //        if ((X <= User.X + User.Width - 5) && (User.X + User.Width - 5 <= X + width)) return true;
                            //}
                            if (User.JumpCounter >= -1)  //на взлёте
                            {
                                if (User.Y + User.Height - 5 >= y)
                                    if ((X <= User.X + User.Width - 30) && (User.X + User.Width - 30 <= X + width)) return true;                                
                            }
                            else  //при падении
                                if (User.Y + User.Height - 10 >= y)
                                    if ((X <= User.X + 5) && (User.X + 5 <= X + width)) return true;
                        }
                    }

                    return false;
                } }

            //кактус под игроком?
            public bool IsPlayerUnder { get
                {
                    if (User.Y + User.Height - 5 <= y)
                    {
                        if ((X <= User.X) && (User.X <= X + width)) return true;
                        else if ((X <= User.X + User.Width) && (User.X + User.Width <= X + width)) return true;
                    }
                    return false;
                } }
        }

        //найти новое расстояние между кактусами
        int findRadius()
        {
            int maximumX = CactusArray.Max(cact => cact.X),
                radius;
            if (maximumX < DisplayWidth)
            {
                radius = DisplayWidth;
                 if (radius - maximumX < 50) radius += 250;
            }
            else radius = maximumX;

            int choice = GameEntity.Rand.Next(5);
            if (choice == 0)
                radius += GameEntity.Rand.Next(15, 20);
            else radius += GameEntity.Rand.Next(250, 400);

            return radius;
        }

        //создаём случайные объекты на экране
        void openRandObjects()
        {
            int choice = GameEntity.Rand.Next(2);
            Stone = new GameEntity(DisplayWidth, DisplayHeight - 80, GameEntity.StoneImages[choice], 10);

            choice = GameEntity.Rand.Next(2);
            Cloud  = new GameEntity(DisplayWidth, 80, GameEntity.CloudImages[choice], 5);
        }

        //двигаем объекты
        void moveObjects(ref Graphics g)
        {
            bool check = Cloud.Move(ref g);
            if (!check)
            {
                var choice = GameEntity.Rand.Next(2);
                Cloud.returnSelf(DisplayWidth, GameEntity.Rand.Next(10, 200), GameEntity.CloudImages[choice]);
            }

            check = Stone.Move(ref g);
            if (!check)
            {
                var choice = GameEntity.Rand.Next(2);
                Stone.returnSelf(DisplayWidth, 500 + GameEntity.Rand.Next(10, 50), GameEntity.StoneImages[choice]);
            }
        }

        //показываем жизни
        void showHealth(ref Graphics g)
        {
            int show = 0,
                x = 20;
            while (show != User.Health)
            {
                g.DrawImage(Health_img, new Rectangle(x, 20, 30, 30));
                x += 40;
                show++;
            }
        }

        //реализация паузы
        void Pause()
        {
            if (timer1.Enabled)
            {
                timer1.Enabled = false;

                pictureBox1.Refresh();
            }
            else
            {
                timer1.Enabled = true;
            }
        }

        //Выводим сообщение на экран
        void printText(ref Graphics g, string message, int x, int y)
        {
            g.DrawString(message, new Font(PrivateFonts.Families[0], 30), new SolidBrush(Color.Black), x, y);
        }

        //начисление очков
        void countScores()
        {
            int _aboveCactus = 0;

            if ((-20 <= User.JumpCounter)&&(User.JumpCounter < 25))
            {
                foreach (var cactus in CactusArray)
                {
                    if (cactus.IsPlayerUnder) _aboveCactus += 1;
                    
                }
                User.MaxAboveCactus = Math.Max(User.MaxAboveCactus, _aboveCactus);
            }
            else
            {
                if (User.JumpCounter < -27)
                {
                    User.Scores += User.MaxAboveCactus;
                    User.MaxAboveCactus = 0;
                }
            }
        }

        //реализация окончания игры
        void GameOver()
        {
            timer1.Enabled = false;
            RunGame = false;

            if (User.Scores > User.MaxScores) User.MaxScores = User.Scores;

            pictureBox1.Refresh();
        }

        //события обновления таймера
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (User.MakeJump) User.Jump();
            countScores();

            pictureBox1.Refresh();

            //проверка столкновений
            var checkCollision = false;  
            foreach (var cactus in CactusArray)
            {
                if (cactus.IsCollision) checkCollision = true;
            }
            if (checkCollision)
                if (!User.CheckHealth()) GameOver();
                
        }

        //процедура прорисовки экрана
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.DrawImage(Background, new Rectangle(0, 0, DisplayWidth, DisplayHeight));
            moveObjects(ref g);

            //прорисовка кактусов
            foreach (Cactus item in CactusArray)
            {
                bool check = item.Move(ref g);
                if (!check)
                {
                    item.returnSelf(findRadius());
                }
            }

            g.DrawString("Scores: " + User.Scores.ToString(), new Font(PrivateFonts.Families[0], 22), new SolidBrush(Color.Black), 600, 10);
            showHealth(ref g);
            User.DrawDino(ref g);

            if (!timer1.Enabled)
                if (!RunGame)
                {
                    printText(ref g, "Game Over. Press enter to play again", 30, 300);
                    printText(ref g, "Max scores: " + User.MaxScores, 250, 350);
                }
                else
                {
                    printText(ref g, "Paused. Press enter to continue", 100, 300);
                }
        }

        //начальная загрузка
        private void Form1_Load(object sender, EventArgs e)
        {
            //loading
            Background = Resources.Properties.Resources.Land;
            Health_img = Resources.Properties.Resources.heart;
            Cactus.CactusImages = new Image[3];
            Cactus.CactusImages[0] = Resources.Properties.Resources.Cactus0;
            Cactus.CactusImages[1] = Resources.Properties.Resources.Cactus1;
            Cactus.CactusImages[2] = Resources.Properties.Resources.Cactus2;
            GameEntity.StoneImages = new Image[2];
            GameEntity.StoneImages[0] = Resources.Properties.Resources.Stone0;
            GameEntity.StoneImages[1] = Resources.Properties.Resources.Stone1;
            GameEntity.CloudImages = new Image[2];
            GameEntity.CloudImages[0] = Resources.Properties.Resources.Cloud0;
            GameEntity.CloudImages[1] = Resources.Properties.Resources.Cloud1;
            User.DinoImages = new Image[5];
            User.DinoImages[0] = Resources.Properties.Resources.Dino0;
            User.DinoImages[1] = Resources.Properties.Resources.Dino1;
            User.DinoImages[2] = Resources.Properties.Resources.Dino2;
            User.DinoImages[3] = Resources.Properties.Resources.Dino3;
            User.DinoImages[4] = Resources.Properties.Resources.Dino4;
            
            //грузим шрифт
            IntPtr data = Marshal.AllocCoTaskMem(Resources.Properties.Resources.PingPong.Length);
            Marshal.Copy(Resources.Properties.Resources.PingPong, 0, data, Resources.Properties.Resources.PingPong.Length);
            PrivateFonts.AddMemoryFont(data, Resources.Properties.Resources.PingPong.Length);
            Marshal.FreeCoTaskMem(data);

            //начальные установки
            openRandObjects();
            User.SetDefs();

            //создание кактусов
            CactusArray = new List<Cactus>();
            for (int idx = 0; idx < 3; idx++)
            {
                int choice = GameEntity.Rand.Next(Cactus.CactusImages.Length);
                CactusArray.Add(new Cactus(DisplayWidth + 20 + (300 * idx), DisplayHeight - 100 - Cactus.CactusImages[choice].Height, Cactus.CactusImages[choice], 10));
            }

            RunGame = true;
            timer1.Enabled = true;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            User.MakeJump = true;
        }

        //обработка нажатий клавиш
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.Close();
                    break;
                case Keys.Space:
                    User.MakeJump = true;
                    break;
                case Keys.Return:
                    if (RunGame) Pause();
                    else Form1_Load(this, new EventArgs());
                    break;
            }
        }

        //создание окна
        public Form1()
        {
            InitializeComponent();
            this.Text = "Run Dino! Run!";

            pictureBox1.Width = DisplayWidth;
            this.Width = DisplayWidth + 12;
            pictureBox1.Height = DisplayHeight;
            this.Height = DisplayHeight + 35;
        }
    }
}
