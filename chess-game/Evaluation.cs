using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chess_game
{
    public static class Evaluation
    {
        private static readonly int[] pieceValues =
        {
            0, // none
            0, 100, 320, 330, 500, 900, // white pieces: King, Pawn, Knight, Bishop, Rook, Queen
            0, -100, -320, -330, -500, -900 // black pieces: King, Pawn, Knight, Bishop, Rook, Queen
        };
        
        public static int Evaluate(Board board)
        {
            int score = 0;
            for (int square = 0; square < 64; square++)
            {
                score += pieceValues[(int)board.Squares[square]];
            }
            return score;
        }
    }
}
