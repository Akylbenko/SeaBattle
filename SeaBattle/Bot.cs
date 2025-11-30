using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace SeaBattle
{
    internal class Bot
    {
        public int[,] myMap = new int[Form1.mapSize, Form1.cellSize];
        public int[,] enemyMap = new int[Form1.mapSize, Form1.cellSize];

        public Button[,] myButtons = new Button[Form1.mapSize, Form1.cellSize];
        public Button[,] enemyButtons = new Button[Form1.mapSize, Form1.cellSize];

        public Bot(int[,] myMap, int[,] enemyMap, Button[,] myButtons, Button[,] enemyButtons)
        {
            this.myMap = myMap;
            this.enemyMap = enemyMap;
            this.myButtons = myButtons;
            this.enemyButtons = enemyButtons;
        }
    }
}
