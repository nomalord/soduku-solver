using Omega_Sudoku;

internal class Program
{
    private static void Main(string[] args)
    {
        SudokuSolver sudokuSolver = new SudokuSolver(81);
        sudokuSolver.Test();
    }
}