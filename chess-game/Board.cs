using System.Collections.Generic;

namespace chess_game
{
    public class Board
    {
        public const string StartFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"; // starting position in FEN notation
        public Piece[] Squares = new Piece[64]; // creates logical board of 64 squares, each square can hold a piece
        public PlayerColour SideToMove = PlayerColour.White;
        public const int WhiteKingside = 1, WhiteQueenside = 2, BlackKingside = 4, BlackQueenside = 8;
        public int CastlingRights;
        public bool CanCastle(int right) => (CastlingRights & right) != 0;
        // castlingMask[square] = the rights that survive a move from or to that square
        private static readonly int[] castlingMask = CalcCastlingMask();
        private static int[] CalcCastlingMask()
        {
            var mask = new int[64];
            for (int i = 0; i < 64; i++) mask[i] = 15; // 15 = binary 1111, keeps every right

            mask[4] = 15 & ~(WhiteKingside | WhiteQueenside); // e1, white king
            mask[7] = 15 & ~WhiteKingside;                    // h1 rook
            mask[0] = 15 & ~WhiteQueenside;                   // a1 rook
            mask[60] = 15 & ~(BlackKingside | BlackQueenside); // e8, black king
            mask[63] = 15 & ~BlackKingside;                    // h8 rook
            mask[56] = 15 & ~BlackQueenside;                   // a8 rook
            return mask;
        }
        public int EnPassantSquare = -1; // -1 means no en passant square, otherwise it is the index of the square where en passant is possible
                                         // the pawn taken en passant is on the same rank as the capturer, on the file it lands on
        private static int EnPassantCaptureSquare(Move move) => move.From / 8 * 8 + move.To % 8;
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
            CastlingRights = 0;
            if (parts[2].Contains("K")) CastlingRights |= WhiteKingside;
            if (parts[2].Contains("Q")) CastlingRights |= WhiteQueenside;
            if (parts[2].Contains("k")) CastlingRights |= BlackKingside;
            if (parts[2].Contains("q")) CastlingRights |= BlackQueenside;

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
        private readonly Stack<(Move move, Piece captured, int castlingRights, int enPassantSquare)> history = new Stack<(Move move, Piece captured, int castlingRights, int enPassantSquare)>();

        public void MakeMove(Move move)
        {
            Piece moving = Squares[move.From];
            Piece captured = Squares[move.To];
            
            history.Push((move, captured, CastlingRights, EnPassantSquare));

            Squares[move.To] = move.Promotion != Piece.None ? move.Promotion : Squares[move.From];
            Squares[move.From] = Piece.None;
            if (move.Flag == MoveFlag.Castle) // the rook jumps over the king
            {
                GetCastleRookSquares(move, out int rookFrom, out int rookTo);
                Squares[rookTo] = Squares[rookFrom];
                Squares[rookFrom] = Piece.None;
            }
            CastlingRights &= castlingMask[move.From] & castlingMask[move.To]; // lose rights if king or rook moved or was captured
            bool doublePush = moving.Type() == PieceType.Pawn && System.Math.Abs(move.To - move.From) == 16;
            if (move.Flag == MoveFlag.EnPassant) // remove the pawn beside us, not the one on the destination
                Squares[EnPassantCaptureSquare(move)] = Piece.None;
            EnPassantSquare = doublePush ? (move.From + move.To) / 2 : -1;
            SideToMove = SideToMove == PlayerColour.White ? PlayerColour.Black : PlayerColour.White;
        }
        public void UndoMove()
        {
            if (history.Count == 0) return;

            var (move, captured, castlingRights, enPassantSquare) = history.Pop();
            CastlingRights = castlingRights;
            EnPassantSquare = enPassantSquare;

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
            if (move.Flag == MoveFlag.EnPassant) // put the captured pawn back beside us
                Squares[EnPassantCaptureSquare(move)] = Squares[move.From] == Piece.wPawn ? Piece.bPawn : Piece.wPawn;
            SideToMove = SideToMove == PlayerColour.White ? PlayerColour.Black : PlayerColour.White;
        }
    }
}
