﻿using System.Data.Common;

namespace Omega_Sudoku
{
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

        public string[] cross(string wholeA, string wholeB)
        {
            return (from sliceA in wholeA
                    from sliceB in wholeB
                    select "" + sliceA + sliceB).ToArray();
        }

        public SudokuSolver(int boardSize)
        {
            _sqrSize = Convert.ToInt32(Math.Sqrt(Math.Sqrt(boardSize)));

            if (Convert.ToInt32(Math.Pow(_sqrSize, 4)) != boardSize)
                throw new IllegalBoardException("Board size must be a square of a square");
            if (_sqrSize < 1)
                throw new IllegalBoardException("Board size must be at least 1x1");


            _rows = new String(Enumerable.Range('A', Convert.ToInt32(Math.Pow(_sqrSize, 2))).Select(i => (Char)i).ToArray());
            _cols = new String(Enumerable.Range(1, Convert.ToInt32(Math.Pow(_sqrSize, 2))).Select(i => (char)(i + 48)).ToArray());

            _digits = _cols;

            if (_cols == null)
            {
                throw new IllegalBoardException("Invalid board size");
            }
            _Cells = cross(_rows, _cols);

            string[] rowString = new string[_sqrSize], columnString = new string[_sqrSize];

            char[] chars;

            for (int i = 0; i < Math.Sqrt(_rows.Length); i++)
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
                      group unit by cell into unitGroup
                      select unitGroup)
                     .ToDictionary(g => g.Key);

