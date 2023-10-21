using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace денис
{

    public partial class Form1 : Form
    {
        private const int gridSize = 30; // Размер клетки
        private int numRows, numCols;
        public int[,] grid;
        private Point startLocation, endLocation;
        private List<Point> path;

        public Form1()
        {
            InitializeComponent();
            InitializeGrid();
            MouseClick += (sender, e) => OnCellClick(sender, e);
        }


        private void InitializeGrid()
        {
            numRows = ClientSize.Height / gridSize;
            numCols = ClientSize.Width / gridSize;

            grid = new int[numRows, numCols];
            path = new List<Point>();

            // Инициализируйте поле, где 0 - пустая клетка, 1 - начало пути, 2 - конец пути, 3 - стена

            startLocation = new Point(1, 1); // Начальная позиция
            endLocation = new Point(numCols - 2, numRows - 2); // Конечная позиция

            // Реализуйте отрисовку поля и элементов на нем
            Paint += (sender, e) => DrawGrid(e.Graphics);
        }

        private void DrawGrid(Graphics g)
        {
            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < numCols; col++)
                {
                    Rectangle rect = new Rectangle(col * gridSize, row * gridSize, gridSize, gridSize);

                    if (grid[row, col] == 1)
                    {
                        g.FillRectangle(Brushes.Green, rect); // Начало пути
                    }
                    else if (grid[row, col] == 2)
                    {
                        g.FillRectangle(Brushes.Red, rect); // Конец пути
                    }
                    else if (grid[row, col] == 3)
                    {
                        g.FillRectangle(Brushes.Black, rect); // Стена
                    }
                    else
                    {
                        g.DrawRectangle(Pens.Gray, rect);
                    }
                }
            }
        }
        private void DrawRoad(Graphics g, List<(int, int)> roadc)
        {
            for (int i = 0; i < roadc.Count; i++)
            {
                Rectangle rect = new Rectangle(roadc[i].Item2 * gridSize, roadc[i].Item1 * gridSize, gridSize, gridSize);
                g.FillRectangle(Brushes.Yellow, rect); // Начало пути
            }
        }

        private void OnCellClick(object sender, MouseEventArgs e)
        {
            int col = e.X / gridSize;
            int row = e.Y / gridSize;

            if (e.Button == MouseButtons.Left)
            {
                grid[row, col] = 1; // Начало пути
                startLocation = new Point(col, row);
            }
            else if (e.Button == MouseButtons.Right)
            {
                grid[row, col] = 2; // Конец пути
                endLocation = new Point(col, row);
            }
            else if (e.Button == MouseButtons.Middle)
            {
                grid[row, col] = 3; // Стена
            }
            
            Invalidate();
        }

        private void FindPath()
        {
            // Реализуйте алгоритм поиска пути (например, A* или Dijkstra) и сохраните найденный путь в переменной path.
            // Перемещение робота по пути также может быть реализовано с использованием таймера и визуализации на поле.
            var start = (1, 1); 
            var end = (2, 2);
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                {
                    for (int j = 0; j < grid.GetLength(1); j++)
                    {
                        if (grid[i, j] == 1) (start, grid[i, j])  = ((i, j), 0);
                        if (grid[i, j] == 2) (end, grid[i, j]) = ((i, j), 0);
                    }
                }
            }
            
            int[,] distance = WaveAlgorithm.FindShortestPath(grid, start, end);
            if (distance != null)
            {
                List<(int, int)> pathCoordinates = WaveAlgorithm.GetShortestPathCoordinates(distance, start, end);
                Paint += (sender, e) => DrawRoad(e.Graphics, pathCoordinates); 
                Invalidate();            }
            else
            {
                Console.WriteLine("Кратчайший путь не найден.");
            }
        }

        private Color GetCellColor(int cellValue)
        {
            switch (cellValue)
            {
                case 1:
                    return Color.Green; // Начало пути
                case 2:
                    return Color.Red; // Конец пути
                case 3:
                    return Color.Black; // Стена
                case 4:
                    return Color.Blue; // Путь
                default:
                    return Color.White; // Пустая клетка
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FindPath();
        }
    }

    class WaveAlgorithm
    {
        // Реализация волнового алгоритма
        public static int[,] FindShortestPath(int[,] grid, (int startX, int startY) start, (int endX, int endY) end)
        {
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);

            int[,] distance = new int[rows, cols];
            int wave = 0;

            Queue<(int, int)> queue = new Queue<(int, int)>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                int count = queue.Count;
                for (int i = 0; i < count; i++)
                {
                    var current = queue.Dequeue();

                    if (current == end)
                    {
                        // Путь найден
                        return distance;
                    }

                    int x = current.Item1;
                    int y = current.Item2;

                    // Соседние клетки
                    int[] dx = { -1, 1, 0, 0 };
                    int[] dy = { 0, 0, -1, 1 };

                    for (int j = 0; j < 4; j++)
                    {
                        int newX = x + dx[j];
                        int newY = y + dy[j];

                        if (newX >= 0 && newX < rows && newY >= 0 && newY < cols && grid[newX, newY] == 0 &&
                            distance[newX, newY] == 0)
                        {
                            distance[newX, newY] = wave + 1;
                            queue.Enqueue((newX, newY));
                        }
                    }
                }

                wave++;
            }

            // Путь не найден
            return null;
        }

        // Получить координаты кратчайшего пути
        public static List<(int, int)> GetShortestPathCoordinates(int[,] distance, (int startX, int startY) start,
            (int endX, int endY) end)
        {
            List<(int, int)> path = new List<(int, int)>();
            if (distance == null)
            {
                return path; // Путь не найден
            }

            int x = end.endX;
            int y = end.endY;

            while (x != start.startX || y != start.startY)
            {
                path.Add((x, y));

                // Найдем соседнюю ячейку с наименьшим значением distance
                int[] dx = { -1, 1, 0, 0 };
                int[] dy = { 0, 0, -1, 1 };
                int minDistance = int.MaxValue;
                int nextX = x;
                int nextY = y;

                for (int i = 0; i < 4; i++)
                {
                    int newX = x + dx[i];
                    int newY = y + dy[i];

                    if (newX >= 0 && newX < distance.GetLength(0) && newY >= 0 && newY < distance.GetLength(1) &&
                        distance[newX, newY] < minDistance && distance[newX, newY] > 0)
                    {
                        minDistance = distance[newX, newY];
                        nextX = newX;
                        nextY = newY;
                    }
                }

                x = nextX;
                y = nextY;
            }

            path.Add(start);

            // Путь построен в обратном порядке, так что перевернем его
            path.Reverse();

            return path;
        }
    }
}