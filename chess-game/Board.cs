using System.Collections.Generic;

namespace chess_game
{
    public class Board
    {
        public const string StartFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"; // starting position in FEN notation
        public Piece[] Squares = new Piece[64]; // creates logical board of 64 squares, each square can hold a piece
        public PlayerColour SideToMove = PlayerColour.White;
        public bool WhiteCastleKingside, WhiteCastleQueenside; // true if castle possible, false if not
        public bool BlackCastleKingside, BlackCastleQueenside; // ditto
        public int EnPassantSquare = -1; // -1 means no en passant square, otherwise it is the index of the square where en passant is possible
        private static readonly Dictionary<char, Piece> fenPieces = new Dictionary<char, Piece>
        {
            ['K'] = Piece.wKing,
            ['P'] = Piece.wPawn,
            ['N'] = Piece.wKnight,
            ['B'] = Piece.wBishop,
            ['R'] = Piece.wRook,
            ['Q'] = Piece.wQueen,
            ['k'] = Piece.bKing,
            ['p'] = Piece.bPawn,
            ['n'] = Piece.bKnight,
            ['b'] = Piece.bBishop,
            ['r'] = Piece.bRook,
            ['q'] = Piece.bQueen
        };
        public void LoadFen(string fen)
        {
            string[] parts = fen.Split(' ');

            // 1. piece placement, from rank 8 down to rank 1
            Squares = new Piece[64];
            int file = 0, rank = 7;
            foreach (char c in parts[0])
            {
                if (c == '/') { file = 0; rank--; }
                else if (char.IsDigit(c)) { file += c - '0'; }
                else { Squares[rank * 8 + file] = fenPieces[c]; file++; }
            }

            // 2. side to move
            SideToMove = parts[1] == "w" ? PlayerColour.White : PlayerColour.Black;

            // 3. castling rights
            WhiteCastleKingside = parts[2].Contains("K");
            WhiteCastleQueenside = parts[2].Contains("Q");
            BlackCastleKingside = parts[2].Contains("k");
            BlackCastleQueenside = parts[2].Contains("q");

            // 4. en passant square, e.g. "e3", or "-" for none
            EnPassantSquare = parts[3] == "-" ? -1 : (parts[3][1] - '1') * 8 + (parts[3][0] - 'a');
        }
        // the rook's start and end squares for a castling move
        private static void GetCastleRookSquares(Move move, out int rookFrom, out int rookTo)
        {
            bool kingside = move.To > move.From;            // king moving towards the h-file
            rookFrom = kingside ? move.From + 3 : move.From - 4; // h-file or a-file rook
            rookTo = kingside ? move.From + 1 : move.From - 1; // the square the king passes over
        }
        private readonly Stack<(Move move, Piece captured)> history = new Stack<(Move move, Piece captured)>();

        public void MakeMove(Move move)
        {
            Piece captured = Squares[move.To];
            history.Push((move, captured));

            Squares[move.To] = move.Promotion != Piece.None ? move.Promotion : Squares[move.From];
            Squares[move.From] = Piece.None;
            if (move.Flag == MoveFlag.Castle) // the rook jumps over the king
            {
                GetCastleRookSquares(move, out int rookFrom, out int rookTo);
                Squares[rookTo] = Squares[rookFrom];
                Squares[rookFrom] = Piece.None;
            }
            SideToMove = SideToMove == PlayerColour.White ? PlayerColour.Black : PlayerColour.White;
        }
        public void UndoMove()
        {
            if (history.Count == 0) return;

            var (move, captured) = history.Pop();

            if (move.Promotion != Piece.None)
                Squares[move.From] = move.Promotion.IsWhite() ? Piece.wPawn : Piece.bPawn; // un-promote
            else
                Squares[move.From] = Squares[move.To];
            Squares[move.To] = captured;
            if (move.Flag == MoveFlag.Castle) // put the rook back in its corner
            {
                GetCastleRookSquares(move, out int rookFrom, out int rookTo);
                Squares[rookFrom] = Squares[rookTo];
                Squares[rookTo] = Piece.None;
            }
            SideToMove = SideToMove == PlayerColour.White ? PlayerColour.Black : PlayerColour.White;
        }
    }
}
