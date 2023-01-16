namespace Omega_Sudoku.IO;

public class FileOutput : AOutput
{
    private string _path { get; set; }
    static FileOutput? _instance = new FileOutput();

    private FileOutput()
    {
    }

    public override void Write(string message)
    {
        using (StreamWriter writer = new StreamWriter(_path))  
        {  
            writer.WriteLine(message);
        }
    }
    public static FileOutput? GetInstance()
    {
        Console.WriteLine("enter a path to write to:");
        _instance._path = Console.ReadLine();
        return _instance;
    }
}