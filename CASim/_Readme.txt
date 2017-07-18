C.A. Simulator, version 1.0
(C) Joshua Lamusga, 2015.

FEATURES
Simulates 2D, square, finite, multi-state (up to 256) automatons. It:
- can parse a set of totalistic or basic directional rules
- can simulate Conway's Game of Life and similar automata
- can simulate Langston's ants and turmites
- allows the user to draw a configuration with the mouse
- allows the user to clear and randomize the configuration
- allows the user to pause and resume the simulation

CONTROLS
Left-click: Increment cell state
Right-click: Decrement cell state
Spacebar: Pause/resume simulation
1: Skip 50 generations.
2: Skip 100 generations.
3: Skip 500 generations.
Enter: Set all cells to random binary states, 0 or 1.
Escape: Set all cells to initial state 0.
Right arrow: Apply one generation update.
Middle-click: Increment/set ant state. Langston's ant mode only.
Shift + Middle-click: Decrement/remove ant state. Langston's ant mode only.

ANT STATES
state 0: increments. moves right for R, left for L. Cardinal.
state 1: decrements. moves right for R, left for L. Cardinal.
state 2: increments. moves left for R, right for L. Cardinal.
state 3: increments. moves right for R, left for L. Diagonal and cardinal.

TODO
Save/load as text. Saving/loading will save rule as well. Implement
generational rules and optimizations. Von Neumann neighborhood (diamonds) and
Moore neighborhood should be implemented with any radius.
Notably cannot emulate:
- WireWorld
- Immigration Game / Rainbow Game of Life
- Cyclic cellular automata (property based on majority property of neighbors)
The starting direction of ants, Ants that turn diagonals.
Patterson's Worms.
Fix memory leaks!

RULES (1D)
Rules in one dimensional automata are formatted as a byte of 8 0's and 1's.
Rules in Langston's ants are formatted as any number of L's and R's, like LRR.
Rules in two dimensional automata are more complicated:

Rules begin with a symbol, then conditions expressed in numbers,
followed by =, and the new value. Rules are separated with |.
White space is ignored.

Symbols:
tb the rule is totalistic for a basic neighborhood (cardinals).
tm the rule is totalistic for Moore's neighborhood (cardinals + diagonals).
d the rule is directional (value is based on neighborhood cells' values)
g the rule is generational (value is based on value of last generation)
x is the current value of the cell.
x+1 is the value of the cell + 1.
x-1 is the value of the cell - 1.

Numbers after tb or tm describe the total sum required to meet the condition.
Numbers after d describe the direction from the current cell: 0 is right, 1 is
up, 2 is left, and 3 is down. Any numbers after this check the cell value.
g cannot have a number. It always means the cell's value last generation.
x does not require a number, but it can have one. If it does, the number means
the required value of the state.
x+1, and x-1 are reserved for the right-hand side of a rule.

Example strings:
"tm3=1|tm2=x" Conway's Game of Life.
"x=0" The cell is unconditionally set to 0.
"x1=0" The cell, if it's state is 1, is set to 0.
"tb3=0" Cells with cardinal sum 3 are set to 0.
"tm14=x+1" Cells with Moore sum 14 are incremented.
"d01=0" If the cell to the right has state 1, set this to 0.
"d23=x" If the cell to the left has state 3, keep the current state.
"tm4=x-1|d11=1" Moore sum 4 decrements state, and is set to 1 if cell up is 1.
"d0x=x+1" Increment the cell if the one on the right matches.

Example incorrect strings:
"tb=" Incomplete rule.
"tb=tb" tb, tm, d, and g cannot appear on right.
"x+1=x+1" x+1 and x-1 cannot appear on left.
"tb=x" tb, tm, and d require a number (x optionally).
"g2=x" g cannot have a number.