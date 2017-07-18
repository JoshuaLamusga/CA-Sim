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
    public partial class CASim2DGui : Window
    {
        private CASim2D simulation;
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
        /// <param name="rows">Number of rows in grid.</param>
        /// <param name="columns">Number of columns in grid.</param>
        /// <param name="cellSize">Pixel width and height per cell.</param>
        /// <param name="isToroidal">Whether the grid wraps.</param>
        /// <param name="rule">String specifying rule.</param>
        /// <param name="interval">Time between automatic updates.</param>
        public CASim2DGui(int rows, int columns, int cellSize,
            bool isToroidal, string rules, TimeSpan interval)
        {
            InitializeComponent();
            simulation = new CASim2D(rows, columns, cellSize,
                isToroidal, rules);

            //Sizes the gui to the automaton.
            display.Width = columns * cellSize;
            display.Height = rows * cellSize;
            display.Focus();
            window.SizeToContent = SizeToContent.WidthAndHeight;

            //Registers input controls.
            window.KeyDown += ProcessKeys;
            window.MouseLeftButtonDown += SetCellActive;
            window.MouseRightButtonDown += SetCellInactive;
            window.MouseWheel += ChangeCellValue;

            //Sets default values.
            cellColors = new List<Color>();
            cellColorDefault = Colors.White;
            genCounter = new DispatcherTimer();
            genCounter.Tag = this;
            genCounter.Interval = interval;
            genCounter.Tick += UpdateSimulation;
            rendering = new WriteableBitmap(
                columns * cellSize,
                rows * cellSize,
                96, 96,
                PixelFormats.Bgra32,
                null); //todo: consider optimizations with palettes.
        }

        /// <summary>
        /// Updates the simulation to the next generation.
        /// </summary>
        private void UpdateSimulation(object sender, EventArgs e)
        {
            simulation.Update();
            UpdateGui(false);
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
            pos.Y = Math.Floor(pos.Y / simulation.cellsize);

            //Calculates position.
            double cellNum = pos.X + (pos.Y * simulation.rows);

            //The cell exists.
            if (cellNum >= 0 && cellNum < simulation.cells.Count())
            {
                simulation.cells[(int)cellNum].state = 1;
            }

            UpdateGui(true);
        }

        /// <summary>
        /// Right-clicking decrements the cell state under the mouse.
        /// </summary>
        private void SetCellInactive(object sender, MouseButtonEventArgs e)
        {
            //Gets the row and column of the clicked cell.
            Point pos = Mouse.GetPosition(display);
            pos.X = Math.Floor(pos.X / simulation.cellsize);
            pos.Y = Math.Floor(pos.Y / simulation.cellsize);

            //Calculates position.
            double cellNum = pos.X + (pos.Y * simulation.rows);

            //The cell exists.
            if (cellNum >= 0 && cellNum < simulation.cells.Count())
            {
                simulation.cells[(int)cellNum].state = 0;
            }

            UpdateGui(true);
        }

        private void ChangeCellValue(object sender, MouseWheelEventArgs e)
        {
            //Gets the row and column of the clicked cell.
            Point pos = Mouse.GetPosition(display);
            pos.X = Math.Floor(pos.X / simulation.cellsize);
            pos.Y = Math.Floor(pos.Y / simulation.cellsize);

            //Calculates position.
            double cellNum = pos.X + (pos.Y * simulation.rows);

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

                UpdateGui(true);
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
                UpdateGui(true);
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
                for (int i = 0; i < simulation.cells.Count(); i++)
                {
                    simulation.cells[i].state = 0;
                }
                UpdateGui(true);
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

                UpdateGui(true);
            }

            //Updates 100 times.
            if (Keyboard.IsKeyDown(Key.D2) ||
                Keyboard.IsKeyDown(Key.NumPad2))
            {
                for (int i = 0; i < 100; i++)
                    simulation.Update();

                UpdateGui(true);
            }

            //Slowly updates 500 times.
            if (Keyboard.IsKeyDown(Key.D3) ||
                Keyboard.IsKeyDown(Key.NumPad3))
            {
                for (int i = 0; i < 500; i++)
                    simulation.Update();

                UpdateGui(true);
            }
        }

        /// <summary>
        /// Updates the image and renders it to the screen.
        /// </summary>
        public void UpdateGui(bool doUpdateAll)
        {
            rendering.Lock();
            
            if (doUpdateAll)
            {
                //Renders all cells.
                for (int i = 0; i < simulation.cells.Count(); i++)
                {
                    CACell2D cell = simulation.cells[i];

                    if (cell.state < cellColors.Count)
                    {
                        rendering.FillRectangle(
                            cell.x, cell.y,
                            cell.x + simulation.cellsize,
                            cell.y + simulation.cellsize,
                            cellColors[cell.state]);
                    }
                    else
                    {
                        rendering.FillRectangle(
                            cell.x, cell.y,
                            cell.x + simulation.cellsize,
                            cell.y + simulation.cellsize,
                            cellColorDefault);
                    }
                }
            }
            else
            {
                //Renders only cells that need to update.
                for (int i = 0; i < simulation.cellsToUpdate.Count(); i++)
                {
                    CACell2D cell = simulation.cellsToUpdate[i];

                    if (cell.state < cellColors.Count)
                    {
                        rendering.FillRectangle(
                            cell.x, cell.y,
                            cell.x + simulation.cellsize,
                            cell.y + simulation.cellsize,
                            cellColors[cell.state]);
                    }
                    else
                    {
                        rendering.FillRectangle(
                            cell.x, cell.y,
                            cell.x + simulation.cellsize,
                            cell.y + simulation.cellsize,
                            cellColorDefault);
                    }
                }
            }

            rendering.Unlock();
            display.Source = rendering;

            //Clears the old list to update.
            simulation.cellsToUpdate.Clear();
        }
        #endregion
    }
}