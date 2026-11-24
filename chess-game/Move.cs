using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chess_game
{
    public enum MoveFlag
    {
        None,
        Castle,      
        EnPassant
    }
    public readonly struct Move
    {
        public readonly int From;  // square index 0-63
        public readonly int To;    // square index 0-63
        public readonly Piece Promotion;
        public readonly MoveFlag Flag;     // marks special moves

        public Move(int from, int to, Piece promotion = Piece.None, MoveFlag flag = MoveFlag.None)
        {
            From = from;
            To = to;
            Promotion = promotion;
            Flag = flag;
        }
    }
}
