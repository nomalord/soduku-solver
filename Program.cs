using Omega_Sudoku;
using Omega_Sudoku.IO;
using static Omega_Sudoku.IO.ConsoleInput;
internal class Program
{
    private static void Main(string[] args)
    {
        var initialBoard = ConsoleInput.GetInstance();
        SudokuSolver sudokuSolver = new SudokuSolver(initialBoard.Length);
        sudokuSolver.Test(initialBoard);
    }
}