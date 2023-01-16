namespace Omega_Sudoku;

public class IllegalBoardCharacter : Exception
{
    public IllegalBoardCharacter(string message) : base(message)
    {
        
    }
}