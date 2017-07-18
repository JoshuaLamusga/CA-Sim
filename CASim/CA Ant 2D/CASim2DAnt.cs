using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CASimulator
{
    public class CASim2DAnt
    {
        private int _rows, _columns, _cellsize;
        private string _rules;
        public int rows
        {
            get
            {
                return _rows;
            }
        }
        public int columns
        {
            get
            {
                return _columns;
            }
        }
        public int cellsize
        {
            get
            {
                return _cellsize;
            }
        }
        public string rules
        {
            get
            {
                return _rules;
            }
            private set
            {
                _rules = value;
            }
        }
        public List<CACell2DAnt> cells;
        public List<CACell2DAnt> cellsToUpdate;
        public CAAnt2D[] ants;

        /// <summary>
        /// Creates a new simulation.
        /// </summary>
        public CASim2DAnt(CAAnt2D[] ants, int rows, int columns, int cellSize, string rules)
        {
            Configure(ants, rows, columns, cellSize, rules);
        }

        /// <summary>
        /// Reconfigures the simulation with the given properties.
        /// </summary>
        /// <param name="ants">Number of ants to begin with.</param>
        /// <param name="rows">Number of grid rows.</param>
        /// <param name="columns">Number of grid columns.</param>
        /// <param name="cellSize">Pixel size per cell.</param>
        /// <param name="isToroidal">Whether grid wraps around.</param>
        /// <param name="rule">String of rule to be parsed.</param>
        public void Configure(CAAnt2D[] ants, int rows, int columns,
            int cellSize, string rules)
        {
            if (rows < 2 || columns < 2)
            {
                throw new Exception("grid must be at least 2*2.");
            }

            _rows = rows;
            _columns = columns;
            _cellsize = cellSize;
            _rules = rules;
            cells = new List<CACell2DAnt>(rows * columns);
            cellsToUpdate = new List<CACell2DAnt>();
            this.ants = ants;

            //Populates cells and creates references.
            for (int i = 0; i < rows * columns; i++)
            {
                CACell2DAnt cell = new CACell2DAnt();
                cell.x = cellSize * (i % columns);
                cell.y = cellSize * (i / columns); //intentional truncation.
                cells.Add(cell);
            }
            for (int i = 0; i < cells.Count(); i++)
            {
                //Up link
                if (i >= columns)
                    cells[i].up = cells[i - columns];
                else
                    cells[i].up = cells[columns * (rows - 1) + i];

                //Down link
                if (i < ((uint)rows - 1) * columns)
                    cells[i].down = cells[i + columns];
                else
                    cells[i].down = cells[i % columns];

                //Left link
                if (i % columns != 0)
                    cells[i].left = cells[i - 1];
                else
                    cells[i].left = cells[i + columns - 1];

                //Right link
                if ((i + 1) % columns != 0)
                    cells[i].right = cells[i + 1];
                else
                    cells[i].right = cells[i - columns + 1];
            }

            UpdateRules(rules);
        }

        /// <summary>
        /// Iterates to change all cells once.
        /// The gui is updated by CASimView.
        /// </summary>
        public void Update()
        {
            cellsToUpdate.Clear();

            //For every ant.
            for (int i = 0; i < ants.Count(); i++)
            {
                CAAnt2D ant = ants[i];

                //For every letter in the rule.
                for (int j = 0; j < _rules.Length; j++)
                {
                    if (ant.position.state == j)
                    {
                        //Queues the cell for gui updates.
                        cellsToUpdate.Add(ant.position);

                        //Increments the cell state.
                        if (ant.type != 1)
                        {
                            ant.position.state++;
                            if (ant.position.state == _rules.Count())
                                ant.position.state = 0;
                        }
                        else
                        {
                            if (ant.position.state > 0)
                                ant.position.state--;
                            else
                                ant.position.state = (byte)(_rules.Count() - 1);
                        }

                        if (ant.type != 2)
                        {
                            if (_rules[j] == 'l')
                            {
                                //Turns left.
                                ant.direction++;
                                if (ant.type != 3)
                                {
                                    if (ant.direction == 4)
                                        ant.direction = 0;
                                }
                                else
                                {
                                    if (ant.direction == 8)
                                        ant.direction = 0;
                                }
                            }
                            else //It's r.
                            {
                                //Turns right.
                                ant.direction--;
                                if (ant.type != 3)
                                {
                                    if (ant.direction == -1)
                                        ant.direction = 3;
                                }
                                else
                                {
                                    if (ant.direction == -1)
                                        ant.direction = 7;
                                }
                            }
                        }
                        else
                        {
                            if (_rules[j] == 'r')
                            {
                                //Turns left.
                                ant.direction++;
                                if (ant.direction == 4)
                                    ant.direction = 0;
                            }
                            else //It's l.
                            {
                                //Turns right.
                                ant.direction--;
                                if (ant.direction == -1)
                                    ant.direction = 3;
                            }
                        }

                        //Moves forward once.
                        if (ant.type != 3)
                        {
                            if (ant.direction == 0)
                                ant.position =
                                    ant.position.right;
                            else if (ant.direction == 1)
                                ant.position =
                                    ant.position.up;
                            else if (ant.direction == 2)
                                ant.position =
                                    ant.position.left;
                            else if (ant.direction == 3)
                                ant.position =
                                    ant.position.down;
                        }
                        else
                        {
                            if (ant.direction == 0)
                                ant.position =
                                    ant.position.right;
                            else if (ant.direction == 1)
                                ant.position =
                                    ant.position.right.up;
                            else if (ant.direction == 2)
                                ant.position =
                                    ant.position.up;
                            else if (ant.direction == 3)
                                ant.position =
                                    ant.position.left.up;
                            else if (ant.direction == 4)
                                ant.position =
                                    ant.position.left;
                            else if (ant.direction == 5)
                                ant.position =
                                    ant.position.left.down;
                            else if (ant.direction == 6)
                                ant.position =
                                    ant.position.down;
                            else if (ant.direction == 7)
                                ant.position =
                                    ant.position.down.right;
                        }
                    }

                    ants[i] = ant;
                }
            }
        }

        /// <summary>
        /// Checks the given string of rules. See _Readme.txt for acceptable
        /// values and examples of rules.
        /// </summary>
        private void UpdateRules(string rules)
        {
            //Resets the rule lists. Must be a string of L and R.
            rules = rules.ToLower().Replace(" ","");
            /// <summary>

            //If there are any characters that aren't l or r, it's invalid.
            MatchCollection matches = Regex.Matches(rules, "[^lr]");
            if (matches.Count != 0)
            {
                throw new Exception("Invalid C.A. rule format.");
            }

            this.rules = rules;
        }

        /// <summary>
        /// Checks the given rules for validity. See _Readme.txt for acceptable
        /// values and examples of rules.
        /// </summary>
        public static bool CheckRules(string rules)
        {
            //Resets the rule lists. Must be a string of L and R.
            rules = rules.ToLower().Replace(" ", "");
            /// <summary>

            //If there are any characters that aren't l or r, it's invalid.
            MatchCollection matches = Regex.Matches(rules, "[^lr]");
            if (matches.Count != 0)
            {
                return false;
            }

            return true;
        }
    }
}