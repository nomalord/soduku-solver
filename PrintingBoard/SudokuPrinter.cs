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
    public static string? print_board(Dictionary<string, string>? gridValues, string[] cells, int sqrSize
    , string rows, string cols, AOutput? output)
    {
        if (gridValues == null) return null;

        if (output == null)
        {
            return string.Join("", (from cell in cells select gridValues[cell]).ToArray());
        }

        var width = 1 + (from cell in cells
                         select (char.Parse(gridValues[cell]) - '0').ToString().Length).Max();
        var line = "\n" + string.Join("+", Enumerable.Repeat(new string('-', width * sqrSize), sqrSize).ToArray());

        string[] lineCheckDig = new string[sqrSize - 1];
        string[] lineCheckLet = new string[sqrSize - 1];

        for (var i = 1; i < sqrSize; i++)
        {
            lineCheckDig[i - 1] = ((int)((double)i / sqrSize * Math.Pow(sqrSize, 2))).ToString();
            lineCheckLet[i - 1] = ((char)(int.Parse(lineCheckDig[i - 1]) - 1 + 'A')).ToString();
        }

        foreach (var row in rows)
            output.Write(string.Join("",
                (from column in cols
                 select Center((char.Parse(gridValues["" + row + column]) - '0').ToString(), width) +
                        (Array.Exists(lineCheckDig, element => element == (column - '0').ToString()) ? "|" : "")
                )
                .ToArray()) + (Array.Exists(lineCheckLet, element => element == row.ToString()) ? line : ""));

        output.Write("");
        //Linq to print the values of GridValues
        output.Write(string.Join("", (from cell in cells select gridValues[cell]).ToArray()));
        return string.Join("", (from cell in cells select gridValues[cell]).ToArray());
    }
}