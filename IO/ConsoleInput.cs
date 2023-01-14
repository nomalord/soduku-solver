namespace Omega_Sudoku.IO;

public class ConsoleInput : IInput
{
    private String Input { get; set; }
    static ConsoleInput _instance = new ConsoleInput();
    private ConsoleInput()
    {
        Input = "";
    }
    public String? Read()
    {
        Console.WriteLine("enter a sudoku Board:");
        return Console.ReadLine();
    }
    public static String GetInstance()
    {
        _instance.Input = _instance.Read();
        return _instance.Input;
    }
}