namespace Omega_Sudoku.IO;

public class ConsoleInput : AInput
{
    static ConsoleInput _instance = new ConsoleInput();
    private ConsoleInput()
    {
        _input = "";
    }
    public override void Read()
    {
        Console.WriteLine("enter a sudoku Board:");
        _input = Console.ReadLine();
    }
    public static ConsoleInput GetInstance()
    {
        return _instance;
    }
}