namespace Omega_Sudoku.IO;

public class ConsoleOutput : IOutput
{
    private String Output { get; set;}
    static ConsoleOutput _instance = new ConsoleOutput();
    private ConsoleOutput()
    {
        Output = "";
    }
    public void Write()
    {
        Console.WriteLine(Output);
    }
    public static void GetInstance(string output)
    {
        _instance.Output = output;
        _instance.Write();
    }
}