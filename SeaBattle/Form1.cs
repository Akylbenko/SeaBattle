using System;
using System.Drawing;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace SeaBattle
{
    public partial class Form1 : Form
    {
        public const int mapSize = 10;
        public const int cellSize = 30;
        public string alphabet = "АБВГДЕЖЗИК";

        public int[,] myMap = new int[mapSize, mapSize];
        public int[,] enemyMap = new int[mapSize, mapSize];

        public Button[,] myButtons = new Button[mapSize, mapSize];
        public Button[,] enemyButtons = new Button[mapSize, mapSize];

        TcpClient client;
        TcpListener server;
        NetworkStream stream;

        bool isMyTurn = false;
        bool connected = false;

        TextBox ipBox;
        TextBox portBox;
        Button hostButton;
        Button joinButton;
        Label statusLabel;

        public Form1()
        {
            InitializeComponent();
            this.Text = "Морской бой";
            Init();
        }

        public void Init()
        {
            CreateGUI();
            CreateMaps();
        }

        private void CreateGUI()
        {
            ipBox = new TextBox();
            ipBox.Location = new Point(10, mapSize * cellSize + 40);
            ipBox.Text = "127.0.0.1";
            this.Controls.Add(ipBox);

            portBox = new TextBox();
            portBox.Location = new Point(120, mapSize * cellSize + 40);
            portBox.Text = "9000";
            this.Controls.Add(portBox);

            hostButton = new Button();
            hostButton.Text = "Host";
            hostButton.Location = new Point(10, mapSize * cellSize + 70);
            hostButton.Click += HostGame;
            this.Controls.Add(hostButton);

            joinButton = new Button();
            joinButton.Text = "Join";
            joinButton.Location = new Point(100, mapSize * cellSize + 70);
            joinButton.Click += JoinGame;
            this.Controls.Add(joinButton);

            statusLabel = new Label();
            statusLabel.Location = new Point(10, mapSize * cellSize + 110);
            statusLabel.Size = new Size(300, 25);
            statusLabel.Text = "Не подключено";
            this.Controls.Add(statusLabel);
        }

        public void CreateMaps()
        {
            this.Width = mapSize * 2 * cellSize + 70;
            this.Height = mapSize * cellSize + 200;

            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    myMap[i, j] = 0;

                    Button b = new Button();
                    b.Location = new Point(j * cellSize, i * cellSize);
                    b.Size = new Size(cellSize, cellSize);
                    this.Controls.Add(b);
                    myButtons[i, j] = b;

                    if (i == 0 || j == 0)
                    {
                        b.BackColor = Color.Gray;
                        if (i == 0 && j > 0) b.Text = alphabet[j - 1].ToString();
                        if (j == 0 && i > 0) b.Text = i.ToString();
                    }
                }
            }

            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    enemyMap[i, j] = 0;

                    Button b = new Button();
                    b.Location = new Point(350 + j * cellSize, i * cellSize);
                    b.Size = new Size(cellSize, cellSize);
                    this.Controls.Add(b);
                    enemyButtons[i, j] = b;

                    if (i == 0 || j == 0)
                    {
                        b.BackColor = Color.Gray;
                        if (i == 0 && j > 0) b.Text = alphabet[j - 1].ToString();
                        if (j == 0 && i > 0) b.Text = i.ToString();
                    }
                    else
                    {
                        b.Click += PlayerShoot;
                    }
                }
            }
        }

        private async void HostGame(object sender, EventArgs e)
        {
            int port = int.Parse(portBox.Text);
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            statusLabel.Text = "Ожидание подключения...";
            client = await server.AcceptTcpClientAsync();
            stream = client.GetStream();
            connected = true;

            isMyTurn = true;
            statusLabel.Text = "Игрок подключён. Ваш ход.";

            StartListening();
        }

        private async void JoinGame(object sender, EventArgs e)
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync(ipBox.Text, int.Parse(portBox.Text));
                stream = client.GetStream();
                connected = true;

                isMyTurn = false;
                statusLabel.Text = "Подключено. Ход противника.";

                StartListening();
            }
            catch
            {
                statusLabel.Text = "Ошибка подключения";
            }
        }

        private async void StartListening()
        {
            await Task.Run(async () =>
            {
                byte[] buffer = new byte[1024];

                while (connected)
                {
                    int bytes = 0;
                    try
                    {
                        bytes = await stream.ReadAsync(buffer, 0, buffer.Length);
                    }
                    catch { break; }

                    if (bytes == 0) break;

                    string json = Encoding.UTF8.GetString(buffer, 0, bytes);
                    var msg = JsonConvert.DeserializeObject<Message>(json);

                    if (msg.Type == "shot")
                    {
                        Invoke(new Action(() =>
                        {
                            HandleIncomingShot(msg.X, msg.Y);
                        }));
                    }
                }
            });
        }

        private void PlayerShoot(object sender, EventArgs e)
        {
            if (!isMyTurn || !connected) return;

            Button b = sender as Button;

            int x = (b.Location.X - 350) / cellSize;
            int y = b.Location.Y / cellSize;

            SendMessage(new Message { Type = "shot", X = x, Y = y });
            isMyTurn = false;
            statusLabel.Text = "Ход противника";
        }

        private void HandleIncomingShot(int x, int y)
        {
            myButtons[y, x].BackColor = Color.Black;
            isMyTurn = true;
            statusLabel.Text = "Ваш ход";
        }

        public class Message
        {
            public string Type;
            public int X;
            public int Y;
        }

        private void SendMessage(Message msg)
        {
            string json = JsonConvert.SerializeObject(msg);
            byte[] data = Encoding.UTF8.GetBytes(json);
            stream.Write(data, 0, data.Length);
        }
    }
}
