namespace Omega_Sudoku;

public class IllegalBoardSize : Exception
{
    public IllegalBoardSize(string message) : base(message)
    {
    }
}