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
    }
}
