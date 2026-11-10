namespace chess_game
{
    public enum Piece
    {
        None,
        wKing, wPawn, wKnight, wBishop, wRook, wQueen,
        bKing, bPawn, bKnight, bBishop, bRook, bQueen
    }

    public enum PlayerColour
    {
        White, Black
    }

    public enum PieceType
    {
        None, King, Pawn, Knight, Bishop, Rook, Queen
    }

    public static class PieceExtensions
    {
        public static bool IsWhite(this Piece p) => p >= Piece.wKing && p <= Piece.wQueen;
        public static bool IsBlack(this Piece p) => p >= Piece.bKing;

        public static PlayerColour Colour(this Piece p) =>
            p.IsWhite() ? PlayerColour.White : PlayerColour.Black;

        public static PieceType Type(this Piece p) =>
            p == Piece.None ? PieceType.None : (PieceType)(((int)p - 1) % 6 + 1);
    }


}
