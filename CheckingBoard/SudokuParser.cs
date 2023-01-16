using System.ComponentModel;

namespace Omega_Sudoku.CheckingBoard;

using static Omega_Sudoku.Constraints.Constraints;

public static class SudokuParser
{
    /// <summary>adds together two lists to form a String Matrix(key = matA, value = matB)</summary>
    public static string[][] Zip(string[]? matA, string[] matB)
    {
        var lengthCheck = Math.Min(matA.Length, matB.Length);
        string[][] zippedMatrix = new string[lengthCheck][];
        for (var i = 0; i < lengthCheck; i++) zippedMatrix[i] = new string[] { matA[i].ToString(), matB[i].ToString() };
        return zippedMatrix;
    }

    /// <summary>Given a string of 81 digits (or . or 0 or -), return a dict of {cell:GridValues}</summary>
    public static Dictionary<string, string>? parse_grid(string grid, string[]? cells, string? digits
        , Dictionary<string, IEnumerable<string>>? peers, Dictionary<string, IGrouping<string, string[]>>? units)
    {
        var grid2 = (from charDigit in grid
            where (!digits.Contains(charDigit))
            select charDigit).Distinct().ToArray();
        if (grid2.Length > 1)
        {
            throw new IllegalBoardCharacter("Illegal character in board");
        }


        Dictionary<string, string>?
            gridValues = cells.ToDictionary(s => s, s => digits); //To start, every cell can be any digit

        if ((from keyValue in Zip(cells, (from initialValue in grid
                    select initialValue.ToString()).ToArray())
                let stringKey = keyValue[0]
                let dictValue = keyValue[1]
                where digits.Contains(dictValue) &&
                      StartConstraints(gridValues, stringKey, dictValue, peers, units) == null
                select stringKey).Any())
        {
            return null;
        }


        return gridValues;
    }
}