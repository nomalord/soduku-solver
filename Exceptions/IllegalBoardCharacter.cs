namespace Omega_Sudoku;

internal class IllegalBoardCharacter : Exception
{
    public IllegalBoardCharacter(string message) : base(message)
    {
        
    }
}