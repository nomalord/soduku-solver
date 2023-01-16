namespace Omega_Sudoku.IO;

public abstract class AInput
{
    protected internal string? Input { get; set; }
    public abstract void Read();
}