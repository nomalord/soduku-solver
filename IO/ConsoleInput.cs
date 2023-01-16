namespace Omega_Sudoku.IO;

public class ConsoleInput : AInput
{
    static ConsoleInput? _instance = new ConsoleInput();

    private ConsoleInput()
    {
        Input = "";
    }

    public override void Read()
    {
        Console.WriteLine("enter a sudoku Board:");
        Input = Console.ReadLine();
    }

    public static ConsoleInput? GetInstance()
    {
        return _instance;
    }
}