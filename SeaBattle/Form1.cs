using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace SeaBattle
{
    public partial class Form1 : Form
    {
        public const int MAP_SIZE = 10;
        public const int CELL_SIZE = 35;
        private string alphabet = "АБВГДЕЖЗИК";

        private int[,] myMap = new int[MAP_SIZE, MAP_SIZE];
        private int[,] enemyMap = new int[MAP_SIZE, MAP_SIZE];
        private Button[,] myButtons = new Button[MAP_SIZE, MAP_SIZE];
        private Button[,] enemyButtons = new Button[MAP_SIZE, MAP_SIZE];

        private TcpClient client;
        private TcpListener server;
        private NetworkStream stream;

        private bool isMyTurn = false;
        private bool connected = false;
        private bool shipsPlaced = false;
        private bool gameStarted = false;
        private bool isPlacingMode = true;

        private TextBox ipBox;
        private TextBox portBox;
        private Button hostButton;
        private Button joinButton;
        private Button autoPlaceButton;
        private Button resetButton;
        private Button readyButton;
        private Button rotateButton;
        private Label statusLabel;
        private Label myFieldLabel;
        private Label enemyFieldLabel;

        private Random rnd = new Random();
        private List<Ship> myShips = new List<Ship>();
        private List<Ship> enemyShips = new List<Ship>();

        private int[] shipsToPlace = { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };
        private int currentShipIndex = 0;
        private bool isHorizontal = true;
        private int currentShipSize = 4;

        public Form1()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            Text = "Морской бой";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            CreateMaps();
            CreateGUI();
            ResetGame();
            UpdateStatus("Расставьте корабли");
        }

        private void CreateMaps()
        {
            this.ClientSize = new Size(MAP_SIZE * 2 * CELL_SIZE + 100, MAP_SIZE * CELL_SIZE + 200);

            for (int i = 0; i < MAP_SIZE; i++)
            {
                for (int j = 0; j < MAP_SIZE; j++)
                {
                    Button b = new Button
                    {
                        Location = new Point(j * CELL_SIZE + 10, i * CELL_SIZE + 30),
                        Size = new Size(CELL_SIZE, CELL_SIZE),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.Azure,
                        Tag = new Point(j, i)
                    };

                    this.Controls.Add(b);
                    myButtons[i, j] = b;

                    if (i == 0 || j == 0)
                    {
                        b.BackColor = Color.LightGray;
                        b.FlatStyle = FlatStyle.Flat;
                        b.Enabled = false;
                        if (i == 0 && j > 0) b.Text = alphabet[j - 1].ToString();
                        if (j == 0 && i > 0) b.Text = i.ToString();
                    }
                }
            }

            for (int i = 0; i < MAP_SIZE; i++)
            {
                for (int j = 0; j < MAP_SIZE; j++)
                {
                    Button b = new Button
                    {
                        Location = new Point(350 + j * CELL_SIZE, i * CELL_SIZE + 30),
                        Size = new Size(CELL_SIZE, CELL_SIZE),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.Azure,
                        Tag = new Point(j, i),
                        Enabled = false
                    };

                    this.Controls.Add(b);
                    enemyButtons[i, j] = b;

                    if (i == 0 || j == 0)
                    {
                        b.BackColor = Color.LightGray;
                        b.FlatStyle = FlatStyle.Flat;
                        b.Enabled = false;
                        if (i == 0 && j > 0) b.Text = alphabet[j - 1].ToString();
                        if (j == 0 && i > 0) b.Text = i.ToString();
                    }
                }
            }
        }

        private void CreateGUI()
        {
            myFieldLabel = new Label
            {
                Text = "Ваше поле",
                Location = new Point(10, 5),
                Size = new Size(100, 20),
                Font = new Font("Arial", 9, FontStyle.Bold)
            };

            enemyFieldLabel = new Label
            {
                Text = "Поле противника",
                Location = new Point(350, 5),
                Size = new Size(150, 20),
                Font = new Font("Arial", 9, FontStyle.Bold)
            };

            ipBox = new TextBox
            {
                Location = new Point(10, MAP_SIZE * CELL_SIZE + 40),
                Width = 100,
                Text = "127.0.0.1",
                Enabled = true
            };

            portBox = new TextBox
            {
                Location = new Point(120, MAP_SIZE * CELL_SIZE + 40),
                Width = 60,
                Text = "8888",
                Enabled = true
            };

            hostButton = new Button
            {
                Text = "Создать игру",
                Location = new Point(190, MAP_SIZE * CELL_SIZE + 40),
                Width = 120
            };

            joinButton = new Button
            {
                Text = "Подключиться",
                Location = new Point(320, MAP_SIZE * CELL_SIZE + 40),
                Width = 120
            };

            autoPlaceButton = new Button
            {
                Text = "Авторасстановка",
                Location = new Point(10, MAP_SIZE * CELL_SIZE + 75),
                Width = 120,
                Enabled = true
            };

            resetButton = new Button
            {
                Text = "Сбросить",
                Location = new Point(140, MAP_SIZE * CELL_SIZE + 75),
                Width = 80,
                Enabled = true
            };

            readyButton = new Button
            {
                Text = "Готов к бою",
                Location = new Point(230, MAP_SIZE * CELL_SIZE + 75),
                Width = 100,
                Enabled = false
            };

            rotateButton = new Button
            {
                Text = "Повернуть корабль",
                Location = new Point(340, MAP_SIZE * CELL_SIZE + 75),
                Width = 120
            };

            statusLabel = new Label
            {
                Location = new Point(10, MAP_SIZE * CELL_SIZE + 115),
                Size = new Size(450, 30),
                Font = new Font("Arial", 10, FontStyle.Bold),
                Text = "Расставьте корабли",
                ForeColor = Color.Blue
            };

            this.Controls.AddRange(new Control[]
            {
                myFieldLabel, enemyFieldLabel, ipBox, portBox, hostButton, joinButton,
                autoPlaceButton, resetButton, readyButton, rotateButton, statusLabel
            });

            hostButton.Click += HostGame;
            joinButton.Click += JoinGame;
            autoPlaceButton.Click += AutoPlaceShipsClick;
            resetButton.Click += ResetGameClick;
            readyButton.Click += ReadyButtonClick;
            rotateButton.Click += RotateButtonClick;

            SetupButtonHandlers();
        }

        private void SetupButtonHandlers()
        {
            for (int i = 1; i < MAP_SIZE; i++)
            {
                for (int j = 1; j < MAP_SIZE; j++)
                {
                    if (myButtons[i, j] != null)
                    {
                        myButtons[i, j].Click += PlaceShipClick;
                    }
                }
            }

            for (int i = 1; i < MAP_SIZE; i++)
            {
                for (int j = 1; j < MAP_SIZE; j++)
                {
                    if (enemyButtons[i, j] != null)
                    {
                        enemyButtons[i, j].Click += EnemyFieldClick;
                    }
                }
            }
        }

        private void RotateButtonClick(object sender, EventArgs e)
        {
            isHorizontal = !isHorizontal;
            UpdateStatus(isHorizontal ? "Горизонтальная ориентация" : "Вертикальная ориентация");
        }

        private void PlaceShipClick(object sender, EventArgs e)
        {
            if (!isPlacingMode || currentShipIndex >= shipsToPlace.Length) return;

            Button b = sender as Button;
            if (b == null) return;

            Point pos = (Point)b.Tag;
            int x = pos.X;
            int y = pos.Y;

            if (x < 1 || y < 1) return;

            if (CanPlaceShip(x, y, currentShipSize, isHorizontal))
            {
                PlaceShip(x, y, currentShipSize, isHorizontal);
                currentShipIndex++;

                if (currentShipIndex < shipsToPlace.Length)
                {
                    currentShipSize = shipsToPlace[currentShipIndex];
                    UpdateStatus($"Разместите {currentShipSize}-палубный корабль. Осталось: {shipsToPlace.Length - currentShipIndex}");
                }
                else
                {
                    shipsPlaced = true;
                    readyButton.Enabled = true;
                    UpdateStatus("Все корабли расставлены. Нажмите 'Готов к бою'");
                }
            }
            else
            {
                UpdateStatus("Невозможно разместить корабль здесь!");
            }
        }

        private void AutoPlaceShipsClick(object sender, EventArgs e)
        {
            ResetMyMap();
            myShips.Clear();

            foreach (int size in shipsToPlace)
            {
                bool placed = false;
                int attempts = 0;

                while (!placed && attempts < 100)
                {
                    int x = rnd.Next(1, MAP_SIZE);
                    int y = rnd.Next(1, MAP_SIZE);
                    bool hor = rnd.Next(2) == 0;

                    if (CanPlaceShip(x, y, size, hor))
                    {
                        PlaceShip(x, y, size, hor);
                        placed = true;
                    }
                    attempts++;
                }

                if (!placed)
                {
                    UpdateStatus("Не удалось автоматически разместить корабли. Попробуйте снова.");
                    ResetMyMap();
                    return;
                }
            }

            currentShipIndex = shipsToPlace.Length;
            shipsPlaced = true;
            readyButton.Enabled = true;
            UpdateStatus("Корабли расставлены автоматически. Нажмите 'Готов к бою'");
        }

        private void PlaceShip(int x, int y, int size, bool horizontal)
        {
            List<Point> cells = new List<Point>();

            for (int i = 0; i < size; i++)
            {
                int nx = horizontal ? x + i : x;
                int ny = horizontal ? y : y + i;

                if (nx < MAP_SIZE && ny < MAP_SIZE)
                {
                    myMap[ny, nx] = 1;
                    if (myButtons[ny, nx] != null)
                    {
                        myButtons[ny, nx].BackColor = Color.DarkBlue;
                    }
                    cells.Add(new Point(nx, ny));
                }
            }

            myShips.Add(new Ship(cells));
        }

        private bool CanPlaceShip(int x, int y, int size, bool horizontal)
        {
            if (horizontal)
            {
                if (x + size >= MAP_SIZE) return false;
            }
            else
            {
                if (y + size >= MAP_SIZE) return false;
            }

            for (int i = -1; i <= size; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int checkX = horizontal ? x + i : x + j;
                    int checkY = horizontal ? y + j : y + i;

                    if (checkX >= 1 && checkX < MAP_SIZE &&
                        checkY >= 1 && checkY < MAP_SIZE)
                    {
                        if (myMap[checkY, checkX] == 1)
                            return false;
                    }
                }
            }
            return true;
        }

        private void EnemyFieldClick(object sender, EventArgs e)
        {
            if (!connected || !gameStarted || !isMyTurn) return;

            Button b = sender as Button;
            if (b == null) return;

            Point pos = (Point)b.Tag;
            int x = pos.X;
            int y = pos.Y;

            if (enemyMap[y, x] != 0) return;

            SendMessage(new GameMessage
            {
                Type = "shot",
                X = x,
                Y = y
            });

            b.Enabled = false;

            isMyTurn = false;
            UpdateStatus("Ожидание результата выстрела...");
        }

        private void ProcessShotResult(int x, int y, bool hit, bool sunk)
        {
            if (x < 1 || y < 1 || x >= MAP_SIZE || y >= MAP_SIZE) return;

            if (hit)
            {
                enemyMap[y, x] = 2; 
                if (enemyButtons[y, x] != null)
                {
                    enemyButtons[y, x].BackColor = Color.Red;
                    enemyButtons[y, x].Text = "X";
                    enemyButtons[y, x].Enabled = false;
                }

                if (sunk)
                {
                    MarkAroundCell(x, y, true);
                }

                isMyTurn = true;
                UpdateStatus("Попадание! Ваш ход снова");
                EnableEnemyField(true);
            }
            else
            {
                enemyMap[y, x] = 3; 
                if (enemyButtons[y, x] != null)
                {
                    enemyButtons[y, x].BackColor = Color.LightGray;
                    enemyButtons[y, x].Text = "•";
                    enemyButtons[y, x].Enabled = false;
                }

                isMyTurn = false;
                UpdateStatus("Промах. Ход противника");
                EnableEnemyField(false);
            }
        }

        private void HandleIncomingShot(int x, int y)
        {
            if (x < 1 || y < 1 || x >= MAP_SIZE || y >= MAP_SIZE) return;

            bool hit = myMap[y, x] == 1;
            bool sunk = false;

            if (hit)
            {
                myMap[y, x] = 2;
                if (myButtons[y, x] != null)
                {
                    myButtons[y, x].BackColor = Color.Red;
                    myButtons[y, x].Text = "X";
                }

                foreach (var ship in myShips)
                {
                    if (ship.Contains(x, y))
                    {
                        ship.Hit(x, y);
                        if (ship.IsSunk())
                        {
                            sunk = true;
                            MarkAroundShip(ship, false);
                        }
                        break;
                    }
                }

                CheckLose();
            }
            else
            {
                myMap[y, x] = 3; 
                if (myButtons[y, x] != null)
                {
                    myButtons[y, x].BackColor = Color.LightGray;
                    myButtons[y, x].Text = "•";
                }
            }

            SendMessage(new GameMessage
            {
                Type = "shot_result",
                X = x,
                Y = y,
                Hit = hit,
                Sunk = sunk
            });

            if (!hit)
            {
                isMyTurn = true;
                UpdateStatus("Противник промахнулся. Ваш ход!");
                EnableEnemyField(true);
            }
            else
            {
                UpdateStatus("Противник попал!");
                EnableEnemyField(false);
            }
        }

        private void MarkAroundShip(Ship ship, bool isEnemy)
        {
            foreach (var cell in ship.Cells)
            {
                MarkAroundCell(cell.X, cell.Y, isEnemy);
            }
        }

        private void MarkAroundCell(int x, int y, bool isEnemy)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = x + dx;
                    int ny = y + dy;

                    if (nx >= 1 && nx < MAP_SIZE && ny >= 1 && ny < MAP_SIZE)
                    {
                        if (isEnemy)
                        {
                            if (enemyMap[ny, nx] == 0)
                            {
                                enemyMap[ny, nx] = 4; 
                                if (enemyButtons[ny, nx] != null)
                                {
                                    enemyButtons[ny, nx].BackColor = Color.LightSlateGray;
                                    enemyButtons[ny, nx].Enabled = false;
                                }
                            }
                        }
                        else
                        {
                            if (myMap[ny, nx] == 0)
                            {
                                myMap[ny, nx] = 4;
                                if (myButtons[ny, nx] != null)
                                {
                                    myButtons[ny, nx].BackColor = Color.LightSlateGray;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CheckLose()
        {
            bool allSunk = true;
            foreach (var ship in myShips)
            {
                if (!ship.IsSunk())
                {
                    allSunk = false;
                    break;
                }
            }

            if (allSunk)
            {
                UpdateStatus("ПОРАЖЕНИЕ! Все ваши корабли уничтожены!");
                gameStarted = false;
                DisableAllButtons();
                MessageBox.Show("Вы проиграли! Все ваши корабли потоплены.", "Конец игры", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void HostGame(object sender, EventArgs e)
        {
            try
            {
                server = new TcpListener(IPAddress.Any, int.Parse(portBox.Text));
                server.Start();

                hostButton.Enabled = false;
                joinButton.Enabled = false;
                ipBox.Enabled = false;
                portBox.Enabled = false;

                UpdateStatus("Ожидание подключения соперника...");

                client = await server.AcceptTcpClientAsync();
                stream = client.GetStream();
                connected = true;

                UpdateStatus("Соперник подключен! Ожидайте начала игры...");
                StartListening();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка: {ex.Message}");
                MessageBox.Show($"Ошибка создания игры: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void JoinGame(object sender, EventArgs e)
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync(ipBox.Text, int.Parse(portBox.Text));
                stream = client.GetStream();
                connected = true;

                hostButton.Enabled = false;
                joinButton.Enabled = false;
                ipBox.Enabled = false;
                portBox.Enabled = false;

                UpdateStatus("Подключено к серверу! Ожидайте начала игры...");
                StartListening();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка подключения: {ex.Message}");
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void StartListening()
        {
            await Task.Run(async () =>
            {
                byte[] buffer = new byte[1024];

                while (connected)
                {
                    try
                    {
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;

                        string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        var message = JsonConvert.DeserializeObject<GameMessage>(json);

                        this.Invoke(new Action(() => ProcessNetworkMessage(message)));
                    }
                    catch (Exception ex)
                    {
                        this.Invoke(new Action(() =>
                        {
                            UpdateStatus($"Ошибка соединения: {ex.Message}");
                            connected = false;
                        }));
                        break;
                    }
                }
            });
        }

        private void ProcessNetworkMessage(GameMessage msg)
        {
            if (msg == null) return;

            switch (msg.Type)
            {
                case "ready":
                    if (shipsPlaced && !gameStarted)
                    {
                        gameStarted = true;
                        isMyTurn = msg.IsHost;
                        EnableEnemyField(isMyTurn);
                        UpdateStatus(isMyTurn ? "Игра началась! Ваш ход" : "Игра началась! Ход противника");
                    }
                    break;

                case "shot":
                    HandleIncomingShot(msg.X, msg.Y);
                    break;

                case "shot_result":
                    ProcessShotResult(msg.X, msg.Y, msg.Hit, msg.Sunk);
                    break;
            }
        }

        private void SendMessage(GameMessage msg)
        {
            if (!connected || stream == null) return;

            try
            {
                string json = JsonConvert.SerializeObject(msg);
                byte[] data = Encoding.UTF8.GetBytes(json);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка отправки: {ex.Message}");
            }
        }

    public class GameMessage
    {
        public string Type { get; set; } // "ready", "shot", "shot_result"
        public int X { get; set; }
        public int Y { get; set; }
        public bool Hit { get; set; }
        public bool Sunk { get; set; }
        public bool IsHost { get; set; }
    }

    public class Ship
    {
        public List<Point> Cells { get; private set; }
        public List<bool> Hits { get; private set; }

        public Ship(List<Point> cells)
        {
            Cells = cells;
            Hits = new List<bool>();
            for (int i = 0; i < cells.Count; i++)
            {
                Hits.Add(false);
            }
        }

        public bool Contains(int x, int y)
        {
            return Cells.Any(c => c.X == x && c.Y == y);
        }

        public void Hit(int x, int y)
        {
            for (int i = 0; i < Cells.Count; i++)
            {
                if (Cells[i].X == x && Cells[i].Y == y)
                {
                    Hits[i] = true;
                    break;
                }
            }
        }

        public bool IsSunk()
        {
            return Hits.All(h => h);
        }
    }
}