            _peers = (from cell in _Cells
                      from unit in _units[cell]
                      from unitString in unit
                      where unitString != cell
                      group unitString by cell into cellUnitGroup
                      select cellUnitGroup)
                     .ToDictionary(g => g.Key, g => g.Distinct());

        }

        /// <summary>adds together two lists to form a String Matrix(key = A, value = B)</summary>
        public string[][] zip(string[] A, string[] B)
        {
            var lengthCheck = Math.Min(A.Length, B.Length);
            string[][] ZippedMatrix = new string[lengthCheck][];
            for (var i = 0; i < lengthCheck; i++)
            {
                ZippedMatrix[i] = new string[] { A[i].ToString(), B[i].ToString() };
            }
            return ZippedMatrix;
        }

        /// <summary>Given a string of 81 digits (or . or 0 or -), return a dict of {cell:GridValues}</summary>
        public Dictionary<string, string> parse_grid(string grid)
        {
            var grid2 = from charDigit in grid
                        where "0.-123456789".Contains(charDigit)
                        select charDigit;

            var GridValues = _Cells.ToDictionary(s => s, s => _digits); //To start, every cell can be any digit

            foreach (var KeyValue in zip(_Cells, (from InitialValue in grid
                                                  select InitialValue.ToString()).ToArray()))
            {
                var StringKey = KeyValue[0];
                var DictValue = KeyValue[1];

                if (_digits.Contains(DictValue) && assign(GridValues, StringKey, DictValue) == null)
                {
                    return null;
                }
            }
            return GridValues;
        }

        /// <summary>Using depth-first search and propagation, try all possible GridValues.</summary>
        public Dictionary<string, string> search(Dictionary<string, string> GridValues)
        {
            if (GridValues == null)
            {
                return null; // Failed earlier
            }
            if (all(from cell in _Cells
                    select GridValues[cell].Length == 1 ? "" : null))
            {
                return GridValues; // Solved!
            }

            // Chose the unfilled cell StringKey with the fewest possibilities
            var LeastPossibilityCell = (from cell in _Cells
                                        where GridValues[cell].Length > 1
                                        orderby GridValues[cell].Length ascending
                                        select cell).First();

            return some(from PossibleValue in GridValues[LeastPossibilityCell]
                        select search(
                            assign(new Dictionary<string, string>(GridValues), LeastPossibilityCell, PossibleValue.ToString())
                            )
                        );
        }

        /// <summary>Eliminate all the other GridValues (except DictValue) from GridValues[StringKey] and propagate &lt;= 2.</summary>
        public Dictionary<string, string> assign(Dictionary<string, string> GridValues, string StringKey, string DictValue)
        {
            if (all(
                    from digitNotStart in GridValues[StringKey]
                    where digitNotStart.ToString() != DictValue
                    select eliminate(GridValues, StringKey, digitNotStart.ToString())
                    )
                )
            {
                return GridValues;
            }
            return null;
        }

        /// <summary>Eliminate DictValue from GridValues[StringKey]; propagate when GridValues or places &lt;= 2.</summary>
        public Dictionary<string, string> eliminate(Dictionary<string, string> GridValues, string StringKey, string DictValue)
        {
            if (!GridValues[StringKey].Contains(DictValue))
            {
                return GridValues;
            }
            GridValues[StringKey] = GridValues[StringKey].Replace(DictValue, "");
            if (GridValues[StringKey].Length == 0)
            {
                return null; //Contradiction: removed last value
            }
            else if (GridValues[StringKey].Length == 1)
            {
                //If there is only one value (LastValue) left in cell, remove it from _peers
                var LastValue = GridValues[StringKey];
                if (!all(from peer in _peers[StringKey]
                         select eliminate(GridValues, peer, LastValue)))
                {
                    return null;
                }
            }

            //Now check the places where DictValue appears in the _units of StringKey
            foreach (var unit in _units[StringKey])
            {
                var ValuePlaces = from cell in unit
                                  where GridValues[cell].Contains(DictValue)
                                  select cell;
                if (ValuePlaces.Count() == 0)
                {
                    return null;
                }
                else if (ValuePlaces.Count() == 1)
                {
                    // DictValue can only be in one place in unit; assign it there
                    if (assign(GridValues, ValuePlaces.First(), DictValue) == null)
                    {
                        return null;
                    }
                }
            }
            return GridValues;
        }

        /// <summary>Checks if all cells have at least 1 possible value &lt;= 2.</summary>
        public bool all<T>(IEnumerable<T> seq)
        {
            foreach (var e in seq)
            {
                if (e == null) return false;
            }
            return true;
        }

        public T some<T>(IEnumerable<T> seq)
        {
            foreach (var e in seq)
            {
                if (e != null) return e;
            }
            return default(T);
        }

        public string Center(string s, int width)
        {
            var n = width - s.Length;
            if (n <= 0) return s;
            var half = n / 2;

            if (n % 2 > 0 && width % 2 > 0) half++;

            return new string(' ', half) + s + new String(' ', n - half);
        }
        
        /// <summary>Used for debugging.</summary>
        public Dictionary<string, string> print_board(Dictionary<string, string> GridValues)
        {
            if (GridValues == null) return null;

            var width = 1 + (from cell in _Cells
                             select (char.Parse(GridValues[cell]) - '0').ToString().Length).Max();
            var line = "\n" + String.Join("+", Enumerable.Repeat(new String('-', width * _sqrSize), _sqrSize).ToArray());

            string[] lineCheckDig = new string[_sqrSize - 1];
            string[] lineCheckLet = new string[_sqrSize - 1];
            
            for(int i = 1; i < _sqrSize; i++)
            {
                lineCheckDig[i-1] = ((int)((double) i / _sqrSize * Math.Pow(_sqrSize, 2))).ToString();
                lineCheckLet[i-1] = ((char)(int.Parse(lineCheckDig[i - 1]) - 1 + 'A')).ToString();
            }

            foreach (var row in _rows)
            {
                Console.WriteLine(String.Join("",
                    (from column in _cols
                     select Center((char.Parse(GridValues["" + row + column]) - '0').ToString(), width) + 
                     (Array.Exists(lineCheckDig, element => element == (column - '0').ToString()) ? "|" : "")
                    )
                     .ToArray()) + (Array.Exists(lineCheckLet, element => element == (row).ToString()) ? line : ""));
            }

            Console.WriteLine();
            return GridValues;
        }

        public void Test()
        {

            var hardest = "850002400720000009004000000000107002305000900040000000000080070017000000000036040";
            //hardest = "000006000059000008200008000045000000003000000006003054000325006000000000000000000";
            
            DateTime start = DateTime.Now;
            //for (var i = 0; i < 300; i++)
            //{
            //    search(parse_grid(hardest));
            //}
            var completeBoard = search(parse_grid(hardest));
            Console.WriteLine("Solving 'hardest' sodoku took on average " + (DateTime.Now - start).TotalMilliseconds + " milliseconds");
            print_board(completeBoard);
            
            //            var top95 = @"4.....8.5.3..........7......2.....6.....8.4......1.......6.3.7.5..2.....1.4......
            //52...6.........7.13...........4..8..6......5...........418.........3..2...87.....
            //6.....8.3.4.7.................5.4.7.3..2.....1.6.......2.....5.....8.6......1....
            //48.3............71.2.......7.5....6....2..8.............1.76...3.....4......5....
            //....14....3....2...7..........9...3.6.1.............8.2.....1.4....5.6.....7.8...
            //......52..8.4......3...9...5.1...6..2..7........3.....6...1..........7.4.......3.
            //6.2.5.........3.4..........43...8....1....2........7..5..27...........81...6.....
            //.524.........7.1..............8.2...3.....6...9.5.....1.6.3...........897........
            //6.2.5.........4.3..........43...8....1....2........7..5..27...........81...6.....
            //.923.........8.1...........1.7.4...........658.........6.5.2...4.....7.....9.....
            //6..3.2....5.....1..........7.26............543.........8.15........4.2........7..
            //.6.5.1.9.1...9..539....7....4.8...7.......5.8.817.5.3.....5.2............76..8...
            //..5...987.4..5...1..7......2...48....9.1.....6..2.....3..6..2.......9.7.......5..
            //3.6.7...........518.........1.4.5...7.....6.....2......2.....4.....8.3.....5.....
            //1.....3.8.7.4..............2.3.1...........958.........5.6...7.....8.2...4.......
            //6..3.2....4.....1..........7.26............543.........8.15........4.2........7..
            //....3..9....2....1.5.9..............1.2.8.4.6.8.5...2..75......4.1..6..3.....4.6.
            //45.....3....8.1....9...........5..9.2..7.....8.........1..4..........7.2...6..8..
            //.237....68...6.59.9.....7......4.97.3.7.96..2.........5..47.........2....8.......
            //..84...3....3.....9....157479...8........7..514.....2...9.6...2.5....4......9..56
            //.98.1....2......6.............3.2.5..84.........6.........4.8.93..5...........1..
            //..247..58..............1.4.....2...9528.9.4....9...1.........3.3....75..685..2...
            //4.....8.5.3..........7......2.....6.....5.4......1.......6.3.7.5..2.....1.9......
            //.2.3......63.....58.......15....9.3....7........1....8.879..26......6.7...6..7..4
            //1.....7.9.4...72..8.........7..1..6.3.......5.6..4..2.........8..53...7.7.2....46
            //4.....3.....8.2......7........1...8734.......6........5...6........1.4...82......
            //.......71.2.8........4.3...7...6..5....2..3..9........6...7.....8....4......5....
            //6..3.2....4.....8..........7.26............543.........8.15........8.2........7..
            //.47.8...1............6..7..6....357......5....1..6....28..4.....9.1...4.....2.69.
            //......8.17..2........5.6......7...5..1....3...8.......5......2..4..8....6...3....
            //38.6.......9.......2..3.51......5....3..1..6....4......17.5..8.......9.......7.32
            //...5...........5.697.....2...48.2...25.1...3..8..3.........4.7..13.5..9..2...31..
            //.2.......3.5.62..9.68...3...5..........64.8.2..47..9....3.....1.....6...17.43....
            //.8..4....3......1........2...5...4.69..1..8..2...........3.9....6....5.....2.....
            //..8.9.1...6.5...2......6....3.1.7.5.........9..4...3...5....2...7...3.8.2..7....4
            //4.....5.8.3..........7......2.....6.....5.8......1.......6.3.7.5..2.....1.8......
            //1.....3.8.6.4..............2.3.1...........958.........5.6...7.....8.2...4.......
            //1....6.8..64..........4...7....9.6...7.4..5..5...7.1...5....32.3....8...4........
            //249.6...3.3....2..8.......5.....6......2......1..4.82..9.5..7....4.....1.7...3...
            //...8....9.873...4.6..7.......85..97...........43..75.......3....3...145.4....2..1
            //...5.1....9....8...6.......4.1..........7..9........3.8.....1.5...2..4.....36....
            //......8.16..2........7.5......6...2..1....3...8.......2......7..3..8....5...4....
            //.476...5.8.3.....2.....9......8.5..6...1.....6.24......78...51...6....4..9...4..7
            //.....7.95.....1...86..2.....2..73..85......6...3..49..3.5...41724................
            //.4.5.....8...9..3..76.2.....146..........9..7.....36....1..4.5..6......3..71..2..
            //.834.........7..5...........4.1.8..........27...3.....2.6.5....5.....8........1..
            //..9.....3.....9...7.....5.6..65..4.....3......28......3..75.6..6...........12.3.8
            //.26.39......6....19.....7.......4..9.5....2....85.....3..2..9..4....762.........4
            //2.3.8....8..7...........1...6.5.7...4......3....1............82.5....6...1.......
            //6..3.2....1.....5..........7.26............843.........8.15........8.2........7..
            //1.....9...64..1.7..7..4.......3.....3.89..5....7....2.....6.7.9.....4.1....129.3.
            //.........9......84.623...5....6...453...1...6...9...7....1.....4.5..2....3.8....9
            //.2....5938..5..46.94..6...8..2.3.....6..8.73.7..2.........4.38..7....6..........5
            //9.4..5...25.6..1..31......8.7...9...4..26......147....7.......2...3..8.6.4.....9.
            //...52.....9...3..4......7...1.....4..8..453..6...1...87.2........8....32.4..8..1.
            //53..2.9...24.3..5...9..........1.827...7.........981.............64....91.2.5.43.
            //1....786...7..8.1.8..2....9........24...1......9..5...6.8..........5.9.......93.4
            //....5...11......7..6.....8......4.....9.1.3.....596.2..8..62..7..7......3.5.7.2..
            //.47.2....8....1....3....9.2.....5...6..81..5.....4.....7....3.4...9...1.4..27.8..
            //......94.....9...53....5.7..8.4..1..463...........7.8.8..7.....7......28.5.26....
            //.2......6....41.....78....1......7....37.....6..412....1..74..5..8.5..7......39..
            //1.....3.8.6.4..............2.3.1...........758.........7.5...6.....8.2...4.......
            //2....1.9..1..3.7..9..8...2.......85..6.4.........7...3.2.3...6....5.....1.9...2.5
            //..7..8.....6.2.3...3......9.1..5..6.....1.....7.9....2........4.83..4...26....51.
            //...36....85.......9.4..8........68.........17..9..45...1.5...6.4....9..2.....3...
            //34.6.......7.......2..8.57......5....7..1..2....4......36.2..1.......9.......7.82
            //......4.18..2........6.7......8...6..4....3...1.......6......2..5..1....7...3....
            //.4..5..67...1...4....2.....1..8..3........2...6...........4..5.3.....8..2........
            //.......4...2..4..1.7..5..9...3..7....4..6....6..1..8...2....1..85.9...6.....8...3
            //8..7....4.5....6............3.97...8....43..5....2.9....6......2...6...7.71..83.2
            //.8...4.5....7..3............1..85...6.....2......4....3.26............417........
            //....7..8...6...5...2...3.61.1...7..2..8..534.2..9.......2......58...6.3.4...1....
            //......8.16..2........7.5......6...2..1....3...8.......2......7..4..8....5...3....
            //.2..........6....3.74.8.........3..2.8..4..1.6..5.........1.78.5....9..........4.
            //.52..68.......7.2.......6....48..9..2..41......1.....8..61..38.....9...63..6..1.9
            //....1.78.5....9..........4..2..........6....3.74.8.........3..2.8..4..1.6..5.....
            //1.......3.6.3..7...7...5..121.7...9...7........8.1..2....8.64....9.2..6....4.....
            //4...7.1....19.46.5.....1......7....2..2.3....847..6....14...8.6.2....3..6...9....
            //......8.17..2........5.6......7...5..1....3...8.......5......2..3..8....6...4....
            //963......1....8......2.5....4.8......1....7......3..257......3...9.2.4.7......9..
            //15.3......7..4.2....4.72.....8.........9..1.8.1..8.79......38...........6....7423
            //..........5724...98....947...9..3...5..9..12...3.1.9...6....25....56.....7......6
            //....75....1..2.....4...3...5.....3.2...8...1.......6.....1..48.2........7........
            //6.....7.3.4.8.................5.4.8.7..2.....1.3.......2.....5.....7.9......1....
            //....6...4..6.3....1..4..5.77.....8.5...8.....6.8....9...2.9....4....32....97..1..
            //.32.....58..3.....9.428...1...4...39...6...5.....1.....2...67.8.....4....95....6.
            //...5.3.......6.7..5.8....1636..2.......4.1.......3...567....2.8..4.7.......2..5..
            //.5.3.7.4.1.........3.......5.8.3.61....8..5.9.6..1........4...6...6927....2...9..
            //..5..8..18......9.......78....4.....64....9......53..2.6.........138..5....9.714.
            //..........72.6.1....51...82.8...13..4.........37.9..1.....238..5.4..9.........79.
            //...658.....4......12............96.7...3..5....2.8...3..19..8..3.6.....4....473..
            //.2.3.......6..8.9.83.5........2...8.7.9..5........6..4.......1...1...4.22..7..8.9
            //.5..9....1.....6.....3.8.....8.4...9514.......3....2..........4.8...6..77..15..6.
            //.....2.......7...17..3...9.8..7......2.89.6...13..6....9..5.824.....891..........
            //3...8.......7....51..............36...2..4....7...........6.13..452...........8..".Split('\n');

            //            foreach (var game in top95)
            //            {
            //                Console.WriteLine(game);
            //                print_board(search(parse_grid(game)));
            //                search(parse_grid(game));
            //            }
            Console.WriteLine("Press enter to finish");
            Console.ReadLine();
        }




    }
}
