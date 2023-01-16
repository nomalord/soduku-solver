using Omega_Sudoku;
using Omega_Sudoku.IO;
using static Omega_Sudoku.IO.ConsoleInput;
public class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Welcome to Omega Sudoku!");
        while (true)
        {
            Console.WriteLine("to stop the program, type 'exit' now or during the next iteration :)");
            switch (Console.ReadLine())
            {
                case "exit":
                    return;
                default:
                    break;
            }

            Console.WriteLine("Please enter whether you want to input through console or file");
            AInput initialBoard = null;
            AOutput output = null;
            switch (Console.ReadLine())
            {
                case "console":
                case "Console":
                case "terminal":
                case "Terminal":
                case "c":
                    initialBoard = ConsoleInput.GetInstance();
                    output = ConsoleOutput.GetInstance();
                    break;
                case "File":
                case "file":
                case "f":
                    initialBoard = FileInput.GetInstance();
                    output = FileOutput.GetInstance();
                    break;
                default:
                    Console.WriteLine("Invalid input");
                    continue;

            }

            SudokuSolver sudokuSolver = new SudokuSolver();
            if (! sudokuSolver.wrapper(initialBoard, output))
            {
                continue;
            }
            try
            {
                sudokuSolver.Solve();
            }
            catch (IllegalBoardException e)
            {
                Console.WriteLine(e.Message);
            }
            

        }
    }
}