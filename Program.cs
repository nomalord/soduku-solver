using Omega_Sudoku;
using Omega_Sudoku.IO;
using static Omega_Sudoku.IO.ConsoleInput;
public class Program
{
    private static void Main(string[] args)
    {
        AInput initialBoard = ConsoleInput.GetInstance();
        AOutput output = ConsoleOutput.GetInstance();
        SudokuSolver sudokuSolver = new SudokuSolver(initialBoard, output);
        sudokuSolver.Test(initialBoard._input);
    }
}