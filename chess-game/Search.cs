using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chess_game
{
    public static class Search
    {
        public static Move FindBestMove(Board board)
        {
            List<Move> moves = MoveGen.GenerateLegalMoves(board);
            Move bestMove = moves[0];
            int bestScore = int.MinValue;

            foreach (Move move in moves)
            {
                board.MakeMove(move);
                int score = -EvaluateForSideToMove(board);
                board.UndoMove();

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }
            return bestMove;
        }
        private static int EvaluateForSideToMove(Board board)
        {
            int score = Evaluation.Evaluate(board);
            return board.SideToMove == PlayerColour.White ? score : -score;
        }

    }
}
