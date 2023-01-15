using System.Data.Common;
using Omega_Sudoku.IO;
using static Omega_Sudoku.Constraints.Constraints;
using static Omega_Sudoku.PrintingBoard.SudokuPrinter;
using static Omega_Sudoku.CheckingBoard.SudokuParser;

namespace Omega_Sudoku;

internal class SudokuSolver
{
    // Throughout this program we have:
    //   row is a row,    e.g. 'A'
    //   column is a column, e.g. '3'
    //   StringKey is a cell, e.g. 'A3'
    //   DictValue is a digit,  e.g. '9'
    //   unit is a unit,   e.g. ['A1','B1','C1','D1','E1','F1','G1','H1','I1']
    //   g is a grid,   e.g. 81 non-blank chars, e.g. starting with '.18...7...
    //   GridValues is a dict of possible GridValues, e.g. {'A1':'123489', 'A2':'8', ...}
    private string _rows;
    private string _cols; //Enumerable.Range(30, 21).ToArray();
    private string _digits; //Enumerable.Range(30, 21).ToArray();
    private string[] _Cells;
    private Dictionary<string, IEnumerable<string>> _peers;
    private Dictionary<string, IGrouping<string, string[]>> _units;
    private int _sqrSize; //bool isSquare = result%1 == 0;
    private AOutput _output;

    public string[] cross(string wholeA, string wholeB)
    {
        return (from sliceA in wholeA
            from sliceB in wholeB
            select "" + sliceA + sliceB).ToArray();
    }

    public SudokuSolver(AInput input, AOutput output)
    {
        input.Read();
        _output = output;
        int boardSize = input._input.Length;
        _sqrSize = Convert.ToInt32(Math.Sqrt(Math.Sqrt(boardSize)));

        if (Convert.ToInt32(Math.Pow(_sqrSize, 4)) != boardSize)
            throw new IllegalBoardSize("Board size must be a square of a square");
        if (_sqrSize < 1)
            throw new IllegalBoardSize("Board size must be at least 1x1");


        _rows = new string(Enumerable.Range('A', Convert.ToInt32(Math.Pow(_sqrSize, 2))).Select(i => (char)i)
            .ToArray());
        _cols = new string(Enumerable.Range(1, Convert.ToInt32(Math.Pow(_sqrSize, 2))).Select(i => (char)(i + 48))
            .ToArray());

        _digits = _cols;

        if (_cols == null) throw new IllegalBoardException("Invalid board size");
        _Cells = cross(_rows, _cols);

        string[] rowString = new string[_sqrSize], columnString = new string[_sqrSize];

        char[] chars;

        for (var i = 0; i < Math.Sqrt(_rows.Length); i++)
        {
            chars = _rows.Skip(i * _sqrSize).Take(_sqrSize).ToArray();
            rowString[i] = new string(chars);

            chars = _cols.Skip(i * _sqrSize).Take(_sqrSize).ToArray();
            columnString[i] = new string(chars);
        }

        var unitlist = (from column in _cols
                select cross(_rows, column.ToString()))
            .Concat(from row in _rows
                select cross(row.ToString(), _cols))
            .Concat(from rowS in rowString
                from columnS in columnString
                select cross(rowS, columnS));

        _units = (from cell in _Cells
                from unit in unitlist
                where unit.Contains(cell)
                group unit by cell
                into unitGroup
                select unitGroup)
            .ToDictionary(g => g.Key);

        _peers = (from cell in _Cells
                from unit in _units[cell]
                from unitString in unit
                where unitString != cell
                group unitString by cell
                into cellUnitGroup
                select cellUnitGroup)
            .ToDictionary(g => g.Key, g => g.Distinct());
    }

    /// <summary>Using depth-first search and propagation, try all possible GridValues.</summary>
    public Dictionary<string, string> search(Dictionary<string, string> GridValues)
    {
        if (GridValues == null) return null; // Failed earlier
        if (all(from cell in _Cells
                select GridValues[cell].Length == 1 ? "" : null))
            return GridValues; // Solved!

        // Choose the unfilled cell StringKey with the fewest possibilities
        var LeastPossibilityCell = (from cell in _Cells
            where GridValues[cell].Length > 1
            orderby GridValues[cell].Length ascending
            select cell).First();

        return some(from possibleValue in GridValues[LeastPossibilityCell]
            select search(
                StartConstraints(new Dictionary<string, string>(GridValues), LeastPossibilityCell, possibleValue.ToString(),
                    _peers, _units)
            )
        );
    }
    

    public T some<T>(IEnumerable<T> seq)
    {
        foreach (var e in seq)
            if (e != null)
                return e;
        return default;
    }

    

    public void Test(string initialBoard)
    {
        // var hardest = "850002400720000009004000000000107002305000900040000000000080070017000000000036040";
        //hardest = "000006000059000008200008000045000000003000000006003054000325006000000000000000000";

        var start = DateTime.Now;
        //for (var i = 0; i < 300; i++)
        //{
        //    search(parse_grid(hardest));
        //}
        var completeBoard = search(parse_grid(initialBoard, _Cells, _digits, _peers, _units));
        Console.WriteLine("Solving 'hardest' sodoku took on average " + (DateTime.Now - start).TotalMilliseconds +
                          " milliseconds");
        print_board(completeBoard, _Cells, _sqrSize, _rows, _cols, _output);

        Console.WriteLine("Press enter to finish");
        Console.ReadLine();
    }
}