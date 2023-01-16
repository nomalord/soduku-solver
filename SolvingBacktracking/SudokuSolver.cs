using System.Data.Common;
using Omega_Sudoku.IO;
using static Omega_Sudoku.Constraints.Constraints;
using static Omega_Sudoku.PrintingBoard.SudokuPrinter;
using static Omega_Sudoku.CheckingBoard.SudokuParser;

namespace Omega_Sudoku;

public class SudokuSolver
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
    private AOutput _output = ConsoleOutput.GetInstance();
    private AInput _input = ConsoleInput.GetInstance();
    private Dictionary<string, string> board;

    public string[] cross(string wholeA, string wholeB)
    {
        return (from sliceA in wholeA
            from sliceB in wholeB
            select "" + sliceA + sliceB).ToArray();
    }


    public SudokuSolver(string rows = null, string cols = null, string digits = null, string[] cells = null, Dictionary<string, IEnumerable<string>> peers = null, Dictionary<string, IGrouping<string, string[]>> units = null, int sqrSize = default, Dictionary<string, string> board = null)
    {
        _rows = rows;
        _cols = cols;
        _digits = digits;
        _Cells = cells;
        _peers = peers;
        _units = units;
        _sqrSize = sqrSize;
        this.board = board;
    }

    /// <summary>Using depth-first search and propagation, try all possible GridValues.</summary>
    /// The search function is a recursive function that attempts to solve a Sudoku puzzle represented
    /// by the input GridValues, which is a dictionary containing the cell values of the puzzle.
    /// The function first checks if the input is null or if all cells have only one possible value,
    /// in which case it returns the input. If not, it selects the cell with the least number of possible
    /// values and iterates through each possible value for that cell, calling the search function recursively
    /// with the updated GridValues that includes the selected value for the chosen cell.
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
    
    /// <summary>
    /// The some function is a helper function that takes an enumerable input of type T and returns
    /// the first non-null element in the sequence. If all elements in the sequence are null,
    /// it returns the default value for the type T. This function is used in the search function to return
    /// the first non-null solution found among the recursive calls.
    /// </summary>
    /// <param name="seq"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T some<T>(IEnumerable<T> seq)
    {
        foreach (var e in seq)
            if (e != null)
                return e;
        return default;
    }

    

    public string Solve(string initialBoard)
    {
        // var hardest = "850002400720000009004000000000107002305000900040000000000080070017000000000036040";
        //hardest = "000006000059000008200008000045000000003000000006003054000325006000000000000000000";

        var start = DateTime.Now;
        //for (var i = 0; i < 300; i++)
        //{
        //    search(parse_grid(hardest));
        //}
        var completeBoard = search(board);
        if(completeBoard == null)
            throw new IllegalBoardException("Board is unsolvable");
        Console.WriteLine("Solving 'hardest' sodoku took on average " + (DateTime.Now - start).TotalMilliseconds +
                          " milliseconds");
        
        return print_board(completeBoard, _Cells, _sqrSize, _rows, _cols, _output);
        
    }


    public void wrapper(AInput input, AOutput output)
    {
        try
        {
            _input = input;
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

            
            board = parse_grid(input._input, _Cells, _digits, _peers, _units);
        }

        catch (IllegalBoardException e)
        {
            Console.WriteLine(e.Message);
        }

        catch (IllegalBoardCharacter e)
        {
            Console.WriteLine(e.Message);
        }
    }
}