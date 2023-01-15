namespace Omega_Sudoku.IO;

public class ConsoleOutput : AOutput
{
    static ConsoleOutput _instance = new ConsoleOutput();
    private ConsoleOutput()
    {
    }
    public override void Write(string message)
    {
        Console.WriteLine(message);
    }
    public static ConsoleOutput GetInstance()
    {
        return _instance;
    }
}