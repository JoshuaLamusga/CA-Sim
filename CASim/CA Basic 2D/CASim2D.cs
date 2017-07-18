using System;
using System.Collections.Generic;
using System.Linq;

namespace CASimulator
{
    public class CASim2D
    {
        private int _rows, _columns, _cellsize;
        private bool _isToroidal;
        private string _rules;
        private List<CARule> rulesParsedTb;
        private List<CARule> rulesParsedTm;
        private List<CARule> rulesParsedX;
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
        public bool isToroidal
        {
            get
            {
                return _isToroidal;
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
        public List<CACell2D> cells;
        public List<CACell2D> cellsToUpdate;

        /// <summary>
        /// Creates a new simulation.
        /// </summary>
        public CASim2D(int rows, int columns, int cellSize,
            bool isToroidal, string rules)
        {
            Configure(rows, columns, cellSize, isToroidal, rules);
        }

        /// <summary>
        /// Reconfigures the simulation with the given properties.
        /// </summary>
        /// <param name="rows">Number of grid rows.</param>
        /// <param name="columns">Number of grid columns.</param>
        /// <param name="cellSize">Pixel size per cell.</param>
        /// <param name="isToroidal">Whether grid wraps around.</param>
        /// <param name="rule">String of rule to be parsed.</param>
        public void Configure(int rows, int columns, int cellSize,
            bool isToroidal, string rules)
        {
            if (rows < 2 || columns < 2)
            {
                throw new Exception("grid must be at least 2*2.");
            }

            _rows = rows;
            _columns = columns;
            _cellsize = cellSize;
            _isToroidal = isToroidal;
            _rules = rules;
            cells = new List<CACell2D>(rows * columns);
            cellsToUpdate = new List<CACell2D>(rows * columns);

            //Populates cells and creates references.
            for (int i = 0; i < rows * columns; i++)
            {
                CACell2D cell = new CACell2D();
                cell.x = cellSize * (i % columns);
                cell.y = cellSize * (i / columns); //intentional truncation.
                cells.Add(cell);
            }
            cellsToUpdate.AddRange(cells); //Queues all cells for update.

            for (int i = 0; i < cells.Count(); i++)
            {
                //Up link
                if (i >= columns)
                {
                    cells[i].up = cells[i - columns];
                }
                else if (isToroidal)
                {
                    cells[i].up = cells[columns * (rows - 1) + i];
                }
                else
                { //todo: double check that this works.
                    cells[i].up = CACell2D.empty;
                }

                //Down link
                if (i < ((uint)rows - 1) * columns)
                {
                    cells[i].down = cells[i + columns];
                }
                else if (isToroidal)
                {
                    cells[i].down = cells[i % columns];
                }
                else
                {
                    cells[i].down = CACell2D.empty;
                }

                //Left link
                if (i % columns != 0)
                {
                    cells[i].left = cells[i - 1];
                }
                else if (isToroidal)
                {
                    cells[i].left = cells[i + columns - 1];
                }
                else
                {
                    cells[i].left = CACell2D.empty;
                }

                //Right link
                if ((i + 1) % columns != 0)
                {
                    cells[i].right = cells[i + 1];
                }
                else if (isToroidal)
                {
                    cells[i].right = cells[i - columns + 1];
                }
                else
                {
                    cells[i].right = CACell2D.empty;
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
            //Calculates cells that need to update.
            for (int i = 0; i < cells.Count(); i++)
            {
                CACell2D cell = cells[i];

                //Since no algorithms include cells beyond the Moore
                //neighborhood with radius 1, if all these cells are
                //constant, the cell won't change and therefore doesn't
                //need an update.
                if (cell.state != cell.prevstate ||
                    cell.right.state != cell.right.prevstate ||
                    cell.right.up.state != cell.right.up.prevstate ||
                    cell.up.state != cell.right.prevstate ||
                    cell.left.up.state != cell.left.up.prevstate ||
                    cell.left.state != cell.left.prevstate ||
                    cell.left.down.state != cell.left.down.prevstate ||
                    cell.down.state != cell.down.prevstate ||
                    cell.right.down.state != cell.right.down.prevstate)
                {
                    cellsToUpdate.Add(cell);
                }
            }

            byte[] newStates = new byte[cellsToUpdate.Count()];

            for (int i = 0; i < newStates.Length; i++)
            {
                byte newState = 0;
                short neighborsCardinal = 0;
                short neighborsDiagonal = 0;

                //Calculates neighborhood
                neighborsCardinal += cellsToUpdate[i].right.state;
                neighborsCardinal += cellsToUpdate[i].up.state;
                neighborsCardinal += cellsToUpdate[i].left.state;
                neighborsCardinal += cellsToUpdate[i].down.state;
                neighborsDiagonal += cellsToUpdate[i].up.left.state;
                neighborsDiagonal += cellsToUpdate[i].up.right.state;
                neighborsDiagonal += cellsToUpdate[i].down.left.state;
                neighborsDiagonal += cellsToUpdate[i].down.right.state;

                //Applies parsed rules.
                for (int j = 0; j < rulesParsedTb.Count(); j++)
                {
                    if (neighborsCardinal == rulesParsedTb[j].lhs)
                    {
                        switch (rulesParsedTb[j].rhs)
                        {
                            case -1:
                                newState = cellsToUpdate[i].state;
                                break;
                            case -2:
                                newState = (byte)(cellsToUpdate[i].state + 1);
                                break;
                            case -3:
                                newState = (byte)(cellsToUpdate[i].state - 1);
                                break;
                            default:
                                newState = (byte)rulesParsedTb[j].rhs;
                                break;
                        }
                    }
                }
                for (int j = 0; j < rulesParsedTm.Count(); j++)
                {
                    if (neighborsCardinal + neighborsDiagonal ==
                        rulesParsedTm[j].lhs)
                    {
                        switch (rulesParsedTm[j].rhs)
                        {
                            case -1:
                                newState = cellsToUpdate[i].state;
                                break;
                            case -2:
                                newState = (byte)(cellsToUpdate[i].state + 1);
                                break;
                            case -3:
                                newState = (byte)(cellsToUpdate[i].state - 1);
                                break;
                            default:
                                newState = (byte)rulesParsedTm[j].rhs;
                                break;
                        }
                    }
                }
                for (int j = 0; j < rulesParsedX.Count(); j++)
                {
                    //-2 is used in lhs to indicate no value is given.
                    if (rulesParsedX[j].lhs == -2)
                    {
                        switch (rulesParsedX[j].rhs)
                        {
                            case -1:
                                newState = cellsToUpdate[i].state;
                                break;
                            case -2:
                                newState = (byte)(cellsToUpdate[i].state + 1);
                                break;
                            case -3:
                                newState = (byte)(cellsToUpdate[i].state - 1);
                                break;
                            default:
                                newState = (byte)rulesParsedX[j].rhs;
                                break;
                        }
                    }
                }

                //Adds the final state.
                newStates[i] = newState;
            }

            //Synchronously updates each cell.
            for (int i = 0; i < newStates.Count(); i++)
            {
                cellsToUpdate[i].prevstate = cellsToUpdate[i].state;
                cellsToUpdate[i].state = newStates[i];
            }
        }

        /// <summary>
        /// Parses the given string of rules. See _Readme.txt for acceptable
        /// values and examples of rules.
        /// </summary>
        private void UpdateRules(string rules)
        {
            /* Splits the string into separate rule, then splits each rule
             * into prefix, lhs, and rhs. Removes whitespace and makes
             * lowercase.
             * 
             * prefix: tb, tm, x allowed values.
             * lhs: non-negative numbers, x, and blank allowed (if prefix = x)
             * rhs: non-negative numbers, x, x+1, x-1 allowed values.
             * 
             * todo: rewrite so queries like "tm2=1tm" don't pass.
             */

            //Resets the rule lists.
            this.rules = rules;
            rulesParsedTb = new List<CARule>();
            rulesParsedTm = new List<CARule>();
            rulesParsedX = new List<CARule>();

            //Splits into separate rule and parses each individually
            //into faster lists to check in the update call.
            List<string> separateRules = rules.Split('|').ToList();
            for (int i = 0; i < separateRules.Count(); i++)
            {
                string rule = separateRules[i].ToLower().Replace(" ","");
                List<string> ruleParts = new List<string>();

                //Gets the prefix and catches bad/missing rule prefixes.
                if (rule.StartsWith("tb")) { ruleParts.Add("tb"); }
                else if (rule.StartsWith("tm")) { ruleParts.Add("tm"); }
                else if (rule.StartsWith("x")) { ruleParts.Add("x"); }
                else { throw new Exception("Invalid C.A. rule prefix."); }

                //Removes the prefix and separates the rule into two parts.
                rule = rule.Remove(0, ruleParts[0].Count());
                ruleParts.AddRange(rule.Split('='));
                if (ruleParts.Count() != 3)
                {
                    throw new Exception("Invalid C.A. rule format.");
                }

                //Tests the values left over to see if they are numbers,
                //placeholders like x, x+1, and x-1, or invalid terms.
                int testVal = -1;
                Int32.TryParse(ruleParts[1], out testVal);
                if (testVal < 0)
                {
                    throw new Exception("Invalid C.A. term: must be a " +
                    "positive number.");
                }

                if (ruleParts[2] != "x" &&
                    ruleParts[2] != "x+1" &&
                    ruleParts[2] != "x-1")
                {
                    testVal = -1;
                    Int32.TryParse(ruleParts[2], out testVal);
                    if (testVal < 0)
                    {
                        throw new Exception("Invalid C.A. term: must be a " +
                        "positive number.");
                    }
                }

                //Ensures lhs isn't invalid or segmented like x+1 makes it.
                if (ruleParts[1].Contains('+') ||
                    ruleParts[1].Contains('-') ||
                    ruleParts[1].Contains('.') ||
                    ruleParts[1].Contains('^'))
                {
                    throw new Exception("Invalid C.A. rule format.");
                }

                //Converts empty strings in lhs (only allowed with prefix 'x')
                //to -2.
                if (ruleParts[1] == "")
                {
                    if (ruleParts[0] == "x")
                    {
                        //Empty strings in lhs become -2.
                        ruleParts[1] = "-2";
                    }
                    else
                    {
                        throw new Exception("Invalid C.A. term: left-hand " +
                            "side of rule must be non-empty or 'x'.");
                    }
                }

                //Converts x for ruleParts[1] to integers, where x = -1
                if (ruleParts[0] == "x")
                {
                    ruleParts[1] = "-1";
                }

                //Converts x x+1 and x-1 for ruleParts[2] to integers, where
                //x = -1, x+1 = -2, x-1 = -3.
                switch (ruleParts[2])
                {
                    case "":
                        throw new Exception("C.A. rule error: rhs cannot " +
                            "be empty.");
                    case "x":
                        ruleParts[2] = "-1";
                        break;
                    case "x+1":
                        ruleParts[2] = "-2";
                        break;
                    case "x-1":
                        ruleParts[2] = "-3";
                        break;
                }

                //Creates the rule and organizes it into a prefix list.
                CARule ruleParsed = new CARule(
                    ruleParts[0],
                    Int32.Parse(ruleParts[1]),
                    Int32.Parse(ruleParts[2]));

                switch (ruleParts[0])
                {
                    case "tb":
                        rulesParsedTb.Add(ruleParsed);
                        break;
                    case "tm":
                        rulesParsedTm.Add(ruleParsed);
                        break;
                    case "x":
                        rulesParsedX.Add(ruleParsed);
                        break;
                }
            }
        }

        /// <summary>
        /// Checks the given rules for validity. See _Readme.txt for acceptable
        /// values and examples of rules.
        /// </summary>
        public static bool CheckRules(string rules)
        {
            /* Splits the string into separate rule, then splits each rule
             * into prefix, lhs, and rhs. Removes whitespace and makes
             * lowercase.
             * 
             * prefix: tb, tm, x allowed values.
             * lhs: non-negative numbers, x, and blank allowed (if prefix = x)
             * rhs: non-negative numbers, x, x+1, x-1 allowed values.
             */

            //Splits into separate rule and parses each individually
            //into faster lists to check in the update call.
            List<string> separateRules = rules.Split('|').ToList();
            for (int i = 0; i < separateRules.Count(); i++)
            {
                string rule = separateRules[i].ToLower().Replace(" ", "");
                List<string> ruleParts = new List<string>();

                //Gets the prefix and catches bad/missing rule prefixes.
                if (rule.StartsWith("tb")) { ruleParts.Add("tb"); }
                else if (rule.StartsWith("tm")) { ruleParts.Add("tm"); }
                else if (rule.StartsWith("x")) { ruleParts.Add("x"); }
                else { return false; }

                //Removes the prefix and separates the rule into two parts.
                rule = rule.Remove(0, ruleParts[0].Count());
                ruleParts.AddRange(rule.Split('='));
                if (ruleParts.Count() != 3)
                {
                    return false;
                }

                //Tests the values left over to see if they are numbers,
                //placeholders like x, x+1, and x-1, or invalid terms.
                int testVal = -1;
                Int32.TryParse(ruleParts[1], out testVal);
                if (testVal < 0)
                {
                    return false;
                }

                if (ruleParts[2] != "x" &&
                    ruleParts[2] != "x+1" &&
                    ruleParts[2] != "x-1")
                {
                    if (ruleParts[2] == "")
                        return false;

                    testVal = -1;
                    Int32.TryParse(ruleParts[2], out testVal);
                    if (testVal < 0)
                    {
                        return false;
                    }
                }

                //Ensures lhs isn't invalid or segmented like x+1 makes it.
                if (ruleParts[1].Contains('+') ||
                    ruleParts[1].Contains('-') ||
                    ruleParts[1].Contains('.') ||
                    ruleParts[1].Contains('^'))
                {
                    return false;
                }

                //Converts empty strings in lhs (only allowed with prefix 'x')
                //to -2.
                if (ruleParts[1] == "")
                {
                    if (ruleParts[0] == "x")
                    {
                        //Empty strings in lhs become -2.
                        ruleParts[1] = "-2";
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
