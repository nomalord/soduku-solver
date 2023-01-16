namespace Omega_Sudoku.IO;

public class FileOutput : AOutput
{   
    private string Path { get; set; }
    static FileOutput? _instance = new FileOutput();

    private FileOutput()
    {
    }

    public override void Write(string message)
    {
        using (StreamWriter writer = File.AppendText(Path))  
        {  
            writer.WriteLine(message);
        }
    }
    public static FileOutput? GetInstance()
    {
        Console.WriteLine("enter a path to write to:");
        _instance.Path = Console.ReadLine();
        return _instance;
    }
}