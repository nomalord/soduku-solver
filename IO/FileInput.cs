namespace Omega_Sudoku.IO;

public class FileInput : AInput
{
    private string _path { get; set; }
    static FileInput _instance = new FileInput();
    private FileInput()
    {
        _input = "";
        _path = "";
    }
    public override void Read()
    {
        Console.WriteLine("enter a file path to the sudoku board / boards:");
        _path = Console.ReadLine();
        try
        {
            _input = File.ReadAllText(_path);
        }
        catch (IOException e)
        {
            Console.WriteLine("please enter a valid file path");
            Read();
        }
    }
    public static FileInput GetInstance()
    {
        return _instance;
    }
}