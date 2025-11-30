using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SeaBattle
{
    public partial class Form1 : Form
    {
        public const int mapSize = 10;
        public const int cellSize = 30;
        public string alphabet = "АБВГДЕЖЗИК";

        public int[,] myMap = new int[mapSize, cellSize];
        public int[,] enemyMap = new int[mapSize, cellSize];

        public Button[,] myButtons = new Button[mapSize, cellSize];
        public Button[,] enemyButtons = new Button[mapSize, cellSize];

        public bool isPlaying = false;

        public Form1()
        {
            InitializeComponent();
            this.Text = "Морской бой";
            Init();
        }

        public void Init()
        {
            CreateMaps();
        }

        public void CreateMaps()
        {
            this.Width = mapSize * 2 * cellSize + 70;
            this.Height = mapSize * cellSize + 170;
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    myMap[i, j] = 0;

                    Button button = new Button();
                    button.Location = new Point(j * cellSize, i * cellSize);
                    button.Size = new Size(cellSize, cellSize);
                    this.Controls.Add(button);
                    myButtons[i, j] = button;
                    if (i == 0 || j == 0)
                    {
                        button.BackColor = Color.Gray;
                        if (i == 0 && j > 0)
                        {
                            button.Text = alphabet[j - 1].ToString();
                        }
                        if (j == 0 && i > 0)
                        {
                            button.Text = i.ToString();
                        }
                    } else
                    {
                        button.Click += ConfigureShips;
                    }

                }
            }

            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    enemyMap[i, j] = 0;

                    Button button = new Button();
                    button.Location = new Point(320 + j * cellSize, i * cellSize);
                    button.Size = new Size(cellSize, cellSize);
                    this.Controls.Add(button);
                    enemyButtons[i, j] = button;
                    if (i == 0 || j == 0)
                    {
                        button.BackColor = Color.Gray;
                        if (i == 0 && j > 0)
                        {
                            button.Text = alphabet[j - 1].ToString();
                        }
                        if (j == 0 && i > 0)
                        {
                            button.Text = i.ToString();
                        }
                    } else
                    {
                        button.Click += PlayerShoot;
                    }
                }
            }

            Label myMapLabel = new Label();
            myMapLabel.Location = new Point(mapSize * cellSize / 2 - 30, mapSize * cellSize + 10);
            myMapLabel.Text = "Карта игрока";
            this.Controls.Add(myMapLabel);

            Label enemyMapLabel = new Label();
            enemyMapLabel.Location = new Point(mapSize * cellSize / 2 + 290, mapSize * cellSize + 10);
            enemyMapLabel.Text = "Карта противника";
            this.Controls.Add(enemyMapLabel);

            Button startButton = new Button();
            startButton.Location = new Point(0, mapSize * cellSize + 30);
            startButton.Size = new Size(120, 30);
            startButton.Text = "Играть!";
            startButton.Click += Start;
            this.Controls.Add(startButton);
        }

        public void Start(object sender, EventArgs e)
        {
            isPlaying = true;
        }

        public void ConfigureShips(object sender, EventArgs e)
        {   
            Button pressedButton = sender as Button;
            if (!isPlaying)
            {
                if (myMap[pressedButton.Location.X / cellSize, pressedButton.Location.Y / cellSize] == 0)
                {
                    pressedButton.BackColor = Color.Red;
                    myMap[pressedButton.Location.X / cellSize, pressedButton.Location.Y / cellSize] = 1;
                } else
                {
                    pressedButton.BackColor = Color.White;
                    myMap[pressedButton.Location.X / cellSize, pressedButton.Location.Y / cellSize] = 0;
                }
            }
        }

        public void PlayerShoot(object sender, EventArgs e)
        {
            Button pressedButton = sender as Button;
            Shoot(enemyMap, pressedButton);
        }

        public bool Shoot(int[,] map, Button pressedButton)
        {
            bool hit = false;
            if (isPlaying)
            {   
                int delta = 0;
                if (pressedButton.Location.X > 320)
                {
                    delta = 320;
                }
                if (map[(pressedButton.Location.X - delta) / cellSize, pressedButton.Location.Y / cellSize] != 0)
                {
                    hit = true;

                    pressedButton.BackColor = Color.Blue;
                    pressedButton.Text = "X";
                }
                else
                {
                    hit = false;
                    pressedButton.BackColor = Color.Black;
                }
            }
            return hit;
        }
    }
}
