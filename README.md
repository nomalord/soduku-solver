
# Sudoku Solver

a Sudoku Solver


## Authors

- [@nomalord](https://www.github.com/nomalord)


## Features

- Solving of Sudoku boards which are perfect square roots of square roots
- interactive ui


## Optimizations

instead of a regular brute force method to solve Sudoku boards
Backtracking was used in order to speedup the proccess.

Backtracking alone however, is not quick enough. constraint
propogation was used to enchanse the solver's ability to solve
boards. by eliminating possible values from the peers of cells
we cut back on the amount of divergent outcomes which derive from
the Backtracking algorithm.


- SudokuBacktracking: the bread and butter of the algorithm, whenever there is a divergence in the possible values of a cell, try the first possible value of the cell with the least amount of possible values.

- constraints: the constraint to help the backtracking run smoother (updates the peers of each cell whenever it is updated and removes used possible values).

- AInput\Aoutput: abstract classes which are the building blocks of the ConsoleInput\Output and FileInput\Output. Make creating new IO options easy.

- SudokuParser: checks whether the board has any illegal characters and creates the dictionary which represents the board.
## Run Locally

Clone the project

```bash
  git clone https://github.com/nomalord/soduku-solver
  git clone https://github.com/nomalord/Sudoku_Testing
```

Go to the project directory

```bash
  cd sudoku-solver
```

## Screenshots

![App Screenshot](https://i.imgur.com/zxVNBZr.png)
## input from console, outputs to console

![App Screenshot](https://i.imgur.com/jsCWfnJ.png)
## input from file, appends to file

![App Screenshot](https://i.imgur.com/ey3QVp3.png)
## exit the solver


## Running Tests

To run tests, make sure you installed the tests project above, open in visual studio and select run tests

