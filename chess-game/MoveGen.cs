using System.Collections.Generic;

namespace chess_game
{
    public static class MoveGenerator
    {
        // knightTargets[square] = every square a knight on that square can jump to.
        // Worked out once when the program starts, so the game only looks them up.
        private static readonly int[][] knightTargets = CalcKnightTargets();

        private static int[][] CalcKnightTargets()
        {
            int[] fileSteps = { 1, 2, 2, 1, -1, -2, -2, -1 };
            int[] rankSteps = { 2, 1, -1, -2, -2, -1, 1, 2 };

            var table = new int[64][];
            for (int square = 0; square < 64; square++)
            {
                int file = square % 8;
                int rank = square / 8;
                var targets = new List<int>();

                for (int i = 0; i < 8; i++)
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
                        AddKnightMoves(board, from, moves);
                        break;
                }
            }
            return moves;
        }

        private static void AddKnightMoves(Board board, int from, List<Move> moves)
        {
            foreach (int to in knightTargets[from])
            {
                Piece target = board.Squares[to];
                if (target == Piece.None || target.Colour() != board.SideToMove) // empty or enemy
                    moves.Add(new Move(from, to));
            }
        }
    }
}