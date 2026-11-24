using System;

namespace chess_game
{
    // random numbers that give every position a (nearly) unique 64-bit fingerprint, used by the transposition table
    public static class Zobrist
    {
        public static readonly ulong[,] PieceSquare = new ulong[13, 64]; // one number per piece on each square (index 0 = Piece.None is unused)
        public static readonly ulong BlackToMove;                         // included when black is to move
        public static readonly ulong[] Castling = new ulong[16];          // one per combination of castling rights (the 0-15 bit flags)
        public static readonly ulong[] EnPassantFile = new ulong[8];      // one per file an en passant capture could land on

        static Zobrist() // runs once, the first time the class is used
        {
            var rng = new Random(20240601); // fixed seed: the same numbers every run, so hashes are reproducible
            for (int piece = 1; piece < 13; piece++)
                for (int square = 0; square < 64; square++)
                    PieceSquare[piece, square] = NextRandom(rng);

            BlackToMove = NextRandom(rng);
            for (int i = 0; i < 16; i++) Castling[i] = NextRandom(rng);
            for (int i = 0; i < 8; i++) EnPassantFile[i] = NextRandom(rng);
        }

        private static ulong NextRandom(Random rng) // Random has no 64-bit method, so join 8 random bytes
        {
            var bytes = new byte[8];
            rng.NextBytes(bytes);
            return BitConverter.ToUInt64(bytes, 0);
        }

        // builds a position's hash from scratch by XOR-ing the numbers for everything in it
        // (Board keeps its hash up to date move by move; this is used when loading a FEN and for checking)
        public static ulong Compute(Board board)
        {
            ulong hash = 0;
            for (int square = 0; square < 64; square++)
            {
                Piece piece = board.Squares[square];
                if (piece != Piece.None) hash ^= PieceSquare[(int)piece, square];
            }

            if (board.SideToMove == PlayerColour.Black) hash ^= BlackToMove;
            hash ^= Castling[board.CastlingRights];
            if (board.EnPassantSquare != -1) hash ^= EnPassantFile[board.EnPassantSquare % 8];
            return hash;
        }
    }
}