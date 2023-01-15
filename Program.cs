using Omega_Sudoku;
using Omega_Sudoku.IO;
using static Omega_Sudoku.IO.ConsoleInput;
public class Program
{
    private static void Main(string[] args)
    {
        while (true)
        {
            Console.WriteLine("Welcome to Omega Sudoku!");
            Console.WriteLine("to stop the program, type 'exit' now or during the next iteration :)");
            switch (Console.ReadLine())
            {
                case "exit":
                    return;
                default:
                    break;
            }

            try
            {
                Console.WriteLine("Please enter whether you want to input through console or file");
                AInput initialBoard = null;
                AOutput output = null;
                switch (Console.ReadLine())
                {
                    case "console":
                    case "Console":
                    case "terminal":
                    case "Terminal":
                        initialBoard = ConsoleInput.GetInstance();
                        output = ConsoleOutput.GetInstance();
                        break;
                    case "File":
                    case "file":
                        initialBoard = FileInput.GetInstance();
                        output = ConsoleOutput.GetInstance();
                        //output = new FileOutput();
                        break;
                
                }
                SudokuSolver sudokuSolver = new SudokuSolver(initialBoard, output);
            
                sudokuSolver.Test(initialBoard._input);
            }
        
            catch (IllegalBoardException e)
            {
                Console.WriteLine(e.Message);
                break;
            }
        }
        
    }
}