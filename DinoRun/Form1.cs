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
        List<Cactus> cactusArray = new List<Cactus>();

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

        //кактус
        class Cactus
        {
            int y, width, height, speed;
            public int X;
            public static Random Rand = new Random();

            public Cactus(int _x, int _y, int _width, int _height, int _speed)
            {
                X = _x;
                y = _y;
                width = _width;
                height = _height;
                speed = _speed;
            }

            //прорисовка кактуса
            public bool Move(ref Graphics g)
            {
                if (X >= -width)
                {
                    g.FillRectangle(new SolidBrush(Color.FromArgb(224, 121, 31)), X, y, width, height);
                    X -= speed;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        //создание кактусов
        void createCactusArr(List<Cactus> cactusArr)
        {
            cactusArr.Add(new Cactus(DisplayWidth + 20, DisplayHeight - 170, 20, 70, 10));
            cactusArr.Add(new Cactus(DisplayWidth + 300, DisplayHeight - 150, 30, 50, 10));
            cactusArr.Add(new Cactus(DisplayWidth + 600, DisplayHeight - 180, 25, 80, 10));
        }

        //прорисовка кактусов
        void drawCactusArr(List<Cactus> cactusArr, ref Graphics g)
        {
            foreach (Cactus item in cactusArr)
            {
                bool check = item.Move(ref g);
                if (!check) item.X = findRadius();
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

            int choice = Cactus.Rand.Next(5);
            if (choice == 0)
                radius += Cactus.Rand.Next(10, 15);
            else radius += Cactus.Rand.Next(200, 350);

            return radius;
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

            g.Clear(Color.White);

            drawCactusArr(cactusArray, ref g);

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
            pictureBox1.Width = DisplayWidth;
            pictureBox1.Height = DisplayHeight;
            createCactusArr(cactusArray);

            timer1.Enabled = true;
        }


    }
}
