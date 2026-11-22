using System.Collections.Generic;

namespace chess_game
{
    public static class MoveGen
    {
        

        private static readonly int[][] knightTargets = CalcTargets(
            new[] { 1, 2, 2, 1, -1, -2, -2, -1 },   // file steps
            new[] { 2, 1, -1, -2, -2, -1, 1, 2 });  // rank steps

        private static readonly int[][] kingTargets = CalcTargets(
            new[] { 0, 1, 1, 1, 0, -1, -1, -1 },
            new[] { 1, 1, 0, -1, -1, -1, 0, 1 });

        private static int[][] CalcTargets(int[] fileSteps, int[] rankSteps)
        {
            var table = new int[64][];
            for (int square = 0; square < 64; square++)
            {
                int file = square % 8;
                int rank = square / 8;
                var targets = new List<int>();

                for (int i = 0; i < fileSteps.Length; i++)
                {
                    int f = file + fileSteps[i];
                    int r = rank + rankSteps[i];
                    if (f >= 0 && f < 8 && r >= 0 && r < 8) // stays on the board
                        targets.Add(r * 8 + f);
                }
                table[square] = targets.ToArray();
            }
            return table;
        }
        private static void AddStepMoves(Board board, int from, int[] targets, List<Move> moves)
        {
            foreach (int to in targets)
            {
                Piece target = board.Squares[to];
                if (target == Piece.None || target.Colour() != board.SideToMove) // empty or enemy
                    moves.Add(new Move(from, to));
            }
        }

        // every move for the side to move (ignoring check for now)
        public static List<Move> GenerateMoves(Board board)
        {
            var moves = new List<Move>();

            for (int from = 0; from < 64; from++)
            {
                Piece piece = board.Squares[from];
                if (piece == Piece.None || piece.Colour() != board.SideToMove) continue;

                switch (piece.Type())
                {
                    case PieceType.Knight:
                        AddStepMoves(board, from, knightTargets[from], moves);
                        break;
                    case PieceType.King:
                        AddStepMoves(board, from, kingTargets[from], moves);
                        break;
                }
            }
            return moves;
        }
    }
}