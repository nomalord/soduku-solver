namespace Omega_Sudoku.IO;

public abstract class AInput
{
    protected internal string? _input { get; set; }
    public abstract void Read();
}