using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chess_game
{
    public static class Search
    {
        private const int Infinity = 1000000;
        public static Move FindBestMove(Board board, int depth) // finds the best move, (depth) moves ahead
        {
            List<Move> moves = MoveGen.GenerateLegalMoves(board);
            Move bestMove = moves[0];
            int bestScore = -Infinity;

            foreach (Move move in moves)
            {
                board.MakeMove(move);
                int score = -Negamax(board, depth - 1);
                board.UndoMove();

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }
            return bestMove;
        }
        // best score the side to move can reach, looking (depth) moves ahead
        private static int Negamax(Board board, int depth)
        {
            if (depth == 0) return EvaluateForSideToMove(board);

            int bestScore = -Infinity;
            foreach (Move move in MoveGen.GenerateLegalMoves(board))
            {
                board.MakeMove(move);
                int score = -Negamax(board, depth - 1); // opponents best flipped to players side
                board.UndoMove();

                if (score > bestScore) bestScore = score;
            }
            return bestScore;
        }
        private static int EvaluateForSideToMove(Board board) 
        {
            int score = Evaluation.Evaluate(board);
            return board.SideToMove == PlayerColour.White ? score : -score;
        }

    }
}
