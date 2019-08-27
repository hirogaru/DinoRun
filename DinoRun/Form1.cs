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
        Image Background;  //Задний фон
        List<Cactus> CactusArray;  //массив с кактусами
        GameEntity Stone, Cloud;  //камни, облака
        PrivateFontCollection PrivateFonts = new PrivateFontCollection();  //игровые шрифты
        bool RunGame = false;  //работает ли игра

        //игрок
        static class User
        {
            static int jumpCounter = 30;
            static int imgCounter = 0;
            public static Image[] DinoImages;
            public static int Width = 70,
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

            //прорисовка игрока
            public static void DrawDino(ref Graphics g)
            {
                if (imgCounter == 25) imgCounter = 0;

                g.DrawImage(DinoImages[imgCounter / 5], new Rectangle(X, Y, Width, Height));

                imgCounter++;
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
            public static Image[] CactusImages;

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

            public bool CollisionY { get { return User.Y + User.Height >= y; } }

            public bool CollisionLeftX { get { return (X <= User.X) && (User.X <= X + width); } }

            public bool CollisionRightX { get { return (X <= User.X + User.Width) && (User.X + User.Width <= X + width); } }
        }

        //создание кактусов
        void createCactusArr()
        {
            CactusArray = new List<Cactus>();
            for (int idx = 0; idx < 3; idx++)
            {
                int choice = GameEntity.Rand.Next(Cactus.CactusImages.Length);
                CactusArray.Add(new Cactus(DisplayWidth + 20 + (300 * idx), DisplayHeight - 100 - Cactus.CactusImages[choice].Height, Cactus.CactusImages[choice], 10));
            }
        }

        //прорисовка кактусов
        void drawCactusArr(ref Graphics g)
        {
            foreach (Cactus item in CactusArray)
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
            int maximumX = CactusArray.Max(cact => cact.X),
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

        //проверка столкновений
        bool checkCollision()
        {
            foreach (var cactus in CactusArray)
            {
                if (cactus.CollisionY)
                    if (cactus.CollisionLeftX) return true;
                    else if (cactus.CollisionRightX) return true;
            }
            return false;
        }

        //реализация окончания игры
        void GameOver()
        {
            timer1.Enabled = false;
            RunGame = false;

            pictureBox1.Refresh();
        }

        //события обновления таймера
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (User.MakeJump) User.Jump();

            pictureBox1.Refresh();

            if (checkCollision()) GameOver();
        }

        //процедура прорисовки экрана
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.DrawImage(Background, new Rectangle(0, 0, DisplayWidth, DisplayHeight));

            drawCactusArr(ref g);
            moveObjects(ref g);

            User.DrawDino(ref g);

            if (!timer1.Enabled)
                if (!RunGame) printText(ref g, "Game Over. Press enter to play again", 30, 300);
                else printText(ref g, "Paused. Press enter to continue", 100, 300);
        }

        //начальная загрузка
        private void Form1_Load(object sender, EventArgs e)
        {
            //loading
            Background = Resources.Properties.Resources.Land;
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

            openRandObjects();
            createCactusArr();
            RunGame = true;
            timer1.Enabled = true;
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

            pictureBox1.Width = this.Width = DisplayWidth;
            pictureBox1.Height = this.Height = DisplayHeight;
        }
    }
}
