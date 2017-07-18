using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CASimulator
{
    public class CASim1D
    {
        private int _columns, _cellsize;
        private bool _isCylindrical;
        private string _rules;
        private int _generation;
        private string elementaryRule;
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
        public bool isToroidal
        {
            get
            {
                return _isCylindrical;
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
        public List<CACell1D> cells;
        public int generation
        {
            get
            {
                return _generation;
            }
            private set
            {
                _generation = value;
            }
        }

        /// <summary>
        /// Creates a new simulation.
        /// </summary>
        public CASim1D(int columns, int cellSize,
            bool isToroidal, string rules)
        {
            Configure(columns, cellSize, isToroidal, rules);
        }

        /// <summary>
        /// Reconfigures the simulation with the given properties.
        /// </summary>
        /// <param name="columns">Number of cells per line.</param>
        /// <param name="cellSize">Pixel size per cell.</param>
        /// <param name="isToroidal">Whether grid wraps around.</param>
        /// <param name="rule">String of rule to be parsed.</param>
        public void Configure(int columns, int cellSize,
            bool isToroidal, string rules)
        {
            if (columns < 2)
            {
                throw new Exception("grid must be at least 2*2.");
            }

            _columns = columns;
            _cellsize = cellSize;
            _isCylindrical = isToroidal;
            _rules = rules;
            cells = new List<CACell1D>(columns);

            //Populates cells and creates references.
            for (int i = 0; i < columns; i++)
            {
                CACell1D cell = new CACell1D();
                cell.x = cellSize * (i % columns);
                cells.Add(cell);
            }
            for (int i = 0; i < cells.Count(); i++)
            {
                //Left link
                if (i % columns != 0)
                {
                    cells[i].left = cells[i - 1];
                }
                else if (isToroidal)
                {
                    cells[i].left = cells[cells.Count - 1];
                }

                //Right link
                if ((i + 1) % columns != 0)
                {
                    cells[i].right = cells[i + 1];
                }
                else if (isToroidal)
                {
                    cells[i].right = cells[0];
                }
            }

            UpdateRules(rules);
        }

        /// <summary>
        /// Iterates to change all cells once.
        /// The gui is updated by CASimView.
        /// </summary>
        public void Update()
        {
            generation++;
            byte[] newStates = new byte[cells.Count()];

            for (int i = 0; i < newStates.Length; i++)
            {
                byte newState = 0;

                //Gets the state of the cells for quick reference.
                bool leftActive = false;
                bool rightActive = false;
                bool thisActive = (cells[i].state == 1);

                if (cells[i].left != null)
                    if (cells[i].left.state == 1)
                        leftActive = true;
                if (cells[i].right != null)
                    if (cells[i].right.state == 1)
                        rightActive = true;

                //Checks each permutation of states for a binary system.
                //This is the heart of elementary cellular rules.
                if (leftActive &&
                    thisActive &&
                    rightActive)
                    newState = Byte.Parse(elementaryRule[0].ToString());

                if (leftActive &&
                    thisActive &&
                    !rightActive)
                    newState = Byte.Parse(elementaryRule[1].ToString());

                if (leftActive &&
                    !thisActive &&
                    rightActive)
                    newState = Byte.Parse(elementaryRule[2].ToString());

                if (leftActive &&
                    !thisActive &&
                    !rightActive)
                    newState = Byte.Parse(elementaryRule[3].ToString());

                if (!leftActive &&
                    thisActive &&
                    rightActive)
                    newState = Byte.Parse(elementaryRule[4].ToString());

                if (!leftActive &&
                    thisActive &&
                    !rightActive)
                    newState = Byte.Parse(elementaryRule[5].ToString());

                if (!leftActive &&
                    !thisActive &&
                    rightActive)
                    newState = Byte.Parse(elementaryRule[6].ToString());

                if (!leftActive &&
                    !thisActive &&
                    !rightActive)
                    newState = Byte.Parse(elementaryRule[7].ToString());

                //Adds the final state.
                newStates[i] = newState;
            }

            //Synchronously updates each cell.
            for (int i = 0; i < newStates.Count(); i++)
            {
                cells[i].prevstate = cells[i].state;
                cells[i].state = newStates[i];
            }
        }

        /// <summary>
        /// Resets back to generation 0.
        /// </summary>
        public void ResetGenerations()
        {
            generation = 0;
        }

        /// <summary>
        /// Parses the given string of rules. See _Readme.txt for acceptable
        /// values and examples of rules.
        /// </summary>
        private void UpdateRules(string rule)
        {
            // Takes an 8-digit binary number for elementary automata.

            if (rule.Count() != 8)
            {
                throw new Exception("C.A. rule error: must be 8 digits.");
            }

            for (int i = 0; i < rule.Count(); i++)
            {
                if (rule[i] != '0' &&
                    rule[i] != '1')
                {
                    throw new Exception("C.A. rule error: must be 0s or 1s.");
                }
            }

            elementaryRule = rule;
        }

        /// <summary>
        /// Checks the given rules for validity. See _Readme.txt for acceptable
        /// values and examples of rules.
        /// </summary>
        public static bool CheckRules(string rule)
        {
            // Takes an 8-digit binary number for elementary automata.
            if (rule.Count() != 8)
            {
                return false;
            }

            for (int i = 0; i < rule.Count(); i++)
            {
                if (rule[i] != '0' &&
                    rule[i] != '1')
                {
                    return false;
                }
            }

            return true;
        }
    }
}