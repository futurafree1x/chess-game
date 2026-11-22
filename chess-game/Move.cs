using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chess_game
{
    public readonly struct Move
    {
        public readonly int From;  // square index 0-63
        public readonly int To;    // square index 0-63

        public Move(int from, int to)
        {
            From = from;
            To = to;
        }
    }
}
