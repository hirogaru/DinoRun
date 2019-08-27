using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;


namespace DinoRun
{
    public partial class Form1 : Form
    {
        static int DisplayWidth = 800,
            DisplayHeight = 600;
        Image background;
        List<Cactus> cactusArray = new List<Cactus>();
        GameEntity stone, cloud;

        //игрок
        static class User
        {
            static int jumpCounter = 30;
            public static int Width = 60,
                Height = 100,
                X = DisplayWidth / 5,
                Y = DisplayHeight - Height - 100;
            public static bool MakeJump = false;

            //прыжок игрока
            public static void Jump()
            {
                if (jumpCounter >= -30)
                {
                    Y -= jumpCounter;
                    jumpCounter -= 3;
                }
                else
                {
                    jumpCounter = 30;
                    MakeJump = false;
                }
            }
        }

        //объект
        class GameEntity
        {
            protected int y, width, height, speed;
            protected Image imgObj;
            public int X;
            public static Random Rand = new Random();
            public static Image[] StoneImages;
            public static Image[] CloudImages;

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
                    return false;
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
            public static Image[] Images;

            public Cactus(int _x, int _y, Image _img,int _speed)
                : base (_x, _y, _img, _speed)
            {

            }

            //возвращаем кактус за экран
            public void returnSelf(int radius)
            {
                int choice = Rand.Next(Images.Length);

                height = Images[choice].Height;
                width = Images[choice].Width;
                y = DisplayHeight - 100 - height;
                X = radius;
                imgObj = Images[choice];
            }
        }

        //создание кактусов
        void createCactusArr()
        {
            for (int idx = 0; idx < 3; idx++)
            {
                int choice = GameEntity.Rand.Next(Cactus.Images.Length);
                cactusArray.Add(new Cactus(DisplayWidth + 20 + (300 * idx), DisplayHeight - 100 - Cactus.Images[choice].Height, Cactus.Images[choice], 10));
            }
        }

        //прорисовка кактусов
        void drawCactusArr(ref Graphics g)
        {
            foreach (Cactus item in cactusArray)
            {
                bool check = item.Move(ref g);
                if (!check)
                {
                    item.returnSelf(findRadius());
                }
            }
        }

        //найти новое расстояние между кактусами
        int findRadius()
        {
            int maximumX = cactusArray.Max(cact => cact.X),
                radius;
            if (maximumX < DisplayWidth)
            {
                radius = DisplayWidth;
                 if (radius - maximumX < 70) radius += 150;
            }
            else radius = maximumX;

            int choice = GameEntity.Rand.Next(5);
            if (choice == 0)
                radius += GameEntity.Rand.Next(10, 15);
            else radius += GameEntity.Rand.Next(200, 350);

            return radius;
        }

        //создаём случайные объекты на экране
        void openRandObjects()
        {
            int choice = GameEntity.Rand.Next(2);
            stone = new GameEntity(DisplayWidth, DisplayHeight - 80, GameEntity.StoneImages[choice], 10);

            choice = GameEntity.Rand.Next(2);
            cloud  = new GameEntity(DisplayWidth, 80, GameEntity.CloudImages[choice], 5);
        }

        //двигаем объекты
        void moveObjects(ref Graphics g)
        {
            bool check = cloud.Move(ref g);
            if (!check)
            {
                var choice = GameEntity.Rand.Next(2);
                cloud.returnSelf(DisplayWidth, GameEntity.Rand.Next(10, 200), GameEntity.CloudImages[choice]);
            }

            check = stone.Move(ref g);
            if (!check)
            {
                var choice = GameEntity.Rand.Next(2);
                stone.returnSelf(DisplayWidth, 500 + GameEntity.Rand.Next(10, 50), GameEntity.StoneImages[choice]);
            }
        }


        //события обновления таймера
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (User.MakeJump) User.Jump();

            pictureBox1.Refresh();
        }

        //процедура прорисовки экрана
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.DrawImage(background, new Rectangle(0, 0, DisplayWidth, DisplayHeight));

            drawCactusArr(ref g);
            moveObjects(ref g);

            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(247,240,22)), User.X, User.Y, User.Width, User.Height);
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
            }
        }

        //начальная загрузка
        public Form1()
        {
            InitializeComponent();
            this.Text = "Run Dino! Run!";

            pictureBox1.Width = this.Width = DisplayWidth;
            pictureBox1.Height = this.Height = DisplayHeight;

            //loading
            background = Resources.Properties.Resources.Land;
            Cactus.Images = new Image[3];
            Cactus.Images[0] = Resources.Properties.Resources.Cactus0;
            Cactus.Images[1] = Resources.Properties.Resources.Cactus1;
            Cactus.Images[2] = Resources.Properties.Resources.Cactus2;
            GameEntity.StoneImages = new Image[2];
            GameEntity.StoneImages[0] = Resources.Properties.Resources.Stone0;
            GameEntity.StoneImages[1] = Resources.Properties.Resources.Stone1;
            GameEntity.CloudImages = new Image[2];
            GameEntity.CloudImages[0] = Resources.Properties.Resources.Cloud0;
            GameEntity.CloudImages[1] = Resources.Properties.Resources.Cloud1;

            openRandObjects();
            createCactusArr();
            timer1.Enabled = true;
        }


    }
}
