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

        private static readonly int[] directionOffsets = { 8, -8, 1, -1, 9, 7, -7, -9 }; // N, S, E, W, NE, NW, SE, SW for sliding pieces

        private static readonly int[][] squaresToEdge = CalcSquaresToEdge(); // precomputed table of how many squares to the edge of the board in each direction for each square

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
        private static int[][] CalcSquaresToEdge()
        {
            var table = new int[64][];
            for (int square = 0; square < 64; square++)
            {
                int file = square % 8;
                int rank = square / 8;

                int north = 7 - rank;
                int south = rank;
                int east = 7 - file;
                int west = file;

                table[square] = new[]
                {
            north, south, east, west,
            System.Math.Min(north, east),   // NE
            System.Math.Min(north, west),   // NW
            System.Math.Min(south, east),   // SE
            System.Math.Min(south, west)    // SW
        };
            }
            return table;
        }

        // rook: directions 0-3, bishop: 4-7, queen: 0-7
        private static void AddSlidingMoves(Board board, int from, int firstDir, int lastDir, List<Move> moves)
        {
            for (int dir = firstDir; dir <= lastDir; dir++)
            {
                int to = from;
                for (int step = 0; step < squaresToEdge[from][dir]; step++)
                {
                    to += directionOffsets[dir];
                    Piece target = board.Squares[to];

                    if (target == Piece.None)
                    {
                        moves.Add(new Move(from, to));        // empty, keep sliding
                    }
                    else
                    {
                        if (target.Colour() != board.SideToMove)
                            moves.Add(new Move(from, to));    // enemy, capture it
                        break;                                // blocked either way
                    }
                }
            }
        }
        private static void AddPawnMoves(Board board, int from, List<Move> moves)
        {
            bool isWhite = board.SideToMove == PlayerColour.White;
            int forward = isWhite ? 8 : -8;   // white moves up the board, black moves down
            int startRank = isWhite ? 1 : 6;

            int oneStep = from + forward;
            if (oneStep < 0 || oneStep > 63) return; // pawn on the last rank, only possible until promotion is added

            if (board.Squares[oneStep] == Piece.None) // pawns can only push onto empty squares
            {
                moves.Add(new Move(from, oneStep));
                int twoSteps = oneStep + forward;
                if (from / 8 == startRank && board.Squares[twoSteps] == Piece.None) // first move, both squares empty
                    moves.Add(new Move(from, twoSteps));
            }
            int file = from % 8;
            if (file > 0) AddPawnCapture(board, from, oneStep - 1, moves); // towards the a-file
            if (file < 7) AddPawnCapture(board, from, oneStep + 1, moves); // towards the h-file
        }

        private static void AddPawnCapture(Board board, int from, int to, List<Move> moves)
        {
            Piece target = board.Squares[to];
            if (target != Piece.None && target.Colour() != board.SideToMove) // enemy pieces only
                moves.Add(new Move(from, to));
        }

        // true if 'square' is attacked by any piece belonging to 'attacker'
        public static bool IsSquareAttacked(Board board, int square, PlayerColour attacker)
        {
            bool white = attacker == PlayerColour.White;
            Piece knight = white ? Piece.wKnight : Piece.bKnight;
            Piece bishop = white ? Piece.wBishop : Piece.bBishop;
            Piece rook = white ? Piece.wRook : Piece.bRook;
            Piece queen = white ? Piece.wQueen : Piece.bQueen;
            Piece pawn = white ? Piece.wPawn : Piece.bPawn;
            Piece king = white ? Piece.wKing : Piece.bKing;

            // knights and kings: if one sits on a square we could jump to, it can jump to us
            foreach (int s in knightTargets[square])
                if (board.Squares[s] == knight) return true;
            foreach (int s in kingTargets[square])
                if (board.Squares[s] == king) return true;
            // sliding pieces: walk out from the square in each direction until the first piece
            for (int dir = 0; dir < 8; dir++)
            {
                int s = square;
                for (int step = 0; step < squaresToEdge[square][dir]; step++)
                {
                    s += directionOffsets[dir];
                    Piece p = board.Squares[s];
                    if (p == Piece.None) continue; // empty, keep looking

                    bool straight = dir < 4; // directions 0-3 are straight, 4-7 are diagonal
                    if (p == queen || (straight && p == rook) || (!straight && p == bishop))
                        return true;
                    break; // any other piece blocks this direction
                }

                // pawns: an attacking pawn stands one rank behind the square, on a neighbouring file
                int behind = square + (white ? -8 : 8);
                if (behind >= 0 && behind < 64)
                {
                    int file = square % 8;
                    if (file > 0 && board.Squares[behind - 1] == pawn) return true; // from the a-file side
                    if (file < 7 && board.Squares[behind + 1] == pawn) return true; // from the h-file side
                }
            }
            return false;
        }

        public static int FindKing(Board board, PlayerColour colour)
        {
            Piece king = colour == PlayerColour.White ? Piece.wKing : Piece.bKing;
            for (int i = 0; i < 64; i++)
                if (board.Squares[i] == king) return i;
            return -1; // no king on the board
        }

        public static bool IsInCheck(Board board, PlayerColour colour)
        {
            int kingSquare = FindKing(board, colour);
            PlayerColour enemy = colour == PlayerColour.White ? PlayerColour.Black : PlayerColour.White;
            return kingSquare != -1 && IsSquareAttacked(board, kingSquare, enemy);
        }

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
                    case PieceType.Rook:
                        AddSlidingMoves(board, from, 0, 3, moves);
                        break;
                    case PieceType.Bishop:
                        AddSlidingMoves(board, from, 4, 7, moves);
                        break;
                    case PieceType.Queen:
                        AddSlidingMoves(board, from, 0, 7, moves);
                        break;
                    case PieceType.Pawn:
                        AddPawnMoves(board, from, moves);
                        break;
                }
            }
            return moves;
        }

        public static List<Move> GenerateLegalMoves(Board board)
        {
            PlayerColour us = board.SideToMove;
            var legalMoves = new List<Move>();

            foreach (Move move in GenerateMoves(board))
            {
                board.MakeMove(move);
                if (!IsInCheck(board, us)) // our king is safe after this move
                    legalMoves.Add(move);
                board.UndoMove();
            }
            return legalMoves;
        }
    }
}