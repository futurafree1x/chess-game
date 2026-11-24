using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chess_game
{
    public static class Evaluation
    {
        // material by PieceType; None, King, Pawn, Knight, Bishop, Rook, Queen
        private static readonly int[] mgValues = { 0, 0, 82, 337, 365, 477, 1025 };
        private static readonly int[] egValues = { 0, 0, 94, 281, 297, 512, 936 };
            
        
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
