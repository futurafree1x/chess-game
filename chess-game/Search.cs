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
        public static int LastDepth { get; private set; }
        // remembers positions already searched; 2^20 slots (about 40 MB), kept between moves because positions repeat
        private static readonly TranspositionTable table = new TranspositionTable(20);
        private const int MateThreshold = MateScore - 1000; // any score beyond this is a mate score

        public static Move Think(Board board, int maxDepth, int timeLimitMs)
        {
            timer.Restart();
            stopped = false;
            stopTimeMs = long.MaxValue;

            Move bestMove = FindBestMove(board, 1);
            LastDepth = 1;
            stopTimeMs = timeLimitMs;

            for (int depth = 2; depth <= maxDepth; depth++)
            {
                if (timer.ElapsedMilliseconds >= timeLimitMs / 2) break;

                Move move = FindBestMove(board, depth, bestMove);
                if (stopped) break;
                bestMove = move;
                LastDepth = depth;
            }
            stopTimeMs = long.MaxValue;
            stopped = false;
            return bestMove;
        }

        public static Move FindBestMove(Board board, int depth, Move? previousBest = null) // finds the best move, (depth) moves ahead
        {
            List<Move> moves = MoveGen.GenerateLegalMoves(board);
            OrderMoves(board, moves);
            if (previousBest.HasValue) MoveToFront(moves, previousBest.Value);
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

            // transposition table: have we searched this exact position before?
            int originalAlpha = alpha;
            Move? tableMove = null;
            if (table.TryGet(board.Hash, out TableEntry entry))
            {
                if (entry.HasMove) tableMove = entry.BestMove; // even a shallower result tells us a good move to try first

                if (entry.Depth >= depth) // searched at least as deep as we need: the stored score can be reused
                {
                    int stored = ScoreFromTable(entry.Score, ply);
                    if (entry.Bound == BoundType.Exact) return stored;
                    if (entry.Bound == BoundType.LowerBound && stored >= beta) return stored;  // already proven too good
                    if (entry.Bound == BoundType.UpperBound && stored <= alpha) return stored; // already proven too weak
                }
            }

            List<Move> moves = MoveGen.GenerateLegalMoves(board);
            if (moves.Count == 0) // no legal moves
                return MoveGen.IsInCheck(board, board.SideToMove) ? -MateScore + ply : 0;

            OrderMoves(board, moves);
            if (tableMove.HasValue) MoveToFront(moves, tableMove.Value); // best move from last time goes first

            int bestScore = -Infinity;
            Move bestMove = moves[0];
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                int score = -Negamax(board, depth - 1, ply + 1, -beta, -alpha); // opponents best flipped to players side
                board.UndoMove();
                if (stopped) return 0;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
                if (score > alpha) alpha = score; // raise guaranteed score
                if (alpha >= beta) break; // wont allow this position, skip remaining moves
            }

            // remember the result, and what kind of result it is
            BoundType bound = bestScore <= originalAlpha ? BoundType.UpperBound  // nothing beat alpha: at most this
                            : bestScore >= beta ? BoundType.LowerBound           // cut off early: at least this
                            : BoundType.Exact;
            table.Store(board.Hash, depth, ScoreToTable(bestScore, ply), bound, bestMove);
            return bestScore;
        }

        // mate scores depend on distance from the root (ply), but a stored position can be reached at a different ply,
        // so they're stored as "mate in N from this position" and converted back when read
        private static int ScoreToTable(int score, int ply)
        {
            if (score > MateThreshold) return score + ply;
            if (score < -MateThreshold) return score - ply;
            return score;
        }

        private static int ScoreFromTable(int score, int ply)
        {
            if (score > MateThreshold) return score - ply;
            if (score < -MateThreshold) return score + ply;
            return score;
        }

        // moves one move to the front of the list (if it's there), keeping the others in order
        private static void MoveToFront(List<Move> moves, Move move)
        {
            int index = moves.IndexOf(move);
            if (index > 0) // found, and not already first
            {
                moves.RemoveAt(index);
                moves.Insert(0, move);
            }
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
