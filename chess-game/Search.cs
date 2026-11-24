using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace chess_game
{
    public static class Search
    {
        private const int Infinity = 1000000;
        private const int MateScore = 100000; // smaller than infinity, bigger than any evaluation
        private static readonly int[] orderValues = { 0, 10, 1, 3, 3, 5, 9 };// rough piece values usedfor ordering: None, King, Pawn, Knight, Bishop, Rook, Queen
        private static readonly Stopwatch timer = new Stopwatch(); // how long the engine has been thinking
        private static long stopTimeMs = long.MaxValue;            // search should stop once the timer passes this
        private static bool stopped; // true when time runs out in the middle of search

        public static Move Think(Board board, int maxDepth, int timeLimitMs)
        {
            timer.Restart();
            stopped = false;
            stopTimeMs = long.MaxValue;

            Move bestMove = FindBestMove(board, 1);
            stopTimeMs = timeLimitMs;

            for (int depth = 2; depth <= maxDepth; depth++)
            {
                if (timer.ElapsedMilliseconds >= timeLimitMs / 2) break;

                Move move = FindBestMove(board, depth);
                if (stopped) break;
                bestMove = move;
            }
            return bestMove;
        }

        public static Move FindBestMove(Board board, int depth) // finds the best move, (depth) moves ahead
        {
            List<Move> moves = MoveGen.GenerateLegalMoves(board);
            OrderMoves(board, moves);
            Move bestMove = moves[0];
            int bestScore = -Infinity;
            int alpha = -Infinity, beta = Infinity;

            foreach (Move move in moves)
            {
                board.MakeMove(move);
                int score = -Negamax(board, depth - 1, 1, -beta, -alpha);
                board.UndoMove();
                if (stopped) break;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                    alpha = score;
                }
            }
            return bestMove;
        }

        private static int MoveOrderScore(Board board, Move move)
        {
            if (!IsCapture(board, move)) return 0; // quiet moves go last

            int victim = move.Flag == MoveFlag.EnPassant ? 1 : orderValues[(int)board.Squares[move.To].Type()];
            int attacker = orderValues[(int)board.Squares[move.From].Type()];
            return 100 + 10 * victim - attacker; //100 keeps every capture above every quiet move
        }
       
        private static int Negamax(Board board, int depth, int ply, int alpha, int beta)  // best score the side to move can reach, looking (depth) moves ahead
        {
            if (timer.ElapsedMilliseconds >= stopTimeMs)
            {
                stopped = true;
                return 0;
            }

            if (depth == 0)
            {
                if (MoveGen.IsInCheck(board, board.SideToMove) && MoveGen.GenerateLegalMoves(board).Count == 0)
                    return -MateScore + ply;
                return Quiescence(board, alpha, beta);
            }

            List<Move> moves = MoveGen.GenerateLegalMoves(board);
            if (moves.Count == 0) // no legal moves
                return MoveGen.IsInCheck(board, board.SideToMove) ? -MateScore + ply : 0;

            OrderMoves(board, moves);

            int bestScore = -Infinity;
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                int score = -Negamax(board, depth - 1, ply + 1, -beta, -alpha); // opponents best flipped to players side
                board.UndoMove();
                if (stopped) return 0;

                if (score > bestScore) bestScore = score;
                if (score > alpha) alpha = score; // raise guaranteed score
                if (alpha >= beta) break; // wont allow this position, skip remaining moves
            }
            return bestScore;
        }

        private static int Quiescence(Board board, int alpha, int beta)
        {
            int standPat = EvaluateForSideToMove(board); // score if we stop capturing now
            if (standPat >= beta) return standPat;        // already too good, the opponent won't allow this
            if (standPat > alpha) alpha = standPat;       // stopping is always allowed

            List<Move> moves = MoveGen.GenerateLegalMoves(board);
            OrderMoves(board, moves); // best captures first, cut-offs happen sooner

            foreach (Move move in moves)
            {
                if (!IsCapture(board, move)) continue;    // only captures are searched here

                board.MakeMove(move);
                int score = -Quiescence(board, -beta, -alpha);
                board.UndoMove();

                if (score >= beta) return score;          // opponent won't allow this, stop searching
                if (score > alpha) alpha = score;         // best capture sequence so far
            }
            return alpha;
        }

        private static int EvaluateForSideToMove(Board board) 
        {
            int score = Evaluation.Evaluate(board);
            return board.SideToMove == PlayerColour.White ? score : -score;
        }

        private static bool IsCapture(Board board, Move move)
        {
            return board.Squares[move.To] != Piece.None || move.Flag == MoveFlag.EnPassant;
        }

        private static void OrderMoves(Board board, List<Move> moves)
        {
            moves.Sort((a, b) => MoveOrderScore(board, b).CompareTo(MoveOrderScore(board, a)));
        }

    }
}
