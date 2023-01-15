using Omega_Sudoku.IO;

namespace Omega_Sudoku.PrintingBoard;

public static class SudokuPrinter
{
    public static string Center(string s, int width)
    {
        var n = width - s.Length;
        if (n <= 0) return s;
        var half = n / 2;

        if (n % 2 > 0 && width % 2 > 0) half++;

        return new string(' ', half) + s + new string(' ', n - half);
    }

    /// <summary>Used for debugging.</summary>
    public static Dictionary<string, string> print_board(Dictionary<string, string> GridValues, string[] _Cells, int _sqrSize
    ,string _rows, string _cols, AOutput output)
    {
        if (GridValues == null) return null;

        var width = 1 + (from cell in _Cells
            select (char.Parse(GridValues[cell]) - '0').ToString().Length).Max();
        var line = "\n" + string.Join("+", Enumerable.Repeat(new string('-', width * _sqrSize), _sqrSize).ToArray());

        string[] lineCheckDig = new string[_sqrSize - 1];
        string[] lineCheckLet = new string[_sqrSize - 1];

        for (var i = 1; i < _sqrSize; i++)
        {
            lineCheckDig[i - 1] = ((int)((double)i / _sqrSize * Math.Pow(_sqrSize, 2))).ToString();
            lineCheckLet[i - 1] = ((char)(int.Parse(lineCheckDig[i - 1]) - 1 + 'A')).ToString();
        }

        foreach (var row in _rows)
            output.Write(string.Join("",
                (from column in _cols
                    select Center((char.Parse(GridValues["" + row + column]) - '0').ToString(), width) +
                           (Array.Exists(lineCheckDig, element => element == (column - '0').ToString()) ? "|" : "")
                )
                .ToArray()) + (Array.Exists(lineCheckLet, element => element == row.ToString()) ? line : ""));

        output.Write("");
        //Linq to print the values of GridValues
        output.Write(string.Join(" ", (from cell in _Cells select GridValues[cell]).ToArray()));
        return GridValues;
    }
}