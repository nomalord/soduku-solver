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
    //   _sqrSize holds the square root of the square root of the board size.
    //      It is used to determine the size of the rows, columns, and sub-grids in the Sudoku board.
    //   AInput input, AOutput output: These are the input and output variables that are passed to the method.
    // They are used to read the input data and write the output data.
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

    

    public string Solve()
    {
        // var hardest = "850002400720000009004000000000107002305000900040000000000080070017000000000036040";
        //hardest = "000006000059000008200008000045000000003000000006003054000325006000000000000000000";

        var start = DateTime.Now;
        //for (var i = 0; i < 300; i++)
        //{
        //    search(parse_grid(hardest));
        //}
        var completeBoard = search(board);
        Console.WriteLine("'Solving' sodoku took on average " + (DateTime.Now - start).TotalMilliseconds +
                          " milliseconds");
        if (completeBoard == null)
            throw new IllegalBoardException("Board is unsolvable");

        return print_board(completeBoard, _Cells, _sqrSize, _rows, _cols, _output);
        
    }

    /// <summary>
    /// The wrapper function is a method that is used to set up a Sudoku board. It takes in two parameters,
    /// AInput input and AOutput output, and performs the following actions:
    /// It assigns the input and output variables to the class-level variables _input and _output.
    ///     It reads the input and assigns the length of the input string to the boardSize variable.
    ///     It calculates the square root of the square root of the board size and assigns it to the _sqrSize variable.
    ///     It checks if the board size is a square of a square and throws an IllegalBoardSize exception if it is not.
    ///     It checks if the _sqrSize is less than 1 and throws an IllegalBoardSize exception if it is.
    /// It generates the rows, columns and digits of the board using Enumerable.Range method, assigns them to _rows,
    /// _cols and _digits respectively.
    ///     It generates the cells of the board by taking the cross product of _rows and _cols and assigns it to _Cells.
    ///     It generates the sub-grids of the board by taking substrings of _rows and _cols and assigns them
    /// to rowString and columnString.
    ///     It generates the units of the board by concatenating the rows, columns and sub-grids and assigns them to unitlist.
    ///     It generates the peers of each cell by grouping the cells in the units by the original cell and assigns it to _peers.
    ///     It parses the input grid and assigns it to the board variable.
    ///     It catches two types of exceptions IllegalBoardException and IllegalBoardCharacter and prints the
    /// message of the exception.
    ///     The wrapper method is a key component of the Sudoku solver as it sets up the initial state of the
    /// board and prepares it for solving.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="output"></param>
    /// <exception cref="IllegalBoardSize"></exception>
    public bool Wrapper(AInput? input, AOutput? output, string Input = "")
    {
        try
        {
            _input = input;
            _output = output;
            int boardSize;

            if (input != null && output != null)
            {
                input.Read();
                boardSize = input._input.Length;
            }
            else
            {
                boardSize = Input.Length;
            }

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

            if (_cols == null) throw new IllegalBoardSize("Invalid board size");
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
            //a list of units that is generated by concatenating three different types of units:
            //
            // The rows: Each row is considered a unit and is represented by a list of cells that are in that row.
            //
            //     The columns: Each column is considered a unit and is represented by a list of cells that are in that column.
            //
            //     The sub-grids: Each sub-grid is considered a unit and is represented by a list of cells that are in that sub-grid.
            var unitlist = (from column in _cols
                            select cross(_rows, column.ToString()))
                .Concat(from row in _rows
                        select cross(row.ToString(), _cols))
                .Concat(from rowS in rowString
                        from columnS in columnString
                        select cross(rowS, columnS));

            //A dictionary that maps each cell to a list of units that the cell belongs to.
            _units = (from cell in _Cells
                      from unit in unitlist
                      where unit.Contains(cell)
                      group unit by cell
                    into unitGroup
                      select unitGroup)
                .ToDictionary(g => g.Key);

            //holds a dictionary of peers for each cell,
            //where each peer is a cell that shares a unit with the original cell.
            //The peers are generated by grouping the cells in the units by the original cell.

            _peers = (from cell in _Cells
                      from unit in _units[cell]
                      from unitString in unit
                      where unitString != cell
                      group unitString by cell
                    into cellUnitGroup
                      select cellUnitGroup)
                .ToDictionary(g => g.Key, g => g.Distinct());

            if (Input == "")
                board = parse_grid(input._input, _Cells, _digits, _peers, _units);
            else
                board = parse_grid(Input, _Cells, _digits, _peers, _units);
            return true;
        }
        
        catch (IllegalBoardException e)
        {
            Console.WriteLine(e.Message);
            return false;
        }

        catch (IllegalBoardCharacter e)
        {
            Console.WriteLine(e.Message);
            throw new IllegalBoardCharacter("");
            return false;
        }
        catch (IllegalBoardSize e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }
}