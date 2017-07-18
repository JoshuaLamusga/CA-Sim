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
    public partial class CASim2DAntGui : Window
    {
        private CASim2DAnt simulation;
        public DispatcherTimer genCounter;

        #region Gui
        private WriteableBitmap rendering;
        public List<Color> cellColors;
        public List<Color> antColors;
        public Color cellColorDefault;
        public Color antColorDefault;
        #endregion

        /// <summary>
        /// Creates a cellular automaton window, which
        /// controls the C.A. simulation with rules executed
        /// through a timer at the specified intervals.
        /// </summary>
        /// <param name="ants">A list of existing ants, if any.</param>
        /// <param name="rows">Number of rows in grid.</param>
        /// <param name="columns">Number of columns in grid.</param>
        /// <param name="cellSize">Pixel width and height per cell.</param>
        /// <param name="rules">String specifying rules.</param>
        /// <param name="interval">Time between automatic updates.</param>
        public CASim2DAntGui(CAAnt2D[] ants, int rows, int columns,
            int cellSize, string rules, TimeSpan interval)
        {
            InitializeComponent();
            simulation = new CASim2DAnt(ants, rows, columns, cellSize, rules);

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
            //window.MouseDown += SetAnt;
            window.MouseDoubleClick += SetAnt;

            //Sets default values.
            cellColors = new List<Color>();
            cellColorDefault = Colors.White;
            antColors = new List<Color>();
            antColorDefault = Colors.Red;
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

        /// <summary>
        /// Middle-clicking modifies an ant at the given position.
        /// No shift: creates (or increments) an ant.
        /// Shift: destroys (or decrements) an ant.
        /// </summary>
        private void SetAnt(object sender, MouseButtonEventArgs e)
        {
            //Only works for middle-mouse button.
            //if (e.MiddleButton != MouseButtonState.Pressed ||
              //  e.LeftButton == MouseButtonState.Pressed ||
                //e.RightButton == MouseButtonState.Pressed)
                //return;

            //Gets the row and column of the clicked cell.
            Point pos = Mouse.GetPosition(display);
            pos.X = Math.Floor(pos.X / simulation.cellsize);
            pos.Y = Math.Floor(pos.Y / simulation.cellsize);

            //Calculates position.
            double cellNum = pos.X + (pos.Y * simulation.rows);

            //The cell exists.
            if (cellNum >= 0 && cellNum < simulation.cells.Count())
            {
                if (Keyboard.GetKeyStates(Key.LeftShift) == KeyStates.Down ||
                    Keyboard.GetKeyStates(Key.RightShift) == KeyStates.Down)
                {
                    //Finds and decrements the ant type if it exists.
                    for (int i = 0; i < simulation.ants.Count(); i++)
                    {
                        if (simulation.ants[i].position ==
                            simulation.cells[(int)cellNum])
                        {
                            simulation.ants[i].type--;

                            //Removes all invalid ants.
                            if (simulation.ants[i].type < 0)
                            {
                                //Copies over a new array without the ant.
                                CAAnt2D[] newAnts = new CAAnt2D[simulation.ants.Length - 1];
                                for (int j = 0; j < simulation.ants.Length; j++)
                                {
                                    if (j < i) //before the displacement
                                    {
                                        newAnts[j] = simulation.ants[j];
                                    }
                                    else if (j > i) //after the displacement
                                    {
                                        newAnts[j - 1] = simulation.ants[j - 1];
                                    }
                                }
                                simulation.ants = newAnts;
                            }
                        }
                    }                    
                }
                else
                {
                    //Finds and increments the ant type if it exists.
                    bool antExists = false;
                    for (int i = 0; i < simulation.ants.Count(); i++)
                    {
                        if (simulation.ants[i].position ==
                            simulation.cells[(int)cellNum])
                        {
                            simulation.ants[i].type++;
                            antExists = true;
                        }
                    }

                    if (!antExists)
                    {
                        CAAnt2D ant = new CAAnt2D(simulation.cells[(int)cellNum], 0, 0);
                        CAAnt2D[] newAnts = new CAAnt2D[simulation.ants.Length + 1];
                        for (int i = 0; i < simulation.ants.Length + 1; i++)
                        {
                            if (i < simulation.ants.Length)
                                newAnts[i] = simulation.ants[i];
                            else
                                newAnts[i] = ant;
                        }

                        simulation.ants = newAnts;
                    }
                }
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
                    simulation.cells[(int)cellNum].state++;
                }
                else if (e.Delta < 0 && //scroll down
                    simulation.cells[(int)cellNum].state > 0)
                {
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
            if (e.Key == Key.Enter && e.IsDown)
            {
                Random rng = new Random();
                for (int i = 0; i < simulation.cells.Count(); i++)
                    simulation.cells[i].state = (byte)rng.Next(2);

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

            //Clears the state of all cells and ants.
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                simulation.ants = new CAAnt2D[0];

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
                    CACell2DAnt cell = simulation.cells[i];

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
                    CACell2DAnt cell = simulation.cellsToUpdate[i];

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

            //Renders ants
            int thirdCellSize = (int)Math.Round(simulation.cellsize / 3d);
            for (int i = 0; i < simulation.ants.Count(); i++)
            {
                CAAnt2D ant = simulation.ants[i];

                if (ant.type < antColors.Count)
                {
                    rendering.FillRectangle(
                        ant.position.x + thirdCellSize,
                        ant.position.y + thirdCellSize,
                        ant.position.x + simulation.cellsize - thirdCellSize,
                        ant.position.y + simulation.cellsize - thirdCellSize,
                        antColors[ant.type]);
                }
                else
                {
                    rendering.FillRectangle(
                        ant.position.x + thirdCellSize,
                        ant.position.y + thirdCellSize,
                        ant.position.x + simulation.cellsize - thirdCellSize,
                        ant.position.y + simulation.cellsize - thirdCellSize,
                        antColorDefault);
                }
            }
            rendering.Unlock();
            display.Source = rendering;
        }
    }
}