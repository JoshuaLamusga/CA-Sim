# CA-Sim
A very limited cellular automata simulator that runs on Windows.

## Elementary Automata
The classic elementary automata are computed from a single row of cells. A ruleset is applied to each cell to determine the new value of that cell. This implementation is limited to a directional (as opposed to totalistic) implementation with a basic neighborhood of 1 that uses two states. This creates only 256 possible rulesets, which can be encoded with 8 0's and 1's for all permutations in a binary simulation of basic neighborhood 1. The string 00011110 is called "rule 30". See: http://mathworld.wolfram.com/ElementaryCellularAutomaton.html.

## 2-D Automata
These 2-D automata are calculated based on the values of neighboring cells and/or the total values of nearby cells in a Moore's neighborhood of 1. Rules are constructed in the general format of LHS=RHS, where the left-hand side describes the condition required to apply the right-hand side as the new value of the current cell.

### General symbols
**x**: The current value of the cell.

### Left-hand only symbols
**tb#**: The rule is totalistic for a basic neighborhood in the cardinal directions. # is a number for the total sum required to meet the condition.  
**tm#**: The rule is totalistic for a Moore's neighborhood of 1 in the cardinal/diagonal directions. # is a number for the total sum required to meet the condition.  
**d#**: The rule is directional so the value is based on the neighborhood cells' values. # is a number describing the direction. 0 is right, 1 is up, 2 is left, 3 is down.  
**x#**: The current value of the cell. # is a number describing the required value of the state to meet the condition.

### Right-hand only symbols
**g**: The rule is generational (a.k.a. temporal) so it's based on the value of the last generation.  
**x+1**: The current value of the cell + 1. This cannot be the left-hand side of the rule.  
**x-1**: The current value of the cell - 1. This cannot be the left-hand side of the rule.  

### Rule examples
*tm3=1|tm2=x* This is Conway's Game of Life. See: https://en.wikipedia.org/wiki/Conway%27s_Game_of_Life  
*x=0* The cell is unconditionally set to 0.  
*x1=0* If the cell's state is 1, it's set to 0.  
*tb3=0* Cells with cardinal sum 3 are set to 0.  
*tm14=x+1* Cells with Moore sum 14 are incremented.  
*d01=0* If the cell to the right has state 1, set this cell to 0.  
*d23=x* If the cell to the left has state 3, keep the current state of this cell.  
*tm4=x-1|d11=1* Moore sum 4 decrements the state, and is set to 1 if the cell above is 1.  
*d0x=x+1* Increments the cell if the cell to the right has the same value.

## Langston's Ant
Rules for Langston's Ant are a string of L's and R's meaning left turn and right turn; respectively. The cells generated have any number of states, and the LR string attributes a direction that the ant should turn in when it moves to a cell with a certain state. The first character defines whether it turns left or right for cells with state 0. The second character does this for cells with state 1, and so on. There are different types of ants (specified by the ant state).
- State 0: The ant increments every cell it touches and moves in cardinal directions.
- State 1: The ant decrements every cell it touches and moves in cardinal directions.
- State 2: The ant increments every cell it touches, but moves opposite of the normal turning direction in cardinal directions.
- State 3: The ant increments every cell it touches, but moves opposite of the normal turning direction in diagonal and cardinal directions.

## Controls
**Left-click**: Increment a cell state in any automata mode.  
**Right-click**: Decrement a cell state in any automata mode.  
**Spacebar**: Pause or resume the simulation.  
**1**: Skip 50 generations.  
**2**: Skip 100 generations.  
**3**: Skip 500 generations.  
**Enter**: Set all cells to random binary states in any automata mode.  
**Escape**: Set all cells to initial state 0 in any automata mode.  
**Right arrow**: Apply one generation update.  
**Double-click**: Increment/set ant state. Langston's ant mode only.  
**Shift + Double-click**: Decrement/remove ant state. Langston's ant mode only.

## Expanding on this experiment
There are many varieties of automata that aren't considered in this implementation at all. Here are some ideas:
- Allow for totalistic rulesets in 1-D automata, where the value of the current cell depends on the total states (as a number) of its neighbors.
- Allow for temporal rulesets in 1-D and 2-D automata, where the value of the current cell depends on the values of the older cell or neighbors.
- Allow for a larger Moore's neighborhood in 1-D and 2-D automata so directional, totalistic, and temporal rulesets can be made more complex.
- Create 1-D and 2-D automata with a different topology such that cells have more/fewer neighbors, like a triangle or hexagon grid.
- Allow for more than two states in 1-D and 2-D automata.
- Implement saving and loading rules as text, and saving/loading the list of cell states for an automaton.
- Keep temporal lists of cell values (perhaps by making the current value the most recent value in a list).
- Implement a Von Neumann neighborhood and Moore neighborhoods > 1.
- Make it possible to implement WireWorld, Immigration Game / Rainbow Game of Life, and Cyclic automata
- Allow the starting direction of ants to be set and have more ants that turn diagonals.
- Implement Patterson's Worms.

## Bugs
These bugs should be easy to fix if you have time; I wrote this awhile ago when I was new to C# and thought I'd never share this project.

- Pressing 1, 2 or 3 in 1-D automata mode will not draw those generations.
- LHS and RHS symbols used in the wrong place will probably crash the program.
- Some rules like x=x do not work.
- There are obvious memory leaks with bitmaps, most visible by running a 1-D simulation, closing it, and running it again.

## General Improvements
There are a lot of things to improve on in this project for usability.

- Setting changes should update with open simulation windows, or main window should be disabled while a simulation is open.
- Settings that don't make sense with a given automata type should be disabled, such as columns in a 1-D simmulation.
- If the main window is closed, child windows should be closed as well.
