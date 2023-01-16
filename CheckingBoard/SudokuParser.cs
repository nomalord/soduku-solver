using System.ComponentModel;

namespace Omega_Sudoku.CheckingBoard;
using static Omega_Sudoku.Constraints.Constraints;

public static class SudokuParser
{
    /// <summary>adds together two lists to form a String Matrix(key = A, value = B)</summary>
    public static string[][] zip(string[] A, string[] B)
    {
        var lengthCheck = Math.Min(A.Length, B.Length);
        string[][] ZippedMatrix = new string[lengthCheck][];
        for (var i = 0; i < lengthCheck; i++) ZippedMatrix[i] = new string[] { A[i].ToString(), B[i].ToString() };
        return ZippedMatrix;
    }
    
    /// <summary>Given a string of 81 digits (or . or 0 or -), return a dict of {cell:GridValues}</summary>
    public static Dictionary<string, string> parse_grid(string grid, string[] _Cells, string _digits
    ,Dictionary<string, IEnumerable<string>> _peers, Dictionary<string, IGrouping<string, string[]>> _units)
    {
        var grid2 = (from charDigit in grid
            where (!_digits.Contains(charDigit))
            select charDigit).Distinct().ToArray();
        if (grid2.Length > 1)
        {
            throw new IllegalBoardCharacter("Illegal character in board");
        }
        

        var gridValues = _Cells.ToDictionary(s => s, s => _digits); //To start, every cell can be any digit

        foreach (var keyValue in zip(_Cells, (from initialValue in grid
                     select initialValue.ToString()).ToArray()))
        {
            var stringKey = keyValue[0];
            var dictValue = keyValue[1];

            if (_digits.Contains(dictValue) && StartConstraints(gridValues, stringKey, dictValue, _peers, _units) == null) return null;
        }
        

        return gridValues;
    }
}