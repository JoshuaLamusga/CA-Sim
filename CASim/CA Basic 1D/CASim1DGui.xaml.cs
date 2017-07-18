using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CASimulator
{
    public partial class CASim1DGui : Window
    {
        private CASim1D simulation;
        public DispatcherTimer genCounter;

        #region Gui
        private WriteableBitmap rendering;
        public List<Color> cellColors;
        public Color cellColorDefault;
        #endregion

        /// <summary>
        /// Creates a cellular automaton window, which
        /// controls the C.A. simulation with rule executed
        /// through a timer at the specified intervals.
        /// </summary>
        /// <param name="columns">Number of columns in grid.</param>
        /// <param name="cellSize">Pixel width and height per cell.</param>
        /// <param name="isToroidal">Whether the grid wraps.</param>
        /// <param name="rule">String specifying rule.</param>
        /// <param name="interval">Time between automatic updates.</param>
        public CASim1DGui(int columns, int cellSize,
            bool isToroidal, string rules, TimeSpan interval)
        {
            InitializeComponent();
            simulation = new CASim1D(columns, cellSize,
                isToroidal, rules);

            //Sizes the gui to the automaton.
            display.Width = columns * cellSize;
            display.Height = cellSize;
            display.Focus();
            window.SizeToContent = SizeToContent.WidthAndHeight;

            //Registers input controls.
            window.KeyDown += ProcessKeys;
            display.MouseLeftButtonDown += SetCellActive;
            display.MouseRightButtonDown += SetCellInactive;
            display.MouseWheel += ChangeCellValue;

            //Sets default values.
            cellColors = new List<Color>();
            cellColorDefault = Colors.White;
            genCounter = new DispatcherTimer();
            genCounter.Tag = this;
            genCounter.Interval = interval;
            genCounter.Tick += UpdateSimulation;
            rendering = new WriteableBitmap(
                columns * cellSize,
                cellSize,
                96, 96,
                PixelFormats.Bgra32,
                BitmapPalettes.BlackAndWhite);
        }

        /// <summary>
        /// Updates the simulation to the next generation.
        /// </summary>
        private void UpdateSimulation(object sender, EventArgs e)
        {
            simulation.Update();
            UpdateGui();
        }

        #region gui
        /// <summary>
        /// Left-clicking increments the cell state under the mouse.
        /// </summary>
        private void SetCellActive(object sender, MouseButtonEventArgs e)
        {
            //Gets the row and column of the clicked cell.
            Point pos = Mouse.GetPosition(display);
            pos.X = Math.Floor(pos.X / simulation.cellsize);

            //Calculates position.
            double cellNum = pos.X;

            //The cell exists.
            if (cellNum >= 0 && cellNum < simulation.cells.Count())
            {
                simulation.cells[(int)cellNum].state = 1;
            }

            UpdateGui();
        }

        /// <summary>
        /// Right-clicking decrements the cell state under the mouse.
        /// </summary>
        private void SetCellInactive(object sender, MouseButtonEventArgs e)
        {
            //Gets the row and column of the clicked cell.
            Point pos = Mouse.GetPosition(display);
            pos.X = Math.Floor(pos.X / simulation.cellsize);

            //Calculates position.
            double cellNum = pos.X;

            //The cell exists.
            if (cellNum >= 0 && cellNum < simulation.cells.Count())
            {
                simulation.cells[(int)cellNum].state = 0;
            }

            UpdateGui();
        }

        private void ChangeCellValue(object sender, MouseWheelEventArgs e)
        {
            //Gets the row and column of the clicked cell.
            Point pos = Mouse.GetPosition(display);
            pos.X = Math.Floor(pos.X / simulation.cellsize);

            //Calculates position.
            double cellNum = pos.X;

            //The cell exists.
            if (cellNum >= 0 && cellNum < simulation.cells.Count())
            {
                if (e.Delta > 0 && //scroll up
                    simulation.cells[(int)cellNum].state < byte.MaxValue)
                {
                    simulation.cells[(int)cellNum].prevstate =
                        simulation.cells[(int)cellNum].state;

                    simulation.cells[(int)cellNum].state++;
                }
                else if (e.Delta < 0 && //scroll down
                    simulation.cells[(int)cellNum].state > 0)
                {
                    simulation.cells[(int)cellNum].prevstate =
                        simulation.cells[(int)cellNum].state;

                    simulation.cells[(int)cellNum].state--;
                }

                UpdateGui();
            }
        }

        /// <summary>
        /// Processes keyboard presses.
        /// Enter: Fill cells with random states.
        /// Space: Starts and stops the simulation.
        /// Escape: Fill cells with state 0.
        /// </summary>
        private void ProcessKeys(object sender, KeyEventArgs e)
        {
            //Randomizes each cell state between 0 and 1.
            if (Keyboard.IsKeyDown(Key.Enter))
            {
                Random rng = new Random();
                for (int i = 0; i < simulation.cells.Count(); i++)
                {
                    simulation.cells[i].prevstate = simulation.cells[i].state;
                    simulation.cells[i].state = (byte)rng.Next(2);
                }
                UpdateGui();
            }

            //Starts and stops the automatic iterations.
            if (Keyboard.IsKeyDown(Key.Space))
            {
                if (genCounter.IsEnabled)
                    genCounter.Stop();
                else
                    genCounter.Start();
            }

            //Clears the state of all cells.
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                simulation.ResetGenerations();
                rendering = new WriteableBitmap(
                    simulation.columns * simulation.cellsize,
                    simulation.cellsize,
                    96, 96,
                    PixelFormats.Bgra32,
                    null);

                for (int i = 0; i < simulation.cells.Count(); i++)
                {
                    simulation.cells[i].state = 0;
                }
                UpdateGui();
            }

            if (Keyboard.IsKeyDown(Key.Right))
            {
                UpdateSimulation(null, null);
            }

            //Quickly updates 50 times.
            if (Keyboard.IsKeyDown(Key.D1) ||
                Keyboard.IsKeyDown(Key.NumPad1))
            {
                for (int i = 0; i < 50; i++)
                    simulation.Update();

                UpdateGui();
            }

            //Updates 100 times.
            if (Keyboard.IsKeyDown(Key.D2) ||
                Keyboard.IsKeyDown(Key.NumPad2))
            {
                for (int i = 0; i < 100; i++)
                    simulation.Update();

                UpdateGui();
            }

            //Slowly updates 500 times.
            if (Keyboard.IsKeyDown(Key.D3) ||
                Keyboard.IsKeyDown(Key.NumPad3))
            {
                for (int i = 0; i < 500; i++)
                    simulation.Update();

                UpdateGui();
            }
        }

        /// <summary>
        /// Updates the image and renders it to the screen.
        /// </summary>
        public void UpdateGui()
        {
            rendering.Lock();

            //Creates a new image from the first with an increased height.
            //Adds 1 to the height to work around an error. TODO: fix.
            rendering = rendering.SetSize(rendering.PixelWidth,
                simulation.cellsize * simulation.generation
                + simulation.cellsize + 1);

            for (int i = 0; i < simulation.cells.Count(); i++)
            {
                CACell1D cell = simulation.cells[i];

                if (cell.state < cellColors.Count)
                {
                    rendering.FillRectangle(
                        cell.x, simulation.cellsize * simulation.generation,
                        cell.x + simulation.cellsize,
                        simulation.cellsize * simulation.generation +
                            simulation.cellsize,
                        BitmapPalettes.BlackAndWhite.Colors[cell.state]);
                }
                else
                {
                    rendering.FillRectangle(
                        cell.x, simulation.cellsize * simulation.generation,
                        cell.x + simulation.cellsize,
                        simulation.cellsize * simulation.generation +
                            simulation.cellsize,
                        Colors.Black);
                }
            }

            //Notifies the elements to update.
            display.Source = rendering;
            display.Height = rendering.Height;
        }
        #endregion
    }
}