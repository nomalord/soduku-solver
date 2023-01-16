using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omega_Sudoku
{
    public class IllegalBoardException : Exception
    {
        public IllegalBoardException(string message) : base(message)
        {
        }
    }
}
