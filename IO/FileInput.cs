namespace Omega_Sudoku.IO;

public class FileInput : AInput
{
    private string Path { get; set; }
    static FileInput? _instance = new FileInput();

    private FileInput()
    {
        Input = "";
        Path = "";
    }

    public override void Read()
    {
        Console.WriteLine("enter a file path to the sudoku board / boards:");
        Path = Console.ReadLine();
        try
        {
            Input = File.ReadAllText(Path);
        }
        catch (IOException e)
        {
            Console.WriteLine("please enter a valid file path");
            Read();
        }
    }

    public static FileInput? GetInstance()
    {
        return _instance;
    }
}