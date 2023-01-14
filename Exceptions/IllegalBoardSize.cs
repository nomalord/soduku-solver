namespace Omega_Sudoku;

internal class IllegalBoardSize : Exception
{
    public IllegalBoardSize(string message) : base(message)
    {
    }
}